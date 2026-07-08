# Changelog

## [3.4.0] - Bannerlord v1.4.6 compatibility fix

Fixes the crash and broken features introduced by the June 2026 Bannerlord update (game v1.4.6),
which renamed/removed several internal APIs the mod accesses via reflection.

### Fixed
- **Crash when a marriage is finalized (e.g. taking a second wife).** The game renamed
  `CampaignEventDispatcher.OnHeroesMarried(Hero, Hero, bool)` to `OnBeforeHeroesMarried`. The mod's
  reflection delegate no longer resolved, throwing `MissingMethodException` on marriage.
  (`MarryAnyone/Actions/MAMarriageAction.cs`)
- **Incest / relation check.** `DefaultMarriageModel.DiscoverAncestors(Hero, int)` was removed; the
  relation check now uses `AreHeroesRelated(Hero, Hero, int)`. (`MarryAnyone/Models/MAMarriageModel.cs`)
- **Companion (townsperson) equipment adjustment.** `CompanionsCampaignBehavior.AdjustEquipment` was
  renamed to `AdjustEquipments(Hero)`. (`MarryAnyone/Behaviors/MARomanceCampaignBehavior.cs`)
- **Ex-spouse / polygamy cleanup.** `Hero.ExSpouses` is now a get-only property backed by the field
  `_exSpouses`, whose type changed from `List<Hero>` to `MBList<Hero>`. The backing list is now
  mutated in place. (`MarryAnyone/Helpers.cs`)

### Changed
- Retargeted the project to Bannerlord v1.4.6 reference assemblies
  (`Bannerlord.ReferenceAssemblies.Core`/`SandBox` `1.4.6.115628`) and set `<GameVersion>1.4.6</GameVersion>`.
- Added a NuGet `Microsoft.NETFramework.ReferenceAssemblies` reference so the project builds without a
  locally installed .NET Framework 4.7.2 targeting pack.

### Notes
- Built against .NET Framework 4.7.2, C# 9.
- All game API references were audited against the installed v1.4.6 assemblies; the four items above
  were the only breaks.
