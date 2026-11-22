using System;
class AnimateDeadControlChecks
{
    static void C(bool value,string message){if(!value)throw new Exception(message);}
    static void Main()
    {
        var control=new AnimateDeadControl();
        C(control.Register(new[]{1},3,10)&&control.Controlled(1,14409)&&!control.Controlled(1,14410),"control lasts exactly twenty-four hours after completion");
        int payments=0;C(control.Command(new[]{1},1,11,id=>true,()=>{payments++;return true;})&&payments==1&&control.OrderFor(1,10000)==1,"a paid mental order persists across turns without repeated payment");
        C(control.Neutral(1,14410)&&control.OrderFor(1,14410)==0&&!control.Command(new[]{1},2,14410,id=>true,()=>{payments++;return true;})&&payments==1,"expiry leaves a registered neutral creature which rejects orders before any action is spent");
        var same=control.Find(1);C(control.Refresh(14410).Count==1&&control.Refresh(14411).Count==0&&ReferenceEquals(same,control.Find(1))&&!same.Destroyed,"expiry clears old orders exactly once and never dismisses the creature");
        C(!control.Reassert(new[]{1},3,14420,id=>false)&&control.Neutral(1,14420),"out-of-range recovery leaves expiry untouched");
        C(control.Reassert(new[]{1},3,14420,id=>true)&&control.Controlled(1,28819)&&control.OrderFor(1,14420)==0,"recast restores control for a fresh day without restoring stale orders");
        C(control.Command(new[]{1},2,14421,id=>true,()=>true)&&control.Reassert(new[]{1},3,15000,id=>true)&&control.OrderFor(1,15000)==2,"early renewal preserves the current instruction");
        C(!control.Register(new[]{2,3},3,0)&&control.Find(2)==null,"base creation quota is one and invalid batches are atomic");
        C(control.Register(new[]{2,3,4},4,0)&&!control.Register(new[]{5,6,7,8},4,0)&&control.Find(5)==null,"higher slots create two extra creatures per level");
        C(!control.Register(new[]{5,5},4,0)&&!control.Register(new[]{1},3,0)&&!control.Register(new[]{0},3,0),"duplicate, reused and primary actor IDs are rejected");
        C(control.Reassert(new[]{1,2,3,4},3,20,id=>true),"base renewal quota is four rather than one");
        long old=control.Find(2).ControlExpires;C(!control.Reassert(new[]{2,99},3,30,id=>true)&&control.Find(2).ControlExpires==old,"missing renewal target cannot partially extend other targets");
        payments=0;C(control.Command(new[]{1,2,3,4},1,21,id=>true,()=>{payments++;return true;})&&payments==1,"one bonus action commands an entire group");
        C(!control.Command(new[]{1,2},2,22,id=>id!=2,()=>{payments++;return true;})&&payments==1&&control.OrderFor(1,22)==1,"an out-of-range group is rejected without changing any order or consuming a bonus action");
        C(!control.Command(new[]{1},2,22,id=>true,()=>false)&&control.OrderFor(1,22)==1,"unavailable bonus action leaves existing orders intact");
        C(control.Destroy(2)&&!control.Controlled(2,22)&&!control.Neutral(2,22)&&!control.Reassert(new[]{2},3,22,id=>true),"destroyed undead cannot act, be treated as neutral living units, or recover control");
        C(!control.Register(new[]{9},3,long.MaxValue-14400+1)&&!control.Reassert(new[]{1},3,-1,id=>true),"invalid world times do not overflow the control deadline");
        for(int slot=3;slot<=9;slot++)C(AnimateDeadControl.CreationLimit(slot)==1+(slot-3)*2&&AnimateDeadControl.RenewalLimit(slot)==4+(slot-3)*2,"slot quotas cover all legal levels");
        control.Clear();C(control.Find(1)==null&&!control.Neutral(1,100),"reset clears control records without retaining actor IDs");
        var casting=new AnimateDeadCasting(1,0);C(casting.EndTurn(1)&&!casting.Ready(10)&&!casting.CanContinue(1,0),"starting Magic action counts once and elapsed time alone cannot complete the cast");
        C(!casting.EndTurn(2)&&!casting.Continue(3,2),"missing a turn's Magic action invalidates continuation without skipping a paid turn");
        for(int i=1;i<10;i++)C(casting.Continue(i+1,i)&&casting.EndTurn(i+1),"one consecutive Magic action per turn");
        C(!casting.Ready(9)&&casting.Ready(10)&&!casting.Continue(11,10),"ten actions and the full minute are both required; no eleventh action is charged");
        Console.WriteLine("PASS: Animate Dead exact control expiry, persistent group mental orders, neutral loss of control, atomic renewed control and quotas, resource payment and overflow");
    }
}
