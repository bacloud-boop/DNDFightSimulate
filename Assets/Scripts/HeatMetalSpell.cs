using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly HeatMetalEffect heatMetal=new HeatMetalEffect();int heatObject;
    bool heatPromptPending;int heatPromptChoice,heatPromptKind,heatPromptActor;
    bool HeatDisadvantage(int actor)=>heatMetal.Disadvantage(actor,worldRounds);
    int HeatDistance(EquipmentPose pose)
    {
        if(pose.Scene!=map.SceneOf(0))return int.MaxValue;
        double x=Math.Max(map.Player.x,Math.Min(map.Player.x+map.PlayerSpaces-1,pose.X/5)),y=Math.Max(map.Player.y,Math.Min(map.Player.y+map.PlayerSpaces-1,pose.Y/5));
        return (int)Math.Round(VisionState.Distance(x*5,y*5,pose.X,pose.Y));
    }
    string HeatObjectError(int id,bool casting)
    {
        SyncEquipmentObjects();var item=equipmentObjects.Find(id);EquipmentPose pose;
        if(item==null||!item.Metal||!EquipmentObjectPose(item,out pose))return "请选择场内存在的金属武器、护甲或盾牌。";
        if(pose.Melded)return "物件已融入形态，不能加热。";
        if(HeatDistance(pose)>60)return "金属物件超出60尺。";
        if(casting){
            bool internalObject=map.IsContained(true)&&item.Owner==0&&pose.Hidden;
            if(item.Covered||pose.Hidden&&!internalObject||map.IsContained(true)&&!internalObject)return "施法时必须看见该金属物件。";
            if(pose.ScaleActor.HasValue&&(pose.ScaleActor==0||pose.ScaleActor==-1)&&Visibility(true,pose.ScaleActor==0)>=2)return "无法看见穿戴／携带该物件的生物及其装备。";
            if(VisibilityBetween(true,map.Player.x,map.Player.y,map.PlayerSpaces,pose.X/5,pose.Y/5,1,pose.ScaleActor.HasValue&&InvisibleWithFaerie(pose.ScaleActor==0))>=2)return "金属物件被黑暗、云雾或失明遮挡。";
        }
        return worldRounds<0||worldRounds>long.MaxValue-10?"世界时间超出范围。":"";
    }
    int? HeatContact(EquipmentObjectRecord item)
    {
        EquipmentPose pose;if(item==null||!EquipmentObjectPose(item,out pose)||pose.Melded)return null;
        if(item.Follower.HasValue){UndeadFollower f;if(!undeadFollowers.TryGetValue(item.Follower.Value,out f)||!f.Unit.Alive)return null;return item.Kind==EquipmentObjectKind.Weapon?f.Equipped!=null&&f.Equipped.Object==item?(int?)f.Unit.Id:null:(int?)f.Unit.Id;}
        int owner=item.Owner;if(owner!=0&&owner!=-1)return null;
        bool held=owner==0?carriedWeapons.Carried(item.InventoryKey??"")&&selectedWeapon.InventoryKey==item.InventoryKey&&!weaponGrip.Stowed:combat.MonsterWeaponIsHeld&&!monsterGroundWeapon.Dropped;
        return HeatMetalEffect.Touching(item.Kind,item.Place,held,pose.Melded,owner==0?!combat.PlayerDead:combat.Hp>0)?(int?)owner:null;
    }
    void DrawHeatMetalChoices()
    {
        SyncEquipmentObjects();foreach(var item in equipmentObjects.Objects){EquipmentPose pose;if(!item.Metal||!EquipmentObjectPose(item,out pose))continue;
            if(GUILayout.Button((heatObject==item.Id?"✓ ":"")+item.Name+" · "+ActorName(item.Owner)+"\n"+pose.Location+(HeatContact(item).HasValue?" · 接触身体":" · 未接触身体"),new GUIStyle(button){fontSize=15,fixedHeight=58}))heatObject=item.Id;
        }
        GUILayout.Label("仅手持或穿戴算接触。收起武器／放入背包、掉落或融入形态后不受后续脉冲伤害；重新持握不立即受伤。护甲与盾牌沿用战斗中锁定穿脱规则。",small);
    }
    IEnumerator CastHeatMetal(int id,int caster,int slot)
    {
        if(HeatObjectError(id,true)!=""){history.Insert(0,"灼热金属目标已失效，施法动作已消耗。");yield break;}
        heatMetal.Begin(id,slot,8L+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(caster)]),combat.Round,worldRounds);
        concentrationSpell="灼热金属";concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=worldRounds+10;
        yield return HeatMetalDamage();
    }
    bool DropHeatedWeapon(EquipmentObjectRecord item,int actor)
    {
        if(item.Kind!=EquipmentObjectKind.Weapon||HeatContact(item)!=actor)return false;
        if(actor>0){var f=Undead(FindSummon(actor));if(f==null||f.Equipped==null||f.Equipped.Object!=item)return false;DropUndeadItem(f,f.Equipped);return !HeatContact(item).HasValue;}
        if(actor==0){carriedWeapons.BeginBattle();if(!carriedWeapons.Drop(item.InventoryKey,map.Player.x,map.Player.y,map.PlayerSpaces,WeaponFallsInWater(map.Player,map.PlayerSpaces),WeaponContainer(true)))return false;weaponGrip.DropHeld();RefreshShillelagh();}
        else monsterGroundWeapon.Drop(map.Monster.x,map.Monster.y,map.MonsterSpaces,WeaponFallsInWater(map.Monster,map.MonsterSpaces),WeaponContainer(false));
        SyncWeaponInventory();return !HeatContact(item).HasValue;
    }
    IEnumerator HeatDropSave(EquipmentObjectRecord item,int actor,long dc)
    {
        Vector2Int p;int n;string name;if(HeatContact(item)!=actor||!HealingHandsTarget(actor,out p,out n,out name))yield break;
        var failed=new HashSet<int>();yield return RollMappedAbilitySave(actor,0,dc,failed,"灼热金属 · 放手",2);
        bool dropped=false;
        if(item.Kind==EquipmentObjectKind.Weapon&&HeatContact(item)==actor){
            if(failed.Contains(actor))dropped=DropHeatedWeapon(item,actor);
            else {heatPromptKind=1;heatPromptActor=actor;heatPromptChoice=0;heatPromptPending=true;while(heatPromptChoice==0)yield return null;heatPromptPending=false;if(heatPromptChoice==2)dropped=DropHeatedWeapon(item,actor);}
        }
        if(!dropped){heatMetal.Retained(actor,worldRounds);history.Insert(0,name+"未放下灼热物件：攻击与属性／技能检定劣势，至玩家下回合开始或法术结束。豁免成功不免除保留物件的劣势。");}
        else history.Insert(0,name+"放下灼热武器，接入地面／水下拾回规则；本次不施加灼热金属劣势。");
    }
    IEnumerator HeatMetalDamage()
    {
        var item=equipmentObjects.Find(heatMetal.ObjectId);int? contact=HeatContact(item);if(!contact.HasValue){history.Insert(0,"灼热金属：物件没有手持或穿戴者，本次不造成生物伤害。");yield break;}
        int actor=contact.Value,count=heatMetal.Dice;long dc=heatMetal.Dc;int[] rolls=new int[count];int raw=0;for(int i=0;i<count;i++){rolls[i]=UnityEngine.Random.Range(1,9);raw+=rolls[i];}
        yield return PresentRoll(8,rolls,"灼热金属："+count+"d8 = "+raw+" 火焰；伤害不进行豁免减半");
        var reduced=new RayDamage();yield return ReduceRayDamage(true,raw,"火焰","灼热金属",reduced);
        int applied=ApplyMappedAreaDamage(actor,0,reduced.Amount,5,"灼热金属");
        if(applied>0){
            bool concentrationFirst=false;
            if(actor==0&&IsConcentrating&&combat.PlayerHp>0){heatPromptKind=2;heatPromptChoice=0;heatPromptPending=true;while(heatPromptChoice==0)yield return null;heatPromptPending=false;concentrationFirst=heatPromptChoice==1;}
            if(actor==0&&concentrationFirst)yield return MaintainConcentration(applied);
            yield return HeatDropSave(item,actor,dc);
            if(actor==0&&!concentrationFirst)yield return MaintainConcentration(applied);
        }
        RefreshBestialSpirit();ReconcileFallenRescue();CheckBattleOutcome();
    }
    string HeatPulseError()
    {
        if(!heatMetal.Active(worldRounds))return "灼热金属已结束。";
        if(rolling||spellTargeting||reactionPending||draggingToken>=0)return "请等待当前操作结束。";
        if(!PlayerTurnAllowed||combat.PlayerHp<=0||combat.PlayerDead||combat.PlayerParalyzed||combat.PlayerFeignDeath||!combat.BonusActionAvailable||summonTurns.Active)return "不是玩家可行动的回合，或附赠动作已使用。";
        if(combat.Round<=heatMetal.CastRound)return "只能在施法后的后续玩家回合再次加热。";
        return HeatObjectError(heatMetal.ObjectId,false);
    }
    void DrawHeatMetalControl()
    {
        Surface(new Rect(350,110,480,745),paperTexture);if(Btn(new Rect(720,125,90,46),"关闭",!rolling)){panel="";return;}
        GUILayout.BeginArea(new Rect(375,190,430,620));GUILayout.Label("灼热金属",heading);var item=equipmentObjects.Find(heatMetal.ObjectId);GUILayout.Label(item==null?"物件已离场":item.Name,heading);
        GUILayout.Label("后续自己的回合可以消耗附赠动作，再次造成同样骰数的火焰伤害。仍须在60尺内；不重新施法、不需要再看见目标、不使用声音或姿势，也不延长专注。",label);
        string error=HeatPulseError();GUILayout.Label(error==""?"可再次加热":error,small);bool old=GUI.enabled;GUI.enabled=old&&error=="";
        if(GUILayout.Button("再次加热（附赠动作）",button))StartCoroutine(PulseHeatMetal());GUI.enabled=old;
        if(item!=null&&HeatContact(item)==0&&item.Kind==EquipmentObjectKind.Weapon&&PlayerTurnAllowed&&!rolling&&GUILayout.Button("放开受热武器（免费）",button))DropHeatedWeapon(item,0);
        GUILayout.EndArea();
    }
    IEnumerator PulseHeatMetal()
    {
        if(HeatPulseError()!=""||!combat.TryUseAftermathBonusAction())yield break;bool busy=rolling;rolling=true;panel="";
        try{yield return HeatMetalDamage();}finally{rolling=busy;}
    }
    void DrawHeatMetalPrompt()
    {
        if(!heatPromptPending)return;Fill(new Rect(0,0,1600,900),new Color(0,0,0,.92f));Surface(new Rect(465,245,670,380),paperTexture);
        GUI.Label(new Rect(495,270,610,80),heatPromptKind==1?TransitActorName(heatPromptActor)+"体质豁免成功 · 可选择放手":"受伤同时触发 · 选择豁免先后",heading);
        if(GUI.Button(new Rect(495,385,610,65),heatPromptKind==1?"保留武器（攻击及属性检定劣势）":"先进行专注豁免",button))heatPromptChoice=1;
        if(GUI.Button(new Rect(495,475,610,65),heatPromptKind==1?"主动放下武器":"先进行灼热金属放手豁免",button))heatPromptChoice=2;
    }
}
