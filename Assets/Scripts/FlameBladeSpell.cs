using System;
using System.Collections;
using UnityEngine;
public sealed partial class TrainingGame
{
    const int FlameBladeLightGroup=7;
    readonly FlameBladeEffect flameBlade=new FlameBladeEffect();int flameBladeTarget=-1;
    bool FlameBladeHeld=>flameBlade.Held&&flameBlade.Active(worldRounds)&&!PlayerInBeastForm;
    bool PlayerOrdinaryWeaponHeld=>!weaponGrip.Stowed&&carriedWeapons.Carried(selectedWeapon.Name);
    bool PlayerFreeHand=>!PlayerInBeastForm&&FlameBladeEffect.FreeHand(PlayerOrdinaryWeaponHeld,PlayerHasShield,FlameBladeHeld);
    bool PlayerCanDrawOrdinaryWeapon=>FlameBladeEffect.CanDraw(PlayerHasShield,FlameBladeHeld);
    bool PlayerReloadHandFree=>FlameBladeEffect.CanReload(PlayerHasShield,FlameBladeHeld);
    void EndFlameBlade(){flameBlade.Clear();vision.ClearLightGroup(FlameBladeLightGroup);}
    void ReleaseFlameBlade(string reason){if(!flameBlade.Held)return;flameBlade.LetGo();vision.ClearLightGroup(FlameBladeLightGroup);history.Insert(0,"焰刃消失："+reason+"；专注与持续时间保留，可在满足空手条件后以附赠动作重召。");}
    void RefreshFlameBlade()
    {
        if(!flameBlade.Active(worldRounds)){vision.ClearLightGroup(FlameBladeLightGroup);return;}
        if(vision.ConsumeDispelledLightGroup(FlameBladeLightGroup)){EndFlameBlade();if(concentrationSpell=="焰刃")EndConcentration("焰刃被黑暗术驱散");return;}
        if(PlayerInBeastForm||!FlameBladeEffect.FreeHand(PlayerOrdinaryWeaponHeld,PlayerHasShield,false))ReleaseFlameBlade(PlayerInBeastForm?"进入兽形":"没有可用手部");
        if(!FlameBladeHeld){vision.ClearLightGroup(FlameBladeLightGroup);return;}
        if(map.Scene!=map.SceneOf(0)||map.IsContained(true)){vision.HideLightGroup(FlameBladeLightGroup);return;}
        double x=(map.Player.x+(map.PlayerSpaces-1)*.5)*5,y=(map.Player.y+(map.PlayerSpaces-1)*.5)*5;
        if(!vision.SetLightGroup(FlameBladeLightGroup,new[]{new[]{x,y}},10,20,flameBlade.Slot,flameBlade.Expires,worldRounds)){EndFlameBlade();if(concentrationSpell=="焰刃")EndConcentration("焰刃照明与黑暗术重叠，被驱散");}
    }
    string FlameBladeError()=>!FlameBladeEffect.ValidTime(worldRounds)?"世界时间超出范围。":PlayerInBeastForm?"兽形暂不能召出或持握焰刃。":!PlayerFreeHand?"焰刃需要一只空手；请收起普通武器或先放开焰刃。":"";
    void CastFlameBlade(int caster,int slot)
    {
        if(!flameBlade.Begin(slot,caster,worldRounds))return;
        concentrationSpell="焰刃";concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=flameBlade.Expires;RefreshFlameBlade();
        if(!flameBlade.Active(worldRounds))return;
        history.Insert(0,"焰刃已生成：占用一只手，10尺明亮光照、外围10尺微光；攻击须另外消耗一个魔法动作。");
    }
    bool FlameBladeOperatorReady=>PlayerTurnAllowed&&!rolling&&!spellTargeting&&!reactionPending&&draggingToken<0&&!summonTurns.Active&&page=="battle"&&map.Locked&&combat.PlayerHp>0&&!combat.PlayerDead&&!combat.PlayerParalyzed&&!combat.PlayerFeignDeath&&!combat.PlayerBanished;
    string FlameBladeRecallError()
    {
        if(!flameBlade.Active(worldRounds)||concentrationSpell!="焰刃")return "焰刃法术已结束。";
        if(FlameBladeHeld)return "焰刃已经持握。";
        if(PlayerInBeastForm)return "兽形暂不能召出或持握焰刃。";
        if(!FlameBladeOperatorReady||!combat.BonusActionAvailable)return "当前不能使用玩家附赠动作。";
        return PlayerFreeHand?"":"重召焰刃需要一只空手。";
    }
    bool FlameBladeTargetAllowed(int target)
    {
        Vector2Int p;int n;string name;
        return target!=BattleMap.RescueId&&map.SameScene(0,target)&&!AcidArrowIsolated(0,target)&&HealingHandsTarget(target,out p,out n,out name)&&BattleMap.BodyDistance(map.Player,map.PlayerSpaces,p,n)<=5;
    }
    string FlameBladeStrikeError()=>!FlameBladeHeld||concentrationSpell!="焰刃"?"请先持握焰刃。":!FlameBladeOperatorReady||!combat.MagicActionAvailable?"焰刃攻击需要一个战斗中的魔法动作。":!FlameBladeTargetAllowed(flameBladeTarget)?"请选择同一位面5尺内且不隔着吞咽边界的存活生物。":"";
    void DrawFlameBladeChoices()
    {
        GUILayout.Label("焰刃占用一只空手；普通双手武器可单手暂持，但不能在持握焰刃时发动双手武器攻击。魔法动作攻击，不触发战士额外攻击或武器斩击。",small);
        if(!flameBlade.Active(worldRounds))return;
        GUILayout.Label(FlameBladeHeld?"焰刃正在手中；专注剩余 "+(flameBlade.Expires-worldRounds)+" 回合":"焰刃暂时消失，专注与计时仍保留。",small);
        foreach(int actor in BreathActors())if(actor!=BattleMap.RescueId&&GUILayout.Button((flameBladeTarget==actor?"✓ ":"")+TransitActorName(actor)+(FlameBladeTargetAllowed(actor)?"":"（当前不可选）"),button))flameBladeTarget=actor;
        string attackError=FlameBladeStrikeError();bool enabled=GUI.enabled;GUI.enabled=enabled&&attackError=="";
        if(GUILayout.Button("使用焰刃攻击（魔法动作）",button))StartCoroutine(StrikeFlameBlade());GUI.enabled=enabled;if(attackError!="")GUILayout.Label(attackError,small);
        string recallError=FlameBladeRecallError();GUI.enabled=enabled&&recallError=="";if(GUILayout.Button("重召焰刃（附赠动作）",button))RecallFlameBlade();GUI.enabled=enabled;
        if(recallError!=""&&!FlameBladeHeld)GUILayout.Label(recallError,small);
        GUI.enabled=enabled&&FlameBladeHeld&&FlameBladeOperatorReady;if(GUILayout.Button("放开焰刃（保留专注）",button))ReleaseFlameBlade("玩家放开");GUI.enabled=enabled;
    }
    void RecallFlameBlade()
    {
        RefreshFlameBlade();if(FlameBladeRecallError()!=""||!combat.TryUseAftermathBonusAction())return;
        if(flameBlade.Evoke(worldRounds,PlayerFreeHand,PlayerInBeastForm)){RefreshFlameBlade();history.Insert(0,"焰刃重召：只消耗附赠动作，不重新施法、开口或刷新持续时间。");}
    }
    IEnumerator StrikeFlameBlade()
    {
        RefreshFlameBlade();if(FlameBladeStrikeError()!=""||!combat.TryUseMagicAction())yield break;
        bool busy=rolling;rolling=true;panel="";int target=flameBladeTarget,slot=flameBlade.Slot,caster=flameBlade.Caster;
        try{
            var hit=new SpellAttackResult();yield return RollMappedSpellAttack(0,target,caster,"焰刃",hit,true,map.Player,map.PlayerSpaces,0);
            if(!hit.Hit){history.Insert(0,"焰刃："+(hit.Intercepted?"镜像拦截":"未命中")+"，伤害0；焰刃与专注仍保留。");yield break;}
            int count=FlameBladeEffect.Dice(slot,hit.Critical),sum=0;var rolls=new int[count];for(int i=0;i<count;i++){rolls[i]=UnityEngine.Random.Range(1,7);sum+=rolls[i];}
            int modifier=AcidSplash.Modifier(attributes[SpellAbilityIndex(caster)]);
            yield return PresentRoll(6,rolls,"焰刃："+count+"d6＋施法调整值 "+modifier+" = "+Math.Max(0,sum+modifier)+" 火焰");
            var raw=new long[13];raw[5]=Math.Max(0,sum+modifier);
            if(target==0||target==-1){var extra=new ExtraMarkDamage();yield return RollMarkDamage(target==0,hit.Critical,extra);if(extra.Raw>0)raw[mark.DamageType]+=extra.Raw;}
            var reduced=new RayDamage();yield return ReduceRayPacket(true,raw,true,"焰刃",reduced);outcomeDeferDepth++;
            try{var retaliation=target==0?CaptureHitRetaliation(0,true):null;int amount=ApplyMappedDamagePacket(target,0,reduced.Raw,hit.Critical);if(retaliation!=null)yield return ResolveHitRetaliation(0,retaliation);history.Insert(0,"焰刃对"+TransitActorName(target)+"造成 "+amount+" 点结算伤害。");yield return MaintainConcentration(target==0?amount:0);if(retaliation!=null)yield return MaintainRetaliationConcentration(retaliation);RefreshBestialSpirit();ReconcileFallenRescue();}finally{outcomeDeferDepth--;CheckBattleOutcome();}
        }finally{rolling=busy;}
    }
}
