using System.Collections;
using UnityEngine;
public sealed partial class TrainingGame
{
    IEnumerator RestMonsterAftermath(bool longRest)
    {
        if(!combat.Aftermath||rolling||combat.Hp<=0)yield break;
        long damageVersion=RestDamageVersion(-1),start=worldRounds,duration=longRest?(SpeciesPhysicalActive(false)?monsterSpecies.LongRestRounds:4800):600;
        if(longRest&&hasMonsterLongRest&&start-lastMonsterLongRestStart<14400)yield break;
        yield return AdvanceAftermathRounds(duration,()=>combat.Hp>0&&RestDamageVersion(-1)==damageVersion);
        if(!aftermathAdvanceCompleted||combat.Hp<=0||RestDamageVersion(-1)!=damageVersion){aftermathMessage="木桩休息被中断。";yield break;}
        if(longRest){lastMonsterLongRestStart=start;hasMonsterLongRest=true;combat.HealMonster(int.MaxValue);combat.CompleteLongRest(false);FinishActorLongRest(-1);monsterSpeciesAbilities.LongRest();}else GrantShortRest(-1);
        aftermathMessage=longRest?"木桩长休完成。":"木桩短休完成，恢复短休次数，可逐颗消耗生命骰治疗。";
    }
    IEnumerator RestRescuerAftermath(bool longRest)
    {
        if(!RescueTurnAllowed||rolling||!rescueCarrier.Conscious||rescueCarrier.Load.Loaded)yield break;
        long damageVersion=RestDamageVersion(BattleMap.RescueId),start=worldRounds,duration=longRest?rescueCarrier.Species.LongRestRounds:600;
        if(longRest&&!rescueCarrier.CanLongRest(start))yield break;
        yield return AdvanceAftermathRounds(duration,()=>rescueCarrier.Conscious&&RestDamageVersion(BattleMap.RescueId)==damageVersion);
        if(!aftermathAdvanceCompleted||!rescueCarrier.Conscious||RestDamageVersion(BattleMap.RescueId)!=damageVersion){rescueMessage="救援者休息被中断。";yield break;}
        if(longRest){if(rescueCarrier.FinishLongRest(start,worldRounds))FinishActorLongRest(BattleMap.RescueId);}else GrantShortRest(BattleMap.RescueId);
        rescueMessage="救援者休息完成；等到下次救援者回合再行动。";
    }
    IEnumerator RestExternalCaster()
    {
        if(!combat.Aftermath||rolling||!externalResurrectionCaster.CanLongRest(worldRounds))yield break;
        long start=worldRounds;yield return AdvanceAftermathRounds(ResurrectionCasterState.LongRestRounds);
        if(aftermathAdvanceCompleted&&externalResurrectionCaster.CompleteLongRest(start,worldRounds)){history.Insert(0,"固定外部施法者完成8小时长休，解除复生术疲惫；其他角色不获得长休收益。");aftermathMessage="外部施法者已恢复施法能力。";}
    }
    IEnumerator RestPlayer(bool longRest)
    {
        if(!OutsideCombat||!PlayerTurnAllowed)yield break;
        long damageVersion=RestDamageVersion(0),start=worldRounds,duration=longRest?(PlayerInBeastForm?4800:playerSpecies.LongRestRounds):600;
        if(longRest&&(combat.PlayerHp<=0||hasLongRest&&start-lastLongRestStart<14400))yield break;
        yield return AdvanceAftermathRounds(duration,()=>combat.PlayerHp>0&&!combat.PlayerDead&&RestDamageVersion(0)==damageVersion);
        if(!aftermathAdvanceCompleted||combat.PlayerHp<=0||combat.PlayerDead||RestDamageVersion(0)!=damageVersion){restMessage="休息被中断，未获得休息收益。";yield break;}
        combat.ClearShortDurationEffects();UpdateStableRecovery();
        if(!longRest){GrantShortRest(0);restMessage="短休完成，已恢复已接入的短休次数；可消耗生命骰治疗。";yield break;}
        lastLongRestStart=start;hasLongRest=true;combat.CompleteLongRest(true);playerResurrectionCaster.CompleteLongRest(start,worldRounds,duration);fighter.LongRest();druid.LongRest();playerSpeciesAbilities.LongRest();playerSpeciesSpellUses.LongRest();highElfCantripReplacement=true;if(steedRules!=null)steedRules.LongRest();EndWildShape("长休");combat.ClearPlayerTemporaryHp();knownFormReplacement=true;combat.HealPlayer(int.MaxValue);ResetRestResources();restMessage="长休完成，生命、生命骰及已接入的长休资源恢复。";
    }
}
