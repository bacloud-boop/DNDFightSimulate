using System.Collections;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly ElementalWeaponEffect elementalWeapon=new ElementalWeaponEffect();int elementalObject,elementalEnergy;
    int PlayerWeaponObject(Weapon weapon)=>equipmentObjects.Weapon(0,weapon.InventoryKey,weapon.InventoryKey,NaturalPlayerSize,!combat.BattleStarted).Id;
    int MonsterWeaponObject=>!MonsterPolymorphed&&combat.MonsterAttackIsWeapon&&combat.MonsterWeaponIsHeld&&!monsterGroundWeapon.Dropped?equipmentObjects.Weapon(-1,"custom","木桩手持武器",monsterProfile.SizeIndex,!combat.BattleStarted).Id:0;
    int EnchantedWeaponAttack(int id)=>timedBuffs.MagicObjectBonus(id,worldRounds)+elementalWeapon.AttackBonus(id,worldRounds);
    bool WeaponObjectMagical(int id)=>EnchantedWeaponAttack(id)>0;
    string ElementalWeaponError(int id)
    {
        SyncEquipmentObjects();var item=equipmentObjects.Find(id);EquipmentPose pose;
        if(item==null||item.Kind!=EquipmentObjectKind.Weapon||!EquipmentObjectPose(item,out pose))return "请选择场内存在的实体武器。";
        if(!EquipmentInTouch(pose))return "武器须在5尺内可接触；融入形态不可触碰，水下武器须先潜水。";
        if(WeaponObjectMagical(id))return "目标已经是魔法武器，请选择非魔法武器。";
        return worldRounds<0||worldRounds>long.MaxValue-600?"世界时间超出范围。":"";
    }
    void DrawElementalWeaponChoices()
    {
        elementalEnergy=GUILayout.SelectionGrid(elementalEnergy,new[]{"强酸","寒冷","火焰","闪电","雷鸣"},3,button);
        SyncEquipmentObjects();foreach(var item in equipmentObjects.Objects){EquipmentPose pose;if(item.Kind!=EquipmentObjectKind.Weapon||!EquipmentObjectPose(item,out pose))continue;
            if(GUILayout.Button((elementalObject==item.Id?"✓ ":"")+item.Name+" · "+pose.Location+(WeaponObjectMagical(item.Id)?" · 魔法武器":""),button))elementalObject=item.Id;
        }
        GUILayout.Label("触碰非魔法武器，命中＋1、额外1d4所选能量；五至六环＋2／2d4，七环以上＋3／3d4。暴击只翻倍额外骰，不增加固定伤害。附魔跟随这一件武器，切换、掉落、转交或融入不会单独终止；自然武器与法术生成的焰刃不属于物件目标。专注至多1小时。",small);
    }
    void CastElementalWeapon(int id,int slot,int type)
    {
        if(ElementalWeaponError(id)!=""||!elementalWeapon.Begin(id,slot,type,worldRounds)){history.Insert(0,"元素武器：目标不再合法，施法动作已消耗。");return;}
        concentrationSpell="元素武器";concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=elementalWeapon.Expires;
        history.Insert(0,"元素武器："+equipmentObjects.Find(id).Name+"命中＋"+elementalWeapon.Bonus+"、额外"+elementalWeapon.Bonus+"d4 "+DamageTypes.Names[type]+"，专注至多1小时。");
    }
    IEnumerator AddEnchantedWeaponDamage(int id,int baseType,bool critical,long[] raw,bool includeMagic=true)
    {
        if(includeMagic&&baseType>=0&&baseType<13)raw[baseType]+=timedBuffs.MagicObjectBonus(id,worldRounds);
        int count=elementalWeapon.AttackBonus(id,worldRounds)*(critical?2:1);if(count==0)yield break;
        var rolls=new int[count];int sum=0;for(int i=0;i<count;i++){rolls[i]=Random.Range(1,5);sum+=rolls[i];}
        yield return PresentRoll(4,rolls,"元素武器：额外 "+sum+" "+DamageTypes.Names[elementalWeapon.DamageType]+(critical?"（暴击骰翻倍）":""));
        raw[elementalWeapon.DamageType]+=sum;
    }
}
