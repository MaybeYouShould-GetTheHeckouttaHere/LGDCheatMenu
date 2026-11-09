# Backups and saved settings

The installer never changes `Assembly-CSharp.dll`. It copies the mod into
`Mods/LongGoneDays.CheatMenu.dll`. If a different version already exists, it keeps
the first replaced DLL as `Mods/LongGoneDays.CheatMenu.dll.lgdcheat-backup`.
Keep this file if you want to roll back. Reinstalling the same version does not
create an unnecessary backup; later updates do not replace the original backup.

`Uninstall.cmd` restores that backup when present, leaving the backup in place.
Without a backup, it removes the cheat-menu DLL. To remove every cheat-menu version
after a rollback, close the game and remove only those two named files manually.

MelonLoader, other mods, game saves, and preferences are retained. If you want to
disable all MelonLoader mods, close the game and rename the game-root `version.dll`
to `version.dll.disabled`. Re-enable it by restoring the original name. See the
[official removal instructions](https://github.com/LavaGang/MelonLoader#un-install)
for complete loader removal.

The shortcut is stored in `UserData/MelonPreferences.cfg`, under
`[LongGoneDaysCheatMenu]`: `ToggleKey`, `RequireCtrl`, `RequireAlt`, `RequireShift`.
Use the menu's shortcut controls whenever possible. The default is C with Ctrl
required and Alt/Shift disabled. Interface size and the owned-only filter currently
last for the session; they are not saved preferences.

Inventory, character levels/stats, resources, and maximum overrides are saved by
the game's normal save system. The menu never automatically saves progress.
Uninstalling cannot undo edits already stored in a save. Keep a separate save slot.
Noclip is temporary and resets when loading or changing locations.
