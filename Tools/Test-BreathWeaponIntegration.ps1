param([string]$UnityData='C:/Program Files/Unity 2022.3.62f3c1/Editor/Data')
$ErrorActionPreference='Stop'
$mono="$UnityData/MonoBleedingEdge/bin/mono.exe"
$csc="$UnityData/MonoBleedingEdge/lib/mono/4.5/csc.exe"
$files=@('Tools/Tests/BreathWeaponChecks.cs','Assets/Scripts/BreathWeaponRules.cs','Assets/Scripts/SpeciesProfile.cs','Assets/Scripts/SpeciesSpellRules.cs','Assets/Scripts/SpeciesTurnPolicy.cs','Assets/Scripts/SpeciesAbilityState.cs','Assets/Scripts/FighterFeatures.cs','Assets/Scripts/CombatState.cs','Assets/Scripts/PlayerClasses.cs','Assets/Scripts/Subclasses2024.cs')
$out=Join-Path $env:TEMP 'DNDFight_BreathWeaponChecks.exe'
& $mono $csc -nologo "-out:$out" @files
if($LASTEXITCODE -ne 0){throw 'Breath model compile failed'}
& $mono $out
if($LASTEXITCODE -ne 0){throw 'Breath model checks failed'}
