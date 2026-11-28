using System.Collections;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly VampiricTouchEffect vampiricTouch=new VampiricTouchEffect();int vampiricTarget=-1;
    bool VampiricTouchActive=>vampiricTouch.Active(worldRounds)&&concentrationSpell=="吸血之触";
    string VampiricTargetError(int target)
    {
        Vector2Int p;int n;string name;if(target==BattleMap.RescueId||!AcidArrowTargetAvailable(target)||!SummonTarget(target,out p,out n,out name))return "请选择存活或倒地但未死亡的玩家、木桩或召唤物。";
        if(AcidArrowIsolated(0,target))return "不能隔着吞咽边界接触目标。";
        return BattleMap.BodyDistance(map.Player,map.PlayerSpaces,p,n)>5?"吸血之触需要接近目标至5尺内。":"";
    }
    string VampiricTouchError()=>!VampiricTouchEffect.ValidTime(worldRounds)?"世界时间超出范围。":VampiricTargetError(vampiricTarget);
    bool VampiricRepeatReady=>page=="battle"&&map.Locked&&VampiricTouchActive&&PlayerTurnAllowed&&!summonTurns.Active&&combat.PlayerCanAct&&combat.MagicActionAvailable;
    void DrawVampiricTouchChoices()
    {
        foreach(int actor in BreathActors())if(actor!=BattleMap.RescueId&&GUILayout.Button((vampiricTarget==actor?"✓ ":"")+TransitActorName(actor),button))vampiricTarget=actor;
        GUILayout.Label("近战法术攻击，5尺内命中 "+VampiricTouchEffect.DiceCount(directSlotLevel)+"d6死灵，暴击翻倍伤害骰；恢复结算后死灵伤害的一半，向下取整，治疗不超过HP上限。临时生命吸收与超出剩余HP的伤害仍计入，抗性与免疫先结算。",small);
        if(!VampiricTouchActive)return;
        GUILayout.Label("现存效果："+vampiricTouch.Slot+"环，剩余 "+(vampiricTouch.Expires-worldRounds)+"回合；再次攻击沿用原施法职业和环阶，不重新施法、不需要成分、不刷新持续时间。",small);
        string error=VampiricTargetError(vampiricTarget);bool old=GUI.enabled;
        GUI.enabled=old&&VampiricRepeatReady&&!rolling&&!spellTargeting&&!reactionPending&&draggingToken<0&&error=="";
        if(GUILayout.Button("使用现存吸血之触（魔法动作）",button))StartCoroutine(RepeatVampiricTouch(vampiricTarget));GUI.enabled=old;
        if(error!="")GUILayout.Label(error,small);
    }
    IEnumerator CastVampiricTouch(int caster,int slot,int target)
    {
        if(!vampiricTouch.Begin(slot,caster,worldRounds))yield break;
        concentrationSpell="吸血之触";concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=vampiricTouch.Expires;
        history.Insert(0,"吸血之触：开始专注至多1分钟；首次攻击失手仍保留后续魔法动作攻击。");
        yield return ResolveVampiricTouchAttack(caster,slot,target);
    }
    IEnumerator RepeatVampiricTouch(int target)
    {
        if(!VampiricRepeatReady||rolling||spellTargeting||reactionPending||draggingToken>=0||VampiricTargetError(target)!=""||!combat.TryUseMagicAction())yield break;
        int caster=vampiricTouch.Caster,slot=vampiricTouch.Slot;bool busy=rolling;rolling=true;panel="";
        try{history.Insert(0,"消耗1个魔法动作使用现存吸血之触；本次不是再次施法，不消耗法术位，也不触发法术反制。");yield return ResolveVampiricTouchAttack(caster,slot,target);}
        finally{rolling=busy;}
    }
    IEnumerator ResolveVampiricTouchAttack(int caster,int slot,int target)
    {
        if(VampiricTargetError(target)!="")yield break;
        var attack=new SpellAttackResult();yield return RollMappedSpellAttack(0,target,caster,"吸血之触",attack,true);
        if(!attack.Hit){history.Insert(0,attack.Intercepted?"吸血之触被镜像拦截，不造成伤害或治疗。":"吸血之触未命中，不造成伤害或治疗。");yield break;}
        int count=VampiricTouchEffect.DiceCount(slot,attack.Critical),sum=0;var rolls=new int[count];for(int i=0;i<count;i++){rolls[i]=Random.Range(1,7);sum+=rolls[i];}
        dice="吸血之触 · 死灵伤害";yield return PresentRoll(6,rolls,count+"d6 = "+sum);
        var raw=new long[13];raw[8]=sum;
        if(target==0||target==-1){var extra=new ExtraMarkDamage();yield return RollMarkDamage(target==0,attack.Critical,extra);if(extra.Raw>0)raw[mark.DamageType]+=extra.Raw;}
        var reduced=new RayDamage();yield return ReduceRayPacket(true,raw,true,"吸血之触合并伤害",reduced);
        reduced.Raw=target==0||target==-1?combat.PrepareIncomingDamage(target==0,reduced.Raw):FindSummon(target).PrepareIncomingDamage(reduced.Raw);
        long necrotic;
        if(target==0||target==-1)necrotic=combat.ResolveTypedPacket(target==0,reduced.Raw)[8];
        else {var unit=FindSummon(target);necrotic=unit.ResistsType(8)?reduced.Raw[8]/2:reduced.Raw[8];}
        outcomeDeferDepth++;try{
            var retaliation=target==0?CaptureHitRetaliation(0,true):null;
            int damage=ApplyMappedDamagePacket(target,0,reduced.Raw,attack.Critical);if(retaliation!=null)yield return ResolveHitRetaliation(0,retaliation);
            // Incapacitation ends concentration even if the ensuing healing
            // restores consciousness during this same resolution.
            RefreshConcentration();int requested=VampiricTouchEffect.Healing(necrotic),actual=combat.HealPlayer(requested);ApplySteedLifeBond(actual,3);
            history.Insert(0,"吸血之触："+TransitActorName(target)+"结算 "+damage+" 点伤害，其中死灵 "+necrotic+"；治疗 "+requested+"，实际恢复 "+actual+" HP。");
            dice="吸血之触 · 实际恢复 "+actual+" HP";yield return MaintainConcentration(target==0?damage:0);if(retaliation!=null)yield return MaintainRetaliationConcentration(retaliation);RefreshUndeadFollowers();RefreshBestialSpirit();ReconcileFallenRescue();
        }finally{outcomeDeferDepth--;CheckBattleOutcome();}
    }
}
