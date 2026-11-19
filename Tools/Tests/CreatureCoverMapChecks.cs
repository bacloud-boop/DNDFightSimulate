using System;
using UnityEngine;
class CreatureCoverMapChecks
{
 static void Check(bool value,string message){if(!value)throw new Exception(message);}
 static Vector2Int V(int x,int y)=>new Vector2Int(x,y);
 static void Main(){
  var map=new BattleMap();map.Place(false,V(5,0));map.Lock();
  Check(map.CreatureCoverBonus(0,-1)==0,"source and target do not obstruct themselves");
  var ally=new SummonedUnit(1,"guard",30,13,30,1,1);
  Check(map.AddSummon(ally,V(2,0)),"place intervening ally");
  Check(map.CreatureCoverBonus(0,-1)==2&&map.CreatureCoverBonus(-1,0)==2,"either side gets cover from a third creature");
  Check(map.CreatureCoverBonus(0,1)==0&&map.CreatureCoverBonus(1,-1)==0,"summon source and target are excluded");
  Check(map.CreatureCoverFromPoint(V(4,0),-1)==0,"explosion centre beyond blocker gives no cover");
  Check(map.CreatureCoverFromPoint(V(0,0),-1)==2,"explosion centre behind blocker grants cover");
  ally.Damage(30);Check(map.CreatureCoverBonus(0,-1)==0,"vanished summon gives no cover");
  Check(map.DeployRescue(V(2,0),1,true)&&map.CreatureCoverBonus(0,-1)==2,"rescue creature can provide cover");
  Check(map.CreatureCoverBonus(0,0)==0&&map.CreatureCoverBonus(0,999)==0,"self target and unknown identities give no cover");
  var corpses=new BattleMap();corpses.Place(false,V(2,0));corpses.Lock();
  var observer=new SummonedUnit(2,"observer",30,13,30,1,1);Check(corpses.AddSummon(observer,V(5,0)),"place target behind persistent primary body");
  Check(corpses.CreatureCoverBonus(0,2)==2,"primary body provides cover regardless of HP or death state");
  Check(corpses.DeployRescue(V(2,1),1,false)&&corpses.AttachRescueCorpse(false,false),"lift corpse away from attack line");
  Check(corpses.CreatureCoverBonus(0,2)==0,"lifted corpse has no separate cover footprint");
  Check(corpses.DropRescueCorpse(V(2,0))&&corpses.CreatureCoverBonus(0,2)==2,"dropping corpse restores cover without cached life state");
  Check(corpses.AttachRescueCorpse(false,true)&&corpses.CreatureCoverBonus(0,2)==2,"dragging preserves corpse cover");
  Check(corpses.MoveRescue(V(2,2),cost=>true)&&corpses.CreatureCoverBonus(0,2)==0,"dragged cover follows corpse rather than its old square");
  Console.WriteLine("PASS: map cover identities, living blockers, summon targets, area origins and rescue units");
 }
}
