using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using System.Linq;
using HarmonyLib.BUTR.Extensions;
using System;
using System.Collections.Generic;
using TaleWorlds.Library;

namespace MarryAnyone.Models
{
    internal sealed class MAMarriageModel : DefaultMarriageModel
    {
        private delegate IEnumerable<Hero> DiscoverAncestorsDelegate(DefaultMarriageModel instance, Hero hero, int n);
        private static readonly DiscoverAncestorsDelegate? DiscoverAncestors = AccessTools2.GetDelegate<DiscoverAncestorsDelegate>(typeof(DefaultMarriageModel), "DiscoverAncestors", new Type[] { typeof(Hero), typeof(int) });

        private bool _mainHeroMarriage = false;
        private bool _mainHero = false;

        public override bool IsCoupleSuitableForMarriage(Hero firstHero, Hero secondHero)
        {
            if (firstHero == null || secondHero == null)
            {
                return false;
            }

            bool isMainHero = firstHero == Hero.MainHero || secondHero == Hero.MainHero;
            if (!isMainHero)
            {
                return base.IsCoupleSuitableForMarriage(firstHero, secondHero);
            }

            if (Romance.GetRomanticLevel(firstHero, secondHero) == Romance.RomanceLevelEnum.Marriage)
            {
                return false;
            }

            bool canMarry1 = false;
            bool canMarry2 = false;
            _mainHeroMarriage = true;
            if (firstHero == Hero.MainHero)
            {
                _mainHero = true;
                canMarry1 = firstHero.CanMarry();
                _mainHero = false;

                canMarry2 = secondHero.CanMarry();
            }
            else if (secondHero == Hero.MainHero)
            {
                canMarry1 = firstHero.CanMarry();

                _mainHero = true;
                canMarry2 = secondHero.CanMarry();
                _mainHero = false;
            }
            bool canMarry = canMarry1 && canMarry2;
            _mainHeroMarriage = false;
            if (!canMarry)
            {
                return false;
            }

            MASettings settings = new();
            if (settings == null)
            {
                return false;
            }

            if (!settings.Incest)
            {
                if (DiscoverAncestors != null)
                {
                    if (DiscoverAncestors(this, firstHero, 3).Intersect(DiscoverAncestors(this, secondHero, 3)).Any())
                    {
                        return false;
                    }
                }
            }

            bool isAttracted = true;
            if (settings.SexualOrientation == "Heterosexual")
            {
                isAttracted = firstHero.IsFemale != secondHero.IsFemale;
            }
            else if (settings.SexualOrientation == "Homosexual")
            {
                isAttracted = firstHero.IsFemale == secondHero.IsFemale;
            }
            return isAttracted;
        }

        public override bool IsSuitableForMarriage(Hero maidenOrSuitor)
        {
            if (!_mainHeroMarriage)
            {
                return base.IsSuitableForMarriage(maidenOrSuitor);
            }
            if (maidenOrSuitor.IsDead || maidenOrSuitor.IsTemplate || maidenOrSuitor.IsPrisoner)
            {
                return false;
            }
            MASettings settings = new();
            bool spouses = maidenOrSuitor.Spouse is not null || maidenOrSuitor.ExSpouses.Any(exSpouse => exSpouse.IsAlive);
            if (!spouses || settings.Cheating || (_mainHero && settings.Polygamy))
            {
                if (maidenOrSuitor.IsFemale)
                {
                    return maidenOrSuitor.CharacterObject.Age >= Campaign.Current.Models.MarriageModel.MinimumMarriageAgeFemale;
                }
                return maidenOrSuitor.CharacterObject.Age >= Campaign.Current.Models.MarriageModel.MinimumMarriageAgeMale;
            }
            return false;
        }
    }
}
