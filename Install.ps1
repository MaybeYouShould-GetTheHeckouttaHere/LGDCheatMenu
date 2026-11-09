param([string]$GameDirectory = (Split-Path $PSScriptRoot))
$ErrorActionPreference = 'Stop'
$game = (Resolve-Path -LiteralPath $GameDirectory).Path
if (Get-Process -Name 'Long Gone Days' -ErrorAction SilentlyContinue) { throw 'Close Long Gone Days first, then run Install.cmd again.' }
$assembly = Join-Path $game 'Long Gone Days_Data\Managed\Assembly-CSharp.dll'
if (!(Test-Path -LiteralPath (Join-Path $game 'Long Gone Days.exe')) -or !(Test-Path -LiteralPath $assembly)) { throw 'Copy the LGD Cheat Menu folder beside Long Gone Days.exe, then run Install.cmd.' }
if ((Get-FileHash -LiteralPath $assembly).Hash -ne '89F1BC5B14AF87266B122D001DFB140280A008261CCFB8B4CCD4D0CB142FE184') { throw 'Unsupported or modified game assembly. This release supports Long Gone Days Build 13316501 with its original game files.' }
$mod = Join-Path $PSScriptRoot 'dist\LongGoneDays.CheatMenu.dll'
if (!(Test-Path -LiteralPath $mod)) { throw 'Compiled mod missing. Download the release ZIP, not the source-code ZIP.' }
$manifest = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'release-manifest.json') -Raw | ConvertFrom-Json
if ((Get-FileHash -LiteralPath $mod).Hash -ne $manifest.modSha256) { throw 'Mod checksum mismatch. Extract a fresh release ZIP.' }
$loader = Join-Path $game 'MelonLoader\net472\MelonLoader.dll'
$proxy = Join-Path $game 'version.dll'
if (Test-Path -LiteralPath $loader) {
    if (!(Test-Path -LiteralPath $proxy)) { throw 'MelonLoader exists but version.dll is missing or disabled. Repair or enable your loader first.' }
    Write-Host 'Keeping the existing MelonLoader installation.'
} else {
    if ((Test-Path -LiteralPath $proxy) -or (Test-Path -LiteralPath (Join-Path $game 'MelonLoader'))) { throw 'An existing loader or version.dll was found. Install or repair MelonLoader yourself first; this installer will not overwrite another loader.' }
    $archive = Join-Path $PSScriptRoot 'payload\MelonLoader.x64.zip'
    if (!(Test-Path -LiteralPath $archive)) { throw 'Bundled MelonLoader archive missing. Extract the complete release ZIP.' }
    if ((Get-FileHash -LiteralPath $archive).Hash -ne '5B2B2F3D1CD42B59EC886C5BDC2663EDAE87A0097A4F4A8F58C0965A99DDA416') { throw 'MelonLoader archive checksum mismatch. Extract a fresh release ZIP.' }
    # Stage and check every destination before copying anything to the game.
    $stage = Join-Path ([IO.Path]::GetTempPath()) ('LGDCheatMenu-' + [guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory -Path $stage | Out-Null
    try {
        Add-Type -AssemblyName System.IO.Compression.FileSystem
        $zip = [IO.Compression.ZipFile]::OpenRead($archive)
        try {
            foreach ($entry in $zip.Entries) {
                $name = $entry.FullName.Replace('/', '\')
                if ($name -ne 'version.dll' -and !$name.StartsWith('MelonLoader\', [StringComparison]::OrdinalIgnoreCase)) { throw "Unexpected loader archive entry: $name" }
                $destination = [IO.Path]::GetFullPath((Join-Path $stage $name))
                if (!$destination.StartsWith($stage + '\', [StringComparison]::OrdinalIgnoreCase)) { throw 'Unsafe archive path.' }
            }
        } finally { $zip.Dispose() }
        Expand-Archive -LiteralPath $archive -DestinationPath $stage
        if (!(Test-Path -LiteralPath (Join-Path $stage 'MelonLoader\net472\MelonLoader.dll'))) { throw 'Incomplete MelonLoader archive.' }
        Copy-Item -LiteralPath (Join-Path $stage 'MelonLoader') -Destination $game -Recurse
        Copy-Item -LiteralPath (Join-Path $stage 'version.dll') -Destination $game
        Write-Host 'Installed MelonLoader 0.7.3 x64.'
    } finally {
        $resolvedStage = [IO.Path]::GetFullPath($stage)
        $tempRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd('\') + '\'
        if ($resolvedStage.StartsWith($tempRoot, [StringComparison]::OrdinalIgnoreCase) -and [IO.Path]::GetFileName($resolvedStage).StartsWith('LGDCheatMenu-')) { Remove-Item -LiteralPath $resolvedStage -Recurse -Force }
    }
}
$mods = Join-Path $game 'Mods'
New-Item -ItemType Directory -Force $mods | Out-Null
$destinationMod = Join-Path $mods 'LongGoneDays.CheatMenu.dll'
$backup = $destinationMod + '.lgdcheat-backup'
if ((Test-Path -LiteralPath $destinationMod) -and !(Test-Path -LiteralPath $backup) -and (Get-FileHash -LiteralPath $destinationMod).Hash -ne $manifest.modSha256) { Copy-Item -LiteralPath $destinationMod -Destination $backup }
Copy-Item -LiteralPath $mod -Destination $destinationMod -Force
Write-Host 'Installed LGD Cheat Menu. Launch the game normally and press Ctrl+C.'
Write-Host 'Change the shortcut under Settings. Save to a separate game slot before experimenting.'
