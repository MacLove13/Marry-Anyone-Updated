# Building Marry Anyone Updated

## What you need
- The game installed (this provides the reference assemblies via NuGet automatically, so you don't
  strictly need the game files to compile — but you do to test).
- A .NET SDK (8.0+). You do **not** need Visual Studio.

### Getting a .NET SDK without installing Visual Studio
You can use a portable SDK that lives in a folder and needs no admin rights:

```powershell
# Download the official installer script and extract an SDK into .\dotnetsdk
Invoke-WebRequest -Uri "https://dot.net/v1/dotnet-install.ps1" -OutFile "dotnet-install.ps1"
./dotnet-install.ps1 -Channel 8.0 -Quality GA -InstallDir ".\dotnetsdk" -NoPath
```

## Build
```powershell
.\dotnetsdk\dotnet.exe build MarryAnyone\MarryAnyone.csproj -c Release
```
The compiled mod appears at `MarryAnyone\bin\Release\net472\MarryAnyone.dll`.

## Install / test the build
Copy that `MarryAnyone.dll` over the one in your module's
`bin\Win64_Shipping_Client\` folder:

- Steam Workshop install:
  `...\steamapps\workshop\content\261550\<workshop id>\bin\Win64_Shipping_Client\MarryAnyone.dll`
- Manual install:
  `...\Mount & Blade II Bannerlord\Modules\MarryAnyoneUpdated\bin\Win64_Shipping_Client\MarryAnyone.dll`

(Back up the original DLL first.) Restart the game.

## When a future Bannerlord update breaks it again
The mod reaches into internal game methods by name (via Harmony/BUTR `AccessTools2`). Updates
sometimes rename or remove those. To find what changed, point the project at the new reference
assemblies:

1. In `MarryAnyone\MarryAnyone.csproj`, bump `<GameVersion>` and the two
   `Bannerlord.ReferenceAssemblies.Core` / `...SandBox` package versions to the new
   `X.Y.Z.NNNNNN` (find the latest on nuget.org).
2. Rebuild. Compile errors point at public-API breaks.
3. For **internal** members accessed by string (they won't cause compile errors), a reliable trick
   is to write a tiny reflection program that `ReflectionOnlyLoadFrom`s the game's
   `TaleWorlds.CampaignSystem.dll` and lists the methods/fields you rely on, so you can see the new
   names/signatures. See `CHANGELOG.md` for the v1.4.6 example (OnHeroesMarried -> OnBeforeHeroesMarried, etc.).
