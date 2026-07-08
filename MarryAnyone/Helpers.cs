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
        // Bannerlord v1.4.6: Hero.ExSpouses is now a get-only property (MBReadOnlyList<Hero>) backed by
        // the field _exSpouses, whose type changed from List<Hero> to MBList<Hero>. There is no longer a
        // separate ExSpouses backing field, so we mutate the _exSpouses list in place.
        private static readonly AccessTools.FieldRef<Hero, MBList<Hero>>? _exSpouses = AccessTools2.FieldRefAccess<Hero, MBList<Hero>>("_exSpouses");

        // Overwrite a hero's ex-spouse list in place so the ExSpouses property (which wraps this field) stays in sync.
        private static void SetExSpouses(Hero hero, List<Hero> newList)
        {
            MBList<Hero> list = _exSpouses!(hero);
            if (list is null)
            {
                list = new MBList<Hero>();
                _exSpouses!(hero) = list;
            }
            list.Clear();
            foreach (Hero h in newList)
            {
                list.Add(h);
            }
        }

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
            if (_exSpouses is null)
            {
                return;
            }

            var exSpouses = hero.ExSpouses?.ToList() ?? new List<Hero>();

            // InformationManager.DisplayMessage(new InformationMessage($"DEBUG: RemoveExSpouses({removalMode})", Colors.Red));

            if (removalMode == RemoveExSpousesMode.Duplicates)
            {
                // Remove duplicates from own list and nulls
                exSpouses = exSpouses.Where(ex => ex is not null).Distinct().ToList();

                // Remove current spouse from ex-spouses list
                if (hero.Spouse is not null && exSpouses.Contains(hero.Spouse))
                {
                    exSpouses.Remove(hero.Spouse);
                    Print($"Removed active spouse {hero.Spouse.Name} from ex-spouses.");
                }
            }
            else
            {
                var cleaned = new List<Hero>();

                foreach (var exSpouse in exSpouses.Where(ex => ex is not null).ToList())
                {
                    if (!exSpouse.IsAlive)
                        continue;

                    if (removalMode == RemoveExSpousesMode.Self || removalMode == RemoveExSpousesMode.All)
                    {
                        // Remove from hero list
                        cleaned.Add(exSpouse);
                    }

                    if (removalMode == RemoveExSpousesMode.All)
                    {
                        var theirExSpouses = _exSpouses(exSpouse)?.ToList() ?? new List<Hero>();
                        theirExSpouses.Remove(hero);

                        SetExSpouses(exSpouse, theirExSpouses);
                    }
                }

                // Remove from hero after loop
                foreach (var ex in cleaned)
                    exSpouses.Remove(ex);
            }

            SetExSpouses(hero, exSpouses);

            Print($"Ex-spouses after cleanup: {exSpouses.Count}");
        }

        public static void CheatOnSpouse()
        {
            if (_exSpouses is null)
            {
                return;
            }
            MBList<Hero> _exSpousesList = _exSpouses(Hero.MainHero);
            List<Hero> cheatedHeroes = _exSpousesList?.Where(exSpouse => exSpouse.IsAlive).ToList() ?? new List<Hero>();

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