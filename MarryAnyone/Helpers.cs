using HarmonyLib;
using HarmonyLib.BUTR.Extensions;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Library;
using static MarryAnyone.Debug;

namespace MarryAnyone
{
    internal static class Helpers
    {
        private static readonly AccessTools.FieldRef<Hero, MBReadOnlyList<Hero>>? ExSpouses = AccessTools2.FieldRefAccess<Hero, MBReadOnlyList<Hero>>("ExSpouses");

        private static readonly AccessTools.FieldRef<Hero, List<Hero>>? _exSpouses = AccessTools2.FieldRefAccess<Hero, List<Hero>>("_exSpouses");

        public enum RemoveExSpousesMode
        {
            Duplicates,
            Self,
            All
        }

        public static void ResetEndedCourtships()
        {
            foreach (Romance.RomanticState romanticState in Romance.RomanticStateList.ToList())
            {
                if (romanticState.Person1 == Hero.MainHero || romanticState.Person2 == Hero.MainHero)
                {
                    if (romanticState.Level == Romance.RomanceLevelEnum.Ended)
                    {
                        romanticState.Level = Romance.RomanceLevelEnum.Untested;
                    }
                }
            }
        }

        public static void RemoveExSpouses(Hero hero, RemoveExSpousesMode removalMode = RemoveExSpousesMode.Duplicates)
        {
            var exSpouses = hero.ExSpouses?.ToList() ?? new List<Hero>();

            // InformationManager.DisplayMessage(new InformationMessage($"DEBUG: RemoveExSpouses({removalMode})", Colors.Red));

            if (removalMode == RemoveExSpousesMode.Duplicates)
            {
                // Remove duplicatas da própria lista
                exSpouses = exSpouses.Distinct().ToList();

                // Remove o cônjuge atual da lista de ex-cônjuges
                if (hero.Spouse is not null && exSpouses.Contains(hero.Spouse))
                {
                    exSpouses.Remove(hero.Spouse);
                    Print($"Removed active spouse {hero.Spouse.Name} from ex-spouses.");
                }
            }
            else
            {
                var cleaned = new List<Hero>();

                foreach (var exSpouse in exSpouses.ToList()) // snapshot seguro
                {
                    if (!exSpouse.IsAlive)
                        continue;

                    if (removalMode == RemoveExSpousesMode.Self || removalMode == RemoveExSpousesMode.All)
                    {
                        // Remove da lista do herói
                        cleaned.Add(exSpouse);
                    }

                    if (removalMode == RemoveExSpousesMode.All)
                    {
                        var theirExSpouses = _exSpouses(exSpouse)?.ToList() ?? new List<Hero>();
                        theirExSpouses.Remove(hero);

                        _exSpouses(exSpouse) = theirExSpouses;
                        ExSpouses(exSpouse) = new MBReadOnlyList<Hero>(theirExSpouses);
                    }
                }

                // Apaga do herói após o loop
                foreach (var ex in cleaned)
                    exSpouses.Remove(ex);
            }

            _exSpouses(hero) = exSpouses;
            ExSpouses(hero) = new MBReadOnlyList<Hero>(exSpouses);

            Print($"Ex-spouses after cleanup: {exSpouses.Count}");
        }

        public static void CheatOnSpouse()
        {
            List<Hero> _exSpousesList = _exSpouses!(Hero.MainHero);
            List<Hero> cheatedHeroes = _exSpousesList.Where(exSpouse => exSpouse.IsAlive).ToList();

            foreach (Hero cheatedHero in cheatedHeroes)
            {
                RemoveExSpouses(cheatedHero, RemoveExSpousesMode.All);
                if (cheatedHero != Hero.MainHero.Spouse)
                {
                    // Almost forgot to add in an ended romantic state for cheated heroes!
                    ChangeRomanticStateAction.Apply(Hero.MainHero, cheatedHero, Romance.RomanceLevelEnum.Ended);
                    Print($"Broke off marriage with {cheatedHero.Name}");
                }
                else
                {
                    Print($"Removed duplicate spouse {cheatedHero.Name}");
                }
            }
        }
    }
}