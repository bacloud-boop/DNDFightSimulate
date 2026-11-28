using System;
class GuardianMovementModelChecks
{
    static void C(bool ok,string text){if(!ok)throw new Exception(text);}
    static void Main()
    {
        bool inside=false;var unit=new SummonedUnit(1,"unit",20,12,30,2,1);unit.SpeedTransform=speed=>inside?speed/2:speed;unit.BeginTurn(1);C(unit.SpendMovement(10),"unit moves before entering aura");inside=true;C(unit.EffectiveSpeed==15&&unit.MovementRemaining==5,"summon preserves spent movement while speed halves");C(unit.SpendMovement(5)&&unit.MovementRemaining==0,"summon cannot exceed halved quota");inside=false;C(unit.MovementRemaining==15,"summon exiting aura restores allowance without restoring spent distance");unit.LoadSpeedLimit=5;inside=true;C(unit.EffectiveSpeed==2,"load cap applied before halving and rounded down");
        inside=false;var rescue=new RescueCarrier();C(rescue.Configure(10,2,30,150,0)&&rescue.BeginTurn(1),"rescue starts independent turn");rescue.SpeedTransform=speed=>inside?speed/2:speed;C(rescue.Move(10),"rescue starts movement");inside=true;C(rescue.EffectiveSpeed==15&&rescue.MovementRemaining==5,"rescue live speed preserves spent movement");inside=false;C(rescue.MovementRemaining==20,"rescue leaving restores allowance");
        var combat=new CombatState();combat.MonsterSpeedTransform=speed=>speed/2;C(combat.MonsterUnmodifiedSpeed==30&&combat.MonsterSpeed==15,"monster exposes raw speed separately from spell reduction");combat.MonsterSpeedBonus=-5;C(combat.MonsterUnmodifiedSpeed==25&&combat.MonsterSpeed==12,"penalties apply before half speed");combat.MonsterRestrained=true;C(combat.MonsterSpeed==0,"restraint still prevents movement");
        Console.WriteLine("PASS: real summoned/rescue/monster speed models preserve spent movement and apply load, spell halving and restraint in order");
    }
}
