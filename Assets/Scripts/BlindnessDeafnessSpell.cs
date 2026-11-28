using System.Collections;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly SensoryAfflictions sensory=new SensoryAfflictions();
    int blindnessPlayerMode,blindnessMonsterMode;
    bool sensoryOrderPending,sensorySavePlayer,sensoryRayOption,sensoryGreaseOption,sensorySleepOption,sensoryDaggersOption,sensorySphereOption;
    long sensorySaveChoice;
    System.Collections.Generic.List<WrathfulSmiteEffects.Entry> sensoryWrathOptions;WrathfulSmiteEffects.Entry wrathfulSaveSelected;
    System.Collections.Generic.List<SensoryAffliction> sensorySaveOptions;
    System.Collections.Generic.List<AcidArrowResidue.Entry> sensoryAcidOptions;
    Vector2 sensorySaveScroll;
    bool CanSeeOwnBody()
    {
        if(SensoryAfflictions.BlocksSight((ActorBlinded(0)),PlayerBlindsight,0))return false;
        if(InvisibleWithFaerie(true)&&!senses.RevealsInvisible(true,0,worldRounds,PlayerBlindsight))return false;
        return vision.Quality(map.Player.x*5,map.Player.y*5,map.Player.x*5,map.Player.y*5,PlayerDarkvision,worldRounds,PlayerBlindsight,senses.TrueSight(true,worldRounds))<2;
    }
    string BlindnessDeafnessError(bool player,int slot,bool both)
    {
        if(both&&slot<3)return "三环起才能同时选择双方。";
        if((!player||both)&&combat.Hp==0)return "死亡木桩不能成为此法术目标。";
        if((!player||both)&&!PlayerSeesMonster)return "必须能看见木桩。";
        if((player||both)&&!CanSeeOwnBody())return "必须能看见自己，才能将自身选为此法术目标。";
        if((!player||both)&&map.DistanceFeet>120)return "木桩超出120尺射程。";
        return sensory.CanAdd(worldRounds,both?2:1)?"":"模拟时间或施法编号超出可记录范围。";
    }
    void DrawBlindnessDeafnessChoices()
    {
        if(directTargetPlayer||directBothTargets){GUILayout.Label("玩家受到的状态",small);blindnessPlayerMode=GUILayout.SelectionGrid(blindnessPlayerMode,new[]{"目盲","耳聋"},2,button);}
        if(!directTargetPlayer||directBothTargets){GUILayout.Label("木桩受到的状态",small);blindnessMonsterMode=GUILayout.SelectionGrid(blindnessMonsterMode,new[]{"目盲","耳聋"},2,button);}
        GUILayout.Label("同名效果以最高DC生效，同DC取最新；所有施法分别计时并在目标回合末各自豁免。非专注。",small);
    }
    IEnumerator ResolveBlindnessDeafness(bool player,int caster,int slot,bool both,bool playerBlind,bool monsterBlind)
    {
        long dc=8+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(caster)]);
        foreach(bool targetPlayer in new[]{false,true})if(both||targetPlayer==player){
            var d20=RollD20Test(targetPlayer,ability:2);int value=d20.Value,modifier=targetPlayer?PlayerSavingThrowModifier(2):MonsterSavingThrowModifier(2);
            var adjustment=new SaveAdjustment();yield return ApplySaveAdjustments(targetPlayer,false,adjustment);modifier+=adjustment.Value;
            dice="目盲／耳聋 · "+(targetPlayer?"玩家":"木桩")+"体质豁免";
            yield return PresentRoll(20,d20.Dice,value+" + "+modifier+" / DC "+dc);
            if((long)value+modifier>=dc){history.Insert(0,(targetPlayer?"玩家":"木桩")+"通过豁免，不受本次目盲／耳聋影响。");continue;}
            bool blind=targetPlayer?playerBlind:monsterBlind;
            var added=sensory.Add(targetPlayer,blind,dc,worldRounds);
            history.Insert(0,(targetPlayer?"玩家":"木桩")+"本次受到"+(blind?"目盲":"耳聋")+"，持续至多1分钟，DC "+dc+"；"+(sensory.Current(targetPlayer,worldRounds).Id==added.Id?"当前生效":"暂被更高DC施法压制")+"。");
        }
    }
    void DrawSensorySaveOrder()
    {
        if(!sensoryOrderPending)return;
        Fill(new Rect(0,0,1600,900),new Color(0,0,0,.93f));Surface(new Rect(465,150,670,630),paperTexture);
        GUILayout.BeginArea(new Rect(495,180,610,570));GUILayout.Label((sensorySavePlayer?"玩家":"木桩")+"回合末 · 选择豁免顺序",heading);
        GUILayout.Label("各次施法独立豁免；首次豁免会消耗心灵之楔等一次性调整。请选择接下来结算哪一次。",small);
        sensorySaveScroll=GUILayout.BeginScrollView(sensorySaveScroll);
        if(sensoryGreaseOption&&GUILayout.Button("油腻术 · 敏捷豁免 · DC "+(ActorGrease(sensorySavePlayer?0:-1)==null?"—":ActorGrease(sensorySavePlayer?0:-1).Dc.ToString()),button))sensorySaveChoice=-2;
        if(sensorySleepOption&&GUILayout.Button("睡眠术 · 感知豁免 · DC "+sleepDc,button))sensorySaveChoice=-3;
        if(sensorySphereOption&&GUILayout.Button("炽焰法球 · 回合末敏捷豁免与火焰伤害",button))sensorySaveChoice=-5;
        if(sensoryDaggersOption&&GUILayout.Button("区域回合末效果 · 匕首之云／咒唤兽群／灵体卫士／造风术／月华之光",button))sensorySaveChoice=-4;
        if(sensoryRayOption&&GUILayout.Button("衰弱射线 · 体质豁免 · DC "+enfeeblement.Dc,button))sensorySaveChoice=-1;
        foreach(var effect in blindingSaveOptions)if(GUILayout.Button("致盲斩 #"+effect.Id+" · 体质豁免 · DC "+effect.Dc,button)){blindingSaveSelected=effect;sensorySaveChoice=BlindingSmiteSaveChoice;}
        foreach(var effect in sensoryWrathOptions)if(GUILayout.Button("激愤斩 #"+effect.Id+" · 感知豁免 · DC "+effect.Dc,button)){wrathfulSaveSelected=effect;sensorySaveChoice=-6;}
        foreach(var entry in sensoryAcidOptions)if(GUILayout.Button(entry.Title+"残留 #"+entry.Id+" · "+entry.Dice+"d4强酸",button))sensorySaveChoice=-(entry.Id+6);
        foreach(var effect in sensorySaveOptions)if(GUILayout.Button("#"+effect.Id+" · "+(effect.Blind?"目盲":"耳聋")+" · DC "+effect.Dc+" · 剩余"+(effect.Expires-worldRounds)+"回合",button))sensorySaveChoice=effect.Id;
        GUILayout.EndScrollView();GUILayout.EndArea();
    }
    IEnumerator RepeatSensorySaves(bool player)
    {
        if(PendingTerrainCount>0)yield return DrainGreaseEntries();
        if(player?combat.PlayerDead:combat.Hp==0)yield break;
        bool sleepPending=!player&&combat.MonsterSleepStage==1&&combat.Round>=sleepRepeatRound;
        var blinding=blindingSmite.Pending(player,worldRounds);var arrows=acidArrows.Pending(player?0:-1);var wrath=wrathfulSmite.Pending(player?0:-1,worldRounds);
        bool spherePending=SphereEndAvailable(player?0:-1);bool daggersPending=DaggersEndAvailable(player?0:-1);var pending=sensory.Pending(player,worldRounds);bool rayPending=RayWeakened(player);var greasePending=ActorGrease(player?0:-1);
        while(pending.Count>0||rayPending||greasePending!=null||sleepPending||daggersPending||spherePending||arrows.Count>0||wrath.Count>0||blinding.Count>0){
            if(player?combat.PlayerDead:combat.Hp==0)yield break;
            long choice=pending.Count>0?pending[0].Id:rayPending?-1:greasePending!=null?-2:sleepPending?-3:daggersPending?-4:spherePending?-5:arrows.Count>0?-(arrows[0].Id+6):wrath.Count>0?-6:BlindingSmiteSaveChoice;
            blindingSaveSelected=blinding.Count>0?blinding[0]:null;
            wrathfulSaveSelected=wrath.Count>0?wrath[0]:null;
            if(pending.Count+(rayPending?1:0)+(greasePending!=null?1:0)+(sleepPending?1:0)+(daggersPending?1:0)+(spherePending?1:0)+arrows.Count+wrath.Count+blinding.Count>1){blindingSaveOptions=blinding;sensoryWrathOptions=wrath;sensoryAcidOptions=arrows;sensorySphereOption=spherePending;sensoryDaggersOption=daggersPending;sensorySaveOptions=pending;sensoryRayOption=rayPending;sensoryGreaseOption=greasePending!=null;sensorySleepOption=sleepPending;sensorySavePlayer=player;sensorySaveChoice=0;sensoryOrderPending=true;while(sensorySaveChoice==0)yield return null;sensoryOrderPending=false;choice=sensorySaveChoice;}
            if(choice==BlindingSmiteSaveChoice){var blindEffect=blindingSaveSelected;blinding.Remove(blindEffect);yield return RepeatBlindingSmiteSave(blindEffect);continue;}
            if(choice<=-7){var entry=arrows.Find(e=>e.Id==-choice-6);arrows.Remove(entry);if(entry!=null)yield return ResolveAcidArrowResidue(entry);continue;}
            if(choice==-6){var wrathEffect=wrathfulSaveSelected;wrath.Remove(wrathEffect);yield return RepeatWrathfulSave(wrathEffect);continue;}
            if(choice==-5){spherePending=false;yield return EndFlamingSphereTurn(player?0:-1);continue;}
            if(choice==-4){daggersPending=false;yield return EndDaggersTurn(player?0:-1);continue;}
            if(choice==-3){sleepPending=false;yield return RepeatSleepSave();continue;}
            if(choice==-2){var field=greasePending;greasePending=null;yield return ResolveGreaseSave(player?0:-1,field,"回合末油脂");continue;}
            if(choice==-1){rayPending=false;yield return RepeatEnfeeblementSave(player);continue;}
            var effect=pending.Find(item=>item.Id==choice);pending.Remove(effect);
            var d20=RollD20Test(player,ability:2);int value=d20.Value,modifier=player?PlayerSavingThrowModifier(2):MonsterSavingThrowModifier(2);
            var adjustment=new SaveAdjustment();yield return ApplySaveAdjustments(player,false,adjustment);modifier+=adjustment.Value;
            dice="回合末 · "+(player?"玩家":"木桩")+"目盲／耳聋 #"+effect.Id+" 体质豁免";
            yield return PresentRoll(20,d20.Dice,value+" + "+modifier+" / DC "+effect.Dc);
            if((long)value+modifier>=effect.Dc){sensory.End(effect.Id);history.Insert(0,"豁免成功，结束目盲／耳聋 #"+effect.Id+"，其他施法仍分别生效或计时。");}
            else history.Insert(0,"目盲／耳聋 #"+effect.Id+" 豁免失败，继续计时。");
        }
    }
    IEnumerator EndPlayerTurnWithSensorySaves()
    {
        rolling=true;yield return RepeatSensorySaves(true);yield return CompletePlayerTurnWithSummons();
    }
}
