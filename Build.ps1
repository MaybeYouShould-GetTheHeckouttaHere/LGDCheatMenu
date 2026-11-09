param([string]$GameDirectory = (Split-Path $PSScriptRoot))
$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$managed = Join-Path $GameDirectory 'Long Gone Days_Data\Managed'
New-Item -ItemType Directory -Force (Join-Path $root 'dist') | Out-Null
$compiler = "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
$refs = @('mscorlib','System','System.Core','Assembly-CSharp','UnityEngine','UnityEngine.CoreModule','UnityEngine.IMGUIModule','UnityEngine.InputLegacyModule','UnityEngine.TextRenderingModule','UnityEngine.Physics2DModule','UnityEngine.AnimationModule','UnityEngine.UI') | ForEach-Object { '/reference:' + (Join-Path $managed ($_ + '.dll')) }
$loader = Join-Path $GameDirectory 'MelonLoader\net472'
$refs += @(('/reference:' + (Join-Path $loader 'MelonLoader.dll')), ('/reference:' + (Join-Path $loader '0Harmony.dll')))
& $compiler /nologo /noconfig /target:library /nostdlib+ /optimize+ ('/out:' + (Join-Path $root 'dist\LongGoneDays.CheatMenu.dll')) $refs (Join-Path $root 'src\CheatMenu.cs') (Join-Path $root 'src\MelonEntry.cs')
if ($LASTEXITCODE -ne 0) { throw 'Mod build failed' }
Write-Host 'Build complete.'
