using System;
class LightSpellChecks
{
    static void C(bool ok,string message){if(!ok)throw new Exception(message);}
    static void Main()
    {
        var catalog=new EquipmentObjectCatalog();
        var sword=catalog.Weapon(0,"longsword","longsword",2,true);C(catalog.Weapon(0,"longsword","two-hand grip",2,true).Id==sword.Id,"weapon grip changes share one physical object identity");
        catalog.Weapon(0,"longsword","longsword",1,true);C(sword.Size==1,"unconfigured equipment follows original body size during preparation");
        C(catalog.Configure(sword.Id,3,true)&&!catalog.Configure(sword.Id,6,false)&&sword.Size==3&&sword.Covered,"size categories are validated atomically");
        catalog.Weapon(0,"longsword","longsword",2,true);C(sword.Size==3,"manual object size overrides body-derived defaults");
        var armor=catalog.Slot(0,EquipmentObjectKind.Armor,1,"armor",true,2,true);var shield=catalog.Slot(0,EquipmentObjectKind.Shield,0,"shield",true,2,true);
        C(catalog.Slot(0,EquipmentObjectKind.Armor,1,"armor",true,2,true).Id==armor.Id,"resynchronizing worn slots preserves identity");
        C(catalog.Configure(armor.Id,3,false),"armor options fixture");catalog.Archive(0,-2);
        C(armor.Place==EquipmentObjectPlace.Remains&&shield.Place==EquipmentObjectPlace.Remains&&sword.Place==EquipmentObjectPlace.Equipped,"reincarnation archives armor/shield while unique weapon inventory controls weapon location separately");
        var replacement=catalog.Slot(0,EquipmentObjectKind.Armor,1,"new armor",true,1,false);C(replacement.Id!=armor.Id&&armor.RemainsId==-2,"new worn equipment cannot reuse archived physical identity");
        catalog.Recover(-2,0);C(armor.Place==EquipmentObjectPlace.Carried&&armor.Carrier==0&&armor.Covered&&armor.Size==3,"recovery follows old equipment into covered backpack without wearing or resizing it");
        int paid=0;C(!catalog.Cover(armor.Id,false,()=>false)&&armor.Covered,"failed interaction does not uncover an object");
        C(catalog.Cover(armor.Id,false,()=>{paid++;return true;})&&paid==1&&!armor.Covered,"uncover consumes exactly the authorized interaction");
        catalog.Recover(-2,0);C(!armor.Covered,"repeated location synchronization cannot re-cover already recovered equipment");
        catalog.Archive(0,-3);C(armor.Place==EquipmentObjectPlace.Remains&&armor.RemainsId==-3&&replacement.RemainsId==-3,"later reincarnation transfers both worn and recovered carried gear once");
        C(LightSpellEffect.CanTouch(3,true,null)&&LightSpellEffect.CanTouch(0,true,0)&&!LightSpellEffect.CanTouch(4,true,0)&&!LightSpellEffect.CanTouch(2,false,null)&&!LightSpellEffect.CanTouch(2,true,-1),"only reachable Large-or-smaller objects not carried by another creature can be targeted");
        C(LightSpellEffect.CanTouch(2,true,-1,-1),"casting restriction is relative to the caster, not object origin");
        var light=new LightSpellEffect();C(light.Cast(armor.Id,12,34,56,0)&&light.Active(599)&&!light.Active(600)&&light.Red==12&&light.Blue==56,"colored nonconcentration light lasts exactly one hour");
        C(!light.Cast(sword.Id,256,0,0,1)&&light.ObjectId==armor.Id&&!light.Cast(sword.Id,0,0,0,long.MaxValue-599),"invalid color/time is atomic");
        C(light.Cast(sword.Id,1,2,3,2)&&light.ObjectId==sword.Id&&light.Expires==602,"recasting replaces the single active attachment");
        var vision=new VisionState{BaseLight=2};C(vision.SetLightGroup(3,new[]{new[]{0.0,0.0}},20,40,0,600,0),"object cantrip light zone created");
        C(vision.Light(0,0,0)==0&&vision.Light(20,0,0)==0&&vision.Light(21,0,0)==1&&vision.Light(40,0,0)==1&&vision.Light(41,0,0)==2,"twenty bright plus twenty additional dim feet, not sixty dim feet");
        vision.HideLightGroup(3);C(vision.Light(0,0,0)==2&&!vision.ConsumeDispelledLightGroup(3)&&light.Active(2),"opaque covering hides illumination without ending duration");
        C(vision.SetLightGroup(3,new[]{new[]{50.0,0.0}},20,40,0,600,0)&&vision.Light(0,0,0)==2&&vision.Light(50,0,0)==0,"uncover or move replaces emitter position without leaving a light trail");
        vision.AddZone(true,0,0,2,0);C(vision.HasDarkness(0)&&vision.ConsumeDispelledLightGroup(3),"outer dim ring overlap dispels Light and Light never dispels Darkness");
        vision.HideLightGroup(3);C(!vision.SetLightGroup(3,new[]{new[]{0.0,0.0}},20,40,0,600,0),"uncovering directly inside existing Darkness cannot recreate dispelled illumination");
        vision.ClearEffects();vision.SetLightGroup(3,new[]{new[]{0.0,0.0}},20,40,0,600,0);vision.AddZone(true,0,0,2,0);vision.HideLightGroup(3);C(vision.ConsumeDispelledLightGroup(3),"hiding cannot erase a pending whole-spell dispel");
        vision.ClearEffects();vision.SetLightGroup(3,new[]{new[]{0.0,0.0}},20,40,0,600,0);vision.AddFog(0,0,1,0);C(vision.Quality(0,0,5,0,0,0)==2,"bright equipment does not remove heavy fog obscuration");
        vision.ClearEffects();vision.SetLightGroup(3,new[]{new[]{0.0,0.0}},20,40,0,600,0);vision.SetDimLightGroup(2,new[]{new[]{80.0,0.0}},10,0,10,0);vision.ClearLightGroup(2);C(vision.Light(0,0,0)==0,"ending concentration on Dancing Lights preserves nonconcentration object light");
        light.Clear();catalog.ResetBattle();var reset=catalog.Weapon(0,"longsword","longsword",2,true);C(!light.Active(2)&&reset.Size==3&&reset.Covered&&catalog.Objects.Count==1,"reset removes live attachments/remains while preserving prepared object options");
        Console.WriteLine("PASS: equipment identity, archive/recovery, size and cover options, touch eligibility, one-hour colored Light, bright/dim radii, moving emitters, concealment and Darkness dispelling");
    }
}
