param([string]$UnityData='C:/Program Files/Unity 2022.3.62f3c1/Editor/Data')
$ErrorActionPreference='Stop'
$mono="$UnityData/MonoBleedingEdge/bin/mono.exe"
$csc="$UnityData/MonoBleedingEdge/lib/mono/4.5/csc.exe"
$out=Join-Path $env:TEMP 'DNDFight_SearingSmiteRuntimeChecks.exe'
# Compile the actual queue method and count expression, not a reimplementation.
$queueSource=Get-Content -LiteralPath Assets/Scripts/GreaseSpell.cs -Raw -Encoding utf8
$queueBegin=$queueSource.IndexOf(' IEnumerator DrainGreaseEntries()')
$queueEnd=$queueSource.IndexOf(' bool GreasePathNeeded',$queueBegin)
if($queueBegin -lt 0 -or $queueEnd -lt 0){throw 'Production queue method not found'}
$countSource=Get-Content -LiteralPath Assets/Scripts/SpikeGrowthSpell.cs -Raw -Encoding utf8
$count=[regex]::Match($countSource,'int PendingTerrainCount=>[^;]+;').Value
if(!$count){throw 'Production queue count not found'}
$endSource=Get-Content -LiteralPath Assets/Scripts/BlindnessDeafnessSpell.cs -Raw -Encoding utf8
$endBegin=$endSource.IndexOf('    IEnumerator RepeatSensorySaves(bool player)')
$endEnd=$endSource.IndexOf('    IEnumerator EndPlayerTurnWithSensorySaves()',$endBegin)
if($endBegin -lt 0 -or $endEnd -lt 0){throw 'Production turn-end method not found'}
$faerieSource=Get-Content -LiteralPath Assets/Scripts/FaerieFireSpell.cs -Raw -Encoding utf8
$adapters=''
foreach($method in @('FaerieOutlined','InvisibleWithFaerie','FaerieAdvantage')){
 $part=[regex]::Match($faerieSource,'bool '+$method+'\([^\r\n]+?=>[^;]+;').Value
 if(!$part){throw 'Production visibility adapter not found'}
 $adapters+=$part
}
$conditionSource=Get-Content -LiteralPath Assets/Scripts/ColorSpraySpell.cs -Raw -Encoding utf8
$conditionBegin=$conditionSource.IndexOf('    bool ActorBlinded(')
$conditionEnd=$conditionSource.IndexOf('    string ColorSprayError()',$conditionBegin)
if($conditionBegin -lt 0 -or $conditionEnd -lt 0){throw 'Production auxiliary condition methods not found'}
$adapters+=$conditionSource.Substring($conditionBegin,$conditionEnd-$conditionBegin)
$queueFile=Join-Path $env:TEMP 'DNDFight_SearingSmiteProductionQueue.cs'
Set-Content -LiteralPath $queueFile -Encoding utf8 -Value ('using System.Collections; public sealed partial class TrainingGame {'+$count+$queueSource.Substring($queueBegin,$queueEnd-$queueBegin)+$endSource.Substring($endBegin,$endEnd-$endBegin)+$adapters+'}')
& $mono $csc -nologo "-out:$out" Tools/Tests/SearingSmiteRuntimeChecks.cs Assets/Scripts/SearingSmiteEffects.cs Assets/Scripts/DivineSmiteRules.cs Assets/Scripts/ShiningSmiteEffect.cs Assets/Scripts/ShiningSmiteSpell.cs Assets/Scripts/VisionState.cs Assets/Scripts/FaerieFireEffect.cs Assets/Scripts/StarryWispEffect.cs Assets/Scripts/ThunderousSmiteSpell.cs Assets/Scripts/SensoryAfflictions.cs Assets/Scripts/BlindingSmiteSpell.cs Assets/Scripts/FeignDeathEffect.cs Assets/Scripts/TurnBoundConditions.cs Assets/Scripts/AcidArrowResidue.cs Assets/Scripts/WrathfulSmiteEffects.cs Assets/Scripts/WrathfulSmiteSpell.cs Tools/Tests/MapValueStubs.cs Assets/Scripts/SearingSmiteSpell.cs Assets/Scripts/StartStatusOrder.cs Assets/Scripts/RegenerationEffects.cs $queueFile
if($LASTEXITCODE -ne 0){throw 'Searing Smite runtime harness compile failed'}
& $mono $out
if($LASTEXITCODE -ne 0){throw 'Searing Smite runtime checks failed'}
