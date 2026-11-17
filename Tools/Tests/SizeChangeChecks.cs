using System;
class SizeChangeChecks
{
 static void Check(bool value,string message){if(!value)throw new Exception(message);}
 static void Main(){
  Check(SizeChangeEffect.ChangedSize(5,1)==-1&&SizeChangeEffect.ChangedSize(0,-1)==-1,"size boundaries reject");
  for(int size=0;size<5;size++)Check(SizeChangeEffect.ChangedSize(size,1)==size+1,"enlarge one category");
  for(int size=1;size<=5;size++)Check(SizeChangeEffect.ChangedSize(size,-1)==size-1,"reduce one category");
  var effect=new SizeChangeEffect();effect.Begin(true,-1,100);
  Check(effect.Applies(true)&&!effect.Applies(false)&&effect.Expires==110,"target and duration");
  Check(SizeChangeEffect.ChangedSize(0,effect.Delta)==-1,"reduced tiny wild shape forbidden");
  Check(SizeChangeEffect.ChangedSize(3,effect.Delta)==2,"modifier follows wild shape size");
  effect.Clear();Check(!effect.Applies(true),"concentration clears");
  Check(CarryingRules.DragSpeed(10,0,70,30)==30,"tiny capacity includes equipment only");
  Check(CarryingRules.DragSpeed(10,0,100,30)==5,"temporary overload slows to five");
  Check(CarryingRules.DragSpeed(10,0,151,30)==0,"beyond drag limit immobilizes");
  Check(CarryingRules.DragSpeed(10,2,151,30)==5&&CarryingRules.DragSpeed(10,3,151,30)==30,"size changes capacity without changing mass");
  var raw=new long[13];raw[1]=3;raw[5]=2;var allocation=new DamageReductionAllocation(raw,8,1);var split=new int[13];split[1]=3;split[5]=1;long[] result=null;
  Check(allocation.Required==4&&allocation.TryAssign(split)&&allocation.TryCommit(out result)&&result[5]==1,"reduction preserves one total before resistance");
  raw=new long[13];var zero=new DamageReductionAllocation(raw,4,1);Check(zero.Required==0,"no damage created from zero");
  var one=new long[13];one[0]=1;Check(new DamageReductionAllocation(one,4,1).Required==0,"one damage cannot be reduced");
  Console.WriteLine("PASS: size limits, shape inheritance, duration, temporary overload, unchanged weight and damage floor");
 }
}
