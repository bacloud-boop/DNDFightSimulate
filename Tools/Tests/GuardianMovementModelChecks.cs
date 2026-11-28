using System;
class GuardianMovementModelChecks
{
    static void C(bool ok,string text){if(!ok)throw new Exception(text);}
    static void Main()
    {
        bool inside=false;var unit=new SummonedUnit(1,"unit",20,12,30,2,1);unit.SpeedTransform=speed=>inside?speed/2:speed;unit.BeginTurn(1);C(unit.SpendMovement(10),"unit moves before entering aura");inside=true;C(unit.EffectiveSpeed==15&&unit.MovementRemaining==5,"summon preserves spent movement while speed halves");C(unit.SpendMovement(5)&&unit.MovementRemaining==0,"summon cannot exceed halved quota");inside=false;C(unit.MovementRemaining==15,"summon exiting aura restores allowance without restoring spent distance");unit.LoadSpeedLimit=5;inside=true;C(unit.EffectiveSpeed==2,"load cap applied before halving and rounded down");
        inside=false;var rescue=new RescueCarrier();C(rescue.Configure(10,2,30,150,0)&&rescue.BeginTurn(1),"rescue starts independent turn");rescue.SpeedTransform=speed=>inside?speed/2:speed;C(rescue.Move(10),"rescue starts movement");inside=true;C(rescue.EffectiveSpeed==15&&rescue.MovementRemaining==5,"rescue live speed preserves spent movement");inside=false;C(rescue.MovementRemaining==20,"rescue leaving restores allowance");
        var combat=new CombatState();combat.MonsterSpeedTransform=speed=>speed/2;C(combat.MonsterUnmodifiedSpeed==30&&combat.MonsterSpeed==15,"monster exposes raw speed separately from spell reduction");combat.MonsterSpeedBonus=-5;C(combat.MonsterUnmodifiedSpeed==25&&combat.MonsterSpeed==12,"penalties apply before half speed");combat.MonsterRestrained=true;C(combat.MonsterSpeed==0,"restraint still prevents movement");
        bool blocked=true;var poisonedUnit=new SummonedUnit(9,"poisoned",20,12,30,2,2);poisonedUnit.BeginTurn(1);poisonedUnit.ActionBlock=()=>blocked;C(!poisonedUnit.Attack()&&!poisonedUnit.TryUseBonus()&&poisonedUnit.CanSpendMovement(5)&&poisonedUnit.UseReaction(),"summon action block preserves movement and reaction");blocked=false;C(poisonedUnit.Attack(),"cured summon can spend preserved action");blocked=true;C(!poisonedUnit.Attack(),"ongoing multiattack cannot bypass action block");rescue.ActionBlock=()=>blocked;C(!rescue.UseAction()&&!rescue.TryUseBonusAction()&&rescue.CanMove(5),"rescue loses actions but keeps movement");blocked=false;C(rescue.ActionsRemaining==1&&rescue.BonusActionAvailable,"curing restores unused rescue resources");
        var ordinary=new CombatState();ordinary.ApplyPoison(true);C(ordinary.PlayerPoisoned&&ordinary.ActionAvailable&&ordinary.BonusActionAvailable,"ordinary poison does not acquire cloud-only action restriction");
        Console.WriteLine("PASS: real summoned/rescue/monster speed models preserve spent movement and apply load, spell halving and restraint in order");
    }
}
