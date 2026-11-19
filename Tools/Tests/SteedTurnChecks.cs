using System;
class SteedTurnChecks
{
 static void C(bool ok,string message){if(!ok)throw new Exception(message);}
 static SummonedUnit U(int id)=>new SummonedUnit(id,"steed",25,12,60,3,1);
 static void Main(){
  var u=U(1);u.BeginTurn(1);u.SpendMovement(15);u.Dash();u.TryUseBonus();u.UseReaction();
  var q=new SummonTurnQueue();C(q.Begin(new[]{u},1,false,1)&&q.Current==u,"adopt a continuing controlled mount");
  C(u.MovementRemaining==105&&!u.ActionAvailable&&!u.BonusAvailable&&!u.ReactionAvailable,"adoption preserves ALL spent resources");
  q.EndCurrent();C(!u.BeginTurn(1),"dismount does not refresh same-round resources");
  C(u.BeginTurn(2)&&u.MovementRemaining==60&&u.ActionAvailable&&u.BonusAvailable&&u.ReactionAvailable,"next turn restores resources");
  C(u.Disengage()&&u.Disengaged&&!u.ActionAvailable,"disengage spends its own action");
  u.EndTurn();u.BeginTurn(3);C(!u.Disengaged,"disengage ends when next turn begins");
  u.LoadSpeedLimit=5;C(u.MovementRemaining==5,"temporary rider overload limits mount speed");
  u.LoadSpeedLimit=0;C(u.MovementRemaining==0,"unresolved or excessive load prevents movement");
  Console.WriteLine("PASS: continuing mount turn, no double refresh, bonus resource, disengage and rider load");
 }
}
