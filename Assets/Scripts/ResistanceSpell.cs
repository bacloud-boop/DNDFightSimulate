using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly ResistanceEffect resistanceSpell=new ResistanceEffect();
    static readonly int[] ResistanceTypes={0,1,2,3,4,5,7,8,9,11,12};
    int resistanceTarget,resistanceType;bool resistanceWilling;
    sealed class ResistanceRoll{public int Actor,Type,Die;public long Before,After;}
    readonly Queue<ResistanceRoll> resistanceRolls=new Queue<ResistanceRoll>();bool drainingResistance;
    void InitializeResistance(){combat.IncomingDamage=(player,raw)=>ResistanceAdjusted(player?0:-1,raw);rescueCarrier.IncomingDamage=raw=>ResistanceAdjusted(BattleMap.RescueId,raw);}
    bool ResistanceWouldHurt(int actor,int type,long raw)
    {
        if(raw<=0)return false;
        if(actor==0||actor==-1){if(actor==0?combat.PlayerDead:combat.Hp<=0)return false;var packet=new long[14];packet[type]=raw;return combat.ResolveTypedPacket(actor==0,packet)[type]>0;}
        if(actor==BattleMap.RescueId)return !rescueCarrier.Dead&&(rescueCarrier.ResistsType(type)?raw/2:raw)>0;
        var unit=FindSummon(actor);if(unit==null||!unit.Alive||unit.IntrinsicImmunity!=null&&unit.IntrinsicImmunity(type)||SilenceUnitImmune(unit,DamageTypes.Names[type]))return false;
        return (unit.ResistsType(type)||type==5&&unit.Aquatic.Submerged?raw/2:raw)>0;
    }
    long[] ResistanceAdjusted(int actor,long[] raw)
    {
        int type=resistanceSpell.Type;
        if(!resistanceSpell.Available(actor,terrainTurn,worldRounds)||type>=raw.Length||!ResistanceWouldHurt(actor,type,raw[type]))return raw;
        int die=UnityEngine.Random.Range(1,5);var result=resistanceSpell.Reduce(actor,raw,die,terrainTurn,worldRounds);
        resistanceRolls.Enqueue(new ResistanceRoll{Actor=actor,Type=type,Die=die,Before=raw[type],After=result[type]});
        history.Insert(0,"抵抗术："+TransitActorName(actor)+"本回合首次有效"+DamageTypes.Names[type]+"伤害减1d4 = "+die+"（"+raw[type]+" → "+result[type]+"），再结算抗性／易伤。");return result;
    }
    IEnumerator DrainResistanceRolls()
    {
        if(drainingResistance||resistanceRolls.Count==0)yield break;bool busy=rolling;string previous=dice;drainingResistance=true;rolling=true;
        try{while(resistanceRolls.Count>0){var r=resistanceRolls.Dequeue();dice=TransitActorName(r.Actor)+" · 抵抗术减伤";yield return PresentRoll(4,new[]{r.Die},DamageTypes.Names[r.Type]+"原始伤害 "+r.Before+" - "+r.Die+" → "+r.After+"；抗性／易伤另算，每回合一次");}}
        finally{drainingResistance=false;rolling=busy;dice=previous;}
    }
    string ResistanceError()
    {
        Vector2Int p;int n;string name;if(!HealingHandsTarget(resistanceTarget,out p,out n,out name)||!BanishLiving(resistanceTarget))return "请选择存活的自愿目标。";
        if(!HealingHandsCanTouch(0,resistanceTarget))return "目标须可接触，距离不超过5尺。";
        if(resistanceTarget==-1&&!resistanceWilling)return "请确认木桩自愿接受保护。";
        return worldRounds<0||worldRounds>long.MaxValue-10?"世界时间超出范围。":"";
    }
    void DrawResistanceChoices()
    {
        GUILayout.Label("接触一名自愿生物，专注1分钟。每个全局回合首次受到所选类型的有效伤害时减1d4，再算抗性／易伤；不消耗反应。力场和心灵不能选择。",small);
        foreach(int actor in BreathActors())if(GUILayout.Button((resistanceTarget==actor?"✓ ":"")+TransitActorName(actor),button))resistanceTarget=actor;
        if(resistanceTarget==-1)resistanceWilling=GUILayout.Toggle(resistanceWilling,"确认木桩自愿接受抵抗术",label);
        var names=new string[ResistanceTypes.Length];for(int i=0;i<names.Length;i++)names[i]=DamageTypes.Names[ResistanceTypes[i]];
        resistanceType=GUILayout.SelectionGrid(resistanceType,names,3,button);
        GUILayout.Label("不与同名法术叠加。免疫或原本结算为0的该类伤害不消耗本回合减伤；临时生命吸收之前仍可减伤。",small);
    }
    string ResistanceStatus(int actor)=>resistanceSpell.Active(worldRounds)&&resistanceSpell.Target==actor?"抵抗术 · "+DamageTypes.Names[resistanceSpell.Type]+(resistanceSpell.Available(actor,terrainTurn,worldRounds)?"（待触发）":"（本回合已用）"):"";
    void CastResistance(int target,int type)
    {
        if(!resistanceSpell.Begin(target,type,worldRounds))return;concentrationSpell="抵抗术";concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=resistanceSpell.Expires;
        history.Insert(0,"抵抗术：保护"+TransitActorName(target)+"，选择"+DamageTypes.Names[type]+"，每回合首次有效该类伤害减1d4；专注至多1分钟。");
    }
}
