param([string]$UnityData='C:/Program Files/Unity 2022.3.62f3c1/Editor/Data')
$ErrorActionPreference='Stop'
$mono="$UnityData/MonoBleedingEdge/bin/mono.exe"
$csc="$UnityData/MonoBleedingEdge/lib/mono/4.5/csc.exe"
$out=Join-Path $env:TEMP 'DNDFight_RegenerationChecks.exe'
& $mono $csc -nologo "-out:$out" Tools/Tests/RegenerationChecks.cs Assets/Scripts/RegenerationEffects.cs Assets/Scripts/AnimateDeadControl.cs
if($LASTEXITCODE -ne 0){throw 'Regeneration model compile failed'}
& $mono $out
if($LASTEXITCODE -ne 0){throw 'Regeneration checks failed'}
