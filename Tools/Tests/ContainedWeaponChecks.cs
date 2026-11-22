using System;
using UnityEngine;
class ContainedWeaponChecks
{
    static void C(bool ok,string message){if(!ok)throw new Exception(message);}
    static void Main()
    {
        var g=new GroundWeaponState(0);C(g.Drop(0,0,1,true,-1)&&g.ContainedBy==-1&&!g.Submerged,"internal drop is distinct from external water");
        int paid=0;C(!g.WithinReach(0,0,1)&&!g.PickUp(0,0,0,1,true,true,()=>true,()=>{paid++;return true;}),"outside cannot see through pickup boundary");
        C(paid==0&&g.WithinReach(0,0,1,-1),"failed exterior pickup never spends interaction");
        g.FollowContainer(8,7,4);C(g.CenterX2==20&&g.CenterY2==18&&g.ContainedBy==-1,"follow exact large host center");
        C(g.PickUp(0,8,7,1,true,true,()=>true,()=>{paid++;return true;},false,-1)&&paid==1&&!g.Dropped&&!g.ContainedBy.HasValue,"inside owner can recover and resets containment");
        C(!g.Drop(0,0,1,false,0)&&!g.Dropped,"own body cannot be its own container");
        g.Drop(0,0,1,false,-1);g.ReleaseContainer(4,3,2,true);
        C(g.Dropped&&!g.ContainedBy.HasValue&&g.Submerged&&g.CenterX2==10&&g.CenterY2==8,"release deposits at current host and uses water landing rule");
        C(!g.PickUp(0,4,3,1,true,true,()=>true,()=>true,false)&&g.PickUp(0,4,3,1,true,true,()=>true,()=>true,true),"released weapon in water requires diving");
        var inventory=new WeaponInventory(new[]{"匕首"},"匕首");inventory.BeginBattle();C(inventory.Drop("匕首",0,0,1,false,-1),"inventory preserves internal identity");
        C(!inventory.PickUp("匕首",w=>true,()=>true)&&inventory.PickUp("匕首",w=>true,()=>true,false,-1),"inventory transfer cannot bypass internal boundary");
        var map=new BattleMap();map.Place(false,new Vector2Int(1,0));C(map.Lock()&&map.BeginContainment(true),"containment fixture");
        var dropped=new GroundWeaponState(0);dropped.Drop(0,0,1,false,-1);int ended=0;
        map.ContainmentEnded=host=>{C(!host,"correct swallowing actor identity");ended++;dropped.ReleaseContainer(map.Monster.x,map.Monster.y,map.MonsterSpaces,false);};
        C(map.CanReleaseContainedAt(new Vector2Int(0,0),5)&&ended==0&&dropped.ContainedBy==-1,"pure placement previews cannot release equipment");
        C(map.ReleaseContainedAt(new Vector2Int(0,0),5)&&ended==1&&!dropped.ContainedBy.HasValue,"normal escape releases all internal items via committed callback");
        C(map.BeginContainment(true),"second swallow");dropped.Reset();dropped.Drop(0,1,1,false,-1);
        var plans=map.MandatoryReleasePlans();C(plans.Count>0&&ended==1&&dropped.ContainedBy==-1,"mandatory release planning is mutation-free for items");
        C(map.ApplyMandatoryReleasePlan(plans[0])&&ended==2&&!dropped.ContainedBy.HasValue,"mandatory death/form release triggers same item policy");
        var flames=new ContinualFlameEffect();ContinualFlameEffect.Source s;C(flames.Cast(1,3,out s),"light attachment");
        var vision=new VisionState{BaseLight=2};vision.HideLightGroup(-s.Group);C(vision.Light(0,0,0)==2,"hidden internal attachment emits no exterior light");
        C(vision.SetLightGroup(-s.Group,new[]{new[]{0d,0d}},20,40,s.Slot,-1,0)&&vision.Light(0,0,0)==0,"attachment can resume after release");
        inventory.ResetBattle();dropped.Reset();C(!dropped.ContainedBy.HasValue&&!inventory.Find("匕首").Ground.ContainedBy.HasValue,"battle reset clears internal location");
        Console.WriteLine("PASS: internal weapon drops, atomic pickup isolation, host following, water release, normal/mandatory release callbacks, pure previews, light hiding and reset");
    }
}
