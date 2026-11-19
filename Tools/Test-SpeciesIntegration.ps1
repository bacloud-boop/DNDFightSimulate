param([string]$UnityData='C:/Program Files/Unity 2022.3.62f3c1/Editor/Data')
$ErrorActionPreference='Stop'
$mono="$UnityData/MonoBleedingEdge/bin/mono.exe"
$csc="$UnityData/MonoBleedingEdge/lib/mono/4.5/csc.exe"
$common=@('SpeciesProfile','SpeciesSpellRules','SpeciesAbilityState','SpeciesTurnPolicy','SpeciesSkillRules','SkillCreationRules','SkillCheckRules','SkillSourceTables','PlayerClasses','Subclasses2024','CombatState','ResurrectionCasterState','RevivalRules','CreatureProfile','ChallengeRatings')
foreach($check in @('SpeciesChecks','SpeciesSpellChecks','MonsterFormChecks','StoneSurfaceChecks')){
 $names=@($common);$files=@("Tools/Tests/$check.cs")
 if($check -eq 'MonsterFormChecks'){$names+=@('MonsterFormData','MonsterSpellSequence','MonsterAttackAbility','AcidSplash','ArmorDefinition','EquipmentLoadRules','CarryingRules','DamageTypes')}
 if($check -eq 'StoneSurfaceChecks'){$files+='Tools/Tests/MapValueStubs.cs';$names+=@('BattleMap','StoneSurfaceMap','ContainmentMap','SummonMap','WaterMap','WaterMovement','DifficultTerrainMap','GreaseMovementMap','RescueMap','RescueCargoMap','RescueReleasePlanner','RidingMap','RidingState','SummonedUnit','AquaticState','BreathingState','PolymorphEffect','RouteCostMeter')}
 foreach($n in ($names | Select-Object -Unique)){$files+="Assets/Scripts/$n.cs"}
 $out=Join-Path $env:TEMP ("DNDFight_"+$check+'.exe')
 & $mono $csc -nologo "-out:$out" @files
 if($LASTEXITCODE -ne 0){throw "Model compile failed: $check"}
 & $mono $out
 if($LASTEXITCODE -ne 0){throw "Checks failed: $check"}
}
