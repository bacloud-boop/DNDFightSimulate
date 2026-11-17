using System;
class RescueCarrierChecks
{
 static void Check(bool v,string m){if(!v)throw new Exception(m);}
 static void Main(){
  var r=new RescueCarrier();Check(r.Configure(10,2,30,150,0)&&r.BeginTurn(1),"rescue profile and independent turn");
  Check(!r.Configure(20,2,60,150,0)&&!r.BeginTurn(1),"cannot reconfigure or refill an active turn");
  Check(r.Move(10)&&r.PickUp(0,true,new BodyWeight(151),true)&&r.ActionsRemaining==0,"pickup consumes rescue action rather than player action");
  Check(!r.CanDepart,"loaded carrier cannot depart before dropping body");
  Check(r.EffectiveSpeed==5&&r.MovementRemaining==0&&!r.Move(1),"picking up heavy drag load cannot refund spent movement");
  r.EndTurn();Check(!r.BeginTurn(1)&&r.BeginTurn(2)&&r.MovementRemaining==5,"only new rescue turn refreshes heavy dragging allowance");
  Check(r.Dash()&&r.MovementRemaining==10&&r.Move(5),"dash uses capped speed");
  Check(r.Drop(()=>true)&&r.MovementRemaining==55&&r.MovementSpent==5,"free drop restores speed but preserves spent movement");
  Check(!r.Move(56)&&r.MovementSpent==5,"invalid movement leaves resource unchanged");
  r.EndTurn();r.Prone=true;
  Check(!r.Configure(10,2,30,150,0,2)&&r.Prone&&r.MovementSpent==5,"invalid dexterity leaves previous profile and state unchanged");
  Check(r.Configure(10,2,30,150,0,3)&&r.DexteritySaveModifier==-4&&!r.Prone&&r.MovementSpent==0,"new deployment clears old prone and movement, floors negative dexterity modifier");
  Check(r.BeginTurn(2)&&r.MovementRemaining==30,"new deployment can start in the same world round as previous departure");
  r.EndTurn();Check(!r.Configure(10,2,30,150,0,21)&&r.Dexterity==3,"dexterity upper limit enforced atomically");
  Check(r.Configure(10,2,30,150,0,20)&&r.DexteritySaveModifier==5,"dexterity upper bound");
  Check(r.Configure(10,2,30,150,0)&&r.Dexterity==10&&r.DexteritySaveModifier==0,"default dexterity is ten without proficiency");
  r.BeginTurn(3);r.Prone=true;Check(r.Stand()&&r.MovementRemaining==15&&!r.Prone,"standing spends only half rescue speed");
  r.EndTurn();Check(!r.Configure(10,2,30,150,0,10,0)&&r.Hp==10&&r.MaxHp==10,"invalid maximum HP rejected without changing state");
  Check(r.Configure(10,2,30,150,0,10,25)&&r.Hp==25&&r.MaxHp==25,"deployment applies and fills configured maximum HP");
  r.BeginTurn(4);Check(r.TakeDamage(7)==7&&r.Hp==18&&r.TurnActive,"nonfatal damage preserves turn");
  Check(r.TakeDamage(-1)==0&&r.Hp==18,"negative damage cannot heal");
  Check(r.TakeDamage(long.MaxValue)==18&&r.Hp==0&&r.Prone&&!r.TurnActive,"large damage clamps to zero and ends active turn");
  Check(!r.CanDepart,"unconscious carrier cannot depart");
  Check(!r.BeginTurn(5)&&!r.Move(1)&&!r.Dash()&&!r.Stand(),"zero HP cannot act or refresh resources");
  Check(r.Dead&&!r.Revive(0,false)&&r.Heal(10)==0,"massive damage kills; healing cannot replace resurrection");
  Check(r.Revive(1,true)&&r.Hp==1&&r.RevivalPenalty==-4&&!r.Dead,"Raise Dead restores one HP and revival penalty");
  r.TakeDamage(1);Check(r.NeedsDeathSave&&r.ResolveDeathSave(20,5)&&r.Hp==1,"natural twenty heals even with revival penalty");
  r.Configure(10,2,30,150,0);r.TakeDamage(10);
  Check(!r.Dead&&r.NeedsDeathSave&&r.ResolveDeathSave(1,1)&&r.DeathFailures==2,"ordinary zero HP starts saves, natural one counts twice");
  Check(!r.ResolveDeathSave(10,1)&&r.DeathFailures==2,"one save per turn sequence");
  r.TakeDamage(1);Check(r.Dead,"damage while unconscious adds a failure and can kill");
  r.Configure(10,2,30,150,0);r.TakeDamage(10);r.ResolveDeathSave(10,1);r.ResolveDeathSave(10,2);r.ResolveDeathSave(10,3);
  Check(r.Stable&&!r.NeedsDeathSave&&r.DeathSuccesses==0&&r.DeathFailures==0,"three successes stabilize and reset counters");
  Check(r.ScheduleRecovery(3,1),"stable recovery time scheduled once");r.ObserveLife(602);Check(r.Hp==0,"no recovery before rolled hour");r.ObserveLife(603);Check(r.Hp==1&&!r.Stable,"stable recovery restores one HP");
  r.TakeDamage(1);r.ResolveDeathSave(10,4);r.ResolveDeathSave(10,5);r.ResolveDeathSave(10,6);r.ScheduleRecovery(604,2);r.TakeDamage(1,true);
  Check(!r.Stable&&r.DeathFailures==2&&r.RecoveryAt==-1,"critical damage breaks stability, adds two failures and cancels recovery");
  r.TakeDamage(1);r.ObserveLife(700);Check(r.Corpse.Dead&&r.Corpse.Age(702)==2,"death records independent rescue corpse clock");
  Check(r.Revive(r.MaxHp,false)&&r.CanDepart&&!r.Corpse.Dead&&r.Hp==10,"full revival clears corpse data and restores configured maximum");
  Console.WriteLine("PASS: rescue configuration, pickup action, heavy drag, dash and cumulative movement");
 }
}
