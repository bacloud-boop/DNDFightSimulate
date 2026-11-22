using System;
using System.Linq;
class DragonBreathChecks
{
    static void C(bool value,string message){if(!value)throw new Exception(message);}
    static void Main()
    {
        var spell=DirectSpell.Find("Dragon’s Breath");C(spell!=null&&spell.Level==2&&spell.Bonus&&spell.Range==5&&spell.Sides==6&&spell.SaveAbility==1&&spell.HalfSave&&!spell.Attack,"2024 second-level touch buff is a Bonus Action, with Dexterity save breaths rather than attacks");
        for(int slot=2;slot<=9;slot++)C(DragonBreathEffect.DiceCount(slot)==slot+1&&spell.DiceCount(20,slot)==slot+1,"3d6 base, +1d6 each slot; character level never scales the buff");
        var effect=new DragonBreathEffect();C(effect.Begin(-1,2,5,15,100)&&effect.Active(100)&&effect.Active(109)&&!effect.Active(110)&&effect.Target==-1&&effect.Dc==15,"one-minute concentration lifetime and caster DC snapshot");
        foreach(int type in new[]{3,4,5,7,9})C(effect.Begin(3,4,type,17,1)&&effect.Type==type,"five legal elemental types chosen at cast time");
        C(!effect.Begin(0,1,5,12,0)&&!effect.Begin(0,10,5,12,0)&&!effect.Begin(0,2,6,12,0)&&effect.Target==3&&effect.Type==9,"invalid slot or Force damage choice cannot replace the valid effect");
        C(!effect.Begin(0,2,5,12,long.MaxValue-9)&&!effect.Begin(0,2,5,12,-1),"duration overflow and negative clocks rejected");
        effect.Clear();C(!effect.Active(1),"concentration loss and reset clear the granted action");
        var combat=new CombatState();C(combat.TryUseBonusAction()&&combat.MagicActionAvailable&&combat.TryUseMagicAction()&&!combat.TryUseMagicAction(),"cast and first breath fit one turn, exactly one bonus and one Magic action");
        combat.EndRound();C(combat.TryUseMagicAction()&&combat.BonusActionAvailable,"later breath uses a new action without another casting bonus action");
        combat.EndRound();combat.AddSurgeAction();C(combat.TryUseMagicAction()&&!combat.TryUseMagicAction()&&combat.ActionsRemaining==1,"restricted Action Surge action cannot be used for a second Magic action");
        var after=new CombatState();after.Aftermath=true;C(after.TryUseAftermathBonusAction()&&after.TryUseAftermathMagicAction(),"aftermath casting and breath use current actor's proper resources");
        var ally=new SummonedUnit(1,"ally",20,10,30,2,1);C(!ally.TryUseAction()&&ally.BeginTurn(1)&&ally.TryUseAction()&&!ally.Attack()&&ally.MovementRemaining==30&&ally.BonusAvailable,"summon breath needs its own active turn and action, leaving movement and bonus action intact");
        combat.DamageResistance=(p,t)=>t==5;C(combat.AdjustTypedDamage(false,19/2,5)==4,"save halves before elemental resistance, with integer flooring");
        combat.DamageImmunity=(p,t)=>t==9;C(combat.AdjustTypedDamage(false,30,9)==0,"poison immunity applies to selected poison breath");
        var policy=new MonsterActionPolicy();C(policy.Order.SequenceEqual(new[]{0,1,2,3})&&policy.Enabled(2),"default order is species, spell cycle, granted breath and custom attack");
        C(policy.Configure(new[]{2,3,0,1},new[]{true,true,true,true})&&policy.Order[0]==2,"granted breath can be prioritized ahead of all other categories");
        int[] draft={3,1,2,0};bool[] allowed={true,true,false,true};C(policy.Configure(draft,allowed),"valid independent enable flags");draft[0]=1;allowed[0]=false;
        C(policy.Order[0]==3&&policy.Enabled(0)&&!policy.Enabled(2),"configuration does not retain mutable caller arrays");
        C(!policy.Configure(new[]{0,0,2,3},new[]{true,true,true,true})&&policy.Order[0]==3&&!policy.Enabled(2),"invalid duplicate order is rejected atomically");
        var copy=policy.Copy();copy.Move(0,1);copy.SetEnabled(0,false);C(policy.Order[0]==3&&policy.Enabled(0)&&copy.Order[1]==3&&!copy.Enabled(0),"saved current and pre-death forms keep independent orders and toggles");
        C(!policy.Move(0,-1)&&!policy.Move(3,1),"priority cannot move outside the four categories");
        var npc=new CombatState();int actions=0,bonuses=0;policy.Configure(new[]{2,1,0,3},new[]{true,true,true,true});
        foreach(int kind in policy.Order)if(policy.Enabled(kind)){if(kind==1){if(npc.TryUseMonsterBonusAction())bonuses++;}else if(npc.TryUseMonsterAction())actions++;}
        C(actions==1&&bonuses==1&&!npc.MonsterActionAvailable&&!npc.MonsterBonusActionAvailable,"mixed priorities share exactly one action and one bonus action; no double attack or breath");
        var unit=new SummonedUnit(2,"breath recipient",10,10,30,2,1);unit.BeginTurn(1);unit.ChooseTemporaryHp(5,true);unit.Damage(8,5);C(unit.Hp==7&&unit.TemporaryHp==0,"existing temporary HP handling applies to breath damage without critical doubling");
        Console.WriteLine("PASS: Dragon's Breath metadata, touch/bonus/magic resources, saved elemental choice/DC, concentration expiry, slot scaling, surge restriction, summon turns, typed mitigation and atomic independent priority configuration");
    }
}
