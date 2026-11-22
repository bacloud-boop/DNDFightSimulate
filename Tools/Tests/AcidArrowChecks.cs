using System;
using System.Linq;
class AcidArrowChecks
{
    static void C(bool value,string message){if(!value)throw new Exception(message);}
    static void Main()
    {
        var spell=DirectSpell.Find("Melf’s Acid Arrow");C(spell!=null&&spell.Level==2&&spell.Range==90&&spell.Attack&&!spell.Melee&&!spell.Bonus&&spell.SaveAbility==-1&&spell.DamageType=="强酸","2024 second-level ranged spell attack, not save or weapon attack");
        for(int slot=2;slot<=9;slot++)C(AcidArrowResidue.InitialDice(slot)==slot+2&&AcidArrowResidue.InitialDice(slot,true)==2*(slot+2)&&AcidArrowResidue.LaterDice(slot)==slot&&spell.DiceCount(20,slot)==slot+2,"4d4 initial and 2d4 later both scale +1d4, only the initial hit's dice can crit");
        C(AcidArrowResidue.InitialDice(1)==0&&AcidArrowResidue.LaterDice(10)==0&&AcidArrowResidue.InitialDamage(19,false)==9,"invalid slots and miss splash half rounding");
        C(!AcidArrowResidue.CrossesBoundary(0,-1,true,false)&&AcidArrowResidue.CrossesBoundary(0,7,true,false)&&AcidArrowResidue.CrossesBoundary(0,-1,false,true),"a swallowed caster may attack its host but cannot attack outside, and an external caster cannot target the swallowed creature");
        C(!AcidArrowResidue.CrossesBoundary(-1,0,false,true)&&AcidArrowResidue.CrossesBoundary(-1,7,false,true)&&!AcidArrowResidue.CrossesBoundary(0,0,true,false),"monster and self-target containment use the same isolation rule");
        var queue=new AcidArrowResidue();var npc=queue.Add(0,-1,2);C(npc!=null&&npc.Dice==2&&queue.Pending(-1).Count==0,"a hit does not immediately deliver its residue");
        queue.StartTurn(0);queue.StartTurn(5);C(queue.Pending(-1).Count==0,"other actors' starts and ends cannot trigger target residue");
        queue.StartTurn(-1);C(queue.Pending(-1).Count==1&&queue.Take(npc.Id)&&!queue.Take(npc.Id)&&queue.Pending(-1).Count==0,"next target turn's end can claim this residue exactly once");
        var self=queue.Add(0,0,2);C(queue.Pending(0).Count==0,"self-target cast waits past its current turn's end");
        queue.StartTurn(-1);C(queue.Pending(0).Count==0,"another monster turn cannot advance the player timer");
        queue.StartTurn(0);var later=queue.Add(-1,0,5);C(queue.Pending(0).Single().Id==self.Id,"a fresh hit during the due turn waits for a later target turn");
        C(queue.Take(self.Id)&&queue.Pending(0).Count==0,"settling the older residue preserves the future entry");
        queue.StartTurn(0);C(queue.Pending(0).Single().Id==later.Id&&queue.Pending(0)[0].Dice==5,"higher-slot delayed roll remains independent and does not inherit initial critical doubling");
        var second=queue.Add(0,0,3);var third=queue.Add(-1,0,2);queue.StartTurn(0);var all=queue.Pending(0);C(all.Count==3&&all.Select(e=>e.Id).Distinct().Count()==3&&all.Sum(e=>e.Dice)==10,"multiple hits from multiple sources stack as independent residues with unique choices");
        C(queue.Take(third.Id)&&queue.Pending(0).Count==2&&queue.Pending(0).Any(e=>e.Id==second.Id),"player may resolve the selected residue without consuming others");
        var ally=queue.Add(0,7,4);queue.StartTurn(7);queue.RemoveUnavailable(id=>id==7);C(queue.Pending(0).Count==0&&queue.Pending(7).Single().Id==ally.Id,"death or lost body clears only affected target entries, not other source/target pairs");
        queue.RemoveTarget(7);C(queue.Pending(7).Count==0&&queue.Starts(7)==1,"target cleanup preserves turn identity for later valid casts");
        C(queue.Add(0,7,1)==null&&queue.Pending(7).Count==0,"invalid later damage slot cannot create a residue");
        queue.Clear();C(queue.Starts(0)==0&&queue.Starts(7)==0&&queue.Pending(0).Count==0,"reset clears all timers and independent entries");
        var combat=new CombatState();C(combat.TryUseMagicAction()&&combat.BonusActionAvailable&&!combat.TryUseMagicAction(),"initial cast spends one Magic action, no bonus action or extra attack");
        combat.DamageResistance=(p,t)=>t==3;C(combat.AdjustTypedDamage(false,AcidArrowResidue.InitialDamage(19,false),3)==4,"miss splash halves before acid resistance with separate flooring");
        combat.DamageImmunity=(p,t)=>t==3;C(combat.AdjustTypedDamage(false,30,3)==0,"acid immunity works for either damage event");
        var zombie=new AnimatedUndeadRules(false).Create(4);bool sawCritical=false;zombie.LethalGuard=(d,t,critical)=>{sawCritical=critical;return !critical;};var packet=new long[13];packet[3]=15;zombie.Damage(15,resolvedTypes:packet,critical:true);C(sawCritical&&!zombie.Alive,"initial critical packet bypasses zombie Fortitude");
        zombie=new AnimatedUndeadRules(false).Create(5);zombie.LethalGuard=(d,t,critical)=>!critical;zombie.Damage(15,resolvedTypes:packet);C(zombie.Hp==1,"noncritical residue preserves Fortitude eligibility");
        var sequence=new MonsterSpellSequence();C(sequence.Configure(4,1,false,new[]{new MonsterSpellEntry("Melf’s Acid Arrow",2)})&&!sequence.Configure(4,1,false,new[]{new MonsterSpellEntry("Melf’s Acid Arrow",1)}),"monster cycle accepts legal upcasting and rejects under-level entry");
        Console.WriteLine("PASS: Acid Arrow 2024 attack metadata, initial/miss/critical damage, independent per-hit residues, target next-turn clocks, selectable one-time claims, cleanup, typed mitigation, Fortitude and monster slots");
    }
}
