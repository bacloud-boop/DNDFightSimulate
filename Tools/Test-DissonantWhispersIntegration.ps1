param([string]$UnityData='C:/Program Files/Unity 2022.3.62f3c1/Editor/Data')
$ErrorActionPreference='Stop'
$mono="$UnityData/MonoBleedingEdge/bin/mono.exe"
$csc="$UnityData/MonoBleedingEdge/lib/mono/4.5/csc.exe"
$names=@('CreatureProfile','ChallengeRatings','BattleMap','CreatureTransitMap','CreatureTransitRules','RetainedRemainsMap','RetainedRemains','ReincarnationRules','RevivalRules','SpeciesProfile','SpeciesSpellRules','SpeciesTurnPolicy','BodyWeight','WeightUnits','StoneSurfaceMap','ContainmentMap','SummonMap','WaterMap','WaterMovement','DifficultTerrainMap','GreaseMovementMap','RescueMap','RescueCargoMap','RescueReleasePlanner','RidingMap','RidingState','SummonedUnit','AquaticState','BreathingState','PolymorphEffect','RouteCostMeter','CreatureCover','CreatureCoverMap','RescueCarrier','RescueSpecies','SpeciesAbilityState','CorpseCarryingState','CarryingRules','WeaponInventory','Weapon','WeaponWeights','GroundWeaponState','PlayerAttributes','AcidSplash','CommandMap','AnimateDeadControl','AnimatedUndeadRules','DissonantWhispersRules','CommandRoute','ReactionMovementMap','DirectSpell','CombatState','PlayerClasses','Subclasses2024','MonsterSpellSequence','MonsterSpellCatalog','BreathWeaponRules')
$files=@('Tools/Tests/DissonantWhispersChecks.cs','Tools/Tests/MapValueStubs.cs','Tools/Tests/WeaponModelStubs.cs')
foreach($name in $names){$files+="Assets/Scripts/$name.cs"}
$out=Join-Path $env:TEMP 'DNDFight_DissonantWhispersChecks.exe'
& $mono $csc -nologo "-out:$out" @files
if($LASTEXITCODE -ne 0){throw 'Dissonant Whispers compile failed'}
& $mono $out
if($LASTEXITCODE -ne 0){throw 'Dissonant Whispers checks failed'}
