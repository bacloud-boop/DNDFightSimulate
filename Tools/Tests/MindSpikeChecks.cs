using System;
class MindSpikeChecks
{
 static void Check(bool v,string m){if(!v)throw new Exception(m);}
 static void Main(){
  var m=new MindSpikeEffect();m.Cast(false,10);
  Check(m.Tracks(false,10)&&!m.Tracks(true,10),"only selected target is tracked");
  Check(m.IgnoresInvisibility(true,false,609)&&!m.IgnoresInvisibility(false,false,609),"invisibility loses benefit only against caster");
  Check(!m.Tracks(false,610),"one hour expires exactly");
  m.Clear();Check(!m.Tracks(false,10),"ending concentration clears tracking");
  m.Cast(true,30);Check(m.Tracks(true,31)&&!m.IgnoresInvisibility(false,true,31),"self target does not reveal player to opponent");
  Check(DirectSpell.Find("Mind Spike").DiceCount(1,2)==3&&DirectSpell.Find("Mind Spike").DiceCount(1,9)==10,"level based slot scaling");
  Console.WriteLine("PASS: mind spike target, caster-only reveal, expiration and slot scaling");
 }
}
