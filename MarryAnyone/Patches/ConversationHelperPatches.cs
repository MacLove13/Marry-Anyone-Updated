﻿using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace MarryAnyone.Patches
{
    // Had to change it a bit from before... Might need to be loaded after some mods...
    [HarmonyPatch(typeof(ConversationHelper))]
    internal sealed class ConversationHelperPatches
    {
        [HarmonyPatch("GetHeroRelationToHeroTextShort")]
        private static void Postfix(ref string __result, Hero queriedHero, Hero baseHero, bool uppercaseFirst)
        {
            string tempResult = __result;
            __result = GetHeroRelationToHeroTextShort(queriedHero, baseHero, uppercaseFirst);
            if (__result == "")
            {
                __result = tempResult;
            }
        }

        private static string GetHeroRelationToHeroTextShort(Hero queriedHero, Hero baseHero, bool uppercaseFirst)
        {
            TextObject? textObject = null;
            if (queriedHero == baseHero)
            {
                textObject = GameTexts.FindText("str_you");
            }
            else if (baseHero.Father == queriedHero
                && (baseHero.Spouse == queriedHero || queriedHero.ExSpouses.Contains(baseHero) || baseHero.ExSpouses.Contains(queriedHero)))
            {
                textObject = GameTexts.FindText("str_fatherhusband");
            }
            else if (baseHero.Mother == queriedHero
                && (baseHero.Spouse == queriedHero || queriedHero.ExSpouses.Contains(baseHero) || baseHero.ExSpouses.Contains(queriedHero)))
            {
                textObject = GameTexts.FindText("str_motherwife");
            }
            else if (baseHero.Siblings.Contains(queriedHero)
                && (baseHero.Spouse == queriedHero || queriedHero.ExSpouses.Contains(baseHero) || baseHero.ExSpouses.Contains(queriedHero)))
            {
                if (!queriedHero.IsFemale)
                {
                    textObject = GameTexts.FindText("str_brotherhusband");
                }
                else
                {
                    textObject = GameTexts.FindText("str_sisterwife");
                }
            }
            else if (baseHero.Children.Contains(queriedHero)
                && (baseHero.Spouse == queriedHero || queriedHero.ExSpouses.Contains(baseHero) || baseHero.ExSpouses.Contains(queriedHero)))
            {
                if (!queriedHero.IsFemale)
                {
                    textObject = GameTexts.FindText("str_sonhusband");
                }
                else
                {
                    textObject = GameTexts.FindText("str_daughterwife");
                }
            }
            else if (baseHero.Spouse == queriedHero || queriedHero.ExSpouses.Contains(baseHero) || baseHero.ExSpouses.Contains(queriedHero))
            {
                if (queriedHero.IsDead || baseHero.IsDead)
                {
                    if (!queriedHero.IsFemale)
                    {
                        textObject = GameTexts.FindText("str_exhusband");
                    }
                    else
                    {
                        textObject = GameTexts.FindText("str_exwife");
                    }
                }
                else if (!queriedHero.IsFemale)
                {
                    textObject = GameTexts.FindText("str_husband");
                }
                else
                {
                    textObject = GameTexts.FindText("str_wife");
                }
            }
            else
            {
                /* Section for spouse's spouse */
                if (baseHero.Spouse is not null)
                {
                    // Spouse to ExSpouse
                    foreach (Hero spouse in baseHero.Spouse.ExSpouses)
                    {
                        List<Hero> otherSpouses = spouse.ExSpouses.Where(x => x.IsAlive).ToList();
                        foreach (Hero otherSpouse in otherSpouses)
                        {
                            if (otherSpouse == queriedHero)
                            {
                                textObject = SpousesSpouse(spouse, queriedHero);
                            }
                        }
                    }
                }
                List<Hero> spouses = baseHero.ExSpouses.Where(x => x.IsAlive).ToList();
                foreach (Hero spouse in spouses)
                {
                    // ExSpouse to Spouse
                    if (spouse.Spouse == queriedHero)
                    {
                        textObject = SpousesSpouse(spouse, queriedHero);
                    }
                    List<Hero> otherSpouses = spouse.ExSpouses.Where(x => x.IsAlive).ToList();
                    // ExSpouse to ExSpouse
                    foreach (Hero otherSpouse in otherSpouses)
                    {
                        if (otherSpouse == queriedHero)
                        {
                            textObject = SpousesSpouse(spouse, queriedHero);
                        }
                    }
                }
            }
            /* Continue through new uppercasing from Bannerlord */
            if (textObject == null)
            {
                return "";
            }
            else if (queriedHero != null)
            {
                textObject.SetCharacterProperties("NPC", queriedHero.CharacterObject, false);
            }
            string text = textObject.ToString();
            if (!char.IsLower(text[0]) != uppercaseFirst)
            {
                char[] array = text.ToCharArray();
                text = uppercaseFirst ? array[0].ToString().ToUpper() : array[0].ToString().ToLower();
                for (int i = 1; i < array.Count(); i++)
                {
                    text += array[i].ToString();
                }
            }
            return text;
        }

        private static TextObject SpousesSpouse(Hero spouse, Hero queriedHero)
        {
            MASettings settings = new();

            // Find out spouse's gender
            // Male spouse
            if (!spouse.IsFemale)
            {
                // Find out spouse's spouse's gender
                if (!queriedHero.IsFemale)
                {
                    // Male other spouse
                    return settings.Polyamory ? FindText("str_husband") : FindText("str_husbands_husband");
                }
                // Female other spouse
                return settings.Polyamory ? FindText("str_wife") : FindText("str_husbands_wife");
            }
            if (!queriedHero.IsFemale)
            {
                return settings.Polyamory ? FindText("str_husband") : FindText("str_wifes_husband");
            }
            return settings.Polyamory ? FindText("str_wife") : FindText("str_wifes_wife");
        }

        private static TextObject FindText(string id)
        {
            return GameTexts.FindText(id, null);
        }
    }
}