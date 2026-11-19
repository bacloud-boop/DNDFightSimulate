using System;
class RidingChecks
{
    static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
    static void Main()
    {
        var riding=new RidingState();int spent=0;
        Check(!riding.TryMount(1,2,2,true,true,()=>{spent++;return true;})&&spent==0,"size-ineligible mounting costs nothing");
        Check(riding.TryMount(1,2,3,true,true,()=>{spent++;return true;})&&spent==1,"legal mount spends once");
        Check(riding.NeedsForcedDismount(3,3,true),"enlarging rider triggers dismount instead of rejecting transformation");
        Check(!riding.TryDismount(()=>false)&&riding.Mounted,"failed placement retains pending mounted identity");
        int x,y;Check(DismountPlacement.FindNearest(10,4,4,2,2,(a,b)=>a>=6,out x,out y)&&x==6&&y==4,"nearest full-footprint placement after transformation");
        Check(riding.TryDismount(()=>true)&&!riding.Mounted,"successful forced dismount clears shared state");
        Check(!DismountPlacement.FindNearest(10,4,4,2,1,(a,b)=>false,out x,out y),"no legal space never fabricates overlap");
        Console.WriteLine("PASS: mounting cost, changed-size dismount, nearest legal placement and atomic failures");
    }
}
