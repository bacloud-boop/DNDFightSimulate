param([string]$UnityData='C:/Program Files/Unity 2022.3.62f3c1/Editor/Data')
$ErrorActionPreference='Stop'
$mono="$UnityData/MonoBleedingEdge/bin/mono.exe"
$csc="$UnityData/MonoBleedingEdge/lib/mono/4.5/csc.exe"
$models=@('RevivalRules','CombatState','PlayerClasses','Subclasses2024','ResurrectionCasterState','PoisonProtectionEffect','RescueCarrier','RescueSpecies','SpeciesProfile','SpeciesSpellRules','SpeciesTurnPolicy','SpeciesAbilityState','CorpseCarryingState','CarryingRules','BodyWeight','WeightUnits','BreathingState')
$models=@($models | Select-Object -Unique)
foreach($check in @('ResurrectionChecks','RevivalChecks','RescueCarrierChecks','SuffocationRevivalChecks')){
 $sources=@("Tools/Tests/$check.cs")+$models.ForEach({"Assets/Scripts/$_.cs"})
 $output=Join-Path $env:TEMP ("DNDFight_"+$check+".exe")
 & $mono $csc -nologo "-out:$output" @sources
 if($LASTEXITCODE -ne 0){throw "Compile failed: $check"}
 & $mono $output
 if($LASTEXITCODE -ne 0){throw "Check failed: $check"}
}
