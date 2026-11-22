param([string]$UnityData='C:/Program Files/Unity 2022.3.62f3c1/Editor/Data')
$ErrorActionPreference='Stop'
$output=Join-Path $env:TEMP 'DNDFight_HellishRebukeChecks.exe'
& "$UnityData/MonoBleedingEdge/bin/mono.exe" "$UnityData/MonoBleedingEdge/lib/mono/4.5/csc.exe" -nologo "-out:$output" Tools/Tests/HellishRebukeChecks.cs Assets/Scripts/HellishRebukeRules.cs Assets/Scripts/CombatState.cs Assets/Scripts/PlayerClasses.cs Assets/Scripts/Subclasses2024.cs
if($LASTEXITCODE -ne 0){throw 'Hellish Rebuke model compile failed'}
& "$UnityData/MonoBleedingEdge/bin/mono.exe" $output
if($LASTEXITCODE -ne 0){throw 'Hellish Rebuke checks failed'}
