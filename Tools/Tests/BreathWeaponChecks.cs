using System;
using System.Collections.Generic;
class BreathWeaponChecks
{
    static void C(bool ok,string message){if(!ok)throw new Exception(message);}
    static void Main()
    {
        BreathTemplate cone,line=default(BreathTemplate);
        C(BreathTemplate.TryCreate(0,3,1,0,1,7.5,3.5,false,out cone)&&BreathTemplate.TryCreate(0,3,1,0,1,7.5,3.5,true,out line),"valid outward template starts at chosen edge midpoint");
        C(cone.Cell(1,3)&&cone.Cell(3,2)&&cone.Cell(3,4)&&!cone.Cell(4,3)&&!cone.Cell(1,2),"15-foot cone uses axial length and equal width at each distance");
        C(line.Cell(6,3)&&!line.Cell(7,3)&&!line.Cell(3,4)&&!line.Cell(0,3),"30-by-5 line is finite, narrow and excludes source body behind origin");
        C(!cone.Cell(1,2)&&cone.Intersects(1,2,2),"multi-cell target qualifies once if any cell center is covered; mere cell-edge overlap does not qualify");
        BreathTemplate corner;C(BreathTemplate.TryCreate(0,0,1,0,0,6,6,false,out corner)&&corner.Valid,"corner origin supports a diagonal cone");
        C(!BreathTemplate.TryCreate(0,0,1,0,1,0,0,false,out corner)&&!BreathTemplate.TryCreate(0,0,1,5,1,5,0,false,out corner)&&!BreathTemplate.TryCreate(0,0,1,0,3,5,0,false,out corner),"invalid inward aim, edge and offset rejected before action reservation");
        C(BreathTemplate.TryCreate(0,0,1,4,1,5,.5,false,out corner)&&!corner.Cell(0,0),"center origin is excluded by default");
        C(BreathTemplate.TryCreate(0,0,1,4,1,5,.5,false,out corner,true)&&corner.Cell(0,0),"creator may include origin; caster follows same center-coverage rule");
        C(!default(BreathTemplate).Cell(1,1),"invalid preview never covers creatures");
        C(BreathWeaponRules.DiceCount(1)==1&&BreathWeaponRules.DiceCount(4)==1&&BreathWeaponRules.DiceCount(5)==2&&BreathWeaponRules.DiceCount(11)==3&&BreathWeaponRules.DiceCount(17)==4&&BreathWeaponRules.DiceCount(20)==4,"damage uses character/racial level thresholds");
        C(BreathWeaponRules.SaveDc(9,2)==9&&BreathWeaponRules.SaveDc(20,int.MaxValue)==2147483660L&&BreathWeaponRules.SaveDc(3,int.MinValue)==-2147483644L,"Constitution rounds down and manually entered extreme proficiency cannot overflow DC");
        int[] types={3,7,5,7,3,5,9,5,4,4};for(int i=0;i<10;i++)C(BreathWeaponRules.DamageType(i)==types[i],"ancestry determines typed damage");
        var state=new SpeciesAbilityState();C(!state.Use(Species2024.Dragonborn,0,0)&&!state.Use(Species2024.Dragonborn,-3,0),"nonpositive proficiency gives zero uses");
        C(state.Use(Species2024.Dragonborn,2,0)&&state.Use(Species2024.Dragonborn,2,0)&&!state.Use(Species2024.Dragonborn,2,0)&&state.BreathSpent==2,"only committed uses spend the long-rest pool");
        state.ShortRest();C(state.Remaining(Species2024.Dragonborn,2)==0,"short rest does not refresh breath");state.LongRest();C(state.Remaining(Species2024.Dragonborn,2)==2,"long rest refreshes breath");
        var fighter=new FighterFeatures();fighter.BeginAttackAction(5);C(fighter.ReplaceAttackWithBreath()&&fighter.AttacksRemaining==1&&!fighter.ReplaceAttackWithBreath()&&fighter.AttacksRemaining==1,"breath replaces exactly one attack in a multiattack action and cannot replace a second");
        fighter.ConsumeAttack();C(fighter.AttacksRemaining==0,"remaining attack is independently clicked");fighter.BeginAttackAction(5);C(fighter.ReplaceAttackWithBreath(),"another Attack action, including Action Surge, may use another breath if rest uses remain");
        fighter.EndRound();C(!fighter.BreathUsedThisAttackAction&&!fighter.ReplaceAttackWithBreath(),"new turn clears replacement state without granting free attacks");
        fighter.BeginAttackAction(20);fighter.RecordMiss(20);C(fighter.ReplaceAttackWithBreath()&&fighter.AttacksRemaining==3&&fighter.NextAttackAdvantage,"save-based breath does not consume studied-attack advantage or roll critical hits");
        var action=new CombatState();action.AddSurgeAction();C(action.TryUseAction()&&action.TryUseAction()&&!action.TryUseAction()&&action.BonusActionAvailable,"Attack replacements may use restricted Surge actions and do not spend bonus action");
        var enemies=new List<BreathBody>{new BreathBody(0,4,3,1),new BreathBody(1,4,4,1),new BreathBody(2,9,9,1)};BreathTemplate plan;
        C(BreathWeaponRules.Choose(10,0,3,1,enemies,out plan)&&plan.Intersects(4,3,1)&&plan.Intersects(4,4,1)&&!plan.Intersects(0,3,1),"monster finds a safe template covering the most reachable enemies");
        C(!BreathWeaponRules.Choose(10,0,0,1,new List<BreathBody>{new BreathBody(0,9,9,1)},out plan),"no reachable target leaves feature and action available for fallback");
        Console.WriteLine("PASS: cone/line center coverage, multi-cell targets, ancestry, level scaling, extreme DC, rest uses, attack replacement, Surge and monster area planning");
    }
}
