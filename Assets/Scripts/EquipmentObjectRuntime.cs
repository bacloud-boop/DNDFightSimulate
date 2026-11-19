using System;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly EquipmentObjectCatalog equipmentObjects=new EquipmentObjectCatalog();
    sealed class EquipmentPose
    {
        public double X,Y;
        public int? Carrier,ScaleActor;
        public bool Submerged,Melded,Hidden;
        public string Location;
    }
    static void EquipmentCenter(EquipmentPose pose,Vector2Int cell,int spaces){pose.X=(cell.x+(spaces-1)*.5)*5;pose.Y=(cell.y+(spaces-1)*.5)*5;}
    void SyncEquipmentObjects()
    {
        bool preparing=!combat.BattleStarted;
        foreach(var item in carriedWeapons.Items)equipmentObjects.Weapon(0,item.Key,item.Key,NaturalPlayerSize,preparing);
        equipmentObjects.Weapon(-1,"custom","木桩手持武器",monsterProfile.SizeIndex,preparing);
        foreach(int actor in new[]{0,-1}){
            var armor=actor==0?playerArmor:monsterArmor;int size=actor==0?NaturalPlayerSize:monsterProfile.SizeIndex;
            equipmentObjects.Slot(actor,EquipmentObjectKind.Armor,armor.ArmorIndex,armor.Armor.Name,armor.WearingArmor,size,preparing);
            equipmentObjects.Slot(actor,EquipmentObjectKind.Shield,0,"盾牌",armor.Shield,size,preparing);
        }
        foreach(var body in map.Remains.Bodies)if(body.NonWeaponGearRecovered)equipmentObjects.Recover(body.Id,0);
    }
    bool EquipmentOnActor(int actor,out EquipmentPose pose)
    {
        pose=new EquipmentPose();
        if(actor==BattleMap.RescueId){if(!map.RescuePosition.HasValue)return false;EquipmentCenter(pose,map.RescuePosition.Value,map.RescueSpaces);pose.Carrier=rescueCarrier.Dead?(int?)null:actor;pose.Submerged=map.RescueAquatic.Submerged;pose.Location="救援者携带";return true;}
        if(actor!=0&&actor!=-1)return false;
        bool player=actor==0;var cell=player?map.Player:map.Monster;int spaces=player?map.PlayerSpaces:map.MonsterSpaces;
        if(map.IsCarried(player)){EquipmentCenter(pose,map.RescuePosition.Value,map.RescueSpaces);pose.Carrier=BattleMap.RescueId;pose.Submerged=map.RescueAquatic.Submerged;pose.Location="救援者肩扛的遗体装备";}
        else {EquipmentCenter(pose,cell,spaces);pose.Carrier=(player?!combat.PlayerDead:combat.Hp>0)?(int?)actor:null;pose.Submerged=player?map.PlayerAquatic.Submerged:map.MonsterAquatic.Submerged;pose.Location=pose.Carrier.HasValue?(player?"玩家携带／穿戴":"木桩携带／穿戴"):"现身体遗体上的装备";}
        pose.ScaleActor=actor;pose.Melded=player?PlayerInBeastForm:MonsterPolymorphed;pose.Hidden=map.IsContained(player);return true;
    }
    bool EquipmentOnRemains(int id,out EquipmentPose pose)
    {
        pose=new EquipmentPose();var body=map.Remains.Find(id);if(body==null)return false;
        if(map.RescueCargoId==id&&!map.RescueCargoDragging&&map.RescuePosition.HasValue){EquipmentCenter(pose,map.RescuePosition.Value,map.RescueSpaces);pose.Carrier=BattleMap.RescueId;pose.Submerged=map.RescueAquatic.Submerged;pose.Location="救援者肩扛的旧躯体装备";}
        else {EquipmentCenter(pose,new Vector2Int(body.X,body.Y),body.Spaces);pose.Submerged=WeaponFallsInWater(new Vector2Int(body.X,body.Y),body.Spaces);pose.Location=body.Name+"上的装备";}
        return true;
    }
    bool EquipmentObjectPose(EquipmentObjectRecord item,out EquipmentPose pose)
    {
        pose=null;if(item==null)return false;
        if(item.Kind==EquipmentObjectKind.Weapon){
            GroundWeaponState ground;
            if(item.Owner==0){var weapon=carriedWeapons.Find(item.InventoryKey);if(weapon==null||weapon.Location==WeaponLocation.Stored)return false;
                if(weapon.Location==WeaponLocation.Carried)return EquipmentOnActor(0,out pose);
                if(weapon.Location==WeaponLocation.Remains)return EquipmentOnRemains(weapon.RemainsId,out pose);ground=weapon.Ground;
            }else {if(!monsterGroundWeapon.Dropped){if(!combat.MonsterWeaponIsHeld)return false;return EquipmentOnActor(-1,out pose);}ground=monsterGroundWeapon;}
            if(!ground.Dropped)return false;pose=new EquipmentPose{X=(ground.CenterX2-1)*2.5,Y=(ground.CenterY2-1)*2.5,Submerged=ground.Submerged,Location=ground.Submerged?"水下落地武器":"落地武器"};return true;
        }
        if(item.Place==EquipmentObjectPlace.Unavailable)return false;
        if(item.Place==EquipmentObjectPlace.Remains)return EquipmentOnRemains(item.RemainsId,out pose);
        if(item.Place==EquipmentObjectPlace.Carried){bool found=EquipmentOnActor(item.Carrier,out pose);if(found)pose.Location="回收后未穿戴携带的装备";return found;}
        return EquipmentOnActor(item.Owner,out pose);
    }
    int EffectiveObjectSize(EquipmentObjectRecord item,EquipmentPose pose)
    {
        int delta=pose!=null&&pose.ScaleActor.HasValue&&sizeChange.Applies(pose.ScaleActor.Value==0)?sizeChange.Delta:0;
        return Math.Max(0,Math.Min(5,item.Size+delta));
    }
    bool EquipmentInTouch(EquipmentPose pose)
    {
        if(pose==null||pose.Melded)return false;
        if(pose.Carrier==0)return true;
        if(pose.Hidden||pose.Submerged&&!map.PlayerAquatic.Submerged)return false;
        double x=Math.Max(map.Player.x,Math.Min(map.Player.x+map.PlayerSpaces-1,pose.X/5)),y=Math.Max(map.Player.y,Math.Min(map.Player.y+map.PlayerSpaces-1,pose.Y/5));
        return Math.Round(VisionState.Distance(x*5,y*5,pose.X,pose.Y),MidpointRounding.ToEven)<=5;
    }
    string EquipmentHandleError(EquipmentObjectRecord item)
    {
        if(page!="battle"||!map.Locked||rolling||spellTargeting||reactionPending||draggingToken>=0)return "请先完成当前操作。";
        if(combat.PlayerHp<=0||combat.PlayerDead||combat.PlayerParalyzed||PlayerInBeastForm||summonTurns.Active)return "玩家当前无法进行物件交互。";
        EquipmentPose pose;if(!EquipmentObjectPose(item,out pose)||!EquipmentInTouch(pose))return "装备不在可接触的5尺范围，或已融入／位于水下。";
        if(pose.Carrier.HasValue&&pose.Carrier!=0)return "不能直接操作其他角色穿戴或携带的装备。";
        if(PlayerHasShield&&!weaponGrip.Stowed)return "请先收起武器，腾出空手进行遮盖／揭开。";
        return "";
    }
    void ToggleEquipmentCover(EquipmentObjectRecord item)
    {
        if(!combat.BattleStarted){equipmentObjects.Configure(item.Id,item.Size,!item.Covered);RefreshLightSpell();return;}
        RefreshLightSpell();if(EquipmentHandleError(item)!="")return;
        if(equipmentObjects.Cover(item.Id,!item.Covered,()=>weaponGrip.TryInteract(combat.Round,worldRounds,()=>combat.TryUseObjectAction()))){combat.MarkBattleStarted();history.Insert(0,(item.Covered?"完全遮盖":"揭开")+item.Name+"；使用本回合物件交互，额外交互消耗动作。");RefreshLightSpell();}
    }
}
