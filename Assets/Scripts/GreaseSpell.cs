using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
 readonly GreaseFields grease=new GreaseFields();
    bool greaseResolving;
 sealed class GreaseEntry {public int Actor;public GreaseField Field;}
 readonly Queue<GreaseEntry> greaseEntries=new Queue<GreaseEntry>();
 void InitializeGrease(){map.MovementObserved=ObserveGreaseMovement;}
 bool FullDifficultTerrain(int x,int y,int spaces){for(int j=0;j<spaces;j++)for(int i=0;i<spaces;i++)if(!entangle.FullFootprint(x+i,y+j,1,worldRounds)&&!grease.Cell(x+i,y+j,worldRounds))return false;return spaces>0;}
 void ObserveGreaseMovement(int actor,Vector2Int from,Vector2Int to,int oldSpaces,int spaces,bool landing)
 {
  if(!map.Locked||!grease.Any(worldRounds))return;var path=landing?new List<Vector2Int>{to}:BattleMap.MovementAnchors(from,to);var previous=from;int previousSpaces=oldSpaces;
  foreach(var cell in path){var field=grease.Current(cell.x,cell.y,spaces,worldRounds);if(field!=null&&!field.Intersects(previous.x,previous.y,previousSpaces))greaseEntries.Enqueue(new GreaseEntry{Actor=actor,Field=field});previous=cell;previousSpaces=spaces;}
 }
 GreaseField ActorGrease(int actor){return actor==0?grease.Current(map.Player.x,map.Player.y,map.PlayerSpaces,worldRounds):actor==-1?grease.Current(map.Monster.x,map.Monster.y,map.MonsterSpaces,worldRounds):null;}

 int GreaseLand(Vector2Int anchor){int mask=0;for(int y=0;y<2;y++)for(int x=0;x<2;x++){var cell=anchor+new Vector2Int(x,y);if(map.Inside(cell,1)&&true)mask|=1<<(y*2+x);}return mask;}
 string GreaseError(){Vector2Int cell;if(!VisionPoint(out cell))return "油脂区域左上格须位于地图内。";return GreaseDistance(cell)>60?"区域中心超出60尺。":grease.CanAdd(worldRounds)?"":"时间或区域编号超出范围。";}
 int GreaseDistance(Vector2Int anchor){double best=double.MaxValue;for(int y=0;y<map.PlayerSpaces;y++)for(int x=0;x<map.PlayerSpaces;x++){double dx=anchor.x+.5-map.Player.x-x,dy=anchor.y+.5-map.Player.y-y;best=System.Math.Min(best,System.Math.Sqrt(dx*dx+dy*dy)*5);}return Mathf.RoundToInt((float)best);}
 void DrawGreaseChoices(){GUILayout.Label("10尺方形（2×2格），输入左上格列／行；中心位于区域中央格线交点。仅陆地生效，不易燃，非专注1分钟。",small);visionColumn=IntegerField(visionColumn,false);visionRow=IntegerField(visionRow,false);}
 void DrawGreaseArea(Rect board){Vector2Int anchor=Vector2Int.zero;bool preview=panel=="spells"&&selectedSpellName=="Grease"&&VisionPoint(out anchor);if(!preview&&!grease.Any(worldRounds))return;for(int y=0;y<map.Size;y++)for(int x=0;x<map.Size;x++){var cell=new Vector2Int(x,y);if(grease.Cell(x,y,worldRounds))Fill(CellRect(board,cell),new Color(.65f,.48f,.05f,.4f));if(preview&&x>=anchor.x&&x<anchor.x+2&&y>=anchor.y&&y<anchor.y+2&&true)Fill(CellRect(board,cell),new Color(.9f,.7f,.15f,.4f));}}
 IEnumerator ResolveGrease(Vector2Int anchor,int caster){var added=grease.Add(anchor.x,anchor.y,GreaseLand(anchor),8L+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(caster)]),worldRounds);var actors=new List<int>{-1,0};foreach(int actor in actors)if(ActorGrease(actor)==added)yield return ResolveGreaseSave(actor,added,"油脂出现");history.Insert(0,"油腻术：仅陆地形成非易燃油脂，非专注1分钟；出现、进入和回合末敏捷豁免失败倒地。");}
 IEnumerator ResolveGreaseSave(int actor,GreaseField field,string reason)
 {
  if(field==null||!field.Active(worldRounds)||actor!=0&&actor!=-1)yield break;
  bool player=actor==0;if(player?combat.PlayerDead:combat.Hp==0)yield break;
  bool automatic=player?combat.PlayerHp==0||combat.PlayerParalyzed:combat.MonsterParalyzed;var roll=RollEnfeeblementSave(player,1);int modifier=player?PlayerSavingThrowModifier(1):MonsterSavingThrowModifier(1);
  var adjustment=new SaveAdjustment();yield return ApplySaveAdjustments(player,automatic,adjustment);modifier+=adjustment.Value;
  if(!automatic)yield return PresentRoll(20,roll.Dice,reason+" · "+(player?"玩家":"木桩")+"敏捷豁免："+roll.Value+" + "+modifier+" / DC "+field.Dc);
  if(automatic||(long)roll.Value+modifier<field.Dc){if(player)combat.PlayerProne=true;else combat.MonsterProne=true;SyncTimedBuffs();history.Insert(0,(player?"玩家":"木桩")+"油腻术豁免失败，倒地。");}else history.Insert(0,(player?"玩家":"木桩")+"通过油腻术敏捷豁免。");
 }
 IEnumerator DrainGreaseEntries(){if(greaseResolving){while(greaseResolving)yield return null;yield break;}bool busy=rolling;rolling=true;greaseResolving=true;try{while(greaseEntries.Count>0){var entry=greaseEntries.Dequeue();yield return ResolveGreaseSave(entry.Actor,entry.Field,"进入油脂");}}finally{rolling=busy;greaseResolving=false;}}
 bool GreasePathNeeded(bool player,Vector2Int target){if(!grease.Any(worldRounds))return false;var from=player?map.Player:map.Monster;int spaces=player?map.PlayerSpaces:map.MonsterSpaces;foreach(var cell in BattleMap.MovementAnchors(from,target)){var field=grease.Current(cell.x,cell.y,spaces,worldRounds);if(field!=null&&!field.Intersects(from.x,from.y,spaces))return true;from=cell;}return false;}
 IEnumerator MoveThroughGrease(bool player,Vector2Int target,bool tactical,bool preserveBusy)
 {
  var origin=player?map.Player:map.Monster;int spaces=player?map.PlayerSpaces:map.MonsterSpaces;bool crawling=player?combat.PlayerProne:combat.MonsterProne;var meter=map.RouteMeter(origin,target,crawling);var previousCell=origin;
  bool aftermath= combat.Aftermath;rolling=true;try{foreach(var cell in BattleMap.MovementAnchors(origin,target)){
   int cost=map.RouteStepCost(meter,previousCell,cell,spaces,0);previousCell=cell;if(cost<1)continue;
   bool legal=map.WithMovementCost(cell,cost,()=>player?map.CanMoveTo(cell,tactical):map.CanMoveMonsterTo(cell));if(!legal)break;
   bool wasProne=player?combat.PlayerProne:combat.MonsterProne;
   yield return ResolveMovement(player,cell,tactical,true,false,true,cost,spaces);if((player?map.Player:map.Monster)!=cell)break;
   if((player?combat.PlayerProne:combat.MonsterProne)&&!wasProne){if(player){movementMessage="油腻术使玩家倒地，停在触发格；剩余路程未扣费。";break;}int stand=combat.MonsterSpeed/2;if(combat.MonsterSpeed<=0||map.MonsterMovementRemaining<stand)break;map.SpendMonsterMovement(stand);combat.MonsterProne=false;SyncTimedBuffs();history.Insert(0,"木桩花费"+stand+"尺移动力起身，继续追击。");}
   if(!aftermath&&combat.BattleFinished||player&&(combat.PlayerDead||combat.PlayerHp<=0||combat.PlayerParalyzed)||!player&&!combat.MonsterCanAct||(player?map.PlayerSpaces:map.MonsterSpaces)!=spaces)break;
  }}finally{if(!preserveBusy)rolling=false;}
 }
}
