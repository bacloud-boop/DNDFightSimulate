param([string]$UnityData='C:/Program Files/Unity 2022.3.62f3c1/Editor/Data')
$ErrorActionPreference='Stop'
$mono="$UnityData/MonoBleedingEdge/bin/mono.exe"
$csc="$UnityData/MonoBleedingEdge/lib/mono/4.5/csc.exe"
$models=@('BeaconHopeEffect','RevivalRules','CombatState','PlayerClasses','Subclasses2024','PoisonProtectionEffect','RescueCarrier','RescueSpecies','SpeciesProfile','SpeciesSpellRules','SpeciesTurnPolicy','SpeciesAbilityState','CorpseCarryingState','CarryingRules','BodyWeight','WeightUnits','BreathingState')
$files=@('Tools/Tests/BeaconSavingThrowChecks.cs')+$models.ForEach({"Assets/Scripts/$_.cs"})
$output=Join-Path $env:TEMP 'DNDFight_BeaconSavingThrowChecks.exe'
& $mono $csc -nologo "-out:$output" @files
if($LASTEXITCODE -ne 0){throw 'Beacon saving-throw model compile failed'}
& $mono $output
if($LASTEXITCODE -ne 0){throw 'Beacon saving-throw model checks failed'}
