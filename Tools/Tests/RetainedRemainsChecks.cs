using System;
using System.Linq;
class RetainedRemainsChecks
{
    static void C(bool ok,string message){if(!ok)throw new Exception(message);}
    static void Main()
    {
        var former=new BodyWeight(150,10);former.SetArmorWeight(16);former.SetWeaponWeight(4);former.SetStoredGear(8);
        var archive=new RemainsArchive();var body=archive.Add(0,2,2,1,"玩家旧躯体",former,1,true);
        former.TryApply("180","0");C(body.Weight.BodyPounds==150&&body.Weight.TotalOunces==150*16+26*16+12,"archive snapshots body and exact ounce equipment independently");
        C(ReincarnationRules.NewBodyPounds(body.Weight)==150&&ReincarnationRules.RestoredHp(23)==23,"new body keeps former weight and restores its own full HP");
        var next=archive.Add(0,3,3,1,"玩家第二具旧躯体",new BodyWeight(),0,false);C(next.Id!=body.Id&&archive.Bodies.Count==2,"multiple generations coexist with distinct IDs");
        var receiver=new BodyWeight();int actions=1;Func<bool> pay=()=>{if(actions<1)return false;actions--;return true;};
        C(!body.RecoverGear(receiver,extra=>false,pay)&&actions==1&&receiver.StoredGearOunces==0,"overload rejection does not pay or move equipment");
        C(body.RecoverGear(receiver,extra=>true,pay)&&actions==0&&body.Weight.GearOunces==4&&receiver.StoredGearOunces==26*16+8,"recovering armor and other gear transfers exact weight once, leaving weapons on the remains");
        C(!body.RecoverGear(receiver,extra=>true,()=>true),"a second pickup cannot duplicate equipment");
        var bag=new WeaponInventory(Weapon.Available.Select(w=>w.Name),"长剑（双手）");bag.BeginBattle();C(bag.LeaveOnRemains(body.Id)&&bag.CarriedOunces==0&&bag.RemainsOunces(body.Id)==48,"one physical longsword remains on old body");
        C(!bag.PickUpFromRemains("长剑（单手）",next.Id,n=>true,()=>true),"another body cannot supply the same item");
        C(!bag.PickUpFromRemains("长剑（单手）",body.Id,n=>false,()=>true)&&bag.RemainsOunces(body.Id)==48,"failed load check leaves weapon ownership intact");
        C(bag.PickUpFromRemains("长剑（单手）",body.Id,n=>true,()=>true)&&bag.Carried("长剑（双手）")&&bag.RemainsOunces(body.Id)==0,"pickup transfers the shared longsword rather than generating another copy");
        var clock=new CorpseClock();clock.Observe(true,9,0);C(ReincarnationRules.Error(clock,ReincarnationRules.DeathLimit-ReincarnationRules.CastingRounds)==""&&ReincarnationRules.Error(clock,ReincarnationRules.DeathLimit-ReincarnationRules.CastingRounds+1)!="","ten-day deadline includes one-hour casting time");
        var undead=new CorpseClock();undead.Observe(true,13,0);C(ReincarnationRules.Error(undead,0)!="","only dead Humanoids qualify");
        C(RevivalRules.Supported("Reincarnate")&&RevivalRules.Duration("Reincarnate")==ReincarnationRules.CastingRounds&&RevivalRules.FullHp("Reincarnate")&&!RevivalRules.Ordeal("Reincarnate"),"shared aftermath routes use one-hour, full-HP reincarnation without adding resurrection ordeal");
        C(RevivalRules.Error("Reincarnate",undead,0)!="","normal revival eligibility cannot bypass Humanoid restriction");
        for(int die=1;die<=10;die++)C((int)ReincarnationRules.SpeciesFromDie(die)==die-1,"species table has all ten 2024 entries");
        Console.WriteLine("PASS: independent old bodies, retained weight, full new HP, exact gear transfer, unique weapons and Reincarnate eligibility");
    }
}
