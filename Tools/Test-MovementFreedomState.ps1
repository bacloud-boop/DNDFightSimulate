param([string]$UnityData='C:/Program Files/Unity 2022.3.62f3c1/Editor/Data')
$ErrorActionPreference='Stop'
$mono="$UnityData/MonoBleedingEdge/bin/mono.exe"
$csc="$UnityData/MonoBleedingEdge/lib/mono/4.5/csc.exe"
$output=Join-Path $env:TEMP 'DNDFight_MovementFreedomStateChecks.exe'
& $mono $csc -nologo "-out:$output" Tools/Tests/MovementFreedomStateChecks.cs Assets/Scripts/CombatState.cs Assets/Scripts/PlayerClasses.cs Assets/Scripts/Subclasses2024.cs
if($LASTEXITCODE -ne 0){throw 'Movement-freedom state compile failed'}
& $mono $output
if($LASTEXITCODE -ne 0){throw 'Movement-freedom state checks failed'}
