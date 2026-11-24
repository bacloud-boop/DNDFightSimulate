using System;
using System.Linq;
class SearingSmiteChecks
{
 static void C(bool value,string text){if(!value)throw new Exception(text);}
 static void Main()
 {
  for(int slot=1;slot<=9;slot++)C(SearingSmiteEffects.Dice(slot)==slot&&SearingSmiteEffects.Dice(slot,true)==slot*2,"initial critical doubles only its own extra dice");
  var effects=new SearingSmiteEffects();var weak=effects.Add(-1,1,20,0);var strong=effects.Add(-1,4,12,1);var equal=effects.Add(-1,4,18,2);
  C(effects.Strongest(-1,2).Id==equal.Id&&effects.Pending(-1,2).Count==3,"highest ongoing dice, then DC, dominate without deleting older spells");
  C(effects.ClaimStart(-1,1,2)&&!effects.ClaimStart(-1,1,2)&&effects.ClaimStart(-1,2,3),"one ongoing damage event per target turn even with multiple casts");
  C(effects.End(equal.Id)&&effects.Strongest(-1,3).Id==strong.Id&&effects.Pending(-1,3).Any(e=>e.Id==weak.Id),"successful save can end any independent cast and reveal an older spell");
  var latest=effects.Add(-1,4,12,3);C(effects.Strongest(-1,3).Id==latest.Id,"equal potency uses most recent casting");
  effects.Add(7,2,14,3);C(effects.Strongest(7,3).Dice==2&&effects.ClaimStart(7,2,3),"different targets keep separate once-per-turn identities");
  var none=new SearingSmiteEffects();C(!none.ClaimStart(-1,3,1),"empty start still records the turn");none.Add(-1,1,10,1);C(!none.ClaimStart(-1,3,1),"creating a new effect midturn cannot retroactively trigger a repeated start callback");
  effects.Refresh(12);C(effects.Pending(-1,12).Count==1&&effects.Pending(-1,12)[0].Id==latest.Id,"each cast has its own one-minute expiry");
  C(effects.Add(-1,0,10,0)==null&&!effects.CanAdd(long.MaxValue-9),"invalid slots and overflow leave state unchanged");
  effects.Clear();C(effects.Pending(-1,3).Count==0&&!effects.ClaimStart(-1,1,3),"reset clears spells and turn identity");
  Console.WriteLine("PASS: Searing Smite slot/critical dice, nonstacking potency, independent suppressed spells/saves/expiry, per-target turn claim and reset");
 }
}
