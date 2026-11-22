using System;
class AuraVitalityChecks
{
    static void C(bool ok,string text){if(!ok)throw new Exception(text);}
    static void Main()
    {
        var aura=new AuraVitalityEffect();
        C(AuraVitalityEffect.Duration==10&&AuraVitalityEffect.RadiusFeet==30&&AuraVitalityEffect.HealingDice==2&&AuraVitalityEffect.HealingSides==6,"fixed one-minute thirty-foot aura heals two d6 with no upcasting multiplier");
        C(!aura.Active(0)&&!aura.ClaimStart(1,0),"uncast aura has no turn-start healing");
        C(aura.Begin(0,0)&&aura.Revision==1&&aura.Expires==10&&aura.Active(0)&&aura.Active(9)&&!aura.Active(10),"casting lasts exactly ten rounds with an exclusive expiry");
        C(!aura.Active(-1)&&!aura.ClaimStart(-1,0)&&!aura.ClaimStart(0,0),"creation heal cannot also trigger the same casting-turn start");
        C(aura.ClaimStart(1,1)&&!aura.ClaimStart(1,1)&&!aura.ClaimStart(0,1),"only one healing opportunity for each new caster turn and no stale turn replay");
        C(aura.ClaimStart(2,1)&&!aura.ClaimStart(2,2),"claim reserves the opportunity even if the runtime chooses to skip healing");
        C(!aura.ClaimStart(5,-1)&&aura.ClaimStart(3,3),"an invalid time claim cannot advance the last granted turn");
        C(!aura.ClaimStart(20,10)&&aura.ClaimStart(4,4),"expired claim cannot reserve a later turn token");
        C(aura.ClaimStart(9,9)&&!aura.Active(10)&&aura.Expires==10&&aura.Revision==1,"healing claims do not renew the minute or alter the casting revision");
        long revision=aura.Revision;
        C(!aura.Begin(-1,10)&&!aura.Begin(5,-1)&&!aura.Begin(long.MaxValue-9,10)&&!aura.Begin(long.MaxValue,10),"invalid times and turn tokens reject before mutation");
        C(aura.Revision==revision&&aura.Expires==10&&aura.Active(9)&&!aura.ClaimStart(9,9),"rejected recasts preserve the whole original effect and its used healing opportunity");
        C(aura.Begin(20,20)&&aura.Revision==revision+1&&aura.Expires==30&&!aura.Active(19)&&aura.Active(20),"valid recast replaces the old concentration effect with a fresh minute and revision");
        C(!aura.ClaimStart(20,20)&&aura.ClaimStart(21,21)&&!aura.ClaimStart(21,21),"recast again excludes the creation turn and permits later once-only healing");
        C(aura.Begin(long.MaxValue-10,long.MaxValue-1)&&aura.Active(long.MaxValue-1)&&!aura.Active(long.MaxValue)&&aura.ClaimStart(long.MaxValue,long.MaxValue-1),"last complete duration and highest positive turn token do not overflow");
        revision=aura.Revision;aura.Clear();
        C(aura.Revision==revision+1&&aura.Expires==0&&!aura.Active(long.MaxValue-1)&&!aura.ClaimStart(long.MaxValue,0),"concentration ending or reset clears the aura and invalidates queued revision work");
        C(aura.Begin(0,0)&&!aura.ClaimStart(0,0)&&aura.ClaimStart(1,1),"new battle can restart world time and caster turns from zero");
        Console.WriteLine("PASS: Aura of Vitality fixed healing, exact duration, creation/start-turn gating, optional skipped claims, recast revision, atomic invalid input, overflow boundaries and reset");
    }
}
