using System;
using UnityEngine;
class ThunderousSmiteMapChecks
{
 static void C(bool v,string message){if(!v)throw new Exception(message);}
 static BattleMap Map(int px,int py,int mx,int my){var m=new BattleMap();C(m.Place(true,new Vector2Int(px,py))&&m.Place(false,new Vector2Int(mx,my))&&m.Lock(),"legal deployment");return m;}
 static void Main(){
  var m=Map(1,2,2,2);int observed=0;m.MovementObserved=(actor,from,to,oldN,n,landing)=>{C(actor==-1&&!landing&&m.Monster==to,"forced steps are committed before terrain observes them");observed++;};
  int player=m.MovementRemaining,monster=m.MonsterMovementRemaining;C(m.ForceMoveAway(false,m.Player,m.PlayerSpaces,10)==10&&m.Monster==new Vector2Int(4,2)&&observed==2,"unblocked push covers ten feet in two observed steps");
  C(m.MovementRemaining==player&&m.MonsterMovementRemaining==monster,"forced movement spends neither creature's movement");
  m=Map(7,2,8,2);C(m.ForceMoveAway(false,m.Player,m.PlayerSpaces,10)==5&&m.Monster==new Vector2Int(9,2),"edge truncates movement without exceeding board");
  m=Map(1,2,2,2);C(m.AddSummon(new SummonedUnit(4,"blocker",10,10,30,2,1),new Vector2Int(4,2)),"blocker placement");C(m.ForceMoveAway(false,m.Player,m.PlayerSpaces,10)==5&&m.Monster==new Vector2Int(3,2),"push stops before occupied cell and never tunnels through a creature");
  m=Map(1,2,2,2);C(m.AddSummon(new SummonedUnit(5,"blocker",10,10,30,2,1),new Vector2Int(3,2)),"adjacent blocker placement");C(m.ForceMoveAway(false,m.Player,m.PlayerSpaces,10)==0&&m.Monster==new Vector2Int(2,2),"immediate blocking produces zero displacement");
  m=Map(1,1,2,2);int moved=m.ForceMoveAway(false,m.Player,m.PlayerSpaces,10);C(moved==7&&m.Monster==new Vector2Int(3,3),"diagonal grid landing never exceeds ten-foot budget");
  Console.WriteLine("PASS: production Thunderous Smite forced path, map edge, creature blocking, diagonal limit, committed terrain observations and unchanged movement pools");
 }
}
