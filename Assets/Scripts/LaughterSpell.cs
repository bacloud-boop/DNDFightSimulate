using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly LaughterEffect laughter=new LaughterEffect();
    readonly HashSet<int> laughterTargets=new HashSet<int>{-1};
    sealed class LaughterSave {public int Actor;public long Revision;}
    readonly Queue<LaughterSave> laughterSaves=new Queue<LaughterSave>();
    bool laughterCasting,drainingLaughter;
    bool Laughing(int actor)=>laughter.Active(actor,worldRounds);
    void InitializeLaughter()
    {
        combat.ExtraIncapacitated=p=>Laughing(p?0:-1);combat.ExtraProne=p=>Laughing(p?0:-1);
        combat.ConditionDamageReceived=(p,n)=>{QueueLaughterDamage(p?0:-1,n);NoteRestDamage(p?0:-1,n);};
        rescueCarrier.ExtraIncapacitated=()=>Laughing(BattleMap.RescueId);rescueCarrier.ExtraProne=rescueCarrier.ExtraIncapacitated;
        rescueCarrier.ConditionDamageReceived=n=>{QueueLaughterDamage(BattleMap.RescueId,n);NoteRestDamage(BattleMap.RescueId,n);};
    }
    void HookLaughterSummon(SummonedUnit unit)
    {
        unit.ExtraIncapacitated=()=>Laughing(unit.Id);unit.ExtraProne=unit.ExtraIncapacitated;
        unit.ConditionDamageReceived=n=>{QueueLaughterDamage(unit.Id,n);NoteRestDamage(unit.Id,n);};
    }
    bool LaughterAlive(int actor)=>actor==0?!combat.PlayerDead:actor==-1?combat.Hp>0:actor==BattleMap.RescueId?!rescueCarrier.Dead:FindSummon(actor)?.Alive==true;
    void EndLaughter(){laughter.Clear();laughterSaves.Clear();}
    void RefreshLaughter()
    {
        foreach(int actor in laughter.Targets())if(!LaughterAlive(actor))laughter.Remove(actor);
        if(!laughterCasting&&concentrationSpell==LaughterEffect.Title&&(laughter.Count==0||worldRounds>=laughter.Expires))EndConcentration("狂笑目标均摆脱效果、死亡或持续时间结束");
    }
    void QueueLaughterDamage(int actor,int amount){if(amount>0&&Laughing(actor))laughterSaves.Enqueue(new LaughterSave{Actor=actor,Revision=laughter.Revision});}
    IEnumerator DrainLaughterSaves()
    {
        if(drainingLaughter)yield break;drainingLaughter=true;
        try{while(laughterSaves.Count>0){var entry=laughterSaves.Dequeue();yield return RepeatLaughterSave(entry.Actor,entry.Revision,true);}}
        finally{drainingLaughter=false;}
    }
    IEnumerator RepeatLaughterSave(int actor,long revision,bool damage=false)
    {
        if(revision!=laughter.Revision||!Laughing(actor)||!LaughterAlive(actor))yield break;
        var failed=new HashSet<int>();yield return RollMappedAbilitySave(actor,0,laughter.Dc,failed,damage?"狂笑受伤重复（优势）":"狂笑回合末重复",3,advantage:damage);
        if(revision!=laughter.Revision||!Laughing(actor))yield break;
        if(!failed.Contains(actor)){laughter.Remove(actor);history.Insert(0,TransitActorName(actor)+"摆脱本次狂笑；其他目标继续独立计时。原有倒地状态保留。");}
        else history.Insert(0,TransitActorName(actor)+"狂笑感知豁免失败，继续倒地、失能，不能自行起身；仍可爬行。");
        RefreshLaughter();
    }
    bool LaughterCanSelect(int actor)
    {
        if(map.IsContained(true)&&actor!=0&&actor!=-1)return false;
        Vector2Int p;int n;string name;if(!LaughterAlive(actor)||!HealingHandsTarget(actor,out p,out n,out name)||!map.SameScene(0,actor)||BattleMap.BodyDistance(map.Player,map.PlayerSpaces,p,n)>30)return false;
        return actor==0?CanSeeOwnBody():actor==-1?PlayerSeesMonster:VisibilityAt(true,p,n)<2;
    }
    string LaughterError(int slot)
    {
        if(laughterTargets.Count<1||laughterTargets.Count>slot)return "请选择1至"+slot+"名不同目标。";
        foreach(int actor in laughterTargets)if(!LaughterCanSelect(actor))return "目标必须存活、可见且在30尺内。";
        return worldRounds<0||worldRounds>long.MaxValue-10?"世界时间超出范围。":"";
    }
    void DrawLaughterChoices()
    {
        GUILayout.Label("30尺内可见生物；每环至多1名。感知失败后倒地、失能，不能自行起身。回合末与每次受伤分别重复豁免，受伤豁免具有优势；成功仅解除该目标。",small);
        foreach(int actor in BreathActors()){bool selected=laughterTargets.Contains(actor);bool next=GUILayout.Toggle(selected,TransitActorName(actor)+(LaughterCanSelect(actor)?"":"（当前不合法）"),label);if(next)laughterTargets.Add(actor);else laughterTargets.Remove(actor);}
        GUILayout.Label("失能禁止动作、附赠动作、反应和说话，并结束专注；不等于麻痹或昏迷，不自动掉落装备。狂笑表现不改变屏息或呼吸。",small);
    }
    IEnumerator CastLaughter(ICollection<int> targets,int caster)
    {
        laughter.Begin(worldRounds,8L+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(caster)]));
        long revision=laughter.Revision;laughterCasting=true;concentrationSpell=LaughterEffect.Title;concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=laughter.Expires;
        try{
            var failed=new HashSet<int>();foreach(int actor in targets)if(LaughterCanSelect(actor))yield return RollMappedAbilitySave(actor,0,laughter.Dc,failed,LaughterEffect.Title,3);
            if(revision!=laughter.Revision||!IsConcentrating)yield break;
            foreach(int actor in failed)if(LaughterAlive(actor)){laughter.Add(actor);FindSummon(actor)?.CancelDodge();history.Insert(0,TransitActorName(actor)+"狂笑豁免失败：倒地、失能，不能自行起身。");}
            if(Laughing(0)){
                fighter.CancelAttackAction();actionWeapon=null;
                if(IsWildShaped){bool restored=false;if(map.HasContainedCreature){yield return ReleaseSwallowedTarget("狂笑失能结束荒野形态",MoonbeamRootSpaces(0),MoonbeamRootSize(0));restored=releaseRestoredHost;}FinishMoonbeamTrueForm(0,restored);ObserveForcedSharedStop(0);if(restored)ObserveForcedSharedStop(-1);}
                EndConcentration("施法者自身狂笑失能");
            }
        }finally{laughterCasting=false;RefreshLaughter();}
    }
}
