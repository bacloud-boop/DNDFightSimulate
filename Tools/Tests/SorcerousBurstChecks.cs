using System;
class SorcerousBurstChecks
{
 static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
 static void Main(){
  Check(SorcerousBurstRules.NextCount(new[]{1,7},0,3,false)==0,"only eights trigger");
  Check(SorcerousBurstRules.NextCount(new[]{8,8},0,3,false)==2,"independent triggers");
  Check(SorcerousBurstRules.NextCount(new[]{8,8},2,3,false)==1,"shared cap across all dice");
  Check(SorcerousBurstRules.NextCount(new[]{8,8},0,3,true)==4,"critical doubles each trigger");
  Check(SorcerousBurstRules.NextCount(new[]{8,8,8,8},4,3,true)==2,"critical total extra cap is doubled not recursive exponential growth");
  Check(SorcerousBurstRules.NextCount(new[]{8,8},6,3,true)==0,"explosion stops at cap even on eights");
  Check(SorcerousBurstRules.NextCount(new[]{8},0,-2,true)==0,"negative casting modifier never adds dice");
  Console.WriteLine("PASS: burst triggers, chained rolls, shared cap and confirmed critical doubling");
 }
}
