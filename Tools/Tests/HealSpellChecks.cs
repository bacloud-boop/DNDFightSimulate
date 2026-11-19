using System;
class HealSpellChecks
{
 static void C(bool ok,string message){if(!ok)throw new Exception(message);}
 static void Main(){
  var c=new CombatState();var sensory=new SensoryAfflictions();
  c.ApplyDamage(300);c.ApplyPoison(false);c.MonsterParalyzed=true;c.MonsterSleepStage=2;
  sensory.Add(false,true,20,0);sensory.Add(false,true,15,0);sensory.Add(false,false,10,0);
  C(HealRules.Apply(c,sensory,false,6,1)==70&&c.Hp==770,"sixth-level fixed healing, no ability modifier");
  C(sensory.Pending(false,1).Count==0&&!c.MonsterPoisoned,"all blind/deaf sources and poison removed");
  C(c.MonsterParalyzed&&c.MonsterSleepStage==2,"unrelated paralysis and sleep retained");
  c.BlockHealing(false);c.ApplyPoison(false);sensory.Add(false,true,20,1);
  C(HealRules.Apply(c,sensory,false,9,2)==0&&c.Hp==770&&!c.MonsterPoisoned&&!sensory.Blinded(false,2),"healing prevention does not block separate condition removal");
  var full=new CombatState();full.ApplyPoison(false);C(HealRules.Apply(full,sensory,false,7,2)==0&&!full.MonsterPoisoned,"full health still receives condition removal");
  c.ApplyDamage(int.MaxValue);sensory.Add(false,true,20,2);C(HealRules.Apply(c,sensory,false,6,3)==0&&c.Hp==0&&sensory.Blinded(false,3),"dead target neither healed nor altered");
  C(HealRules.Amount(7)==80&&HealRules.Amount(9)==100,"upcast fixed totals");
  Console.WriteLine("PASS: fixed healing, suppressed conditions, healing block, full HP, unrelated effects and dead targets");
 }
}
