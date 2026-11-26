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
$queueFile=Join-Path $env:TEMP 'DNDFight_SearingSmiteProductionQueue.cs'
Set-Content -LiteralPath $queueFile -Encoding utf8 -Value ('using System.Collections; public sealed partial class TrainingGame {'+$count+$queueSource.Substring($queueBegin,$queueEnd-$queueBegin)+'}')
& $mono $csc -nologo "-out:$out" Tools/Tests/SearingSmiteRuntimeChecks.cs Assets/Scripts/SearingSmiteEffects.cs Assets/Scripts/DivineSmiteRules.cs Assets/Scripts/ThunderousSmiteSpell.cs Assets/Scripts/SearingSmiteSpell.cs Assets/Scripts/StartStatusOrder.cs Assets/Scripts/RegenerationEffects.cs $queueFile
if($LASTEXITCODE -ne 0){throw 'Searing Smite runtime harness compile failed'}
& $mono $out
if($LASTEXITCODE -ne 0){throw 'Searing Smite runtime checks failed'}
