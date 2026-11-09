# Compatibility and troubleshooting

## Supported game

Long Gone Days **Build 13316501**, Windows x64, Mono Unity backend. Other builds
have not been tested; the installer refuses a different or modified game assembly.
This is a game-specific mod, not a general RPG Maker or Unity cheat menu.

The installer checks `Long Gone Days_Data/Managed/Assembly-CSharp.dll` against SHA256:

`89F1BC5B14AF87266B122D001DFB140280A008261CCFB8B4CCD4D0CB142FE184`

## Requirements

Windows with Windows PowerShell 5.1 or newer and the Microsoft Visual C++ x64
runtime required by MelonLoader. Most gaming PCs already have the runtime. If the
loader reports missing runtime libraries, install the official
[Microsoft Visual C++ x64 redistributable](https://aka.ms/vs/17/release/vc_redist.x64.exe).

The release bundles the official **MelonLoader 0.7.3 x64** archive and installs it
offline if there is no loader. An existing MelonLoader installation is preserved;
only 0.7.3 has been used with this mod. An existing unexplained `version.dll` or
incomplete loader is never overwritten. Repair that loader first.

## Other mods

The mod uses MelonLoader and Harmony runtime patches. It does not rewrite game
assemblies or need another UI framework. Other MelonLoader mods can coexist, but
patches to the same input, movement, stats, or battle constructors may conflict.
No claim is made that every other mod is compatible.

## Troubleshooting

- **Download says source code:** choose `LGDCheatMenu.zip` under release assets.
- **Game not found:** put the whole `LGD Cheat Menu` folder beside `Long Gone Days.exe`.
- **Checksum or missing payload error:** extract a fresh complete release ZIP.
- **Unsupported game assembly:** use the supported build with original game files. Do not copy a DLL from another game or build.
- **Loader missing or disabled:** restore `version.dll` or repair your existing MelonLoader installation.
- **Menu missing:** check `MelonLoader/Latest.log` for `Long Gone Days Cheat Menu` and Harmony errors. Check the saved shortcut in `UserData/MelonPreferences.cfg`.
- **Issue after another mod:** try with only `LongGoneDays.CheatMenu.dll` in Mods, retaining the other mods elsewhere for later restoration.

When filing an issue, include the game build, mod version, loader version, steps
to reproduce, and relevant log errors. Review logs for personal paths before sharing.

The user confirmed the overlay and immediate inventory editing worked in game.
Release packaging is additionally checked with isolated installer fixtures; this
does not establish compatibility with untested builds or other mods.
