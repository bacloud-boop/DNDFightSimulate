using System;
using System.Collections.Generic;
class ColorSprayChecks
{
    static void C(bool ok,string message){if(!ok)throw new Exception(message);}
    static void Main()
    {
        var conditions=new TurnBoundConditions();C(conditions.Add(0,-1,0,10)&&!conditions.Has(-1,0,9)&&conditions.Has(-1,0,10),"blindness begins when the spell resolves");
        conditions.EndTurn(-1);C(conditions.Has(-1,0,10),"no target-turn-end escape or expiry");conditions.EndTurn(0);C(conditions.Has(-1,0,11),"survives the casting turn");conditions.EndTurn(0);C(!conditions.Has(-1,0,11),"ends precisely at the caster's next turn end");
        conditions.Clear();conditions.Add(0,-1,0,20);conditions.Add(-1,-1,0,20);C(conditions.End(-1,0,20)==2&&!conditions.Has(-1,0,20),"restoration removes every short blindness source");
        conditions.Add(0,0,0,30);conditions.Add(0,0,1,30);conditions.End(0,0,30);C(!conditions.Has(0,0,30)&&conditions.Has(0,1,30),"ending blindness preserves other conditions");
        conditions.Clear();conditions.Add(0,-1,0,40);conditions.EndTurn(0);conditions.Add(0,-1,0,41);conditions.EndTurn(0);C(conditions.Has(-1,0,41),"recasting refreshes the next-turn deadline");conditions.EndTurn(0);C(!conditions.Has(-1,0,41),"refreshed effect still expires");
        conditions.Add(0,0,0,50);C(!conditions.Add(0,0,0,long.MaxValue)&&conditions.Has(0,0,50),"invalid time is atomic");conditions.Refresh(52);C(!conditions.Has(0,0,52),"world-time waiting expires short combat conditions");
        var combat=new CombatState();var sensory=new SensoryAfflictions();var feign=new FeignDeathEffect();
        conditions.Add(0,0,0,60);sensory.Add(true,true,15,60);feign.Cast(0,60);
        combat.ExtraConditionRemoval=(player,condition,now)=>conditions.End(player?0:-1,condition,now)+(condition==0&&feign.EndBlind(player?0:-1,now)?1:0);
        C(RestorationRules.EndCondition(combat,sensory,true,0,60)==3&&!conditions.Has(0,0,60)&&!sensory.Blinded(true,60)&&!feign.Blind(0,60)&&feign.Active(0,60),"one removal clears Color Spray, Blindness and Feign blindness without ending the other spells");
        conditions.Add(0,0,0,61);HealRules.Apply(combat,sensory,true,6,61);C(!conditions.Has(0,0,61),"Heal removes blindness even without actual healing");
        conditions.Add(0,0,0,61);MassHealRules.Apply(combat,sensory,true,5,61);C(conditions.Has(0,0,61),"Mass Heal must actually restore HP");combat.ApplyPlayerDamage(2);MassHealRules.Apply(combat,sensory,true,5,61);C(!conditions.Has(0,0,61),"actual mass healing clears Color Spray blindness");
        conditions.Add(-1,12,0,62);conditions.Add(0,int.MinValue,0,62);C(conditions.Has(12,0,62)&&conditions.Has(int.MinValue,0,62),"summons and deployed rescuers have independent blindness");
        BreathTemplate cone;C(BreathTemplate.TryCreate(0,0,1,0,1,4,.5,false,out cone),"existing 15-foot cone geometry");C(!cone.Intersects(0,0,1)&&cone.Intersects(1,0,1)&&!cone.Intersects(4,0,1),"source exclusion and fifteen-foot length");
        C(cone.Intersects(3,1,2),"one covered cell suffices for a multi-cell creature");
        C(SensoryAfflictions.BlocksSight(true,0,5)&&!SensoryAfflictions.BlocksSight(true,10,5)&&SensoryAfflictions.BlocksSight(true,10,15),"blindness blocks ordinary sight, respecting blindsight reach");
        C(Array.IndexOf(MonsterSpellSequence.Supported,"Color Spray")>=0&&MonsterSpellSequence.BaseLevel("Color Spray")==1,"monster cone casting configuration");
        conditions.Clear();C(!conditions.Has(12,0,62),"battle reset clears short conditions");
        Console.WriteLine("PASS: Color Spray caster-turn blindness, independent sources, all-source restoration, healing rules, creature targets, cone coverage, special senses and monster setup");
    }
}
