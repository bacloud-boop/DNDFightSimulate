using System.Collections;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly WitchBoltEffect witchBolt=new WitchBoltEffect();int witchTarget=-1;
    string WitchLinkError(int target)
    {
        Vector2Int p;int n;
        if(target==BattleMap.RescueId||!AcidArrowTargetAvailable(target)||!map.BodyPresent(target)||!map.SameScene(0,target)||!map.SceneBody(target,out p,out n))return "目标已死亡、消失或离开当前位面。";
        if(AcidArrowIsolated(0,target))return "目标与施法者之间形成吞咽全身掩护。";
        if(target==0)n=map.PlayerSpaces;
        return BattleMap.BodyDistance(map.Player,map.PlayerSpaces,p,n)>60?"双方距离超过60尺。":"";
    }
    string WitchBoltError()
    {
        if(!WitchBoltEffect.ValidTime(worldRounds))return "时间超出范围。";
        if(bestialSpirit.Active&&witchTarget==bestialSpirit.Unit.Id&&concentrationSpell=="召唤野兽")return "开始新的专注会使该灵魄消失，不能选择它。";
        return WitchLinkError(witchTarget);
    }
    void RefreshWitchBolt()
    {
        if(concentrationSpell!="巫术箭")return;
        string error=WitchLinkError(witchBolt.Target);if(!witchBolt.Active(worldRounds)||error!="")EndConcentration(error==""?"巫术箭持续时间结束":error);
    }
    string WitchPulseError()
    {
        RefreshWitchBolt();if(concentrationSpell!="巫术箭"||!witchBolt.Active(worldRounds))return "巫术箭已经结束。";
        if(rolling||spellTargeting||reactionPending||draggingToken>=0||summonTurns.Active)return "请等待当前操作结束。";
        if(!PlayerTurnAllowed||!combat.PlayerCanAct||!combat.BonusActionAvailable)return "当前不能使用附赠动作。";
        return witchBolt.CanPulse(combat.Round,worldRounds)?"":"只能在施法后的后续自身回合使用。";
    }
    void DrawWitchBoltChoices()
    {
        foreach(int actor in BreathActors())if(actor!=BattleMap.RescueId&&GUILayout.Button((witchTarget==actor?"✓ ":"")+TransitActorName(actor),button))witchTarget=actor;
        GUILayout.Label("初始远程法术攻击命中 "+WitchBoltEffect.InitialDice(directSlotLevel)+"d12闪电；首次未命中或被镜像拦截仍绑定原目标。后续自身回合可消耗附赠动作自动造成1d12，升环仅增加初始伤害。",small);
        if(concentrationSpell!="巫术箭"||!witchBolt.Active(worldRounds))return;
        GUILayout.Label("现存绑定："+TransitActorName(witchBolt.Target)+"；后续伤害不重新施法、不再攻击检定，不因漏用一个回合而结束，也不刷新持续时间。",small);
        string error=WitchPulseError();bool old=GUI.enabled;GUI.enabled=old&&error=="";
        if(GUILayout.Button("对绑定目标放电（附赠动作）",button))StartCoroutine(PulseWitchBolt());GUI.enabled=old;
        if(error!="")GUILayout.Label(error,small);
    }
    IEnumerator CastWitchBolt(int caster,int slot,int target)
    {
        if(WitchLinkError(target)!=""||WitchBoltEffect.InitialDice(slot)==0||!witchBolt.Begin(target,combat.Round,worldRounds))yield break;
        concentrationSpell="巫术箭";concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=witchBolt.Expires;
        var attack=new SpellAttackResult();yield return RollMappedSpellAttack(0,target,caster,"巫术箭",attack);
        if(!attack.Hit){history.Insert(0,attack.Intercepted?"巫术箭被镜像拦截：本体首次伤害0，仍绑定原目标，后续可使用附赠动作放电。":"巫术箭首次未命中，仍绑定目标，后续可使用附赠动作放电。");RefreshWitchBolt();yield break;}
        yield return WitchBoltDamage(target,WitchBoltEffect.InitialDice(slot,attack.Critical),attack.Critical,true);
    }
    IEnumerator PulseWitchBolt()
    {
        if(WitchPulseError()!=""||!combat.TryUseBonusAction())yield break;
        bool busy=rolling;rolling=true;panel="";
        try{yield return WitchBoltDamage(witchBolt.Target,1,false,false);}finally{rolling=busy;}
    }
    IEnumerator WitchBoltDamage(int target,int count,bool critical,bool initial)
    {
        if(WitchLinkError(target)!=""){RefreshWitchBolt();yield break;}
        int sum=0;var rolls=new int[count];for(int i=0;i<count;i++){rolls[i]=Random.Range(1,13);sum+=rolls[i];}
        dice=initial?"巫术箭 · 初始闪电":"巫术箭 · 自动放电";yield return PresentRoll(12,rolls,count+"d12 = "+sum+" 闪电"+(initial?"":"；无攻击检定，不会暴击"));
        var packet=new long[13];packet[7]=sum;
        if(initial&&(target==0||target==-1)){var extra=new ExtraMarkDamage();yield return RollMarkDamage(target==0,critical,extra);if(extra.Raw>0)packet[mark.DamageType]+=extra.Raw;}
        var reduced=new RayDamage();yield return ReduceRayPacket(true,packet,true,"巫术箭伤害",reduced);
        outcomeDeferDepth++;try{
            int amount=ApplyMappedDamagePacket(target,0,reduced.Raw,critical);history.Insert(0,"巫术箭："+TransitActorName(target)+"本次结算 "+amount+" 点伤害。");
            yield return MaintainConcentration(target==0?amount:0);RefreshUndeadFollowers();RefreshBestialSpirit();ReconcileFallenRescue();RefreshWitchBolt();
        }finally{outcomeDeferDepth--;CheckBattleOutcome();}
    }
}
