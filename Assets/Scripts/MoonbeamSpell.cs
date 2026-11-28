using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
 const int MoonbeamLightGroup=9;
 readonly MoonbeamEffect moonbeam=new MoonbeamEffect();
 sealed class Moonburst{public long Revision;public int Slot,Amount,Remaining;public long Dc;public Vector2Int Origin;public bool Ready,Held;}
 readonly Dictionary<long,Moonburst> moonbursts=new Dictionary<long,Moonburst>();long moonburstSequence;
 Vector2Int MoonCell=>new Vector2Int(moonbeam.X,moonbeam.Y);
 void EndMoonbeam(bool cancelPending=false)
 {
  moonbeam.Clear();if(cancelPending){foreach(var burst in moonbursts.Values)if(burst.Held){burst.Held=false;outcomeDeferDepth--;}moonbursts.Clear();}
  startStatuses.RemoveAll(e=>e.Kind==6&&!moonbursts.ContainsKey(e.Group));vision.ClearLightGroup(MoonbeamLightGroup);
 }
 void FinishMoonburst(long id,Moonburst burst){if(--burst.Remaining>0)return;moonbursts.Remove(id);if(burst.Held){burst.Held=false;outcomeDeferDepth--;}}
 bool MoonBody(int actor,out Vector2Int p,out int n){p=Vector2Int.zero;n=0;string name;return moonbeam.Active(worldRounds)&&map.SceneOf(actor)==moonbeam.Scene&&HealingHandsTarget(actor,out p,out n,out name)&&!((actor==0||actor==-1)&&map.IsContained(actor==0));}
 bool InMoonbeam(int actor){Vector2Int p;int n;return MoonBody(actor,out p,out n)&&moonbeam.Intersects(p.x,p.y,n,worldRounds);}
 bool MoonbeamShapeBlocked(int actor){if(!InMoonbeam(actor)){moonbeam.Leave(actor);return false;}return moonbeam.Barred(actor);}
 bool MoonbeamEndAvailable(int actor)=>InMoonbeam(actor)&&moonbeam.Available(actor,terrainTurn,worldRounds);
 void RefreshMoonbeam()
 {
  if(!moonbeam.Active(worldRounds)){vision.ClearLightGroup(MoonbeamLightGroup);return;}
  foreach(int actor in BreathActors())if(!InMoonbeam(actor))moonbeam.Leave(actor);
  if(moonbeam.Scene!=0){vision.HideLightGroup(MoonbeamLightGroup);return;}
  if(vision.ConsumeDispelledLightGroup(MoonbeamLightGroup)||!vision.SetDimLightGroup(MoonbeamLightGroup,new[]{new[]{moonbeam.X*5.0,moonbeam.Y*5.0}},5,moonbeam.Slot,moonbeam.Expires,worldRounds)){
   EndMoonbeam();if(concentrationSpell=="月华之光")EndConcentration("光柱微光与黑暗术重叠，低环法术被驱散");
  }
 }
 string MoonbeamError(){Vector2Int p;if(!VisionPoint(out p)||p.x<0||p.y<0||p.x>=map.Size||p.y>=map.Size)return "请选择地图内的光柱中心整数坐标。";if(map.DistanceToCell(true,p)>120)return "光柱中心超出120尺。";if(map.IsContained(true))return "吞咽内部不能向外创造光柱。";return worldRounds<0||worldRounds>long.MaxValue-10?"世界时间超出范围。":"";}
 void DrawMoonbeamChoices()
 {
  visionColumn=IntegerField(visionColumn,false);visionRow=IntegerField(visionRow,false);
  GUILayout.Label("5尺半径、40尺高的光柱与微光，专注1分钟；二环2d10光耀，每升环+1d10。出现、目标从外进入、光柱新覆盖目标或目标回合末体质豁免，成功半伤，每目标每回合至多一次。盟友和自身也可能受影响。",small);
  GUILayout.Label("失败时若已变形，恢复原形，离开光柱前不能再次变形。后续自己的回合可用魔法动作将光柱连续移动至多60尺；始终在光柱内的目标不因横移重复触发，等回合末结算。无重新施法、声音或姿势要求。",small);
  if(moonbeam.Active(worldRounds)){string error=MoonbeamMoveError();bool enabled=GUI.enabled;GUI.enabled=enabled&&error=="";if(GUILayout.Button("移动现存光柱（魔法动作）",button)){Vector2Int p;if(VisionPoint(out p))StartCoroutine(MoveMoonbeam(p));}GUI.enabled=enabled;if(error!="")GUILayout.Label(error,small);}
 }
 IEnumerator CastMoonbeam(Vector2Int p,int caster,int slot)
 {
  if(!moonbeam.Begin(p.x,p.y,map.SceneOf(0),slot,8L+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(caster)]),worldRounds,terrainTurn))yield break;
  concentrationSpell="月华之光";concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=moonbeam.Expires;RefreshMoonbeam();
  if(!moonbeam.Active(worldRounds)){history.Insert(0,"月华之光被黑暗术驱散，未产生新的伤害触发。");yield break;}
  QueueMoonbeam(BreathActors(),MoonCell);if(PendingTerrainCount>0)yield return DrainGreaseEntries();
  history.Insert(0,"月华之光：5尺半径，"+slot+"d10光耀，体质豁免成功半伤；创建立即触发，计入每目标每回合一次。");
 }
 void QueueMoonbeam(IEnumerable<int> selected,Vector2Int origin)
 {
  var actors=new List<int>();foreach(int actor in selected)if(InMoonbeam(actor)&&moonbeam.Claim(actor,terrainTurn,worldRounds))actors.Add(actor);if(actors.Count==0)return;
  long group=++moonburstSequence;moonbursts[group]=new Moonburst{Revision=moonbeam.Revision,Slot=moonbeam.Slot,Dc=moonbeam.Dc,Origin=origin,Remaining=actors.Count,Held=true};outcomeDeferDepth++;
  foreach(int actor in actors)startStatuses.Add(new StartStatus{Actor=actor,Kind=6,Revision=moonbeam.Revision,Origin=origin,Group=group});
 }
 void QueueMoonbeamEnd(int actor){if(MoonbeamEndAvailable(actor))QueueMoonbeam(new[]{actor},MoonCell);}
 void ObserveMoonbeamMovement(int actor,Vector2Int from,Vector2Int to,int oldSpaces,int spaces,bool landing)
 {
  if(!moonbeam.Active(worldRounds))return;
  if(actor==BattleMap.RescueId&&map.RescueCargoPlayer.HasValue){bool player=map.RescueCargoPlayer.Value;var cargo=player?map.Player:map.Monster;int n=player?map.PlayerSpaces:map.MonsterSpaces;ObserveMoonbeamMovement(player?0:-1,new Vector2Int(cargo.x-(to.x-from.x),cargo.y-(to.y-from.y)),cargo,n,n,landing);}
  Vector2Int actual;int count;if(!MoonBody(actor,out actual,out count)){moonbeam.Leave(actor);return;}
  var path=landing&&!map.MovementIsJump?new List<Vector2Int>{to}:BattleMap.MovementAnchors(from,to);if(from==to&&oldSpaces!=spaces)path.Add(to);var previous=from;int old=oldSpaces;
  foreach(var cell in path){bool before=moonbeam.Intersects(previous.x,previous.y,old,worldRounds),after=moonbeam.Intersects(cell.x,cell.y,spaces,worldRounds);if(!after)moonbeam.Leave(actor);if(!before&&after){
   // A jump can pass through the cylinder and land outside it. The trigger's
   // coverage is therefore evaluated at this path point, not the final body.
   QueueMoonbeamAt(actor,cell,spaces);
  }previous=cell;old=spaces;}
 }
 void QueueMoonbeamAt(int actor,Vector2Int p,int n)
 {
  if(!moonbeam.Intersects(p.x,p.y,n,worldRounds)||!moonbeam.Claim(actor,terrainTurn,worldRounds))return;long group=++moonburstSequence;
  moonbursts[group]=new Moonburst{Revision=moonbeam.Revision,Slot=moonbeam.Slot,Dc=moonbeam.Dc,Origin=MoonCell,Remaining=1,Held=true};outcomeDeferDepth++;startStatuses.Add(new StartStatus{Actor=actor,Kind=6,Revision=moonbeam.Revision,Origin=MoonCell,Group=group});
 }
 bool MoonbeamPathNeeded(Vector2Int from,Vector2Int to,int spaces){if(!moonbeam.Active(worldRounds)||moonbeam.Scene!=map.Scene)return false;foreach(var p in BattleMap.MovementAnchors(from,to))if(moonbeam.Intersects(p.x,p.y,spaces,worldRounds))return true;return false;}
 string MoonbeamMoveError()
 {
  if(!moonbeam.Active(worldRounds)||concentrationSpell!="月华之光")return "没有现存光柱。";if(!moonbeam.CanMove(worldRounds,terrainTurn))return "只能在施法后的回合移动光柱。";
  if(rolling||reactionPending||spellTargeting||draggingToken>=0||summonTurns.Active)return "请先完成当前操作。";
  if(terrainActor!=0||!PlayerTurnAllowed||!(combat.Aftermath?combat.AftermathMagicActionAvailable:combat.MagicActionAvailable))return "当前没有可用的魔法动作。";
  if(map.SceneOf(0)!=moonbeam.Scene||map.Scene!=moonbeam.Scene||map.IsContained(true))return "请在光柱所在场景、吞咽外控制。";
  Vector2Int p;if(!VisionPoint(out p)||p.x<0||p.y<0||p.x>=map.Size||p.y>=map.Size)return "请选择地图内的整数中心坐标。";int distance=BattleMap.FeetBetween(MoonCell,p);return distance<=0||distance>60?"光柱每次须移动1至60尺。":"";
 }
 IEnumerator MoveMoonbeam(Vector2Int destination)
 {
  Vector2Int p;if(MoonbeamMoveError()!=""||!VisionPoint(out p)||p!=destination||!combat.TryUseAftermathMagicAction())yield break;bool busy=rolling;string saved=panel;rolling=true;panel="";
  try{foreach(var cell in BattleMap.MovementAnchors(MoonCell,destination)){
   if(!moonbeam.Active(worldRounds))break;var old=MoonCell;var entered=new List<int>();foreach(int actor in BreathActors()){Vector2Int b;int n;if(MoonBody(actor,out b,out n)&&!MoonbeamEffect.Covers(old.x,old.y,b.x,b.y,n)&&MoonbeamEffect.Covers(cell.x,cell.y,b.x,b.y,n))entered.Add(actor);}
   moonbeam.Place(cell.x,cell.y);RefreshMoonbeam();QueueMoonbeam(entered,cell);if(PendingTerrainCount>0)yield return DrainGreaseEntries();
  }history.Insert(0,"月华之光沿途连续移动；只在新覆盖目标时触发，每目标本回合至多一次，不刷新专注时间。");}finally{panel=saved;rolling=busy;}
 }
 int MoonbeamRootSize(int actor){bool player=actor==0;int size=player?NaturalPlayerSize:monsterProfile.SizeIndex;return sizeChange.Applies(player)?SizeChangeEffect.ChangedSize(size,sizeChange.Delta):size;}
 int MoonbeamRootSpaces(int actor)=>CreatureProfile.SpacesForSize(MoonbeamRootSize(actor));
 void FinishMoonbeamTrueForm(int actor,bool restoredGeometry)
 {
  if(!restoredGeometry&&!map.RestoreTrueFormFootprint(actor==0,MoonbeamRootSpaces(actor),MoonbeamRootSize(actor)))return;
  // Joint placement already restored the root body; do not briefly expand an
  // intermediate Wild Shape over the passenger that was just released.
  if(polymorph.Active&&polymorph.TargetPlayer==(actor==0))polymorph.Clear();
  if(actor==0){wildForm=null;wildNormalOffset=Vector2Int.zero;combat.SetPlayerArmorOverride(null);map.SetPlayerSpeed(NaturalPlayerSpeed);}else combat.MonsterFormArmor=null;
  fighter.CancelAttackAction();actionWeapon=null;SyncTimedBuffs();
 }
 bool MoonbeamShifted(int actor)=>actor==0?PlayerPolymorphed||IsWildShaped:actor==-1&&MonsterPolymorphed;
 IEnumerator ResolveMoonbeamSave(int actor,long revision,long group)
 {
  Moonburst burst;if(!moonbursts.TryGetValue(group,out burst)||burst.Revision!=revision)yield break;
  Vector2Int p;int n;string name;if(!HealingHandsTarget(actor,out p,out n,out name)){FinishMoonburst(group,burst);CheckBattleOutcome();yield break;}
  outcomeDeferDepth++;try{
   if(!burst.Ready){var rolls=new int[burst.Slot];int sum=0;for(int i=0;i<rolls.Length;i++){rolls[i]=UnityEngine.Random.Range(1,11);sum+=rolls[i];}dice="月华之光 · 共享光耀伤害";yield return PresentRoll(10,rolls,burst.Slot+"d10 = "+sum+" 光耀");var reduced=new RayDamage();yield return ReduceRayDamage(true,sum,"光耀","月华之光",reduced);burst.Amount=reduced.Amount;burst.Ready=true;}
   var failed=new HashSet<int>();yield return RollMappedAbilitySave(actor,0,burst.Dc,failed,"月华之光",2,burst.Origin);bool changed=failed.Contains(actor)&&MoonbeamShifted(actor);
   bool jointRestore=false;if(changed&&map.HasContainedCreature){yield return ReleaseSwallowedTarget("月华之光恢复原形并放出被吞目标",MoonbeamRootSpaces(actor),MoonbeamRootSize(actor));jointRestore=releaseRestoredHost;}
   int actual=ApplyMappedAreaDamage(actor,0,failed.Contains(actor)?burst.Amount:burst.Amount/2,11,"月华之光",failed.Contains(actor));
   if(changed){if(moonbeam.Active(worldRounds)&&revision==moonbeam.Revision)moonbeam.Bar(actor);FinishMoonbeamTrueForm(actor,jointRestore);ObserveForcedSharedStop(actor);if(jointRestore)ObserveForcedSharedStop(actor==0?-1:0);history.Insert(0,name+"月华之光豁免失败，恢复原形；离开光柱前不能再次变形。");}
   yield return MaintainConcentration(actor==0?actual:0);RefreshMoonbeam();RefreshBestialSpirit();ReconcileFallenRescue();
  }finally{FinishMoonburst(group,burst);outcomeDeferDepth--;CheckBattleOutcome();}
 }
 void DrawMoonbeamArea(Rect board)
 {
  Vector2Int preview=Vector2Int.zero;bool selecting=panel=="spells"&&selectedSpellName=="Moonbeam"&&VisionPoint(out preview);
  for(int y=0;y<map.Size;y++)for(int x=0;x<map.Size;x++){var p=new Vector2Int(x,y);if(moonbeam.Scene==map.Scene&&moonbeam.Intersects(x,y,1,worldRounds))Fill(CellRect(board,p),new Color(.7f,.8f,1f,.27f));if(selecting&&MoonbeamEffect.Covers(preview.x,preview.y,x,y,1))Fill(CellRect(board,p),new Color(.8f,.9f,1f,.36f));}
 }
}
