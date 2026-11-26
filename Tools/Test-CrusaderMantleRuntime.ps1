param([string]$UnityData='C:/Program Files/Unity 2022.3.62f3c1/Editor/Data')
$ErrorActionPreference='Stop'
$mono="$UnityData/MonoBleedingEdge/bin/mono.exe"
$csc="$UnityData/MonoBleedingEdge/lib/mono/4.5/csc.exe"
$out=Join-Path $env:TEMP 'DNDFight_CrusaderMantleRuntimeChecks.exe'
$source=Get-Content Assets/Scripts/BattleMap.cs -Raw -Encoding utf8
$begin=$source.IndexOf('    public static int BodyDistance(')
$end=$source.IndexOf('    public static bool LeavesReach(',$begin)
if($begin -lt 0 -or $end -lt 0){throw 'Production footprint distance not found'}
$geometry=Join-Path $env:TEMP 'DNDFight_CrusaderMantleGeometry.cs'
Set-Content -LiteralPath $geometry -Encoding utf8 -Value ('using UnityEngine; public static class BattleMap {'+$source.Substring($begin,$end-$begin)+'}')
& $mono $csc -nologo "-out:$out" Tools/Tests/CrusaderMantleRuntimeChecks.cs Tools/Tests/MapValueStubs.cs Assets/Scripts/CrusaderMantleSpell.cs Assets/Scripts/CombatState.cs Assets/Scripts/PlayerClasses.cs Assets/Scripts/Subclasses2024.cs $geometry
if($LASTEXITCODE -ne 0){throw 'Crusader Mantle runtime harness compile failed'}
& $mono $out
if($LASTEXITCODE -ne 0){throw 'Crusader Mantle runtime checks failed'}
