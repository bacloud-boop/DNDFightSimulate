param([string]$UnityData='C:/Program Files/Unity 2022.3.62f3c1/Editor/Data',[switch]$HailThorns,[switch]$CallLightning,[switch]$SleetStorm,[switch]$SpiritGuardians,[switch]$StinkingCloud,[switch]$GustWind,[switch]$Resistance,[switch]$Moonbeam,[switch]$Laughter,[switch]$Prayer,[switch]$Cordon)
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
$adapter+=$save.Substring($a,$b-$a)
if($HailThorns){
 $weapon=Get-Content Assets/Scripts/TrainingGame.cs -Raw -Encoding utf8
 $a=$weapon.IndexOf('        if(hailHit.Before)yield return ResolveHailThorns(hailHit);')
 $b=$weapon.IndexOf('        dice=hitLabel+', $a)
 if($a -lt 0 -or $b -lt 0){throw 'Production Hail/weapon damage dispatch not found'}
 $adapter+='IEnumerator ProductionHailOrder(HailHit hailHit,int damage){var weaponTypes=combat.CaptureAdjustedDamage(false);var reduced=new RayDamage{Raw=weaponTypes};if(reduced.Raw==null){reduced.Raw=new long[14];reduced.Raw[13]=damage;}'+$weapon.Substring($a,$b-$a)+'yield break;}'
}
if($SleetStorm){
 $grease=Get-Content Assets/Scripts/GreaseSpell.cs -Raw -Encoding utf8
 $adapter+=[regex]::Match($grease,'bool FullDifficultTerrain\([^\r\n]+').Value
 $vision=Get-Content Assets/Scripts/VisionSystem.cs -Raw -Encoding utf8
 $a=$vision.IndexOf('if(sleetStorm.Scene==');$b=$vision.IndexOf('if(SensoryAfflictions.BlocksSight',$a)
 if($a -lt 0 -or $b -lt 0){throw 'Production Sleet visibility adapter not found'}
 $adapter+='int SleetVision(double ax,double ay,double bx,double by,int blind){bool viewerPlayer=true;double distance=Vector2Int.Distance(new Vector2Int((int)ax,(int)ay),new Vector2Int((int)bx,(int)by));int quality=0;'+$vision.Substring($a,$b-$a)+'return quality;}'
}
if($SpiritGuardians){
 $movement=Get-Content Assets/Scripts/BattleMap.cs -Raw -Encoding utf8
 $a=$movement.IndexOf('    public System.Func<int> MonsterTurnSpeed;');$b=$movement.IndexOf('    public int TacticalMovementRemaining',$a)
 if($a -lt 0 -or $b -lt 0){throw 'Production live movement budget not found'}
 $adapter+='sealed partial class MapStub {'+$movement.Substring($a,$b-$a)+'}'
}
$adapter+='}'
$file=Join-Path $env:TEMP 'DNDFight_ConjureAnimalsProductionAdapters.cs'
Set-Content -LiteralPath $file -Encoding utf8 -Value ($geometry+$adapter.Replace('using System;using System.Collections;using System.Collections.Generic;using UnityEngine;',''))
if($Laughter){
 $laughterFile=Join-Path $env:TEMP 'DNDFight_LaughterProduction.cs'
 Set-Content -LiteralPath $laughterFile -Encoding utf8 -Value ((Get-Content Assets/Scripts/LaughterSpell.cs -Raw -Encoding utf8).Replace('SummonedUnit','Unit'))
}
$extra=@('-main:PackChecksMain');if($HailThorns){$extra=@('-main:HailChecksMain','Assets/Scripts/HailThornsSpell.cs','Tools/Tests/HailThornsRuntimeChecks.cs')}
if($CallLightning){$extra=@('-main:LightningChecksMain','Assets/Scripts/CallLightningEffect.cs','Assets/Scripts/CallLightningSpell.cs','Tools/Tests/CallLightningRuntimeChecks.cs')}
if($SleetStorm){$extra=@('-define:SLEET_RUNTIME','-main:SleetChecksMain','Assets/Scripts/SleetStormEffect.cs','Assets/Scripts/SleetStormSpell.cs','Tools/Tests/SleetStormRuntimeChecks.cs')}
if($SpiritGuardians){$extra=@('-define:GUARDIANS_RUNTIME','-main:GuardiansChecksMain','Assets/Scripts/DamageTypes.cs','Assets/Scripts/Alignment2024.cs','Assets/Scripts/SpiritGuardiansEffect.cs','Assets/Scripts/SpiritGuardiansSpell.cs','Tools/Tests/SpiritGuardiansRuntimeChecks.cs')}
if($StinkingCloud){$extra=@('-define:STINKING_RUNTIME','-main:StinkingChecksMain','Assets/Scripts/StinkingCloudEffect.cs','Assets/Scripts/StinkingCloudSpell.cs','Tools/Tests/StinkingCloudRuntimeChecks.cs')}
if($GustWind){$extra=@('-define:GUST_RUNTIME','-main:GustChecksMain','Assets/Scripts/BreathWeaponRules.cs','Assets/Scripts/GustWindEffect.cs','Assets/Scripts/GustWindSpell.cs','Assets/Scripts/VisionState.cs','Assets/Scripts/StinkingCloudEffect.cs','Tools/Tests/GustWindRuntimeChecks.cs')}
if($Resistance){$extra=@('-define:RESISTANCE_RUNTIME','-main:ResistanceChecksMain','Assets/Scripts/ResistanceEffect.cs','Assets/Scripts/ResistanceSpell.cs','Assets/Scripts/DamageTypes.cs','Tools/Tests/ResistanceRuntimeChecks.cs')}
if($Moonbeam){$extra=@('-define:MOONBEAM_RUNTIME','-main:MoonbeamChecksMain','Assets/Scripts/MoonbeamEffect.cs','Assets/Scripts/MoonbeamSpell.cs','Assets/Scripts/VisionState.cs','Assets/Scripts/SizeChangeEffect.cs','Tools/Tests/MoonbeamRuntimeChecks.cs')}
if($Laughter){$extra=@('-define:LAUGHTER_RUNTIME','-main:LaughterChecksMain','Assets/Scripts/LaughterEffect.cs',$laughterFile,'Assets/Scripts/DamageEffectOrder.cs','Tools/Tests/LaughterRuntimeChecks.cs')}
if($Prayer){$extra=@('-define:PRAYER_RUNTIME','-main:PrayerChecksMain','Assets/Scripts/PrayerCasting.cs','Assets/Scripts/HitDicePool.cs','Assets/Scripts/HitDiceRuntime.cs','Assets/Scripts/PrayerOfHealingSpell.cs','Tools/Tests/PrayerRuntimeChecks.cs')}
if($Cordon){$extra=@('-define:CORDON_RUNTIME','-main:CordonChecksMain','Assets/Scripts/CordonArrowsEffect.cs','Assets/Scripts/CordonArrowsSpell.cs','Tools/Tests/CordonRuntimeChecks.cs')}
& $mono $csc -nologo -nowarn:0649,0414 @extra "-out:$out" Tools/Tests/ConjureAnimalsRuntimeChecks.cs Tools/Tests/MapValueStubs.cs Assets/Scripts/ConjureAnimalsEffect.cs Assets/Scripts/ConjureAnimalsSpell.cs Assets/Scripts/ConjureAnimalsControl.cs Assets/Scripts/RouteCostMeter.cs Assets/Scripts/PoisonProtectionEffect.cs Assets/Scripts/CombatState.cs Assets/Scripts/PlayerClasses.cs Assets/Scripts/Subclasses2024.cs $file
if($LASTEXITCODE -ne 0){throw 'Conjure Animals runtime harness compile failed'}
& $mono $out
if($LASTEXITCODE -ne 0){throw 'Conjure Animals runtime checks failed'}
