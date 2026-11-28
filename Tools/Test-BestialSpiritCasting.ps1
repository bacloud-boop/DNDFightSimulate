param([string]$UnityData='C:/Program Files/Unity 2022.3.62f3c1/Editor/Data')
$ErrorActionPreference='Stop'
$models=@('SummonedUnit','CreatureProfile','ChallengeRatings','AquaticState','BreathingState','PolymorphEffect','OtherworldlySteedRules','RidingState','SummonTurnQueue','BestialSpiritRules','BestialSpiritSession','CombatState','PlayerClasses','Subclasses2024','HitDicePool','UnderwaterCombatRules','BestialSpiritCasting','SummonBeastSpell')
$maps=@('BattleMap','ContainmentMap','SummonMap','WaterMap','WaterMovement','DifficultTerrainMap','GreaseMovementMap','RescueMap','RescueCargoMap','RescueReleasePlanner','RidingMap','CreatureCoverMap','CreatureCover','RouteCostMeter','JumpMap','CreatureTransitMap','CreatureTransitRules','RetainedRemainsMap','RetainedRemains','BodyWeight','WeightUnits')
$sources=@('Tools/Tests/BestialSpiritCastingChecks.cs','Tools/Tests/MapValueStubs.cs')+($models+$maps).ForEach({"Assets/Scripts/$_.cs"})
$output=Join-Path $env:TEMP 'DNDFight_BestialSpiritCastingChecks.exe'
& "$UnityData/MonoBleedingEdge/bin/mono.exe" "$UnityData/MonoBleedingEdge/lib/mono/4.5/csc.exe" -nologo -nowarn:0649,0414 "-out:$output" @sources
if($LASTEXITCODE -ne 0){throw 'Bestial Spirit runtime compile failed'}
& "$UnityData/MonoBleedingEdge/bin/mono.exe" $output
if($LASTEXITCODE -ne 0){throw 'Bestial Spirit runtime checks failed'}
