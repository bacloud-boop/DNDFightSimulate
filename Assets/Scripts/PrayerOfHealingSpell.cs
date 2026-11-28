using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    PrayerCasting prayerChannel;
    readonly HashSet<int> prayerTargets=new HashSet<int>{0};
    bool prayerChoicePending,prayerFinishing;int prayerChoice=-1,prayerChoiceActor;bool prayerChoosingOrder;bool prayerHealingFirst=true;Vector2 prayerPromptScroll;
    bool PrayerInRange(int actor,Vector2Int? source=null,Vector2Int? target=null,int targetSpaces=0)
    {
        Vector2Int p;int n;if(!HitDiceActorAlive(actor)||!map.SameScene(0,actor)||!map.SceneBody(actor,out p,out n))return false;
        if(map.IsContained(true)&&actor!=0&&actor!=-1||actor==-1&&map.IsContained(false)&&!map.IsContained(true))return false;
        return BattleMap.BodyDistance(source??map.Player,map.PlayerSpaces,target??p,targetSpaces>0?targetSpaces:n)<=30;
    }
    string PrayerError(int slot)
    {
        if(prayerChannel!=null)return "治疗祷言正在持续施法，请继续或取消。";
        if(worldRounds<0||worldRounds>long.MaxValue-101||slot<2||slot>9)return "施法环阶或世界时间不合法。";
        if(prayerTargets.Count<1||prayerTargets.Count>5)return "请选择1至5名不同生物。";
        foreach(int actor in prayerTargets){if(!PrayerInRange(actor))return "所有目标必须存活、同场景且在30尺内。";if(prayerUsed.Contains(actor))return "所选目标已受益，须完成长休后再次接受治疗祷言。";if(bestialSpirit.Active&&actor==bestialSpirit.Unit.Id&&concentrationSpell=="召唤野兽")return "持续施法需要专注，会使所选野兽灵魄消失。";}
        return "";
    }
    void DrawPrayerChoices()
    {
        GUILayout.Label("10分钟持续施法；每回合消耗魔法动作并维持专注。选择1至5名生物，须在整个施法期间始终位于30尺内，离开后即使返回也不能受益。无需看见目标。",small);
        foreach(int actor in BreathActors()){bool selected=prayerTargets.Contains(actor);bool next=GUILayout.Toggle(selected,TransitActorName(actor)+(prayerUsed.Contains(actor)?"（长休前不能再受益）":""),label);if(next)prayerTargets.Add(actor);else prayerTargets.Remove(actor);}
        GUILayout.Label("完成后获得短休收益及每环1d8治疗（2环2d8）；不加施法属性。可以选择短休与法术治疗的先后，并逐颗选择消耗生命骰。每名目标完成长休前只能受益一次。",small);
    }
    void BeginPrayer(ICollection<int> targets,int slot)
    {
        EndConcentration("开始治疗祷言的持续施法");prayerChannel=new PrayerCasting(targets,slot,combat.Round,worldRounds);
        concentrationSpell="治疗祷言（施法中）";concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=prayerChannel.Finish+1;
        history.Insert(0,"开始治疗祷言：本回合已花魔法动作，之后每回合须继续，持续10分钟。");RefreshPrayerTargets();
    }
    void RefreshPrayerTargets()
    {
        if(prayerChannel==null)return;foreach(int actor in prayerChannel.Targets)if(!PrayerInRange(actor)){prayerChannel.Exclude(actor);history.Insert(0,TransitActorName(actor)+"离开治疗祷言射程、离场或死亡，本次不能再受益。");}
    }
    void ObservePrayerMovement(int actor,Vector2Int from,Vector2Int to,int spaces,bool landing)
    {
        var channel=prayerChannel;if(channel==null)return;
        foreach(var cell in landing?new List<Vector2Int>{to}:BattleMap.MovementAnchors(from,to))foreach(int target in channel.Targets){
            // An attached rider/cargo follows the source's real offset. The
            // final refresh also validates scene changes and footprint changes.
            Vector2Int p;int n;if(!map.SceneBody(target,out p,out n)){channel.Exclude(target);continue;}
            Vector2Int offset=new Vector2Int(cell.x-to.x,cell.y-to.y);
            bool primaryTogether=map.IsContained(true)||map.IsContained(false);
            bool primaryMove=primaryTogether&&(actor==0||actor==-1);
            bool mountedMove=map.RiderMountId>0&&(actor==0||actor==map.RiderMountId);
            int cargo=map.RescueCargoPlayer.HasValue?(map.RescueCargoPlayer.Value?0:-1):int.MaxValue;
            bool cargoMove=map.RescueCargoPlayer.HasValue&&(actor==BattleMap.RescueId||actor==cargo);
            bool follows=primaryMove&&(target==0||target==-1)||mountedMove&&(target==0||target==map.RiderMountId)||cargoMove&&(target==cargo||target==BattleMap.RescueId);
            bool sourceFollows=actor==0||primaryMove||mountedMove||cargoMove&&cargo==0;
            Vector2Int source=sourceFollows?new Vector2Int(map.Player.x+offset.x,map.Player.y+offset.y):map.Player;
            if(!PrayerInRange(target,source,target==actor?cell:follows?new Vector2Int(p.x+offset.x,p.y+offset.y):p,target==actor?spaces:n))channel.Exclude(target);
        }
        RefreshPrayerTargets();
    }
    IEnumerator ContinuePrayer(bool automatic=false)
    {
        var channel=prayerChannel;if(channel==null||!automatic&&rolling||!PlayerTurnAllowed||!PlayerCanSustainCast||!SpellVoiceAllowed(true,PrayerCasting.Name)||!ArmorSpellAllowed(PrayerCasting.Name)||!channel.CanContinue(combat.Round,worldRounds))yield break;
        if(!(combat.Aftermath?combat.TryUseAftermathMagicAction():combat.TryUseMagicAction()))yield break;
        channel.Continue(combat.Round,worldRounds);bool busy=rolling;rolling=true;PlayerCastWhileSubmerged(PrayerCasting.Name);
        try{var stopped=new SpellInterruption();yield return TryInterruptSpell(true,PrayerCasting.Name,stopped);if(stopped.Cancelled)EndConcentration("治疗祷言持续施法被反制");}finally{rolling=busy;}
    }
    IEnumerator AutoContinuePrayer()
    {
        var channel=prayerChannel;if(channel==null||rolling||!combat.Aftermath||!PlayerTurnAllowed||!PlayerCanSustainCast)yield break;
        while(prayerChannel==channel&&worldRounds<channel.Finish){
            if(channel.PaidRound!=combat.Round)yield return ContinuePrayer(true);
            if(prayerChannel!=channel||channel.PaidRound!=combat.Round)yield break;
            yield return AdvanceAftermathRounds(1,()=>prayerChannel==channel&&PlayerCanSustainCast);
            if(!aftermathAdvanceCompleted)yield break;
        }
    }
    void DrawPrayerChannel()
    {
        if(prayerChannel==null)return;GUILayout.Label("治疗祷言施法中 · 剩余"+Math.Max(0,prayerChannel.Finish-worldRounds)*6+"秒 · 合格目标"+prayerChannel.Count,small);
        bool old=GUI.enabled;GUI.enabled=old&&!rolling&&PlayerTurnAllowed&&PlayerCanSustainCast&&prayerChannel.CanContinue(combat.Round,worldRounds);
        if(GUILayout.Button("继续治疗祷言（魔法动作）",button))StartCoroutine(ContinuePrayer());GUI.enabled=old;
        GUI.enabled=old&&!rolling&&combat.Aftermath&&PlayerTurnAllowed&&PlayerCanSustainCast;if(GUILayout.Button("战后自动持续施法至完成（逐回合结算）",button))StartCoroutine(AutoContinuePrayer());GUI.enabled=old;
        if(GUILayout.Button("取消治疗祷言",button))EndConcentration("主动取消治疗祷言");
    }
    void EndPrayerCastingTurn(){if(prayerChannel!=null&&(!prayerChannel.EndTurn(combat.Round)||!PlayerCanSustainCast))EndConcentration("未继续治疗祷言或施法者不能继续施法");}
    IEnumerator FinishPendingPrayer()
    {
        var channel=prayerChannel;if(channel==null||prayerFinishing||worldRounds<channel.Finish)yield break;
        RefreshPrayerTargets();prayerChannel=null;
        bool success=channel.Ready(worldRounds)&&PlayerCanSustainCast;EndConcentration("治疗祷言持续施法结束");
        if(!success){history.Insert(0,"治疗祷言失败：未连续完成施法，或没有始终在射程内的目标。");yield break;}
        bool busy=rolling;rolling=true;prayerFinishing=true;
        try{
            var targets=new List<int>();foreach(int actor in channel.Targets)if(PrayerInRange(actor)&&!prayerUsed.Contains(actor)){targets.Add(actor);prayerUsed.Add(actor);}
            if(targets.Count==0)yield break;
            int sum=0;var rolls=new int[channel.Slot];for(int i=0;i<rolls.Length;i++){rolls[i]=UnityEngine.Random.Range(1,9);sum+=rolls[i];}
            dice="治疗祷言 · 共享治疗骰";yield return PresentRoll(8,rolls,rolls.Length+"d8 = "+sum+"，无施法属性加值");
            prayerChoosingOrder=true;prayerChoice=-1;prayerChoicePending=true;try{while(prayerChoice<0)yield return null;prayerHealingFirst=prayerChoice==0;}finally{prayerChoicePending=false;prayerChoosingOrder=false;}
            if(prayerHealingFirst)ApplyPrayerHealing(targets,sum,rolls.Length*8);
            foreach(int actor in targets)GrantShortRest(actor);
            foreach(int actor in targets)yield return ChoosePrayerRestDice(actor);
            if(!prayerHealingFirst)ApplyPrayerHealing(targets,sum,rolls.Length*8);
            ObserveDeaths();CheckBattleOutcome();
        }finally{prayerChoicePending=false;prayerFinishing=false;rolling=busy;}
    }
    void ApplyPrayerHealing(IEnumerable<int> targets,int amount,int maximum)
    {
        foreach(int actor in targets)if(HitDiceActorAlive(actor)){int actual=HealRegenerationTarget(actor,RollHealingAmount(actor,amount,maximum));if(actor==0)ApplySteedLifeBond(actual,2);history.Insert(0,"治疗祷言："+TransitActorName(actor)+"实际恢复"+actual+"HP，获得短休收益；完成长休前不能再次受益。");}
    }
    IEnumerator ChoosePrayerRestDice(int actor)
    {
        while(HitDiceActorAlive(actor)){
            prayerChoiceActor=actor;prayerChoice=-1;prayerChoicePending=true;try{while(prayerChoice<0)yield return null;}finally{prayerChoicePending=false;}
            if(prayerChoice==int.MaxValue)yield break;
            if(actor==0){int index=prayerChoice;if(index<0||index>=hitDiceSpent.Length||hitDiceSpent[index]>=classes[index])continue;hitDiceSpent[index]++;
                int sides=classes.HitDie(index),die=UnityEngine.Random.Range(1,sides+1),modifier=AcidSplash.Modifier(HitDiceConstitution(0)),heal=HitDicePool.Healing(die,modifier);dice="治疗祷言短休 · 玩家生命骰";
                yield return PresentRoll(sides,new[]{die},die+" + 体质 "+modifier+" = "+heal+"（最低1）");combat.HealPlayer(RollHealingAmount(0,heal,HitDicePool.Healing(sides,modifier)));
            }else yield return SpendActorRestDie(actor);
        }
    }
    void DrawPrayerPrompt()
    {
        Fill(new Rect(0,0,1600,900),new Color(0,0,0,.94f));Surface(new Rect(420,170,760,570),paperTexture);GUILayout.BeginArea(new Rect(450,200,700,510));
        prayerPromptScroll=GUILayout.BeginScrollView(prayerPromptScroll);
        if(prayerChoosingOrder){GUILayout.Label("治疗祷言 · 选择同时收益的顺序",heading);if(GUILayout.Button("先法术治疗，再获得短休收益／使用生命骰",button))prayerChoice=0;if(GUILayout.Button("先获得短休收益／使用生命骰，再法术治疗",button))prayerChoice=1;}
        else{
            int actor=prayerChoiceActor;GUILayout.Label(TransitActorName(actor)+" · 短休生命骰（自愿消耗）",heading);
            if(actor==0){for(int i=0;i<hitDiceSpent.Length;i++)if(classes[i]>hitDiceSpent[i]&&GUILayout.Button(PlayerClasses.Names[i]+" · d"+classes.HitDie(i)+" 剩"+(classes[i]-hitDiceSpent[i]),button))prayerChoice=i;}
            else {var pool=ActorHitDice(actor);if(pool!=null){GUILayout.Label("剩余"+pool.Remaining+" / "+pool.Count+" d"+pool.Sides,small);if(pool.Remaining>0&&GUILayout.Button("消耗1颗生命骰",button))prayerChoice=0;}}
            if(GUILayout.Button("不再消耗，继续结算",button))prayerChoice=int.MaxValue;
        }
        GUILayout.EndScrollView();GUILayout.EndArea();
    }
}
