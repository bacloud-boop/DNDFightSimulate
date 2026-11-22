using System;
using System.Collections.Generic;
class HellishRebukeChecks
{
    static void C(bool ok,string text){if(!ok)throw new Exception(text);}
    static void Main()
    {
        var combat=new CombatState();var sources=new List<int?>();var amounts=new List<int>();
        combat.DamageReceived=(player,source,amount)=>{if(player){sources.Add(source);amounts.Add(amount);}};
        combat.ApplyPlayerDamage(2);C(sources[0]==-1&&amounts[0]==2,"legacy dummy attacks identify the dummy");
        combat.ApplyPlayerDamage(2,sourcePlayer:true);C(sources[1]==0,"self-inflicted spell damage identifies caster rather than dummy");
        combat.ApplyPlayerDamage(2,sourcePlayer:null,sourceActor:7);C(sources[2]==7,"summoned creature damage preserves its identity");
        combat.ApplyPlayerDamage(2,sourcePlayer:null);C(!sources[3].HasValue&&!HellishRebukeRules.CanTrigger(2,sources[3],5,true,true),"Warding Bond transfers and unowned damage cannot trigger");
        combat.GrantPlayerTemporaryHp(20);int hp=combat.PlayerHp;combat.ApplyPlayerDamage(4);C(combat.PlayerHp==hp&&amounts[4]==4,"damage absorbed by temporary HP still records the original positive damage");
        combat.ApplyPlayerDamage(0);C(sources.Count==5,"zero damage creates no trigger");
        C(HellishRebukeRules.CanTrigger(1,-1,60,true,true)&&!HellishRebukeRules.CanTrigger(1,-1,61,true,true)&&!HellishRebukeRules.CanTrigger(1,-1,-1,true,true),"exact sixty-foot boundary");
        C(!HellishRebukeRules.CanTrigger(1,-1,5,false,true)&&!HellishRebukeRules.CanTrigger(0,-1,5,true,true)&&!HellishRebukeRules.CanTrigger(1,-1,5,true,false),"visibility, positive damage and reaction eligibility are mandatory");
        C(combat.TryUseDamageReaction()&&!combat.TryUseDamageReaction(),"only one reaction can be spent");combat.EndRound(false);C(combat.TryUseDamageReaction(),"reaction refreshes at next player turn");
        combat.EndRound(false);combat.BlockReactions(true);C(!combat.TryUseDamageReaction(),"reaction-blocking conditions respected");
        combat.EndRound(false);combat.PlayerParalyzed=true;C(!combat.TryUseDamageReaction(),"paralysis prevents reaction");combat.PlayerParalyzed=false;
        combat.PlayerFeignDeath=true;C(!combat.TryUseDamageReaction(),"Feign Death incapacitation prevents reaction");combat.PlayerFeignDeath=false;
        combat.ApplyDamage(int.MaxValue);C(!combat.PlayerCanAct&&combat.TryUseDamageReaction(),"postcombat reactions remain possible while player is conscious");
        combat.EndRound(false);combat.ApplyPlayerDamage(int.MaxValue);C(!combat.CanPlayerDamageReact&&!combat.TryUseDamageReaction(),"fatal damage and unconsciousness cannot produce a late reaction");
        for(int slot=1;slot<=9;slot++)C(HellishRebukeRules.DiceCount(slot)==slot+1,"slot increases damage by one d10");C(HellishRebukeRules.DiceCount(0)==0&&HellishRebukeRules.DiceCount(10)==0,"invalid slot rejected");
        var resistant=new CombatState();resistant.DamageResistance=(p,t)=>t==5;C(resistant.AdjustTypedDamage(false,21/2,5)==5,"successful save halves before fire resistance, both rounded down");
        Console.WriteLine("PASS: Hellish Rebuke creature attribution, excluded transfers, temporary HP, visibility/range, reactions, incapacitation, refresh, slot growth and mitigation");
    }
}
