using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class TrainingGame
{
    int playerHealingTarget,rescueHealingTarget=BattleMap.RescueId;
    SpeciesAbilityState HealingHandsState(int caster)=>caster==BattleMap.RescueId?rescueCarrier.SpeciesAbilities:SpeciesAbilities(caster==0);
    int HealingHandsProficiency(int caster)=>caster==BattleMap.RescueId?rescueCarrier.ProficiencyBonus:SpeciesProficiency(caster==0);
    bool HealingHandsSourceReady(int caster)
    {
        if(page!="battle"||!map.Locked||!HealingHandsState(caster).CanUse(Species2024.Aasimar,HealingHandsProficiency(caster),worldRounds))return false;
        if(caster==BattleMap.RescueId)return map.RescuePosition.HasValue&&rescueCarrier.Conscious&&rescueCarrier.TurnActive&&rescueCarrier.ActionsRemaining>0&&rescueCarrier.Species.Kind==Species2024.Aasimar;
        if(!SpeciesPhysicalActive(caster==0)||Species(caster==0).Kind!=Species2024.Aasimar)return false;
        return caster==0?(combat.Aftermath?combat.AftermathMagicActionAvailable:combat.MagicActionAvailable):combat.MonsterCanAct&&combat.MonsterActionAvailable;
    }
    bool HealingHandsTarget(int id,out Vector2Int cell,out int spaces,out string name)
    {
        cell=Vector2Int.zero;spaces=1;name="";if(!map.OnScene(id))return false;
        if(id==0&&!combat.PlayerDead){cell=map.Player;spaces=map.PlayerSpaces;name="玩家";return true;}
        if(id==-1&&combat.Hp>0){cell=map.Monster;spaces=map.MonsterSpaces;name="木桩";return true;}
        if(id==BattleMap.RescueId&&map.RescuePosition.HasValue&&!rescueCarrier.Dead){cell=map.RescuePosition.Value;spaces=map.RescueSpaces;name="救援者";return true;}
        var unit=FindSummon(id);if(unit==null)return false;cell=new Vector2Int(unit.X,unit.Y);spaces=unit.Spaces;name=unit.Name;return true;
    }
    bool HealingHandsCanTouch(int caster,int target)
    {
        Vector2Int from,to;int fromSpaces,toSpaces;string name;
        if(!HealingHandsTarget(caster,out from,out fromSpaces,out name)||!HealingHandsTarget(target,out to,out toSpaces,out name))return false;
        if(caster==target)return true;
        if((caster==0||caster==-1)&&map.IsContained(caster==0)&&target!=(caster==0?-1:0))return false;
        if((target==0||target==-1)&&map.IsContained(target==0)&&caster!=(target==0?-1:0))return false;
        return BattleMap.BodyDistance(from,fromSpaces,to,toSpaces)<=5;
    }
    IEnumerator UseHealingHands(int caster,int target,bool automatic=false)
    {
        if(!HealingHandsSourceReady(caster)||!HealingHandsCanTouch(caster,target)||(!automatic&&(rolling||spellTargeting||reactionPending||draggingToken>=0)))yield break;
        if(caster==BattleMap.RescueId){if(!rescueCarrier.UseHealingHands(worldRounds))yield break;}
        else {if(!(caster==0?combat.TryUseAftermathMagicAction():combat.TryUseMonsterAction()))yield break;HealingHandsState(caster).Use(Species2024.Aasimar,HealingHandsProficiency(caster),worldRounds);}
        bool busy=rolling;string saved=panel;panel="";rolling=true;
        try{
            int count=Math.Max(0,HealingHandsProficiency(caster));long total=0;var shown=new int[Math.Min(2,count)];
            for(int i=0;i<count;i++){int value=UnityEngine.Random.Range(1,5);total+=value;if(i<shown.Length)shown[i]=value;if((i&4095)==4095){dice="治愈之手 · 已投 "+(i+1)+" / "+count+" 个d4";yield return null;}}
            dice=ActorName(caster)+" · 治愈之手 · "+count+"d4";
            if(count>0)yield return PresentRoll(4,shown,count+"d4 合计 "+total+(count>2?"（展示前两骰，合计包含所有骰子）":""));
            int amount=(int)Math.Min(int.MaxValue,total),actual=0;string targetName;
            Vector2Int cell;int spaces;
            if(HealingHandsTarget(target,out cell,out spaces,out targetName)){
                int rolled=amount;amount=RollHealingAmount(target,amount,4L*count);
                if(target==0)actual=combat.HealPlayer(amount);else if(target==-1)actual=combat.HealMonster(amount);else if(target==BattleMap.RescueId)actual=rescueCarrier.Heal(amount);else {var unit=FindSummon(target);if(unit!=null)actual=unit.Heal(amount);}
                history.Insert(0,ActorName(caster)+"以魔法动作使用治愈之手："+count+"d4="+total+(amount!=rolled?"；希望信标按最大值 "+amount+" 治疗":"")+"，"+targetName+"实际恢复 "+actual+" HP；消耗本次长休的1次特性，不施放法术。");
            }
            dice="治愈之手 · 实际治疗 "+actual;ObserveDeaths();
        }finally{panel=saved;rolling=busy;}
    }
    IEnumerator TryMonsterSpeciesMagic(bool beforeSpell,bool unified=false)
    {
        if((unified?!(combat.MonsterActionAvailable&&SpeciesPhysicalActive(false)&&combat.MonsterCanAct):!SpeciesTurnPolicy.ShouldAttempt(monsterSpecies.BonusPriority,beforeSpell,combat.MonsterActionAvailable,SpeciesPhysicalActive(false),combat.MonsterCanAct))||monsterSpecies.Kind!=Species2024.Aasimar||combat.Hp>=combat.MaxHp||HealingHandsProficiency(-1)<=0)yield break;
        yield return UseHealingHands(-1,-1,true);
    }
    void DrawHealingHandsChoices(int caster)
    {
        GUILayout.Label("治愈之手：魔法动作，接触一个生物，恢复熟练加值数量的d4生命；每长休1次。不是施法，不消耗法术位。",small);
        var ids=new List<int>{0,-1};if(map.RescuePosition.HasValue)ids.Add(BattleMap.RescueId);foreach(var unit in map.SceneSummons)if(unit.Alive)ids.Add(unit.Id);
        int target=caster==0?playerHealingTarget:rescueHealingTarget;bool enabled=GUI.enabled;
        foreach(int id in ids){Vector2Int cell;int spaces;string name;if(!HealingHandsTarget(id,out cell,out spaces,out name))continue;
            GUI.enabled=enabled&&!rolling;bool near=HealingHandsCanTouch(caster,id);
            if(GUILayout.Button((target==id?"✓ ":"")+name+(near?" · 接触范围内":" · 无法接触"),button)){target=id;if(caster==0)playerHealingTarget=id;else rescueHealingTarget=id;}
        }
        GUI.enabled=enabled&&!rolling&&!spellTargeting&&!reactionPending&&draggingToken<0&&HealingHandsSourceReady(caster)&&HealingHandsCanTouch(caster,target);
        if(GUILayout.Button("使用治愈之手（动作） · 剩 "+HealingHandsState(caster).Remaining(Species2024.Aasimar,HealingHandsProficiency(caster)),button))StartCoroutine(UseHealingHands(caster,target));
        GUI.enabled=enabled;
    }
}
