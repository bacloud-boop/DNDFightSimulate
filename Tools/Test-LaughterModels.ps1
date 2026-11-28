param([string]$UnityData='C:/Program Files/Unity 2022.3.62f3c1/Editor/Data')
$ErrorActionPreference='Stop'
$models=@('PolymorphEffect','RevivalRules','CombatState','PlayerClasses','Subclasses2024','RescueCarrier','RescueSpecies','SpeciesProfile','SpeciesSpellRules','SpeciesTurnPolicy','SpeciesAbilityState','CorpseCarryingState','CarryingRules','BodyWeight','WeightUnits','BreathingState','SummonedUnit','CreatureProfile','ChallengeRatings','AquaticState')
$sources=@('Tools/Tests/LaughterModelChecks.cs')+$models.ForEach({"Assets/Scripts/$_.cs"})
$output=Join-Path $env:TEMP 'DNDFight_LaughterModelChecks.exe'
& "$UnityData/MonoBleedingEdge/bin/mono.exe" "$UnityData/MonoBleedingEdge/lib/mono/4.5/csc.exe" -nologo "-out:$output" @sources
if($LASTEXITCODE -ne 0){throw 'Laughter models compile failed'}
& "$UnityData/MonoBleedingEdge/bin/mono.exe" $output
if($LASTEXITCODE -ne 0){throw 'Laughter models check failed'}
