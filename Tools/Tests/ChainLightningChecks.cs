using System;
using System.Collections.Generic;
class ChainLightningChecks
{
 static void C(bool ok,string m){if(!ok)throw new Exception(m);}
 static void Main(){
  var alive=new HashSet<int>{0,-1,1,2,3,4,5,6};var visible=new HashSet<int>{0,-1};
  Func<int,int,int> distance=(a,b)=>a==b?0:a==0||b==0?150:30;
  C(ChainLightningRules.Valid(-1,new[]{1,2,3},6,alive.Contains,visible.Contains,distance),"three secondary targets at 30 feet need not be visible");
  C(!ChainLightningRules.Valid(-1,new[]{1,2,3,4},6,alive.Contains,visible.Contains,distance),"base slot limits secondary bolts");
  C(ChainLightningRules.Valid(-1,new[]{1,2,3,4,5,6},9,alive.Contains,visible.Contains,distance),"ninth slot adds three secondary targets");
  C(!ChainLightningRules.Valid(-1,new[]{-1},6,alive.Contains,visible.Contains,distance)&&!ChainLightningRules.Valid(-1,new[]{1,1},6,alive.Contains,visible.Contains,distance),"no creature can receive two bolts");
  C(!ChainLightningRules.Valid(1,new[]{2},6,alive.Contains,visible.Contains,distance),"initial target must be visible");
  C(!ChainLightningRules.Valid(-1,new[]{99},6,alive.Contains,visible.Contains,distance),"missing target rejected");
  C(!ChainLightningRules.Valid(-1,new[]{1},6,alive.Contains,visible.Contains,(a,b)=>a==0?151:30),"initial range enforced");
  C(!ChainLightningRules.Valid(-1,new[]{1},6,alive.Contains,visible.Contains,(a,b)=>a==0?150:31),"jump range measured from first target");
  C(ChainLightningRules.Valid(0,new int[0],6,alive.Contains,visible.Contains,distance),"self initial target and no additional targets allowed");
  Console.WriteLine("PASS: primary sight/range, hidden secondary targets, slot count, no repeated bolts and invalid targets");
 }
}
