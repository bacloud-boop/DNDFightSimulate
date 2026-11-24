using System;
using System.Linq;
class BanishmentEffectChecks
{
    static void C(bool value,string text){if(!value)throw new Exception(text);}
    static void Main()
    {
        for(int slot=4;slot<=9;slot++)C(BanishmentEffect.MaximumTargets(slot)==slot-3,"upcasting adds one target each slot");
        C(BanishmentEffect.MaximumTargets(3)==0&&BanishmentEffect.MaximumTargets(10)==0,"invalid spell slots");
        for(int type=0;type<14;type++)C(BanishmentEffect.PermanentType(type)==new[]{0,2,5,6,7}.Contains(type),"2024 permanent departure uses creature type, never native plane");
        var spell=new BanishmentEffect();C(spell.Begin(1,5,10,new[]{new BanishmentEffect.Target(-1,7),new BanishmentEffect.Target(6,9)}),"upcast targets enter one shared concentration");
        C(spell.Away(-1)&&spell.Away(6)&&spell.Concentrating&&spell.Expires==20,"away preserves identity and shares one-minute expiry");
        C(!spell.End(9)&&spell.Concentrating,"time reversal cannot end a valid spell");
        C(spell.End(19)&&spell.Entries.All(e=>e.State==BanishmentEffect.Location.Returning)&&!spell.PermanentlyDeparted(-1),"early concentration loss returns every target, including planar types");
        C(spell.Away(-1)&&!spell.Begin(1,4,19,new[]{new BanishmentEffect.Target(7,9)}),"returning targets stay away until placement succeeds; a new cast cannot overwrite pending returns");
        C(spell.FinishReturn(-1)&&!spell.Away(-1)&&!spell.FinishReturn(-1)&&spell.Away(6),"each successful placement restores exactly its own target once");
        spell.FinishReturn(6);C(spell.Begin(1,5,20,new[]{new BanishmentEffect.Target(-1,7),new BanishmentEffect.Target(6,9)})&&spell.End(30),"full uninterrupted minute completes");
        C(spell.PermanentlyDeparted(-1)&&!spell.FinishReturn(-1)&&spell.Entries.Single(e=>e.Actor==6).State==BanishmentEffect.Location.Returning,"fiend departs without returning while humanoid returns");
        spell.FinishReturn(6);C(spell.Begin(1,4,30,new[]{new BanishmentEffect.Target(8,9)})&&spell.PermanentlyDeparted(-1),"new concentration does not undo permanent non-death departure");
        C(!spell.End(29)&&spell.End(31)&&spell.PermanentlyDeparted(-1),"ending a later cast preserves an earlier permanent departure");
        spell.Clear();C(spell.Entries.Count==0&&!spell.Away(-1)&&!spell.Concentrating,"manual battle reset removes every departure and pending return");
        C(!spell.Begin(1,4,long.MaxValue-9,new[]{new BanishmentEffect.Target(1,9)})&&!spell.Begin(1,4,0,new[]{new BanishmentEffect.Target(1,9),new BanishmentEffect.Target(1,9)}),"overflow and duplicate/over-budget targets rejected atomically");
        C(!spell.Begin(1,5,0,new[]{new BanishmentEffect.Target(1,9),new BanishmentEffect.Target(2,14)})&&spell.Entries.Count==0,"invalid creature type cannot partially remove earlier targets");
        Console.WriteLine("PASS: Banishment lifecycle model, upcast target budget, early return, full-minute type-dependent departure, independent pending placements and reset; runtime/map integration pending");
    }
}
