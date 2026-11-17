using System;
using UnityEngine;
class EntangleChecks
{
 static void Check(bool value,string message){if(!value)throw new Exception(message);}
 static Vector2Int V(int x,int y)=>new Vector2Int(x,y);
 static void Main(){
  var effect=new EntangleEffect();effect.Begin(2,2,15,0);
  Check(effect.FullFootprint(2,2,2,0)&&!effect.FullFootprint(1,2,2,0),"large body must fully enter difficult terrain");
  Check(effect.Intersects(1,2,2,0)&&!effect.Intersects(0,0,2,0),"initial save uses area intersection");
  Check(!effect.Bind(0,0)&&effect.Bind(-1,0)&&effect.Bind(7,0),"caster excluded, monster and summon bind independently");
  Check(effect.Release(-1)&&!effect.Restrained(-1,0)&&effect.Restrained(7,0)&&effect.FullFootprint(2,2,1,0),"escape preserves other bindings and terrain");
  Check(!effect.Restrained(-1,0),"reentering does not automatically bind again");
  Check(effect.Active(9)&&!effect.Active(10)&&!effect.Restrained(7,10),"one minute expiry");
  effect.Clear();Check(!effect.Active(0),"concentration and reset clear terrain");effect.Begin(2,2,15,0);
  var map=new BattleMap();map.DifficultTerrain=(x,y,s)=>effect.FullFootprint(x,y,s,0);
  map.Place(true,V(0,2));map.Lock();
  Check(map.PlayerMovementCost(V(3,2))==25,"only rough portion adds distance");
  map.PlayerCrawling=true;Check(map.PlayerMovementCost(V(3,2))==40,"crawl and difficult terrain add rather than multiply");
  Check(map.DifficultTerrainCost(V(1,2),V(2,2),2)==5&&map.DifficultTerrainCost(V(2,2),V(1,2),2)==0,"large unit entry boundary");
  map.RestrainedActor=player=>!player;Check(!map.CanMoveMonsterTo(V(8,9))&&!map.CanJump(false,V(8,9)),"restrained blocks walking and jumping");
  effect.Bind(-1,0);effect.Release(-1);Check(effect.Active(0),"freeing all targets never ends terrain");
  Console.WriteLine("PASS: initial targets, caster exclusion, independent escape, full-footprint terrain, additive costs, movement blocks and expiry");
 }
}
