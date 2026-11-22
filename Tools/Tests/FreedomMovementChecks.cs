using System;
class FreedomMovementChecks
{
    static void C(bool ok,string text){if(!ok)throw new Exception(text);}
    static void Main()
    {
        var freedom=new FreedomMovementEffect();
        C(FreedomMovementEffect.TargetLimit(4)==1&&FreedomMovementEffect.TargetLimit(9)==6&&FreedomMovementEffect.TargetLimit(3)==0&&FreedomMovementEffect.TargetLimit(10)==0,"only slots four through nine grant one through six targets");
        C(freedom.Begin(new[]{0},4,10)&&freedom.Active(0,10)&&freedom.Active(0,609)&&!freedom.Active(0,610),"base casting lasts exactly one hour with exclusive expiry");
        C(!freedom.Active(0,9)&&!freedom.Active(-1,10)&&!freedom.Active(0,-1),"future, unselected and invalid time queries are inactive");
        C(freedom.Begin(new[]{-1,1},5,110)&&freedom.Active(-1,110)&&freedom.Active(1,709)&&!freedom.Active(-1,710),"fifth-level slot supports two independent creature identities");
        C(freedom.Active(0,609)&&!freedom.Active(0,610),"another casting leaves the original target's expiry unchanged");
        C(freedom.Begin(new[]{0,2},5,210)&&freedom.Active(0,809)&&freedom.Active(2,809)&&!freedom.Active(0,810),"recasting refreshes selected target for a fresh hour without adding old duration");
        C(freedom.Active(-1,709)&&!freedom.Active(-1,710),"refreshing one batch does not refresh another batch's creatures");
        C(freedom.Begin(new[]{3,4,5,6,7,int.MinValue},9,300)&&freedom.Active(int.MinValue,899),"ninth-level slot supports six targets including rescue actor identity");
        C(!freedom.Begin(new[]{3,4,5,6,7,8,9},9,400)&&!freedom.Active(8,400)&&freedom.Active(3,899)&&!freedom.Active(3,900),"too many targets neither partially add new creatures nor extend old ones");
        C(!freedom.Begin(new[]{3,3},5,500)&&freedom.Active(3,899)&&!freedom.Active(3,900),"duplicate targets reject the whole draft rather than bypassing the slot count");
        C(!freedom.Begin(new[]{10,10},9,500)&&!freedom.Active(10,500),"a duplicate draft cannot partially grant a new target");
        C(!freedom.Begin(null,4,500)&&!freedom.Begin(new int[0],4,500),"empty and missing target collections are rejected");
        C(!freedom.Begin(new[]{0},3,500)&&!freedom.Begin(new[]{0},10,500)&&!freedom.Begin(new[]{0},int.MinValue,500),"unsupported slots reject without changing existing timers");
        C(!freedom.Begin(new[]{0},4,-1)&&!freedom.Begin(new[]{0},4,long.MaxValue-599)&&!freedom.Begin(new[]{0},4,long.MaxValue),"negative and overflowing time is rejected atomically");
        C(freedom.Active(0,809)&&!freedom.Active(0,810),"invalid drafts preserve prior target expiry");
        C(freedom.Begin(new[]{20},4,long.MaxValue-600)&&freedom.Active(20,long.MaxValue-1)&&!freedom.Active(20,long.MaxValue),"maximum complete duration fits without overflow");
        freedom.Clear();C(!freedom.Active(0,300)&&!freedom.Active(-1,300)&&!freedom.Active(int.MinValue,500)&&!freedom.Active(20,long.MaxValue-1),"reset clears targets from all independent castings");
        C(freedom.Begin(new[]{-1},4,0)&&freedom.Active(-1,599)&&!freedom.Active(-1,600),"a new battle may start again at time zero after reset");
        Console.WriteLine("PASS: Freedom of Movement slot scaling, independent one-hour timers, refresh without stacking, duplicate/over-limit rejection, atomic drafts, overflow boundaries and reset");
    }
}
