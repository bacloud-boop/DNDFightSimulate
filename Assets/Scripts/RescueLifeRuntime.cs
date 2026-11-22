using System.Collections;
using UnityEngine;
public sealed partial class TrainingGame
{
    bool RescueDepartureReady=>!map.RescuePosition.HasValue||rescueCarrier.CanDepart;
    void ReconcileFallenRescue()
    {
        if(!map.RescuePosition.HasValue||rescueCarrier.Conscious||!rescueCarrier.Load.Loaded)return;
        if(map.ForceReleaseRescueCargo()){rescueCarrier.Load.Clear();rescueDropMode=false;history.Insert(0,"救援者失去意识，搬运对象已强制放下；未消耗动作或移动力。");}
    }
    IEnumerator RescueDeathSave()
    {
        if(!map.RescuePosition.HasValue||!rescueCarrier.NeedsDeathSave)yield break;
        string saved=panel;panel="";bool busy=rolling;rolling=true;
        try{
            var roll=RollSavingThrowDice(BeaconHope(BattleMap.RescueId),false);int die=roll.Value;
            yield return PresentRoll(20,roll.Dice,"救援者死亡豁免 · D20 "+die+"，调整 "+(rescueCarrier.RevivalPenalty+rescueCarrier.Breathing.D20Penalty+BondBonus(BattleMap.RescueId)));
            rescueCarrier.ResolveDeathSave(die,worldRounds,BondBonus(BattleMap.RescueId));rescueCarrier.ObserveLife(worldRounds);ReconcileFallenRescue();
            history.Insert(0,"救援者："+rescueCarrier.LifeLabel+"，死亡豁免成功 "+rescueCarrier.DeathSuccesses+" / 3，失败 "+rescueCarrier.DeathFailures+" / 3。");
            if(rescueCarrier.Stable&&rescueCarrier.RecoveryAt<0){int hours=Random.Range(1,5);yield return PresentRoll(4,new[]{hours},"救援者稳定后 "+hours+" 小时恢复1生命");rescueCarrier.ScheduleRecovery(worldRounds,hours);}
        }finally{panel=saved;rolling=busy;}
    }
    string RescueRevivalError(string name,bool external)
    {
        string absent=MissingBodyError(name,BattleMap.RescueId,external);if(absent!="")return absent;
        if(!combat.Aftermath||!map.RescuePosition.HasValue)return "救援者必须位于战后现场。";
        rescueCarrier.ObserveLife(worldRounds);
        if(!(external?externalResurrectionCaster:playerResurrectionCaster).CanCast)return "施法者受复生术疲惫影响，须先完成长休。";
        if(!external){
            if(!CanPlayerAftermathCast)return "玩家当前不能施法。";
            if(!KnowsRevival(name))return "当前职业不能施放该法术。";
            if(!SpellVoiceAllowed(true,name)||!ArmorSpellAllowed(name))return "施法成分或护甲限制阻止施法。";
            if(map.BodyPresent(BattleMap.RescueId)&&BattleMap.BodyDistance(map.Player,map.PlayerSpaces,map.RescuePosition.Value,map.RescueSpaces)>5)return "玩家须接近救援者至5尺内。";
        }
        if(name!="Gentle Repose"&&!revivalWilling)return "请先确认灵魂愿意复活。";
        if(name=="Reincarnate"){string placement=ReincarnationPlacementError(BattleMap.RescueId);if(placement!="")return placement;}
        return RevivalRules.Error(name,rescueCarrier.Corpse,worldRounds);
    }
    void DrawRescueLifeControls()
    {
        DrawMissingBodyChoices();DrawRescueWebEscape();GUILayout.Label("生命状态："+rescueCarrier.LifeLabel+(PassTraceBenefit(BattleMap.RescueId)?" · 无踪步":"")+(BeaconHope(BattleMap.RescueId)?" · 希望信标":"")+(MovementFreedom(BattleMap.RescueId)?" · 行动自如":"")+" · AC "+rescueCarrier.ArmorClass+" · 临时生命 "+rescueCarrier.TemporaryHp+(ActorBlinded(BattleMap.RescueId)?" · 目盲":"")+(rescueCarrier.Restrained?" · 蛛网束缚":""),small);
        DrawRescueBreathingStatus();
        if(rescueCarrier.NeedsDeathSave)GUILayout.Label("每推进6秒进行一次死亡豁免。",small);
        DrawExternalCasterRest();
        if(!rescueCarrier.Dead)return;
        revivalWilling=GUILayout.Toggle(revivalWilling,"确认救援者灵魂愿意复活",label);
        foreach(string name in new[]{"Revivify","Raise Dead","Resurrection","Reincarnate","Gentle Repose","True Resurrection"})foreach(bool external in new[]{false,true}){
            string error=RescueRevivalError(name,external);bool old=GUI.enabled;GUI.enabled=!rolling&&error=="";
            if(GUILayout.Button((external?"外部施法者":"玩家")+" → 救援者："+DirectSpell.Find(name).Chinese,button))StartCoroutine(CastRescueRevival(name,external));
            GUI.enabled=old;
            if(error!="")GUILayout.Label(error,small);
        }
    }
    IEnumerator CastRescueRevival(string name,bool external)
    {
        if(name=="Reincarnate"){yield return CastReincarnation(BattleMap.RescueId,external);yield break;}
        if(RescueRevivalError(name,external)!="")yield break;
        bool busy=rolling;rolling=true;long invisibilityToken=senses.Token(true,worldRounds);
        try{
            if(!external)PlayerCastWhileSubmerged(name);
            if(!external&&RevivalRules.Duration(name)>1)EndConcentration("开始长时间施法");
            yield return AdvanceRevivalTime(name,external);
            string error=RevivalRules.CompletionError(name,rescueCarrier.Corpse,worldRounds);if(error=="")error=MissingBodyError(name,BattleMap.RescueId,external);
            if(!external&&(!aftermathAdvanceCompleted||!CanPlayerAftermathCast))error="施法者失能，复活失败。";
            if(error==""){
                if(name=="Gentle Repose")rescueCarrier.Corpse.Preserve(worldRounds);
                else {bool taxed=ResurrectionCasterState.RequiresExertion(name,rescueCarrier.Corpse,worldRounds);if((name!="True Resurrection"||RestoreMissingBody(BattleMap.RescueId,external))&&rescueCarrier.Revive(RevivalRules.FullHp(name)?rescueCarrier.MaxHp:1,RevivalRules.Ordeal(name))){RecordResurrectionExertion(external,taxed);RestoreRescueBreathingAfterRevival();}}
                history.Insert(0,DirectSpell.Find(name).Chinese+"对救援者结算完成；当前"+rescueCarrier.LifeLabel+"，保留战后状态。");
            }else history.Insert(0,"救援者复活失败："+error);
        }finally{if(!external)BreakOrdinaryInvisibility(true,"施法",invisibilityToken);rolling=busy;}
    }
}
