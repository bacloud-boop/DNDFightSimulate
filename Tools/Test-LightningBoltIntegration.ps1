param([string]$UnityData='C:/Program Files/Unity 2022.3.62f3c1/Editor/Data')
$ErrorActionPreference='Stop'
$mono="$UnityData/MonoBleedingEdge/bin/mono.exe"
$csc="$UnityData/MonoBleedingEdge/lib/mono/4.5/csc.exe"
$out=Join-Path $env:TEMP 'DNDFight_LightningBoltChecks.exe'
& $mono $csc -nologo "-out:$out" Tools/Tests/LightningBoltChecks.cs Assets/Scripts/BreathWeaponRules.cs Assets/Scripts/CombatState.cs Assets/Scripts/PlayerClasses.cs Assets/Scripts/Subclasses2024.cs
if($LASTEXITCODE -ne 0){throw 'Lightning Bolt model compile failed'}
& $mono $out
if($LASTEXITCODE -ne 0){throw 'Lightning Bolt model checks failed'}
