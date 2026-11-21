using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    SummonedUnit steed;
    OtherworldlySteedRules steedRules;
    int steedCaster=9,steedKind;
    bool riderWasProne,mountWasProne,mountFallPending;
    string steedColumn="2",steedRow="2",steedTeleportColumn="2",steedTeleportRow="2";
    bool IsSteed(SummonedUnit u)=>u!=null&&u==steed;
    bool ControlledSteed=>steed!=null&&steed.Alive&&map.RiderMountId==steed.Id&&combat.PlayerHp>0&&!combat.PlayerDead&&!combat.PlayerParalyzed&&!combat.PlayerFeignDeath;
    long RiderWeight()
    {
        var body=PlayerInBeastForm?BeastWeight(PlayerBeast.name).Weight:playerWeight.Weight;
        if(!body.Known)return -1;
        long mass=(long)body.BodyPounds.Value*16;
        if(sizeChange.Applies(true))mass=sizeChange.Delta>0?mass*8:System.Math.Max(1,mass/8);
        return mass+PlayerEquipmentWeight+(playerCorpseLoad.Loaded?playerCorpseLoad.WeightOunces:0);
    }
    int SummonAbility(SummonedUnit u,int ability)=>IsSteed(u)?steedRules.Ability(ability):bestialSpirit.Rules.Ability(ability);
    int SummonAttackBonus(SummonedUnit u)=>IsSteed(u)?classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(steedCaster)]):bestialSpirit.Rules.AttackBonus;
    int SummonDamageBonus(SummonedUnit u)=>IsSteed(u)?steedRules.DamageBonus:bestialSpirit.Rules.FixedDamage;
    int SummonDamageType(SummonedUnit u)=>IsSteed(u)?steedRules.DamageType:2;
    bool SteedCell(out Vector2Int cell){int x,y;cell=Vector2Int.zero;if(!int.TryParse(steedColumn,out x)||!int.TryParse(steedRow,out y))return false;cell=new Vector2Int(x-1,y-1);return map.Inside(cell,2);}
    void DrawFindSteedChoices()
    {
        steedKind=GUILayout.SelectionGrid(steedKind,new[]{"天界","妖精","邪魔"},3,button);
        GUILayout.Label("大型坐骑，输入左上格列／行。骑乘后双方按完整2×2占地判定距离和范围。四环以上的飞行继续暂缓。",small);
        steedColumn=IntegerField(steedColumn,false);steedRow=IntegerField(steedRow,false);
        Vector2Int cell;if(SteedCell(out cell))GUILayout.Label("距玩家 "+map.DistanceToCell(true,cell)+"尺",small);
    }
    string FindSteedError(int slot)
    {
        if(slot<2||slot>9||nextSummonId==int.MaxValue)return "施法环阶或单位编号不合法。";
        Vector2Int cell;if(!SteedCell(out cell))return "坐骑完整2×2占地须位于地图内。";
        if(map.DistanceToCell(true,cell)>30)return "召唤位置超出30尺。";
        var candidate=new OtherworldlySteedRules(slot,(SteedKind)steedKind).CreateUnit(nextSummonId);
        return map.CanAddSummon(candidate,cell)?"":"位置被占据，或会阻挡恢复原形。";
    }
    void ApplyFindSteed(int caster,int slot)
    {
        if(FindSteedError(slot)!=""){history.Insert(0,"寻获坐骑：落位已失效。");return;}
        Vector2Int cell;SteedCell(out cell);
        var rules=new OtherworldlySteedRules(slot,(SteedKind)steedKind);var unit=rules.CreateUnit(nextSummonId);
        if(!map.AddSummon(unit,cell))return;
        DismissSteed();steed=unit;steedRules=rules;steedCaster=caster;nextSummonId++;
        bool changed;steed.ReceiveVerbalOrders(SummonCommand.AssistCombat,out changed);
        history.Insert(0,"寻获坐骑："+new[]{"天界","妖精","邪魔"}[steedKind]+"异界坐骑，HP "+unit.MaxHp+"，AC "+unit.Armor+"，步行60尺。无需专注，旧坐骑消失。");
    }
    void DismissSteed(){if(steed==null)return;map.RemoveSummon(steed.Id);steed=null;steedRules=null;}
    void RefreshSteed()
    {
        if(steed==null)return;
        if(!steed.Alive||combat.PlayerDead){DismissSteed();return;}
        if(map.Riding&&!RidingState.SizeAllowed(EffectiveCreatureSize(true),steed.Size)){
            if(map.DismountRider())history.Insert(0,"体型变化正常生效：不再满足骑乘条件，已在最近合法位置强制下马。");
        }
        steed.LoadSpeedLimit=map.RiderMountId==steed.Id?(RiderWeight()<0?0:CarryingRules.DragSpeedOunces(18,steed.Size,RiderWeight(),steed.Speed)):int.MaxValue;
        if(map.RiderMountId==steed.Id){
            if((combat.PlayerProne&&!riderWasProne)||(steed.Prone&&!mountWasProne))mountFallPending=true;
            if(!combat.Aftermath&&ControlledSteed&&!steed.TurnActive&&!summonTurns.Active)steed.BeginTurn(combat.Round);
        }
        riderWasProne=combat.PlayerProne;mountWasProne=steed.Prone;
        if(mountFallPending&&!rolling&&!summonActing&&!greaseResolving){mountFallPending=false;StartCoroutine(ResolveMountedFall());}
    }
    IEnumerator ResolveMountedFall()
    {
        if(!map.Riding)yield break;rolling=true;
        try{var roll=RollArmorSave(true,1);int modifier=PlayerSavingThrowModifier(1);var adjustment=new SaveAdjustment();yield return ApplySaveAdjustments(true,false,adjustment,1);modifier+=adjustment.Value;
            yield return PresentRoll(20,roll.Dice,"骑乘落马 · 敏捷豁免 "+roll.Value+" + "+modifier+" / DC 10");
            if(combat.PlayerHp==0||combat.PlayerParalyzed||(long)roll.Value+modifier<10){if(map.DismountRider()){combat.PlayerProne=true;history.Insert(0,"落马：最近合法位置着地并倒地。");}}
        }finally{rolling=false;}
    }
    void DrawRidingControls()
    {
        if(steed==null||!steed.Alive)return;
        GUILayout.Label(map.Riding?"骑乘中 · 共享坐骑完整占地":"异界坐骑 · 可骑乘",heading);
        GUILayout.Label("坐骑携带上限 "+WeightUnits.Format(CarryingRules.CapacityOunces(18,steed.Size))+"；骑手含装备与搬运负重 "+(RiderWeight()<0?"形态体重尚未填写":WeightUnits.Format(RiderWeight())),small);
        bool enabled=GUI.enabled;GUI.enabled=!rolling&&!summonTurns.Active&&combat.PlayerCanAct;
        int cost=map.PlayerSpeed/2;
        if(!map.Riding){GUI.enabled=GUI.enabled&&RidingState.SizeAllowed(EffectiveCreatureSize(true),steed.Size)&&map.CanMountSummon(steed.Id)&&map.PlayerSpeed>0&&map.MovementRemaining>=cost&&RiderWeight()>=0&&CarryingRules.CanCarryOunces(18,steed.Size,0,RiderWeight());
            if(GUILayout.Button("上马 · "+cost+"尺玩家移动力",button)&&map.SpendPlayerMovement(cost)){map.MountSummon(steed.Id);combat.MarkBattleStarted();RefreshSteed();}}
        else {Vector2Int cell;GUI.enabled=GUI.enabled&&map.MovementRemaining>=cost&&map.FindDismount(map.RiderBodySpaces,out cell)&&FearAllowsMovement(0,cell);
            if(GUILayout.Button("下马 · "+cost+"尺玩家移动力",button)&&map.DismountRider()){map.SpendPlayerMovement(cost);combat.MarkBattleStarted();}}
        GUI.enabled=enabled;
    }
    void ApplySteedLifeBond(int actual,int spellLevel)
    {
        if(actual<=0||spellLevel<1||steed==null||!steed.Alive||BattleMap.BodyDistance(map.Player,map.PlayerSpaces,new Vector2Int(steed.X,steed.Y),steed.Spaces)>5)return;
        int restored=steed.Heal(actual);history.Insert(0,"生命羁绊：坐骑恢复 "+restored+" HP（玩家实际治疗 "+actual+"）。");
    }
}
