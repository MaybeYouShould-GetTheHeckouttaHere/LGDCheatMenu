param([string]$GameDirectory = (Split-Path $PSScriptRoot))
$ErrorActionPreference = 'Stop'
if (Get-Process -Name 'Long Gone Days' -ErrorAction SilentlyContinue) { throw 'Close Long Gone Days first.' }
$game = (Resolve-Path -LiteralPath $GameDirectory).Path
if (!(Test-Path -LiteralPath (Join-Path $game 'Long Gone Days.exe'))) { throw 'Copy the LGD Cheat Menu folder beside Long Gone Days.exe first.' }
$mod = Join-Path $game 'Mods\LongGoneDays.CheatMenu.dll'
$backup = $mod + '.lgdcheat-backup'
if (Test-Path -LiteralPath $backup) {
    Copy-Item -LiteralPath $backup -Destination $mod -Force
    Write-Host 'Restored the previous cheat-menu version. Backup retained.'
} else {
    if (Test-Path -LiteralPath $mod) { Remove-Item -LiteralPath $mod }
    Write-Host 'Removed LGD Cheat Menu.'
}
Write-Host 'MelonLoader, other mods, preferences and game saves are unchanged.'
