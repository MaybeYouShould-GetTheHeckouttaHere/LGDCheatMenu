# LGD Cheat Menu

A mouse-driven cheat menu for **Long Gone Days on Windows**. Edit your party,
inventory, and character stats while you play.

Inspired by the MZ, MV, and [VX Ace Cheat Menu](https://github.com/MaybeYouShould-GetTheHeckouttaHere/VXCheatMenu)
workflow, with a standalone Unity overlay. This release supports **Build 13316501**.

## Install

1. [Download the ZIP](https://github.com/MaybeYouShould-GetTheHeckouttaHere/LGDCheatMenu/releases/latest/download/LGDCheatMenu.zip) and extract it.
2. Close your game.
3. Copy the **`LGD Cheat Menu` folder** into the game folder, beside **`Long Gone Days.exe`**.
4. Open that folder and double-click **`Install.cmd`**, then launch the game normally.

The ZIP includes the compiled mod and MelonLoader, so you do not need to download
either separately. If MelonLoader is already installed, the installer keeps it.
Your original game files stay unchanged. [Compatibility and requirements](docs/compatibility.md).

For updates, replace the `LGD Cheat Menu` folder with the new release and run
`Install.cmd` again. [Backups and saved settings](docs/backups-and-settings.md).

## Open and use

Press **Ctrl+C** to open or close the menu. **Esc** also closes it. Change the
shortcut under **Settings > Change shortcut**; your choice is remembered.

Click the tabs and fields to make changes. **Item counts update as you type**;
other number fields apply with **Set** or **Enter**. All number fields accept
digits only. Drag the header to move the menu and adjust its size in **Settings**.

The game pauses while the menu is open. Save through the game's normal save menu
to keep inventory and character edits. Use a separate save slot before experimenting.

## Main features

- Items, weapons, armor, and equipment quantities
- Item search, category filters, and **Show Only Owned Items**
- Character levels and base combat stats
- Current and maximum HP and SP/morale
- Restore resources and clear status effects and buffs
- Noclip through walls, with a return-to-safe-tile button
- A configurable shortcut and resizable interface

[Controls and feature details](docs/usage.md).

## Uninstall

Close the game and double-click **`Uninstall.cmd`** inside `LGD Cheat Menu`.
This removes the mod, or restores the previous version if the installer made a
backup. MelonLoader and other mods stay installed. It does not undo changes you
saved to your game progress. [Full removal details](docs/backups-and-settings.md).

## Having trouble?

- **Menu missing?** Focus the game and press **Ctrl+C**, or your custom shortcut.
- **Installer error?** Extract the complete release ZIP and check that the folder is beside the game executable.
- **Noclip feels stuck?** You can always turn it off. Re-enable it to leave a wall; map edges and occupied NPC tiles stay blocked.
- **Other problems?** Check [troubleshooting](docs/compatibility.md#troubleshooting) and include the error text when [reporting an issue](https://github.com/MaybeYouShould-GetTheHeckouttaHere/LGDCheatMenu/issues).

More detail: [compatibility](docs/compatibility.md),
[backups and saved settings](docs/backups-and-settings.md),
[development](docs/development.md).

Licensed under the [MIT License](LICENSE). MelonLoader and its bundled dependencies
retain their own licenses; see [third-party notices](THIRD-PARTY-NOTICES.md).
