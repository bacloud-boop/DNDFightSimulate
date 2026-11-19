using System.Collections;
using UnityEngine;
public sealed partial class TrainingGame
{
 IEnumerator MoveSpiritThroughGrease(SummonedUnit unit,Vector2Int target)
 {
  var origin=new Vector2Int(unit.X,unit.Y);bool crawling=unit.Prone,previous=summonActing;var meter=map.RouteMeter(origin,target,crawling);var previousCell=origin;summonActing=true;
  try{foreach(var cell in BattleMap.MovementAnchors(origin,target)){
   int beforeTravel=meter.Travelled;int cost=map.RouteStepCost(meter,previousCell,cell,unit.Spaces,unit.SwimSpeed);previousCell=cell;if(cost<1)continue;
   if(!map.WithMovementCost(cell,cost,()=>map.CanMoveSummon(unit.Id,cell)))break;bool prone=unit.Prone;
   yield return MoveSpiritWithReactions(unit,cell,true,cost,meter.Travelled-beforeTravel);if(unit.X!=cell.x||unit.Y!=cell.y)break;
   if(!unit.Alive||combat.BattleFinished||!prone&&unit.Prone)break;
  }}finally{summonActing=previous;}
 }
 IEnumerator MoveRescueThroughGrease(Vector2Int target)
 {
  if(!map.RescuePosition.HasValue||rolling)yield break;
  bool previous=rolling;rolling=true;var origin=map.RescuePosition.Value;bool crawling=rescueCarrier.Prone;var meter=map.RouteMeter(origin,target,crawling);var previousCell=origin;
  try{foreach(var cell in BattleMap.MovementAnchors(origin,target)){
   int beforeTravel=meter.Travelled;int cost=map.RouteStepCost(meter,previousCell,cell,map.RescueSpaces,map.RescueSwimmingSpeed);previousCell=cell;if(cost<1)continue;bool prone=rescueCarrier.Prone;
   if(!map.WithMovementCost(cell,cost,()=>map.MoveRescue(cell,rescueCarrier.Move),meter.Travelled-beforeTravel))break;
   if(PendingTerrainCount>0)yield return DrainGreaseEntries();
   if(!rescueCarrier.Conscious||!prone&&rescueCarrier.Prone){rescueMessage="救援者在油脂中倒地，停在触发格；可自行起身或爬行。";yield break;}
  }rescueMessage="救援移动完成，剩余 "+rescueCarrier.MovementRemaining+"尺。";}finally{rolling=previous;}
 }
}
