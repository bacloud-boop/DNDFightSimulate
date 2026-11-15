using System;
class PolymorphChecks
{
 static void Check(bool v,string m){if(!v)throw new Exception(m);}
 static void Main(){
  var effect=new PolymorphEffect();var scores=new[]{12,15,11,8,3,6};var saves=new[]{1,2,0,-1,-4,-2};var skills=new int[18];for(int i=0;i<18;i++)skills[i]=-999;skills[12]=3;
  Check(PolymorphEffect.AllowedChallenge(1,1)&&!PolymorphEffect.AllowedChallenge(2,1)&&PolymorphEffect.AllowedChallenge(0,0),"exact fractional CR limits");
  effect.Begin(true,"test",20,12,40,60,0,2,scores,saves,skills);scores[0]=30;saves[0]=20;skills[12]=20;
  Check(effect.Ability(0)==12&&effect.Ability(4)==3&&effect.Save(0)==1&&effect.Skill(12,3)==3,"replace all abilities and use independent beast proficiency data");
  Check(effect.Skill(0,0)==1&&effect.Skill(1,1)==2,"untrained checks use beast ability");
  Check(!effect.ShouldEnd(619,1,false)&&effect.ShouldEnd(620,1,false)&&effect.ShouldEnd(21,0,false)&&effect.ShouldEnd(21,5,true),"expiry and temporary HP end conditions");
  bool invalid=false;try{effect.Begin(false,"bad",30,10,30,0,0,5,scores,saves,skills);}catch(ArgumentException){invalid=true;}
  Check(invalid&&effect.TargetPlayer&&effect.Form=="test"&&effect.Expires==620,"bad replacement leaves current effect intact");
  effect.Clear();Check(!effect.Active&&!effect.ShouldEnd(999,0,true),"clear releases temporary layer");
  Console.WriteLine("PASS: polymorph CR, stat snapshots, beast checks, expiry and atomic replacement");
 }
}
