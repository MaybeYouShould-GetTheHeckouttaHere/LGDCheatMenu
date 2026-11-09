param([Parameter(Mandatory = $true)][string]$MelonLoaderArchive)
$ErrorActionPreference = 'Stop'
$archive = (Resolve-Path -LiteralPath $MelonLoaderArchive).Path
if ((Get-FileHash -LiteralPath $archive).Hash -ne '5B2B2F3D1CD42B59EC886C5BDC2663EDAE87A0097A4F4A8F58C0965A99DDA416') { throw 'Expected the unmodified official MelonLoader 0.7.3 x64 archive.' }
$mod = Join-Path $PSScriptRoot 'dist\LongGoneDays.CheatMenu.dll'
if (!(Test-Path -LiteralPath $mod)) { throw 'Run Build.ps1 first.' }
$output = Join-Path $PSScriptRoot 'release'
$stage = Join-Path $output ('stage-' + [guid]::NewGuid().ToString('N'))
$folder = Join-Path $stage 'LGD Cheat Menu'
New-Item -ItemType Directory -Force $folder,(Join-Path $folder 'dist'),(Join-Path $folder 'payload') | Out-Null
foreach ($name in @('Install.ps1','Install.cmd','Uninstall.ps1','Uninstall.cmd','README.md','CHANGELOG.md','LICENSE','THIRD-PARTY-NOTICES.md','docs')) { Copy-Item -LiteralPath (Join-Path $PSScriptRoot $name) -Destination $folder -Recurse }
Copy-Item -LiteralPath $mod -Destination (Join-Path $folder 'dist\LongGoneDays.CheatMenu.dll')
Copy-Item -LiteralPath $archive -Destination (Join-Path $folder 'payload\MelonLoader.x64.zip')
$manifest = [ordered]@{ version = '1.1.2'; gameBuild = '13316501'; melonLoaderVersion = '0.7.3'; modSha256 = (Get-FileHash -LiteralPath $mod).Hash; melonLoaderSha256 = (Get-FileHash -LiteralPath $archive).Hash }
$manifest | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $folder 'release-manifest.json') -Encoding UTF8
$zip = Join-Path $output 'LGDCheatMenu.zip'
Compress-Archive -LiteralPath $folder -DestinationPath $zip -Force
((Get-FileHash -LiteralPath $zip).Hash.ToLowerInvariant() + '  LGDCheatMenu.zip') | Set-Content -LiteralPath (Join-Path $output 'SHA256SUMS.txt') -Encoding ASCII
Write-Host "Created $zip"
