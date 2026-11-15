using System;
class ShillelaghChecks
{
 static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
 static void Main(){
  var effect=new ShillelaghEffect();Check(!effect.Active&&!effect.Applies("短棒",0),"inactive by default");
  Check(effect.Begin("短棒",3,100)&&effect.Ability==3&&effect.Expires==110,"successful cast targets held club and lasts ten rounds");
  Check(effect.Applies("短棒",109)&&!effect.Applies("长棍",109)&&!effect.Applies("短棒",110),"exact weapon and expiry boundary");
  Check(!effect.Begin("巨棒",3,101)&&effect.Weapon=="短棒"&&effect.Expires==110,"greatclub is not club; invalid recast preserves old effect");
  Check(!effect.Begin("长棍",6,101)&&!effect.Begin("长棍",3,long.MaxValue)&&effect.Expires==110,"invalid ability or overflowing clock is atomic");
  Check(!effect.Refresh("短棒",false,109)&&effect.Active,"same weapon click does not count as letting go");
  Check(effect.Refresh("长剑（双手）",false,109)&&!effect.Active&&!effect.Applies("短棒",109),"switching away ends effect permanently rather than suspending it");
  effect.Begin("长棍",3,200);Check(effect.Refresh("长棍",true,201)&&!effect.Active,"melding weapon ends effect");
  effect.Begin("短棒",3,300);Check(effect.Begin("长棍",3,305)&&effect.Weapon=="长棍"&&effect.Expires==315,"recast replaces rather than stacks durations");
  Check(effect.Refresh("长棍",false,315)&&!effect.Active,"expiry clears effect");
  int[] levels={1,4,5,10,11,16,17,20},sides={8,8,10,10,12,12,6,6};
  for(int i=0;i<levels.Length;i++){Check(ShillelaghEffect.Sides(levels[i])==sides[i],"total level die boundary");int count=levels[i]>=17?2:1;Check(ShillelaghEffect.Count(levels[i])==count&&ShillelaghEffect.Count(levels[i],true)==2*count,"critical doubles the complete replacement dice pool");}
  effect.Clear();Check(!effect.Active,"reset clears effect");
  Console.WriteLine("PASS: eligible weapons, replacement, letting go, melding, duration, total-level scaling and critical dice");
 }
}
