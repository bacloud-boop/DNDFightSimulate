using System;
using UnityEngine;
class FeignDeathChecks
{
    static void C(bool ok,string message){if(!ok)throw new Exception(message);}
    static void Main()
    {
        var effect=new FeignDeathEffect();
        C(effect.Cast(0,7)&&effect.Active(0,606)&&!effect.Active(0,607)&&effect.Remaining(0,7)==600,"one hour uses world time, with an exclusive expiry");
        C(effect.Cast(-1,8)&&effect.EndBlind(0,9)&&!effect.Blind(0,9)&&effect.Active(0,9)&&effect.Blind(-1,9),"ending blindness leaves the other effects and actors intact");
        C(!effect.Cast(0,long.MaxValue-599)&&effect.Active(0,9),"invalid time cannot replace a valid effect");
        var combat=new CombatState{PlayerFeignDeath=true,MonsterFeignDeath=true};
        C(!combat.PlayerCanAct&&!combat.TryUseAction()&&!combat.TryUseBonusAction()&&!combat.MonsterCanAct&&combat.MonsterSpeed==0,"incapacitation and speed zero without death");
        C(!combat.PlayerDead&&!combat.BattleFinished&&!combat.NeedsDeathSave&&!combat.PlayerProne&&!combat.MonsterHelpless&&!combat.PlayerParalyzed,"feigning is neither death, unconsciousness, paralysis nor forced prone");
        for(int i=0;i<13;i++)C(combat.AdjustTypedDamage(true,9,i)==(i==10?9:4),"only psychic damage bypasses the spell resistance");
        combat.PlayerEnergyResistance=5;combat.PlayerUnderwater=true;C(combat.AdjustTypedDamage(true,9,5)==4,"overlapping resistance is applied once");
        combat.ApplyPoison(true);C(!combat.PlayerPoisoned,"poisoned immunity does not mean poison damage immunity");
        var sensory=new SensoryAfflictions();sensory.Add(true,true,12,9);sensory.Add(true,true,13,9);effect.Cast(0,9);
        combat.ExtraConditionRemoval=(player,condition,now)=>condition==0&&effect.EndBlind(player?0:-1,now)?1:0;
        C(RestorationRules.EndCondition(combat,sensory,true,0,9)==3&&!effect.Blind(0,9)&&effect.Active(0,9),"restoration ends all blindness sources, preserving feigned incapacitation");
        effect.Cast(0,10);combat.PlayerFeignDeath=false;HealRules.Apply(combat,sensory,true,6,10);C(!effect.Blind(0,10),"Heal clears blindness even at full HP");
        effect.Cast(0,11);MassHealRules.Apply(combat,sensory,true,5,11);C(effect.Blind(0,11),"Mass Heal at full HP cannot clear blindness");
        combat.ApplyPlayerDamage(2);MassHealRules.Apply(combat,sensory,true,5,11);C(!effect.Blind(0,11),"Mass Heal clears blindness after actual healing");
        var unit=new SummonedUnit(1,"spirit",30,12,30,2,2);unit.BeginTurn(1);unit.Dodge();unit.SetFeignDeath(true,true);
        C(unit.Alive&&!unit.Dodging&&unit.MovementRemaining==0&&!unit.Attack()&&!unit.UseReaction()&&!unit.Dash(),"living summons lose Dodge and cannot act or react");
        C(unit.Damage(9,9)==4&&unit.Damage(9,10)==9,"summons resist poison damage but not psychic damage");
        var map=new BattleMap();map.EnemyEligible=id=>!effect.Active(id,11);
        C(map.NearestEnemyInReach(true,int.MaxValue)==-1,"a lone feigned player is ignored by target selection");
        map.Lock();var ally=new SummonedUnit(2,"ally",20,12,30,2,1);C(map.AddSummon(ally,new Vector2Int(1,0)),"summon target fixture");
        C(map.NearestEnemyInReach(true,int.MaxValue)==2,"the monster redirects to an available nonfeigned ally");
        effect.Cast(2,11);C(map.NearestEnemyInReach(true,int.MaxValue)==-1,"all feigned enemies leave no attack target");
        var clock=new CorpseClock();clock.Observe(combat.PlayerDead,9,11);C(!clock.Dead,"feigning cannot start a corpse clock");
        var rescuer=new RescueCarrier();rescuer.BeginTurn(1);var weight=new BodyWeight(150,0);
        C(rescuer.PickUp(0,effect.Active(0,11),weight,false)&&rescuer.ActionsRemaining==0,"carrying a feigned body uses the existing action and weight rules");
        rescuer.FeignDeath=true;C(!rescuer.Conscious&&!rescuer.Dead&&rescuer.Hp>0&&!rescuer.Move(5)&&!rescuer.TryUseReaction(),"rescuers also stay alive but cannot act");
        C(rescuer.TakeDamage(9,false,9)==4,"rescuers share nonpsychic resistance");
        effect.Clear();combat.ResetBattle();C(!effect.Active(0,11)&&combat.PlayerCanAct&&!combat.PlayerFeignDeath&&!combat.MonsterFeignDeath,"reset clears feigned status and resources");
        Console.WriteLine("PASS: Feign Death duration, independent blindness, real life state, typed resistance, immunity, restoration, action gates, target exclusion and carrying");
    }
}
