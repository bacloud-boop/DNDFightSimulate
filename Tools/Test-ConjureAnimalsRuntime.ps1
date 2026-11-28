param([string]$UnityData='C:/Program Files/Unity 2022.3.62f3c1/Editor/Data')
$ErrorActionPreference='Stop'
$mono="$UnityData/MonoBleedingEdge/bin/mono.exe";$csc="$UnityData/MonoBleedingEdge/lib/mono/4.5/csc.exe"
$out=Join-Path $env:TEMP 'DNDFight_ConjureAnimalsRuntimeChecks.exe'
$map=Get-Content Assets/Scripts/BattleMap.cs -Raw -Encoding utf8
$a=$map.IndexOf('    public static int BodyDistance(');$b=$map.IndexOf('    public static bool LeavesReach(',$a)
if($a -lt 0 -or $b -lt 0){throw 'Production geometry not found'}
$geometry='using System;using System.Collections;using System.Collections.Generic;using UnityEngine;using Random=UnityEngine.Random;public static class BattleMap {public const int RescueId=int.MinValue;public static int FeetBetween(Vector2Int a,Vector2Int b)=>Mathf.RoundToInt(Vector2Int.Distance(a,b)*5);public static bool Overlap(Vector2Int a,int n,Vector2Int b,int m)=>a.x<b.x+m&&a.x+n>b.x&&a.y<b.y+m&&a.y+n>b.y;'+$map.Substring($a,$b-$a)
$path=Get-Content Assets/Scripts/GreaseMovementMap.cs -Raw -Encoding utf8
$geometry+=[regex]::Match($path,'public static List<Vector2Int> MovementAnchors\([^\r\n]+').Value+'}'
$queue=Get-Content Assets/Scripts/GreaseSpell.cs -Raw -Encoding utf8
$a=$queue.IndexOf(' IEnumerator DrainGreaseEntries()');$b=$queue.IndexOf(' bool GreasePathNeeded',$a)
if($a -lt 0 -or $b -lt 0){throw 'Production dispatcher not found'}
$count=[regex]::Match((Get-Content Assets/Scripts/SpikeGrowthSpell.cs -Raw -Encoding utf8),'int PendingTerrainCount=>[^;]+;').Value
$adapter='using System;using System.Collections;using System.Collections.Generic;using UnityEngine;public sealed partial class TrainingGame {'+$count+$queue.Substring($a,$b-$a)
$save=Get-Content Assets/Scripts/ResurrectionRuntime.cs -Raw -Encoding utf8
$a=$save.IndexOf('    ArmorSaveRoll RollD20Test(');$b=$save.IndexOf('    void RecordResurrectionExertion(',$a)
$adapter+=$save.Substring($a,$b-$a)
$save=Get-Content Assets/Scripts/BreathWeapon.cs -Raw -Encoding utf8
$a=$save.IndexOf('    IEnumerator RollMappedAbilitySave(');$b=$save.IndexOf('    IEnumerator UseBreath(',$a)
$adapter+=$save.Substring($a,$b-$a)+'}'
$file=Join-Path $env:TEMP 'DNDFight_ConjureAnimalsProductionAdapters.cs'
Set-Content -LiteralPath $file -Encoding utf8 -Value ($geometry+$adapter.Replace('using System;using System.Collections;using System.Collections.Generic;using UnityEngine;',''))
& $mono $csc -nologo -nowarn:0649,0414 "-out:$out" Tools/Tests/ConjureAnimalsRuntimeChecks.cs Tools/Tests/MapValueStubs.cs Assets/Scripts/ConjureAnimalsEffect.cs Assets/Scripts/ConjureAnimalsSpell.cs Assets/Scripts/ConjureAnimalsControl.cs Assets/Scripts/RouteCostMeter.cs Assets/Scripts/PoisonProtectionEffect.cs Assets/Scripts/CombatState.cs Assets/Scripts/PlayerClasses.cs Assets/Scripts/Subclasses2024.cs $file
if($LASTEXITCODE -ne 0){throw 'Conjure Animals runtime harness compile failed'}
& $mono $out
if($LASTEXITCODE -ne 0){throw 'Conjure Animals runtime checks failed'}
