param([string]$UnityData='C:/Program Files/Unity 2022.3.62f3c1/Editor/Data')
$ErrorActionPreference='Stop'
$mono="$UnityData/MonoBleedingEdge/bin/mono.exe"
$csc="$UnityData/MonoBleedingEdge/lib/mono/4.5/csc.exe"
$model=@('SummonedUnit','CreatureProfile','ChallengeRatings','AquaticState','BreathingState','PolymorphEffect','OtherworldlySteedRules','RidingState','SummonTurnQueue','BestialSpiritRules','BestialSpiritSession')
$maps=@('BattleMap','ContainmentMap','SummonMap','WaterMap','WaterMovement','DifficultTerrainMap','GreaseMovementMap','RescueMap','RescueCargoMap','RescueReleasePlanner','RidingMap','CreatureCoverMap','CreatureCover','RouteCostMeter','JumpMap','CreatureTransitMap','CreatureTransitRules','RetainedRemainsMap','RetainedRemains','BodyWeight','WeightUnits')
foreach($check in @('SteedRulesChecks','RidingChecks','SteedTurnChecks','RidingMapChecks','SummonTurnChecks','SummonIntegrationChecks','CreatureCoverMapChecks','WaterMapChecks')){
 $sources=@("Tools/Tests/$check.cs")+$model.ForEach({"Assets/Scripts/$_.cs"})
 if($check -in @('RidingMapChecks','SummonIntegrationChecks','CreatureCoverMapChecks','WaterMapChecks')){$sources+=@('Tools/Tests/MapValueStubs.cs')+$maps.ForEach({"Assets/Scripts/$_.cs"})}
 if($check -eq 'SummonIntegrationChecks'){$sources+=@('Assets/Scripts/CombatState.cs','Assets/Scripts/PlayerClasses.cs','Assets/Scripts/Subclasses2024.cs','Assets/Scripts/SummonGeometry.cs')}
 if($check -eq 'WaterMapChecks'){$sources+='Assets/Scripts/UnderwaterCombatRules.cs'}
 $output=Join-Path $env:TEMP ("DNDFight_"+$check+".exe")
 & $mono $csc -nologo "-out:$output" @sources
 if($LASTEXITCODE -ne 0){throw "Compile failed: $check"}
 & $mono $output
 if($LASTEXITCODE -ne 0){throw "Check failed: $check"}
}
