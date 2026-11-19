using System;
class SuffocationRevivalChecks
{
 static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
 static void Main(){
  var breath=new BreathingState();breath.Observe(false,0,10,true);for(int i=0;i<6;i++)breath.EndTurn(i,i);
  var combat=new CombatState();combat.ApplyFatalExhaustion(true,breath.SuffocationExhaustion);
  var corpse=new CorpseClock();corpse.Observe(combat.PlayerDead,9,5);
  Check(combat.PlayerDead&&breath.Fatal,"fatal suffocation kills actor");
  Check(combat.Revive(true,1,false),"revival spell resolves before persistent condition");corpse.Clear();
  int guardCalls=0;combat.DeathWardGuard=player=>{guardCalls++;return true;};
  Check(!breath.ReconcileRevival(false)&&breath.SuffocationExhaustion==6,"revival in unbreathable environment preserves lethal exhaustion");
  combat.ApplyFatalExhaustion(true,breath.SuffocationExhaustion);corpse.Observe(combat.PlayerDead,9,20);
  Check(combat.PlayerDead&&combat.PlayerHp==0&&guardCalls==0,"retained condition immediately kills again without treating it as new exhaustion increment");
  Check(corpse.DeathAt==20&&corpse.Age(20)==0,"new death creates a new corpse clock");
  Check(combat.Revive(true,1,false)&&breath.ReconcileRevival(true),"revival after restoring breathable environment succeeds");
  Check(!combat.PlayerDead&&combat.PlayerHp==1&&!breath.Fatal&&breath.SuffocationExhaustion==0,"normal revival clears suffocation but does not heal beyond spell amount");
  Check(!combat.ApplyFatalExhaustion(true,5)&&!combat.PlayerDead,"nonlethal exhaustion cannot kill");
  Console.WriteLine("PASS: persistent suffocation, immediate re-death, fresh corpse clock and breathable revival");
 }
}
