using System.Collections;
using UnityEngine;

public sealed partial class TrainingGame
{
    bool CanUseFeature => combat.PlayerCanAct && !spellTargeting && !rolling && panel=="" && page=="battle" && draggingToken<0;
    void SecondWind()
    {
        if(PlayerPolymorphed || !CanUseFeature || fighter.WindRemaining(classes[0])<=0 || !combat.TryUseBonusAction())return;
        fighter.UseWind();StartCoroutine(HealWithSecondWind(classes[0]));
    }
    IEnumerator HealWithSecondWind(int fighterLevel)
    {
        rolling=true;logScroll=Vector2.zero;
        int value=Random.Range(1,11);
        dice="回气 · 1d10 + 战士等级 "+fighterLevel;
        yield return PresentRoll(10,new[]{value},"回气："+value+" + 战士等级 "+fighterLevel+" = "+(value+fighterLevel)+"，治疗不超过生命上限");
        int healing=value+fighterLevel,actual=combat.HealPlayer(healing);
        dice="回气 D10="+value+" + "+fighterLevel+"，实际恢复 "+actual;
        history.Insert(0,dice+"，生命 "+combat.PlayerHp+" / "+combat.PlayerMaxHp+"。");
        if(fighterLevel>=5){map.GrantTacticalMovement();history.Insert(0,"战术转进：获得独立 "+(map.PlayerSpeed/2)+" 尺移动，本回合使用，不引发借机攻击。");}
        rolling=false;
    }
    void ActionSurge()
    {
        if(PlayerPolymorphed || !CanUseFeature || !fighter.CanSurge(classes[0]))return;
        fighter.UseSurge();combat.AddSurgeAction();
        dice="动作如潮 · 本回合额外获得 1 个动作";
        history.Insert(0,dice+"，本回合不能再次使用。");logScroll=Vector2.zero;
    }
    void ResetCombat()
    {
        if(rolling || panel!="" || draggingToken>=0)return;
        playerSpeciesAbilities.Reset();monsterSpeciesAbilities.Reset();playerSpeciesSpellUses.LongRest();highElfCantripReplacement=false;
        EndFaerieFire();spikeGrowth.Clear();spikeEntries.Clear();grease.Clear();greaseEntries.Clear();EndEntangle();poisonProtection.Clear();
        jumpEffect.Clear();
        commandEffect.Clear();commandTargets.Clear();commandTargets.Add(-1);commandActor=int.MaxValue;
        ExitAftermathTurns();EndCloudDaggers();EndWeb();terrainMovementEvent=0;terrainTurn=0;terrainActor=0;turnConditions.Clear();ClearStarryWisp();feignDeath.Clear();SyncFeignDeath();sensory.Clear();enfeeblement.Clear();
        monsterSpells.Reset();playerResurrectionCaster.Reset();externalResurrectionCaster.Reset();
        rescueCarrier.Reset();map.Remains.Clear();playerWeight.Weight.SetStoredGear(0);monsterWeight.Weight.SetStoredGear(0);playerCorpseLoad.Clear();monsterCorpseLoad.Clear();
        DismissSteed();mountFallPending=false;steedFears.Clear();summonTurns.Reset();summonActing=summonEndRequested=false;
        shillelagh.Clear();EndLightSpell("");equipmentObjects.ResetBattle();
        weaponGrip.Reset();
        ammunition.Reset();
        mageArmor.Clear();
        playerBreathing.Reset();monsterBreathing.Reset();waterBreathingExpires[0]=waterBreathingExpires[1]=0;selectedWaterBreathingSummons.Clear();
        senses.Clear();hasMonsterLongRest=false;playerCorpse.Clear();monsterCorpse.Clear();revivalWilling=false;mirrorImage.Clear();deathWard.Clear();vision.ClearEffects();agathys.Clear();aidEffects.Clear();timedBuffs.Clear();combat.MonsterSpeedBonus=0;EndConcentration("重置战斗");EndWildShape("重置战斗");druid.LongRest();knownFormReplacement=false;outcomeRecorded=false;stableRecoveryAt=-1;combat.ResetBattle();ResetWeaponInventory();fighter.ResetBattle();actionWeapon=null;BeginDeployment();
        dice="战斗已重置 · 可修改配置";
        history.Insert(0,"—— 重置战斗：双方回满、回合归 1、特性次数与行动额度恢复 ——");
        logScroll=Vector2.zero;
    }
}
