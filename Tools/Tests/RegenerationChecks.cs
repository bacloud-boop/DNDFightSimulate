using System;
class RegenerationChecks
{
 static void C(bool v,string m){if(!v)throw new Exception(m);}
 static void Main()
 {
  var effects=new RegenerationEffects();C(effects.SetMissing(0,"左手；右足",0)&&effects.Begin(0,10),"manual body parts and nonconcentration effect");
  C(effects.Refresh(29).Count==0&&effects.Missing(0)!=""&&effects.Refresh(30).Count==1&&effects.Missing(0)=="","two-minute regrowth uses elapsed time, not turn count");
  C(effects.StartTurn(0,1,10)&&!effects.StartTurn(0,1,10)&&effects.StartTurn(0,2,11),"once per target turn, including unconscious turns");
  C(!effects.StartTurn(7,1,10)&&effects.Active(0,609)&&!effects.Active(0,610),"no unrelated target healing and exact one-hour lifetime");
  effects.Begin(0,100);C(effects.Begin(0,110)&&effects.StartTurn(0,3,110)&&!effects.StartTurn(0,3,110),"recasting renews duration without duplicate start pulses");
  effects.SetMissing(0,"左手",120);effects.SetMissing(0,"左手",125);C(effects.Refresh(140).Count==1,"rendering unchanged text cannot postpone regrowth");
  effects.SetMissing(0,"左手",150);effects.SetMissing(0,"双手",155);C(effects.Refresh(170).Count==0&&effects.Refresh(175).Count==1,"new manual loss starts a new two-minute repair timer");
  effects.SetMissing(-1,"尾部",0);effects.Begin(-1,0);effects.RestoreBody(-1);C(effects.Missing(-1)==""&&effects.Refresh(20).Count==0,"whole-body revival clears missing-part records without false later feedback");
  var clock=new AnimateDeadCasting(1,0);for(int round=2;round<=10;round++)C(clock.Continue(round,round-1),"each later own turn pays a Magic action");C(clock.Ready(10)&&!clock.CanContinue(11,10),"one-minute cast requires exactly ten paid actions");
  var interrupted=new AnimateDeadCasting(1,0);C(!interrupted.EndTurn(2)&&!interrupted.Ready(10),"skipping a required turn invalidates long casting");
  C(!effects.Begin(0,long.MaxValue-599)&&!effects.SetMissing(0,null,0),"overflow and invalid text do not mutate state");
  effects.Clear();C(effects.Missing(0)==""&&!effects.Active(0,110),"reset clears effects, records and per-turn stamps");
  Console.WriteLine("PASS: Regenerate one-hour lifetime, target-turn pulse deduplication, manual missing parts, two-minute regrowth, renewal, whole-body restoration and ten-action casting");
 }
}
