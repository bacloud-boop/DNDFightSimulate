using System;
using System.Collections;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly RegenerationEffects regeneration=new RegenerationEffects();int regenerateTarget;
    sealed class RegenerateChannel {public int Target;public AnimateDeadCasting Clock;}
    RegenerateChannel regenerateChannel;
    string RegenerateTargetError(int actor)
    {
        Vector2Int p;int n;string name;
        if(!HealingHandsTarget(actor,out p,out n,out name)||!HealingHandsCanTouch(0,actor))return "需要接触同场景中仍存活的目标（5尺内）。";
        if(bestialSpirit.Active&&actor==bestialSpirit.Unit.Id&&concentrationSpell=="召唤野兽")return "长时间施法需要专注，会使该灵魄消失。";
        return "";
    }
    string RegenerateError()=>regenerateChannel!=null?"再生术正在持续施法，请继续或取消。":!RegenerationEffects.ValidTime(worldRounds)||worldRounds>long.MaxValue-610?"时间超出范围。":RegenerateTargetError(regenerateTarget);
    void DrawRegenerateChoices()
    {
        foreach(int actor in BreathActors())if(GUILayout.Button((regenerateTarget==actor?"✓ ":"")+TransitActorName(actor),button))regenerateTarget=actor;
        GUILayout.Label("缺失部位记录（手动填写；不自动产生断肢或附加战斗减益）",small);
        string current=regeneration.Missing(regenerateTarget),next=GUILayout.TextField(current,fieldStyle);if(next!=current)regeneration.SetMissing(regenerateTarget,next,worldRounds);
        GUILayout.Label("持续施法1分钟，每回合消耗魔法动作并保持专注。完成后恢复4d8＋15HP，并在1小时内每个目标回合开始恢复1HP；缺失部位2分钟后再生。完成后的效果无需专注，不因施法者死亡而结束。",small);
        DrawRegenerateChannel();
    }
    void BeginRegenerate(int target)
    {
        if(RegenerateTargetError(target)!="")return;EndConcentration("开始再生术的持续施法");
        regenerateChannel=new RegenerateChannel{Target=target,Clock=new AnimateDeadCasting(combat.Round,worldRounds)};
        concentrationSpell="再生术（施法中）";concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=regenerateChannel.Clock.Finish+1;
        history.Insert(0,"开始再生术：本回合已花魔法动作，之后每回合须继续，完成前须维持专注。");
    }
    void DrawRegenerateChannel()
    {
        if(regenerateChannel==null)return;GUILayout.Label("再生术施法中 · 剩余 "+Math.Max(0,regenerateChannel.Clock.Finish-worldRounds)*6+"秒",small);
        bool old=GUI.enabled;GUI.enabled=old&&!rolling&&regenerateChannel.Clock.PaidRound!=combat.Round;
        if(GUILayout.Button("继续再生术（魔法动作）",button))StartCoroutine(ContinueRegenerate());GUI.enabled=old;
        if(GUILayout.Button("取消再生术施法",button))EndConcentration("主动取消再生术");
    }
    IEnumerator ContinueRegenerate()
    {
        var channel=regenerateChannel;if(channel==null||rolling||!PlayerTurnAllowed||!PlayerCanSustainCast||!SpellVoiceAllowed(true,"Regenerate")||!ArmorSpellAllowed("Regenerate")||!channel.Clock.CanContinue(combat.Round,worldRounds))yield break;
        if(!(combat.Aftermath?combat.TryUseAftermathMagicAction():combat.TryUseMagicAction()))yield break;
        channel.Clock.Continue(combat.Round,worldRounds);rolling=true;PlayerCastWhileSubmerged("Regenerate");
        try{var stopped=new SpellInterruption();yield return TryInterruptSpell(true,"Regenerate",stopped);if(stopped.Cancelled)EndConcentration("再生术施法被反制");}finally{rolling=false;}
    }
    void EndRegenerateCastingTurn(){if(regenerateChannel!=null&&!regenerateChannel.Clock.EndTurn(combat.Round))EndConcentration("本回合未消耗魔法动作继续再生术");}
    void RefreshRegenerate()
    {
        foreach(int actor in regeneration.Refresh(worldRounds))history.Insert(0,"再生术："+TransitActorName(actor)+"缺失部位已再生，清除手动记录。");
        if(regenerateChannel!=null&&!rolling&&worldRounds>=regenerateChannel.Clock.Finish)StartCoroutine(FinishPendingRegenerate());
    }
    IEnumerator FinishPendingRegenerate()
    {
        var channel=regenerateChannel;if(channel==null||worldRounds<channel.Clock.Finish)yield break;
        regenerateChannel=null;EndConcentration("再生术持续施法完成");
        if(!channel.Clock.Ready(worldRounds)||!PlayerCanSustainCast||RegenerateTargetError(channel.Target)!=""){history.Insert(0,"再生术失败：施法未连续完成，或目标已无法接触。");yield break;}
        yield return CompleteRegenerate(channel.Target);
    }
    int HealRegenerationTarget(int actor,int amount)=>actor==0?combat.HealPlayer(amount):actor==-1?combat.HealMonster(amount):actor==BattleMap.RescueId?rescueCarrier.Heal(amount):FindSummon(actor)?.Heal(amount)??0;
    IEnumerator CompleteRegenerate(int target)
    {
        bool busy=rolling;rolling=true;try{
            int sum=0;var rolls=new int[4];for(int i=0;i<4;i++){rolls[i]=UnityEngine.Random.Range(1,9);sum+=rolls[i];}
            dice="再生术 · 初始治疗";yield return PresentRoll(8,rolls,"4d8＋15 = "+(sum+15));
            if(RegenerateTargetError(target)!=""||!regeneration.Begin(target,worldRounds))yield break;
            int amount=RollHealingAmount(target,sum+15,47),actual=HealRegenerationTarget(target,amount);if(target==0)ApplySteedLifeBond(actual,7);
            history.Insert(0,"再生术："+TransitActorName(target)+"实际恢复 "+actual+"HP；未来1小时每回合开始恢复1HP，2分钟后恢复已记录的缺失部位。");ObserveDeaths();
        }finally{rolling=busy;}
    }
    void RegenerationTurnStarts(int actor,long stamp)
    {
        if(!regeneration.StartTurn(actor,stamp,worldRounds))return;
        startStatuses.Add(new StartStatus{Actor=actor,Kind=0});
    }
}
