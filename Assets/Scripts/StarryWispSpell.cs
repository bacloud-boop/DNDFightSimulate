using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly StarryWispEffect starryWisp=new StarryWispEffect();
    readonly HashSet<int> starryLightGroups=new HashSet<int>();
    int starryTarget=-1;
    string StarryWispError()
    {
        Vector2Int cell;int spaces;string name;
        if(!SummonTarget(starryTarget,out cell,out spaces,out name))return "请选择仍在场的生物。";
        if(BattleMap.BodyDistance(map.Player,map.PlayerSpaces,cell,spaces)>60)return "目标超出60尺。";
        return worldRounds<0||worldRounds>long.MaxValue-2?"世界时间超出范围。":"";
    }
    void DrawStarryChoices()
    {
        foreach(int id in new[]{0,-1})if(GUILayout.Button((starryTarget==id?"✓ ":"")+(id==0?"玩家（自身）":"木桩"),button))starryTarget=id;
        foreach(var unit in map.Summons)if(unit.Alive&&GUILayout.Button((starryTarget==unit.Id?"✓ ":"")+unit.Name,button))starryTarget=unit.Id;
        GUILayout.Label("命中后发出10尺微光，至施法者下个回合结束不能获得隐形收益；不额外给予攻击优势。物件攻击等待环境物件受损系统。",small);
    }
    void BindStarryWisp(int caster,int target)
    {
        if(!starryWisp.Bind(caster,target,worldRounds))return;
        RefreshStarryWisp();history.Insert(0,"点点星芒："+ActorName(target)+"发出10尺微光，至施法者下个回合结束不能获得隐形收益；原隐形法术及其专注保留。");
    }
    void RefreshStarryWisp()
    {
        var keep=new HashSet<int>();
        foreach(var mark in starryWisp.Active(worldRounds)){
            if(vision.ConsumeDispelledLightGroup(mark.Group)){starryWisp.Remove(mark.Group);continue;}
            Vector2Int cell;int spaces;
            if(mark.Target==0||mark.Target==-1){bool player=mark.Target==0;cell=player?map.Player:map.Monster;spaces=map.IsContained(player)?0:map.IsCarried(player)?map.RescueSpaces:player?map.PlayerSpaces:map.MonsterSpaces;}
            else {var unit=FindSummon(mark.Target);if(unit==null||!unit.Alive){starryWisp.Remove(mark.Group);continue;}cell=new Vector2Int(unit.X,unit.Y);spaces=unit.Spaces;}
            var points=new List<double[]>();AddFaeriePoints(points,cell,spaces);
            if(vision.SetDimLightGroup(mark.Group,points,10,0,mark.EndWorld,worldRounds))keep.Add(mark.Group);else starryWisp.Remove(mark.Group);
        }
        foreach(int group in starryLightGroups)if(!keep.Contains(group))vision.ClearLightGroup(group);
        starryLightGroups.Clear();foreach(int group in keep)starryLightGroups.Add(group);
    }
    void ClearStarryWisp(){starryWisp.Clear();foreach(int group in starryLightGroups)vision.ClearLightGroup(group);starryLightGroups.Clear();}
    void DrawStarryMap(Rect board)
    {
        foreach(var mark in starryWisp.Active(worldRounds)){
            Vector2Int cell;int spaces;string name;
            if(!SummonTarget(mark.Target,out cell,out spaces,out name))continue;
            var rect=BodyRect(board,cell,spaces);Fill(rect,new Color(.6f,.72f,1f,.25f));GUI.Label(new Rect(rect.x,rect.y,150,20),"星芒 · 微光10尺",small);
        }
    }
    IEnumerator ResolvePlayerStarryWisp(int caster,int target)
    {
        Vector2Int cell;int spaces;string name;if(!SummonTarget(target,out cell,out spaces,out name))yield break;
        var unit=target>0?FindSummon(target):null;bool primary=target==0||target==-1,player=target==0;
        bool sees=primary?Visibility(true,player)<2:VisibilityAt(true,cell,spaces)<2;
        bool targetSees=primary?Visibility(player,true)<2:SpiritSees(unit,0,map.Player,map.PlayerSpaces);
        int distance=BattleMap.BodyDistance(map.Player,map.PlayerSpaces,cell,spaces);
        bool helpless=primary?player?combat.PlayerHp==0||combat.PlayerParalyzed:combat.MonsterHelpless:unit.SleepStage==2;
        bool prone=primary?player?combat.PlayerProne:combat.MonsterProne:unit.Prone;
        bool restrained=primary?Entangled(player):unit.Restrained;
        bool advantage=fighter.ConsumeAdvantage(classes[0])|FaerieAdvantage(target,sees)|!targetSees|helpless|(prone&&distance<=5)|restrained;
        if(primary)advantage|=combat.ConsumeGuidingLight(player);
        bool disadvantage=!sees||combat.PlayerPoisoned||combat.PlayerProne||ResurrectionD20Disadvantage(true)||FearDisadvantage(true)||Entangled(true)||RayAttackDisadvantage(true,SpellAbilityIndex(caster))||prone&&distance>5;
        disadvantage|=combat.MonsterCanAct&&MonsterSeesPlayer&&map.DistanceFeet<=5;
        if(player)disadvantage|=senses.Blurred(worldRounds);
        if(unit!=null)disadvantage|=unit.Dodging&&unit.EffectiveSpeed>0&&targetSees;
        int first=UnityEngine.Random.Range(1,21),second=advantage!=disadvantage?UnityEngine.Random.Range(1,21):0,value=second==0?first:advantage?Math.Max(first,second):Math.Min(first,second);
        int bless=combat.PlayerBlessed?UnityEngine.Random.Range(1,5):0,bane=combat.PlayerBaned?UnityEngine.Random.Range(1,5):0,ward=player&&combat.PlayerBladeWard?UnityEngine.Random.Range(1,5):0;
        int bonus=classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(caster)])+PlayerAquaticD20Penalty+bless-bane-ward;
        int cover=map.CreatureCoverBonus(0,target),armor=(primary?player?combat.PlayerArmorClass:combat.MonsterArmorClass:unit.Armor)+cover;
        BreakOrdinaryInvisibility(true,"进行攻击检定");mirrorIntercepted=false;
        if(player&&value!=1&&(value==20||(long)value+bonus>=armor)){yield return ResolvePlayerHitDefense(value,(long)value+bonus,sees,PerceivesWithSpecialSense(true,true),cover);armor=combat.PlayerArmorClass+cover;}
        dice="点点星芒 · 远程法术攻击";yield return PresentRoll(20,second==0?new[]{first}:new[]{first,second},"采用 "+value+" + "+bonus+" / AC "+armor);
        if(bless>0)yield return PresentRoll(4,new[]{bless},"祝福术 +"+bless);if(bane>0)yield return PresentRoll(4,new[]{bane},"灾祸术 -"+bane);if(ward>0)yield return PresentRoll(4,new[]{ward},"剑刃防护 -"+ward);
        if(mirrorIntercepted||value==1||value!=20&&(long)value+bonus<armor){if(!mirrorIntercepted)fighter.RecordMiss(classes[0]);history.Insert(0,"点点星芒："+(mirrorIntercepted?"镜像拦截":"未命中")+"，不附加微光。");yield break;}
        bool critical=value==20||helpless&&distance<=5;int[] rolls=new int[AcidSplash.DiceCount(classes.TotalLevel)*(critical?2:1)];int raw=0;
        for(int i=0;i<rolls.Length;i++){rolls[i]=UnityEngine.Random.Range(1,9);raw+=rolls[i];}
        yield return PresentRoll(8,rolls,rolls.Length+"d8="+raw+" 光耀"+(critical?"（暴击）":""));
        var packet=new long[DamageTypes.Names.Length];packet[DamageTypes.Index("光耀")]=raw;
        if(primary){var extra=new ExtraMarkDamage();yield return RollMarkDamage(player,critical,extra);if(extra.Raw>0)packet[mark.DamageType]+=extra.Raw;}
        var reduced=new RayDamage();yield return ReduceRayPacket(true,packet,true,"点点星芒",reduced);
        BindStarryWisp(0,target);
        if(primary){int amount=ResolveRayPacket(player,reduced.Raw);if(player){combat.ApplyPlayerDamage(amount,critical,sourcePlayer:true);yield return MaintainConcentration(amount);}else combat.ApplyDamage(amount);history.Insert(0,"点点星芒对"+name+"造成 "+amount+" 点结算伤害。");}
        else {int amount=0;for(int i=0;i<reduced.Raw.Length;i++)amount+=(int)Math.Min(int.MaxValue-amount,unit.ResistsType(i)?reduced.Raw[i]/2:reduced.Raw[i]);unit.Damage(amount);history.Insert(0,"点点星芒对"+name+"造成 "+amount+" 光耀伤害。");}
        RefreshStarryWisp();CheckBattleOutcome();
    }
}
