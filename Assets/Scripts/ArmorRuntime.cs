using System;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly ArmorEquipment playerArmor=new ArmorEquipment(),monsterArmor=new ArmorEquipment(true);
    readonly WeaponGripState weaponGrip=new WeaponGripState();
    readonly AmmunitionState ammunition=new AmmunitionState();
    ArmorTraining monsterArmorTraining=ArmorTraining.Light|ArmorTraining.Medium|ArmorTraining.Heavy|ArmorTraining.Shield;
    bool clericProtector,druidWarden;
    ArmorTraining PlayerArmorTraining=>ArmorEquipment.ForPlayer(classes,clericProtector,druidWarden);
    bool PlayerArmorMelded=>PlayerInBeastForm;
    bool PlayerArmorUntrained=>playerArmor.Untrained(PlayerArmorTraining,PlayerArmorMelded);
    bool PlayerHasShield=>playerArmor.Shield&&!PlayerArmorMelded;
    bool PlayerWeaponReactionReady=>PlayerInBeastForm||!weaponGrip.Stowed&&WeaponHandError(selectedWeapon)=="";
    bool MonsterArmorAttackDisadvantage=>!MonsterPolymorphed&&combat.MonsterAttackIsWeapon&&monsterArmor.Untrained(monsterArmorTraining);
    bool MonsterCanCastAttack=>MonsterPolymorphed||(combat.MonsterAttackIsWeapon?(!combat.MonsterWeaponIsHeld||!monsterGroundWeapon.Dropped):!monsterArmor.Untrained(monsterArmorTraining));
    int PlayerArmorSpeedPenalty=>playerArmor.SpeedPenalty(EffectivePlayerAbility(0),PlayerArmorMelded);
    int MonsterArmorSpeedPenalty=>monsterArmor.SpeedPenalty(EffectiveMonsterAbility(0),MonsterPolymorphed);
    bool ArmorD20Disadvantage(bool player,int ability)=>(player?playerArmor:monsterArmor).D20Disadvantage(ability,player?PlayerArmorTraining:monsterArmorTraining,player?PlayerArmorMelded:MonsterPolymorphed);
    bool ArmorSpellAllowed(string spell)
    {
        if(!playerResurrectionCaster.CanCast||!SpellVoiceAllowed(true,spell))return false;
        if(PlayerArmorUntrained)return false;
        if(!PlayerHasShield||weaponGrip.Stowed)return true;
        if(spellMetadata==null)spellMetadata=JsonUtility.FromJson<SpellMetadataSet>(Resources.Load<TextAsset>("Rules2024/Spells").text);
        foreach(var entry in spellMetadata.spells)if(entry.name==spell)return Array.IndexOf(entry.components,"姿势")<0;
        return true;
    }
    string WeaponHandError(Weapon weapon)=>!carriedWeapons.Carried(weapon.Name)?"武器未携带或已掉落。":PlayerHasShield&&weapon.RequiresTwoHands?"盾牌占用一只手，无法使用双手武器攻击。":!ammunition.CanFire(weapon.Name,!PlayerHasShield)?"武器未装填；盾牌占手，无法随攻击装填。":"";
    sealed class ArmorSaveRoll {public int Value;public int[] Dice;}
    ArmorSaveRoll RollArmorSave(bool player,int ability,bool disadvantage=false,bool avoidsOrEndsPoisoned=false,bool avoidsOrEndsCharmed=false,bool avoidsOrEndsFrightened=false)
    {
        disadvantage|=ability==1&&Entangled(player);
        disadvantage|=SizeStrengthDisadvantage(player,ability);
        disadvantage|=ArmorD20Disadvantage(player,ability)||RayStrengthDisadvantage(player,ability);
        bool advantage=SpeciesSaveAdvantage(player,ability,avoidsOrEndsPoisoned,avoidsOrEndsCharmed,avoidsOrEndsFrightened)||SizeStrengthAdvantage(player,ability)||avoidsOrEndsPoisoned&&combat.HasPoisonConditionSaveAdvantage(player);
        return RollD20Test(player,advantage,disadvantage);
    }
    void SyncArmorEquipment()
    {
        mageArmor.Observe(true,MageArmorTargetWearing(true),worldRounds);mageArmor.Observe(false,MageArmorTargetWearing(false),worldRounds);
        int dex=EffectivePlayerAbility(1);
        int armor=ArmorFormulaRules.BestBase(playerArmor,dex,EffectivePlayerAbility(2),EffectivePlayerAbility(3),!PlayerPolymorphed&&classes[10]>0,!PlayerPolymorphed&&classes[8]>0,PlayerArmorMelded?PlayerBeast.armor:-1,PlayerArmorMelded,mageArmor.BaseArmor(true,dex,worldRounds));
        combat.PlayerEquipmentArmor=armor+playerArmor.ShieldBonus(PlayerArmorTraining,PlayerArmorMelded);
        int monsterBase=MonsterPolymorphed?PolymorphBeast.armor:monsterArmor.Custom?combat.MonsterBaseArmorClass:monsterArmor.BaseArmor(EffectiveMonsterAbility(1));
        monsterBase=Math.Max(monsterBase,mageArmor.BaseArmor(false,EffectiveMonsterAbility(1),worldRounds));
        combat.MonsterEquipmentArmor=monsterBase+monsterArmor.ShieldBonus(monsterArmorTraining,MonsterPolymorphed);
        playerWeight.Weight.SetArmorWeight(playerArmor.Weight);monsterWeight.Weight.SetArmorWeight(monsterArmor.Weight);
    }
    void ToggleHeldWeapon()
    {
        if(!PlayerTurnAllowed||rolling||spellTargeting||reactionPending||PlayerInBeastForm||combat.PlayerHp<=0||combat.PlayerDead||combat.PlayerParalyzed||combat.PlayerFeignDeath||summonTurns.Active)return;
        bool preparation=page=="deployment"||!map.Locked;
        bool stow=!weaponGrip.Stowed;if(!stow&&!carriedWeapons.Carried(selectedWeapon.Name)){dice="武器未携带或已掉落，无法取出。";return;}
        if(!weaponGrip.TrySetStowed(stow,combat.Round,worldRounds,()=>combat.TryUseAction(),preparation)){dice="本回合自由交互已用，且没有可用动作收起／取出武器。";return;}
        if(!preparation)combat.MarkBattleStarted();RefreshShillelagh();
        history.Insert(0,(stow?"收起":"取出")+selectedWeapon.Name+"。每回合一次自由物件交互，额外操作消耗动作；攻击动作还可使用自身的装备操作额度。");
    }
    bool SelectHeldWeapon(Weapon weapon)
    {
        if(!PlayerTurnAllowed||combat.PlayerFeignDeath||!carriedWeapons.Carried(weapon.Name))return false;
        if(weapon==selectedWeapon)return true;
        if(combat.BattleStarted&&!weaponGrip.Stowed&&!weaponGrip.TrySetStowed(true,combat.Round,worldRounds,()=>combat.TryUseAction())){dice="请先收起当前武器：自由交互已用且没有可用动作。";return false;}
        selectedWeapon=weapon;RefreshShillelagh();return true;
    }
}
