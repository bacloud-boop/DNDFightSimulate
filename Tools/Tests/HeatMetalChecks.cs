using System;
class HeatMetalChecks
{
    static void C(bool v,string text){if(!v)throw new Exception(text);}
    static void Main()
    {
        var objects=new EquipmentObjectCatalog();var shield=objects.Slot(0,EquipmentObjectKind.Shield,0,"盾牌",true,2,true);
        C(shield.Metal,"shield defaults to metal");var sword=objects.Weapon(0,"长剑","长剑",2,true);var staff=objects.Weapon(0,"长棍","长棍",2,true);
        C(sword.Metal&&!staff.Metal,"common weapon materials have separate defaults");
        var leather=objects.Slot(0,EquipmentObjectKind.Armor,2,"皮甲",true,2,true);C(!leather.Metal,"leather is not automatically a metal object");
        var plate=objects.Slot(-1,EquipmentObjectKind.Armor,12,"全身板甲",true,2,true);C(plate.Metal,"plate is metal");
        C(objects.Configure(shield.Id,3,true,false)&&!shield.Metal,"preparation can explicitly choose wooden shield");objects.Configure(shield.Id,2,false);C(!shield.Metal,"size and cover edits do not overwrite material");
        objects.ResetBattle();shield=objects.Slot(0,EquipmentObjectKind.Shield,0,"盾牌",true,2,true);C(!shield.Metal&&shield.Size==2,"material preference survives reset");
        var heat=new HeatMetalEffect();C(heat.Begin(sword.Id,2,14,3,10)&&heat.Dice==2&&heat.Active(19)&&!heat.Active(20),"second-level spell has 2d8 and a one-minute duration");
        C(!heat.CanPulse(3,10,5)&&heat.CanPulse(4,11,60)&&!heat.CanPulse(4,11,61),"later-turn pulse requires range and cannot be used on casting turn");
        heat.Retained(-1,10);C(heat.Disadvantage(-1,10)&&!heat.Disadvantage(0,10),"retention disadvantage is target-specific");heat.CasterTurnStarts();C(!heat.Disadvantage(-1,11),"next caster start clears retention disadvantage");
        C(!heat.Begin(0,9,14,4,11)&&heat.Slot==2,"invalid object rejects atomically");C(heat.Begin(sword.Id,9,14,4,11)&&heat.Dice==9,"upcasting adds a d8 per slot");
        C(HeatMetalEffect.Touching(EquipmentObjectKind.Weapon,EquipmentObjectPlace.Equipped,true,false,true),"held weapon contacts body");
        C(!HeatMetalEffect.Touching(EquipmentObjectKind.Weapon,EquipmentObjectPlace.Equipped,false,false,true),"stowed or backpack weapons do not contact body");
        C(HeatMetalEffect.Touching(EquipmentObjectKind.Armor,EquipmentObjectPlace.Equipped,false,false,true),"worn armor contacts body");
        C(!HeatMetalEffect.Touching(EquipmentObjectKind.Shield,EquipmentObjectPlace.Carried,false,false,true),"recovered backpack shield is not donned");
        C(!HeatMetalEffect.Touching(EquipmentObjectKind.Weapon,EquipmentObjectPlace.Equipped,true,true,true)&&!HeatMetalEffect.Touching(EquipmentObjectKind.Armor,EquipmentObjectPlace.Remains,false,false,false),"melded equipment and corpse equipment do not damage a living wearer");
        heat.Retained(0,11);heat.Clear();C(!heat.Active(11)&&!heat.Disadvantage(0,11),"concentration loss clears object and penalties");
        Console.WriteLine("PASS: material defaults/overrides/persistence, contact rules, duration, upcast dice, pulse timing/range, retention disadvantage and cleanup");
    }
}
