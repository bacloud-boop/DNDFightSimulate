using System;
using UnityEngine;
class RidingMapChecks
{
 static void Check(bool ok,string s){if(!ok)throw new Exception(s);}
 static Vector2Int V(int x,int y)=>new Vector2Int(x,y);
 static BattleMap Make(out SummonedUnit steed){var map=new BattleMap();map.Lock();steed=new OtherworldlySteedRules(2,SteedKind.Celestial).CreateUnit(1);Check(map.AddSummon(steed,V(1,0)),"summon steed beside player");return map;}
 static void Main(){
  SummonedUnit steed;var map=Make(out steed);var old=map.Player;Check(!map.CanPlaceSummon(map.Player,1),"summons cannot overlap unmounted player");
  Check(map.CanMountSummon(1)&&map.Player==old&&!map.Riding,"mount preview is pure");
  Check(map.MountSummon(1)&&map.PlayerSpaces==2&&map.RiderBodySpaces==1&&map.Player==V(1,0),"shared whole footprint retains rider body");
  Check(!map.CanMoveTo(V(0,0)),"personal walking requires dismount");
  Check(steed.BeginTurn(1)&&map.MoveSummon(1,V(2,0))&&map.Player==V(2,0)&&steed.MovementRemaining==55,"mount movement carries rider and spends mount pool");
  Check(!map.CanPlace(false,V(2,0)),"monster cannot overlap mount");
  Check(map.CreatureCoverBonus(0,-1)==0&&map.CreatureCoverBonus(-1,1)==0,"rider and mount do not grant cover to each other");
  Check(map.TransformAt(true,2,map.Player,1)&&!map.Riding&&map.PlayerSpaces==2&&steed.Alive&&!BattleMap.Overlap(map.Player,2,V(steed.X,steed.Y),2),"large transformation succeeds and immediately dismounts to legal footprint");
  Check(map.RestoreForm(true)&&map.PlayerSpaces==1,"restore actual rider body rather than borrowed mount size");
  map=Make(out steed);Check(map.MountSummon(1)&&map.TryResizePlayerAt(1,map.Player)&&map.Riding,"small shape retains ride");
  Check(map.CanTeleportSummon(1,V(3,3),60)&&map.TeleportSummon(1,V(3,3),60)&&map.Player==V(3,3),"fey teleport carries rider");
  Check(map.DismountRider()&&!map.Riding&&map.PlayerSpaces==1&&!BattleMap.Overlap(map.Player,1,V(steed.X,steed.Y),2),"ordinary dismount leaves separate legal body");
  map=Make(out steed);map.MountSummon(1);steed.Damage(25);map.RemoveSummon(1);Check(!map.Riding&&map.PlayerSpaces==1&&map.Inside(map.Player,1),"dead vanished mount releases rider");
  Console.WriteLine("PASS: shared footprint, pure preview, mounted movement, cover, transformed dismount, restoration, teleport and vanished mount");
 }
}
