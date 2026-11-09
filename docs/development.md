# Development

## Build

Requirements: Windows, .NET Framework's C# compiler, the supported game, and
MelonLoader 0.7.3 installed beside the game. No game assemblies are redistributed.

```powershell
.\Build.ps1 -GameDirectory 'D:\Games\Long Gone Days'
```

Output: `dist/LongGoneDays.CheatMenu.dll`. References are resolved from the game's
Managed directory and `MelonLoader/net472`. Source uses the older compiler's C#
syntax intentionally. No NuGet restore is required.

## Release package

Download the official archive from
`https://github.com/LavaGang/MelonLoader/releases/download/v0.7.3/MelonLoader.x64.zip`.
The pinned SHA256 is checked before packaging and again on installation:

`5B2B2F3D1CD42B59EC886C5BDC2663EDAE87A0097A4F4A8F58C0965A99DDA416`

```powershell
.\Package.ps1 -MelonLoaderArchive 'D:\Downloads\MelonLoader.x64.zip'
```

This creates `release/LGDCheatMenu.zip`, a checksum text file, and a release manifest.
The ZIP contains a single `LGD Cheat Menu` folder with installers, compiled DLL,
documentation/licenses, and the unchanged loader archive. Source-only downloads
cannot install until built and packaged. The manifest checks the mod checksum.

## Architecture

`src/MelonEntry.cs` declares the Melon mod, preferences, lifecycle callbacks, and
Harmony patches. `src/CheatMenu.cs` owns the independent Unity IMGUI overlay and
uses native game inventory/stat APIs. The screen is not dimmed. GUI color, depth,
matrix, and enabled state are restored after drawing.

Harmony blocks InputController buttons/axes while open, overrides maximum stats,
copies persistent base stats into battle instances, resets exploration on save
load, and narrowly patches movement checks for the current player's noclip.
The transpiler requires both expected call sites. Occupied cells are not bypassed.

Maximum HP/SP overrides are extra keys in the existing serialized stat dictionary,
prefixed `__lgd_base_`. Equipment/buffs remain separate. Items are individual
instances; quantity reduction consumes only unequipped copies. Numeric fields
filter typed characters and pasted text; item fields apply nonempty changes
immediately. Empty text is an editing state, not zero.

## Validation

Build, inspect Melon metadata/Harmony target groups, and confirm the installed DLL
matches the artifact. Exercise installation with and without a loader, a repeated
install, backup/restore, corrupt payload rejection, unsupported-game rejection,
and unrelated-file preservation in an isolated fixture. Do not publish fixture
game files. Check a save/load round trip, battle resources, shortcuts, and noclip
in game on a separate save before expanding support.
