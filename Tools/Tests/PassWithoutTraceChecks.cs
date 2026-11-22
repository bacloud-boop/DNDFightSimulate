using System;
class PassWithoutTraceChecks
{
    static void C(bool v,string message){if(!v)throw new Exception(message);}
    static void Main()
    {
        var e=new PassWithoutTraceEffect();C(!e.Active(0),"initial inactive");
        C(e.Begin(new[]{-1,5,int.MinValue},0),"all creature identities");
        C(e.CheckBonus(0,1,3,true,0)==10&&e.CheckBonus(-1,1,3,true,599)==10,"caster and chosen creature receive flat stealth bonus");
        C(e.CheckBonus(4,1,3,true,0)==0&&e.CheckBonus(0,1,3,false,0)==0,"selection and moving range gate independently");
        C(e.CheckBonus(0,1,-1,true,0)==0&&e.CheckBonus(0,0,3,true,0)==0&&e.CheckBonus(0,1,2,true,0)==0,"no generic dexterity, alternative-ability stealth or other skill bonus");
        C(e.Protects(5,true,1)&&!e.Protects(5,false,1)&&e.Protects(5,true,2),"leaving and returning never consumes or renews effect");
        C(!e.Begin(new[]{1,1},1)&&!e.Begin(null,1)&&!e.Begin(new int[0],-1)&&!e.Begin(new int[0],long.MaxValue-599),"invalid recasts atomic");
        C(e.Protects(5,true,599)&&!e.Protects(5,true,600)&&!e.Active(-1),"exact one-hour expiry and invalid clock");
        C(e.Begin(new int[0],10)&&e.Protects(0,true,10)&&!e.Protects(5,true,10),"recast replaces targets and always includes caster");
        C(e.Begin(new[]{7},long.MaxValue-600)&&e.Active(long.MaxValue-1)&&!e.Active(long.MaxValue),"maximum representable full duration");
        e.Clear();C(!e.Active(0)&&!e.Protects(0,true,long.MaxValue-1),"concentration ending and reset clear every benefit");
        Console.WriteLine("PASS: Pass without Trace selection, exact duration, moving aura exit/reentry, Dex Stealth-only bonus, atomic recast, overflow and concentration cleanup");
    }
}
