using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
 readonly SpikeGrowthEffect spikeGrowth=new SpikeGrowthEffect();
 sealed class SpikeEntry {public int Actor;public int Count;}
 readonly Queue<SpikeEntry> spikeEntries=new Queue<SpikeEntry>();
 int PendingTerrainCount=>greaseEntries.Count+spikeEntries.Count+webEntries.Count+daggerEntries.Count+heroismEntries.Count+auraStarts.Count+startStatuses.Count;
 string SpikeGrowthError(){Vector2Int cell;if(!VisionPoint(out cell))return "荆棘中心必须位于地图内。";if(map.DistanceToCell(true,cell)>150)return "中心超出150尺。";return worldRounds<0||worldRounds>long.MaxValue-100?"世界时间超出范围。":"";}
 void DrawSpikeGrowthChoices(){GUILayout.Label("指定20尺半径球形地面区域的中心列／行；水面与潜水均受影响。每实际经过5尺掷2d4穿刺，不按移动力消耗计伤；跳跃与传送不计地面路程。",small);visionColumn=IntegerField(visionColumn,false);visionRow=IntegerField(visionRow,false);}
 void ResolveSpikeGrowth(Vector2Int cell,int caster){spikeGrowth.Begin(cell.x,cell.y,8L+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(caster)]),worldRounds);concentrationSpell="荆棘丛生";concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=worldRounds+100;history.Insert(0,"荆棘丛生：地面困难地形，专注至多10分钟；出现与静止不造成伤害，经过每5尺分别掷2d4穿刺。木桩照常追击。");}
 void DrawSpikeGrowthArea(Rect board){Vector2Int anchor=Vector2Int.zero;bool preview=panel=="spells"&&selectedSpellName=="Spike Growth"&&VisionPoint(out anchor);if(!preview&&!spikeGrowth.Active(worldRounds))return;for(int y=0;y<map.Size;y++)for(int x=0;x<map.Size;x++){if(spikeGrowth.Cell(x,y,worldRounds))Fill(CellRect(board,new Vector2Int(x,y)),new Color(.4f,.22f,.03f,.42f));long dx=x-anchor.x,dy=y-anchor.y;if(preview&&dx*dx+dy*dy<=16)Fill(CellRect(board,new Vector2Int(x,y)),new Color(.7f,.4f,.1f,.35f));}}
 void ObserveSpikeMovement(int actor,Vector2Int from,Vector2Int to,int spaces,bool landing){
  if(landing||!map.Locked||!spikeGrowth.Active(worldRounds)||from==to)return;
  if((actor==0||actor==-1)&&(map.IsContained(actor==0)||map.IsCarried(actor==0)))return;
  var meter=map.RouteMeter(from,to,false);var previous=from;int count=0;
  foreach(var cell in BattleMap.MovementAnchors(from,to)){int before=meter.Travelled;meter.Next(BattleMap.FeetBetween(previous,cell),false,false);int feet=map.MovementTravelDistance??meter.Travelled-before;if(spikeGrowth.Intersects(cell.x,cell.y,spaces,worldRounds))count+=spikeGrowth.Travel(actor,feet,worldRounds);previous=cell;}
  if(count>0)spikeEntries.Enqueue(new SpikeEntry{Actor=actor,Count=count});
 }
 bool SpikePathNeeded(bool player,Vector2Int to){if(!spikeGrowth.Active(worldRounds))return false;int spaces=player?map.PlayerSpaces:map.MonsterSpaces;foreach(var cell in BattleMap.MovementAnchors(player?map.Player:map.Monster,to))if(spikeGrowth.Intersects(cell.x,cell.y,spaces,worldRounds))return true;return false;}
 IEnumerator ResolveSpikeEntry(SpikeEntry entry){
  for(int i=0;i<entry.Count;i++){
   if(!spikeGrowth.Active(worldRounds))yield break;
   bool player=entry.Actor==0,monsterActor=entry.Actor==-1,rescue=entry.Actor==BattleMap.RescueId;var unit=entry.Actor>0?FindSummon(entry.Actor):null;
   if(player&&combat.PlayerDead||monsterActor&&combat.Hp==0||rescue&&(!map.RescuePosition.HasValue||rescueCarrier.Dead)||!player&&!monsterActor&&!rescue&&(unit==null||!unit.Alive))yield break;
   string name=player?"玩家":monsterActor?"木桩":rescue?"救援者":unit.Name;string saved=panel;if(rescue)panel="";
   try{
    int a=Random.Range(1,5),b=Random.Range(1,5);yield return PresentRoll(4,new[]{a,b},"荆棘丛生 · "+name+"经过5尺："+(a+b)+"穿刺");
    var reduced=new RayDamage();yield return ReduceRayDamage(true,a+b,"穿刺","荆棘丛生",reduced);
    int damage=reduced.Amount;
    if(player||monsterActor){damage=combat.AdjustTypedDamage(player,damage,1);if(player)combat.ApplyPlayerDamage(damage,sourcePlayer:true);else combat.ApplyDamage(damage);if(player)yield return MaintainConcentration(damage);}
    else if(rescue){rescueCarrier.TakeDamage(damage,damageType:1);rescueCarrier.ObserveLife(worldRounds);ReconcileFallenRescue();}
    else ApplySummonSpellDamage(unit,damage,"穿刺");
    history.Insert(0,name+"因荆棘丛生承受 "+damage+" 点穿刺伤害。");RefreshConcentration();CheckBattleOutcome();
   }finally{if(rescue)panel=saved;}
  }
 }
}
