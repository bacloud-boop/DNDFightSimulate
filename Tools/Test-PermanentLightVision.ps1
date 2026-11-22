param([string]$UnityData='C:/Program Files/Unity 2022.3.62f3c1/Editor/Data')
$ErrorActionPreference='Stop'
$mono="$UnityData/MonoBleedingEdge/bin/mono.exe"
$csc="$UnityData/MonoBleedingEdge/lib/mono/4.5/csc.exe"
$output=Join-Path $env:TEMP 'DNDFight_PermanentLightVisionChecks.exe'
& $mono $csc -nologo "-out:$output" Tools/Tests/PermanentLightVisionChecks.cs Assets/Scripts/VisionState.cs
if($LASTEXITCODE -ne 0){throw 'Permanent-light vision compile failed'}
& $mono $output
if($LASTEXITCODE -ne 0){throw 'Permanent-light vision checks failed'}
