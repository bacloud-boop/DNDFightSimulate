param([string]$UnityData='C:/Program Files/Unity 2022.3.62f3c1/Editor/Data')
$ErrorActionPreference='Stop'
$mono="$UnityData/MonoBleedingEdge/bin/mono.exe"
$csc="$UnityData/MonoBleedingEdge/lib/mono/4.5/csc.exe"
$names=@('CreatureProfile','ChallengeRatings','BattleMap','RetainedRemainsMap','RetainedRemains','ReincarnationRules','RevivalRules','SpeciesProfile','SpeciesSpellRules','SpeciesTurnPolicy','BodyWeight','WeightUnits','StoneSurfaceMap','ContainmentMap','SummonMap','WaterMap','WaterMovement','DifficultTerrainMap','GreaseMovementMap','RescueMap','RescueCargoMap','RescueReleasePlanner','RidingMap','RidingState','SummonedUnit','AquaticState','BreathingState','PolymorphEffect','RouteCostMeter','CreatureCover','CreatureCoverMap','RescueCarrier','CorpseCarryingState','CarryingRules','WeaponInventory','Weapon','WeaponWeights','GroundWeaponState','PlayerAttributes','AcidSplash')
foreach($check in @('RetainedRemainsChecks','RetainedRemainsMapChecks','RescueCargoChecks','RescueForcedDropChecks','RescueReleaseChecks','RescueMapChecks','WeaponInventoryChecks','CorpseCarryingChecks')){
 $out=Join-Path $env:TEMP ("DNDFight_"+$check+'.exe')
 $files=@("Tools/Tests/$check.cs",'Tools/Tests/MapValueStubs.cs','Tools/Tests/WeaponModelStubs.cs')+$names.ForEach({"Assets/Scripts/$_.cs"})
 & $mono $csc -nologo "-out:$out" @files
 if($LASTEXITCODE -ne 0){throw "Compile failed: $check"}
 & $mono $out
 if($LASTEXITCODE -ne 0){throw "Check failed: $check"}
}
