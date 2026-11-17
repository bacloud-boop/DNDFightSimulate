using System;
class DamageReductionChecks
{
 static void Check(bool value,string message){if(!value)throw new Exception(message);}
 static void Main(){
  var raw=new long[13];raw[5]=6;raw[0]=4;var allocation=new DamageReductionAllocation(raw,8);raw[5]=99;
  Check(allocation.Required==8&&allocation.Damage(5)==6,"snapshot and required deduction");
  var split=new int[13];split[5]=6;split[0]=1;long[] result;
  Check(allocation.TryAssign(split)&&!allocation.TryCommit(out result),"partial cannot commit");
  split[0]=3;Check(!allocation.TryAssign(split)&&allocation.Assigned==7,"over budget atomic");
  split[0]=2;Check(allocation.TryAssign(split)&&allocation.TryCommit(out result)&&result[0]==2&&result[5]==0,"manual split");
  split[0]=-1;Check(!allocation.TryAssign(split),"negative rejected");
  var low=new long[13];low[0]=-4;low[5]=6;var limited=new DamageReductionAllocation(low,8);split=new int[13];split[5]=6;
  Check(limited.Required==6&&limited.TryAssign(split)&&limited.TryCommit(out result)&&result[0]==0&&result[5]==0,"typed floor and penalty capped by total");
  var huge=new long[13];huge[0]=huge[5]=long.MaxValue;Check(new DamageReductionAllocation(huge,8).Required==8,"sum overflow safe");
  Check(new DamageReductionAllocation(new long[13],8).Complete,"zero damage needs no modal");
  Console.WriteLine("PASS: mandatory allocation, atomic edits, per-type limits, typed floor, snapshots and overflow");
 }
}
