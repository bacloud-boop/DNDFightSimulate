using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly AnimateDeadControl undeadControl=new AnimateDeadControl();
    sealed class UndeadItem
    {
        public EquipmentObjectRecord Object;public Weapon Definition;public WeaponInventory.Item Inventory;
        public GroundWeaponState Ground;public int Ounces;public bool Held,Custom;public MonsterDamage Damage;
        public bool Melee;public int Reach,Type;
    }
    sealed class UndeadFollower
    {
        public SummonedUnit Unit;public AnimatedUndeadRules Rules;public int Source,Owner,Corpse;
        public BodyWeight Weight;public int ArmorIndex;public bool Shield;
        public readonly List<UndeadItem> Items=new List<UndeadItem>();public UndeadItem Equipped;
        public int LightRound=-1;public string LastLightKey;public AmmunitionState Ammo;
        public readonly WeaponGripState Grip=new WeaponGripState();
    }
    readonly Dictionary<int,UndeadFollower> undeadFollowers=new Dictionary<int,UndeadFollower>();
    sealed class UndeadSave {public int[] Dice;public long Total,Dc;public string Name;public bool Success;}
    readonly Queue<UndeadSave> undeadSaves=new Queue<UndeadSave>();bool drainingUndeadSaves;
    UndeadFollower Undead(SummonedUnit unit){UndeadFollower value;return unit!=null&&undeadFollowers.TryGetValue(unit.Id,out value)?value:null;}
    bool NeutralUndead(int actor)=>undeadControl.Neutral(actor,worldRounds);
    bool UndeadFortitude(UndeadFollower follower,int damage,long[] types,bool critical)
    {
        if(!follower.Rules.FortitudeEligible(damage,types,critical))return false;
        var roll=RollSummonSavingThrow(follower.Unit,2);long dc=AnimatedUndeadRules.FortitudeDc(damage),total=(long)roll.Value+follower.Rules.Save(2)+BondBonus(follower.Unit.Id)+follower.Unit.Breathing.D20Penalty;
        undeadSaves.Enqueue(new UndeadSave{Dice=roll.Dice,Dc=dc,Total=total,Name=follower.Unit.Name,Success=total>=dc});return total>=dc;
    }
    IEnumerator DrainUndeadSaves()
    {
        if(drainingUndeadSaves||undeadSaves.Count==0)yield break;drainingUndeadSaves=true;bool busy=rolling;rolling=true;
        try{while(undeadSaves.Count>0){var save=undeadSaves.Dequeue();yield return PresentRoll(20,save.Dice,save.Name+" · 亡灵坚韧："+save.Total+" / DC "+save.Dc+(save.Success?"，保留1HP":"，失败"));}}
        finally{drainingUndeadSaves=false;rolling=busy;}
    }
    IEnumerable<UndeadItem> CollectUndeadItems(int source)
    {
        SyncEquipmentObjects();int owner=source>=-1||source==BattleMap.RescueId?source:map.Remains.Find(source).Owner;
        if(owner==0)foreach(var item in carriedWeapons.Items){
            bool carried=source==0&&item.Location==WeaponLocation.Carried||source<-1&&item.Location==WeaponLocation.Remains&&item.RemainsId==source;
            Vector2Int p;int n;string name;bool nearGround=AnimateBodyInfo(source,out p,out n,out name)&&item.Location==WeaponLocation.Ground&&item.FollowerId==0&&item.Ground.WithinReach(p.x,p.y,n);
            if(!carried&&!nearGround)continue;var definition=Weapon.Available.First(w=>w.InventoryKey==item.Key);
            yield return new UndeadItem{Object=equipmentObjects.Weapon(0,item.Key,item.Key,2,false),Definition=definition,Inventory=item,Ground=item.Ground,Ounces=item.WeightOunces,Held=false,Melee=definition.IsMelee,Reach=definition.MaxRangeFeet,Type=DamageTypes.Index(definition.DamageType)};
        }
        if(source==-1&&combat.MonsterWeaponIsHeld&&!UndeadClaimsCustomWeapon()){
            var packet=new MonsterDamage{Bonus=monster.Bonus};Array.Copy(monster.Counts,packet.Counts,5);Array.Copy(monster.Types,packet.Types,6);
            yield return new UndeadItem{Object=equipmentObjects.Weapon(-1,"custom","木桩手持武器",2,false),Ground=monsterGroundWeapon,Ounces=combat.MonsterWeaponOunces,Custom=true,Damage=packet,Melee=combat.MonsterAttackIsMelee,Reach=combat.MonsterAttackRange,Type=monsterBaseWeaponType,Held=!monsterGroundWeapon.Dropped};
        }
    }
    bool UndeadClaimsCustomWeapon()=>undeadFollowers.Values.Any(f=>f.Unit.Alive&&f.Items.Any(i=>i.Custom));
    bool UndeadEquipmentPose(EquipmentObjectRecord item,out EquipmentPose pose)
    {
        pose=null;UndeadFollower follower;if(!item.Follower.HasValue||!undeadFollowers.TryGetValue(item.Follower.Value,out follower))return false;
        var weapon=follower.Items.Find(i=>i.Object==item);
        if(weapon!=null&&weapon.Ground.Dropped){var ground=weapon.Ground;pose=new EquipmentPose{X=(ground.CenterX2-1)*2.5,Y=(ground.CenterY2-1)*2.5,Submerged=ground.Submerged,Location="亡灵掉落武器"};return true;}
        if(follower.Unit.Alive){bool found=EquipmentOnActor(follower.Unit.Id,out pose);if(found&&weapon!=null&&follower.Equipped!=weapon)pose.Carrier=null;return found;}
        return follower.Corpse!=0&&EquipmentOnRemains(follower.Corpse,out pose);
    }
    int UndeadWeaponModifier(UndeadFollower follower)
    {
        var weapon=follower.Equipped;int strength=AcidSplash.Modifier(follower.Rules.Ability(0)),dex=AcidSplash.Modifier(follower.Rules.Ability(1));
        return weapon==null?strength:weapon.Definition==null?strength:weapon.Definition.HasFinesse?Math.Max(strength,dex):weapon.Definition.IsMelee?strength:dex;
    }
    void RefreshUndeadFollowers()
    {
        foreach(int id in undeadControl.Refresh(worldRounds)){var lost=undeadFollowers[id];lost.Unit.ClearOrders();history.Insert(0,lost.Unit.Name+"控制到期，转为中立停留；可重新施法恢复控制。");}
        foreach(var follower in undeadFollowers.Values){
            var unit=follower.Unit;
            if(!unit.Alive){
                if(follower.Corpse!=0)continue;
                foreach(var item in follower.Items)if(item==follower.Equipped||item.Custom)DropUndeadItem(follower,item);
                follower.Weight.SetWeaponWeight(follower.Items.Where(i=>!i.Ground.Dropped).Sum(i=>i.Ounces));var corpse=map.Remains.Add(follower.Owner,unit.X,unit.Y,1,unit.Name+"遗体",follower.Weight,follower.ArmorIndex,follower.Shield);
                if(corpse==null)continue;corpse.CreatureType=13;follower.Corpse=corpse.Id;carriedWeapons.FollowerRemains(unit.Id,corpse.Id);foreach(var item in equipmentObjects.Objects)if(item.Follower==unit.Id){item.Follower=null;if(item.Kind!=EquipmentObjectKind.Weapon){item.Place=EquipmentObjectPlace.Remains;item.RemainsId=corpse.Id;}}undeadControl.Destroy(unit.Id);continue;
            }
            if(unit.SleepStage==2){foreach(var item in follower.Items)if(item==follower.Equipped)DropUndeadItem(follower,item);}
            int ounces=follower.Items.Where(i=>!i.Ground.Dropped).Sum(i=>i.Ounces);follower.Weight.SetWeaponWeight(ounces);
            long cap=CarryingRules.CapacityOunces(follower.Rules.Ability(0),2),gear=follower.Weight.GearOunces;
            int load=gear<=cap?int.MaxValue:gear<=cap*2?5:0;
            int armorPenalty=follower.ArmorIndex>0&&ArmorDefinition.All[follower.ArmorIndex].Strength>follower.Rules.Ability(0)?10:0;
            unit.LoadSpeedLimit=Math.Min(load,Math.Max(0,follower.Rules.Speed-armorPenalty));
        }
    }
    void DropUndeadItem(UndeadFollower follower,UndeadItem item)
    {
        if(item.Ground.Dropped)return;var unit=follower.Unit;
        bool water=WeaponFallsInWater(new Vector2Int(unit.X,unit.Y),unit.Spaces);
        bool done=item.Inventory!=null?carriedWeapons.FollowerDrop(item.Inventory.Key,unit.Id,unit.X,unit.Y,water):item.Ground.Drop(unit.X,unit.Y,1,water);
        if(done&&follower.Equipped==item){follower.Equipped=null;follower.Grip.DropHeld();}
    }
    void AdoptUndeadEquipment(UndeadFollower follower)
    {

        foreach(var item in follower.Items){item.Object.Follower=follower.Unit.Id;if(item.Inventory!=null)carriedWeapons.GiveToFollower(item.Inventory.Key,follower.Unit.Id);}
        var inherited=equipmentObjects.Objects.Where(item=>item.Kind!=EquipmentObjectKind.Weapon&&(item.Place==EquipmentObjectPlace.Remains&&item.RemainsId==follower.Source||item.Place==EquipmentObjectPlace.Equipped&&item.Owner==follower.Source||item.Place==EquipmentObjectPlace.Carried&&item.Carrier==follower.Source)).ToArray();
        if(follower.Source==0||follower.Source==-1)equipmentObjects.Archive(follower.Source,follower.Source);
        foreach(var item in inherited){item.Follower=follower.Unit.Id;item.Place=EquipmentObjectPlace.Carried;item.Carrier=follower.Unit.Id;item.RemainsId=0;}
        follower.Equipped=follower.Items.FirstOrDefault(i=>i.Held&&!i.Ground.Dropped);follower.Grip.DropHeld();
    }
    void ResetUndeadFollowers(){foreach(var restore in undeadEquipmentReset.Values)restore();undeadEquipmentReset.Clear();CancelAnimateDead();undeadControl.Clear();undeadFollowers.Clear();undeadSaves.Clear();}
}
