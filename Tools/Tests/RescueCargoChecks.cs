using System;
using UnityEngine;
class RescueCargoChecks
{
 static void Check(bool v,string m){if(!v)throw new Exception(m);}
 static Vector2Int V(int x,int y)=>new Vector2Int(x,y);
 static void Main(){
  var map=new BattleMap();map.SetWater(V(0,0),true);map.Lock();map.SwitchWaterState(true,true);
  Check(map.DeployRescue(V(0,1),1,true)&&map.CanAttachRescueCorpse(true),"rescue enters next to body");
  Check(map.AttachRescueCorpse(true,false)&&map.IsCarried(true)&&map.OccupiedSpaces(true)==0,"carried body no longer independently occupies floor");
  Check(!map.PlayerAquatic.Submerged&&map.Player==map.RescuePosition.Value,"surface carrier brings body to breathable surface");
  Check(!map.CanMoveTo(V(1,0))&&!map.RemoveRescue(),"carried target cannot move independently or disappear with carrier removal");
  Check(map.MoveRescue(V(0,2),cost=>true)&&map.Player==V(0,2),"carried body follows rescue position");
  Check(!map.DropRescueCorpse(V(0,2))&&map.IsCarried(true),"cannot drop overlapping rescuer");
  Check(map.DropRescueCorpse(V(1,2))&&!map.IsCarried(true)&&map.Player==V(1,2),"chosen adjacent drop restores full body occupancy");
  map.RemoveRescue();map.Unlock();map.Place(true,V(0,0));map.Lock();map.DeployRescue(V(0,1),1,true);
  Check(map.AttachRescueCorpse(true,true)&&map.OccupiedSpaces(true)==1,"dragged body retains independent footprint");
  Check(!map.CanMoveRescue(V(0,0)),"dragging cannot pull body outside map");
  Check(map.MoveRescue(V(0,2),cost=>true)&&map.Player==V(0,1),"drag keeps relative offset while moving both bodies");
  Check(map.DropRescueCorpse(V(0,1))&&!map.RescueCargoPlayer.HasValue,"drag can stop at its current body position");
  Console.WriteLine("PASS: rescue pickup geometry, carried and dragged occupancy, surface rescue and legal drop");
 }
}
