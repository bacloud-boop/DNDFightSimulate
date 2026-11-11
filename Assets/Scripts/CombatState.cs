using System;
public sealed class CombatState
{
    public int PlayerArmorClass { get; } = 10;
    public int MonsterProficiencyBonus { get; set; } = 0;
    public int MonsterAttackRange { get; private set; } = 5;
    public void UpdateMonsterAttackRange(int rangeFeet) {
        if(rangeFeet<0)throw new ArgumentOutOfRangeException(nameof(rangeFeet));
        MonsterAttackRange=rangeFeet;
    }
    public int MonsterArmorClass { get; private set; } = 10;
    public void UpdateMonsterArmorClass(int armorClass) {
        if(armorClass < 5 || armorClass > 30) throw new ArgumentOutOfRangeException(nameof(armorClass));
        MonsterArmorClass = armorClass;
    }
    public int MaxHp { get; private set; } = 1000;
    public int PlayerMaxHp { get; private set; } = 10;
    public int Hp { get; private set; } = 1000;
    public int PlayerHp { get; private set; } = 10;
    public int Round { get; private set; } = 1;
    public bool BattleStarted { get; private set; }
    public bool PlayerReactionAvailable { get; private set; } = true;
    public bool MonsterReactionAvailable { get; private set; } = true;
    public bool TryUseReaction(bool player) {
        if(player) {if(!PlayerReactionAvailable)return false;PlayerReactionAvailable=false;}
        else {if(!MonsterReactionAvailable)return false;MonsterReactionAvailable=false;}
        BattleStarted=true;return true;
    }
    public void StartMonsterTurn() {MonsterReactionAvailable=true;}
    public void MarkBattleStarted() { BattleStarted=true; }
    public int ActionsRemaining { get; private set; } = 1;
    public int ActionsThisRound { get; private set; } = 1;
    public int RestrictedActionsRemaining {get;private set;}
    public bool MagicActionAvailable => ActionsRemaining>RestrictedActionsRemaining;
    public bool TryUseMagicAction(){if(!MagicActionAvailable)return false;BattleStarted=true;ActionsRemaining--;return true;}
    public bool ActionAvailable => ActionsRemaining>0;
    public bool BonusActionAvailable { get; private set; } = true;
    public bool TryUseAction() {
        if(!ActionAvailable) return false;
        BattleStarted=true;ActionsRemaining--;if(RestrictedActionsRemaining>0)RestrictedActionsRemaining--;return true;
    }
    public bool TryUseBonusAction() {
        if(!BonusActionAvailable) return false;
        BattleStarted=true;BonusActionAvailable=false;return true;
    }
    public void AddSurgeAction() { BattleStarted=true;ActionsRemaining++;ActionsThisRound++;RestrictedActionsRemaining++; }
    public int HealPlayer(int amount) {int before=PlayerHp;PlayerHp=(int)Math.Min(PlayerMaxHp,(long)PlayerHp+Math.Max(0,amount));return PlayerHp-before;}
    void RefreshActions() {ActionsRemaining=1;ActionsThisRound=1;RestrictedActionsRemaining=0;BonusActionAvailable=true;}
    public void ResetBattle() {Hp=MaxHp;PlayerHp=PlayerMaxHp;Round=1;BattleStarted=false;PlayerReactionAvailable=true;MonsterReactionAvailable=true;RefreshActions();}
    public void ApplyDamage(int damage) { if(damage < 0) throw new ArgumentOutOfRangeException(nameof(damage)); Hp = Math.Max(0, Hp - damage); }
    public void ApplyPlayerDamage(int damage) { PlayerHp = Math.Max(0, PlayerHp - Math.Max(0, damage)); }
    public void UpdatePlayerMaxHp(int constitution, PlayerClasses classes) {

        int newMaxHp = classes.CalculateMaxHp(constitution);
        if(newMaxHp != PlayerMaxHp) { PlayerMaxHp = newMaxHp; PlayerHp = newMaxHp; }

    }
    public void UpdateMonsterMaxHp(int newMaxHp) {
        if(newMaxHp < 1) throw new ArgumentOutOfRangeException(nameof(newMaxHp));
        if(newMaxHp != MaxHp) { MaxHp = newMaxHp; Hp = newMaxHp; }
    }
    public void EndRound() { BattleStarted=true;Round++;PlayerReactionAvailable=true;RefreshActions(); }
    public bool ResetIfDead() { if(Hp>0 && PlayerHp>0) return false; ResetBattle(); return true; }
}
