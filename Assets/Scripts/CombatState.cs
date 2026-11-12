using System;
public sealed class CombatState
{
    public bool PlayerParalyzed {get;set;}
    public bool PlayerSlowed {get;private set;}
    public bool MonsterSlowed {get;private set;}
    public bool PlayerOpportunityBlocked {get;private set;}
    public bool MonsterOpportunityBlocked {get;private set;}
    public int MonsterSpeed => MonsterSlowed?20:30;
    public void ApplyFrostSlow(bool player){if(player)PlayerSlowed=true;else MonsterSlowed=true;}
    public void BlockOpportunity(bool player){if(player)PlayerOpportunityBlocked=true;else MonsterOpportunityBlocked=true;}
    public bool MonsterProne {get;set;}
    public bool MonsterParalyzed {get;set;}
    public bool MonsterCanAct => Hp>0&&!MonsterParalyzed;
    public bool PlayerDead {get;private set;}
    public bool PlayerStable {get;private set;}
    public int DeathSuccesses {get;private set;}
    public int DeathFailures {get;private set;}
    public bool NeedsDeathSave => PlayerHp==0&&!PlayerDead&&!PlayerStable;
    public bool PlayerCanAct => PlayerHp>0&&!PlayerDead&&!PlayerParalyzed&&Hp>0;
    public bool BattleFinished => PlayerDead || (Hp==0 && (PlayerHp>0||PlayerStable));
    public string PlayerLifeLabel => PlayerDead?"死亡":PlayerParalyzed?"麻痹":PlayerHp>0?"清醒":PlayerStable?"稳定 / 昏迷":"倒地 / 昏迷";
    public void ResolveDeathSave(int die) {
        if(!NeedsDeathSave)return;
        if(die<1||die>20)throw new ArgumentOutOfRangeException(nameof(die));
        if(die==20){HealPlayer(1);return;}
        if(die==1)DeathFailures+=2;else if(die>=10)DeathSuccesses++;else DeathFailures++;
        if(DeathFailures>=3)PlayerDead=true;
        else if(DeathSuccesses>=3){PlayerStable=true;DeathSuccesses=0;DeathFailures=0;}
    }
    int playerMockeryUntil;bool monsterMockery;
    public void ApplyMockery(bool player){if(player)playerMockeryUntil=Round+1;else monsterMockery=true;}
    public bool ConsumeMockery(bool player){bool active=player?playerMockeryUntil>0:monsterMockery;if(player)playerMockeryUntil=0;else monsterMockery=false;return active;}
    public void FinishMonsterTurnEffects(){monsterMockery=false;}
    int playerHealingBlockedUntil,monsterHealingBlockedUntil;
    public bool PlayerHealingBlocked => playerHealingBlockedUntil>0;
    public bool MonsterHealingBlocked => monsterHealingBlockedUntil>0;
    public void BlockHealing(bool player){if(player)playerHealingBlockedUntil=Round+1;else monsterHealingBlockedUntil=Round+1;}
    public void ExpirePlayerTurnEffects(){if(Round>=playerMockeryUntil)playerMockeryUntil=0;if(Round>=playerHealingBlockedUntil)playerHealingBlockedUntil=0;if(Round>=monsterHealingBlockedUntil)monsterHealingBlockedUntil=0;}
    public void ClearShortDurationEffects(){ShieldActive=false;PlayerSlowed=false;MonsterSlowed=false;PlayerOpportunityBlocked=false;MonsterOpportunityBlocked=false;playerMockeryUntil=0;monsterMockery=false;playerHealingBlockedUntil=0;monsterHealingBlockedUntil=0;}
    public int PlayerTemporaryHp {get;private set;}
    public void GrantPlayerTemporaryHp(int amount){PlayerTemporaryHp=Math.Max(PlayerTemporaryHp,Math.Max(0,amount));}
    public void ClearPlayerTemporaryHp(){PlayerTemporaryHp=0;}
    int? playerArmorOverride;
    public bool ShieldActive {get;private set;}
    public void ActivateShield(){ShieldActive=true;}
    public int PlayerArmorClass => (playerArmorOverride??10)+(ShieldActive?5:0);
    public void SetPlayerArmorOverride(int? armor){playerArmorOverride=armor;}
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
        if(player) {if(!PlayerCanAct||!PlayerReactionAvailable)return false;PlayerReactionAvailable=false;}
        else {if(!MonsterCanAct||!MonsterReactionAvailable)return false;MonsterReactionAvailable=false;}
        BattleStarted=true;return true;
    }
    public void StartMonsterTurn() {MonsterOpportunityBlocked=false;MonsterReactionAvailable=true;}
    public void MarkBattleStarted() { BattleStarted=true; }
    public int ActionsRemaining { get; private set; } = 1;
    public int ActionsThisRound { get; private set; } = 1;
    public int RestrictedActionsRemaining {get;private set;}
    public bool MagicActionAvailable => PlayerCanAct&&ActionsRemaining>RestrictedActionsRemaining;
    public bool TryUseMagicAction(){if(!MagicActionAvailable)return false;BattleStarted=true;ActionsRemaining--;return true;}
    public bool ActionAvailable => PlayerCanAct&&ActionsRemaining>0;
    public bool BonusActionAvailable { get; private set; } = true;
    public bool TryUseAction() {
        if(!ActionAvailable) return false;
        BattleStarted=true;ActionsRemaining--;if(RestrictedActionsRemaining>0)RestrictedActionsRemaining--;return true;
    }
    public bool TryUseBonusAction() {
        if(!PlayerCanAct||!BonusActionAvailable) return false;
        BattleStarted=true;BonusActionAvailable=false;return true;
    }
    public void AddSurgeAction() {if(!PlayerCanAct)return; BattleStarted=true;ActionsRemaining++;ActionsThisRound++;RestrictedActionsRemaining++; }
    public int HealPlayer(int amount) {if(PlayerDead||PlayerHealingBlocked)return 0;int before=PlayerHp;PlayerHp=(int)Math.Min(PlayerMaxHp,(long)PlayerHp+Math.Max(0,amount));if(PlayerHp>0){PlayerStable=false;DeathSuccesses=0;DeathFailures=0;}return PlayerHp-before;}
    public void KillPlayer(){PlayerTemporaryHp=0;PlayerHp=0;PlayerDead=true;PlayerStable=false;PlayerReactionAvailable=false;}
    public int HealMonster(int amount){if(Hp==0||MonsterHealingBlocked)return 0;int before=Hp;Hp=(int)Math.Min(MaxHp,(long)Hp+Math.Max(0,amount));return Hp-before;}
    public bool StabilizePlayer(){if(PlayerHp!=0||PlayerDead)return false;PlayerStable=true;DeathSuccesses=0;DeathFailures=0;return true;}
    void RefreshActions() {ActionsRemaining=1;ActionsThisRound=1;RestrictedActionsRemaining=0;BonusActionAvailable=true;}
    public void ResetBattle() {ClearShortDurationEffects();PlayerTemporaryHp=0;playerArmorOverride=null;PlayerParalyzed=false;MonsterParalyzed=false;MonsterProne=false;PlayerDead=false;PlayerStable=false;DeathSuccesses=0;DeathFailures=0;Hp=MaxHp;PlayerHp=PlayerMaxHp;Round=1;BattleStarted=false;PlayerReactionAvailable=true;MonsterReactionAvailable=true;RefreshActions();}
    public void ApplyDamage(int damage) { if(damage < 0) throw new ArgumentOutOfRangeException(nameof(damage)); Hp = Math.Max(0, Hp - damage); }
    public void ApplyPlayerDamage(int damage,bool critical=false) {
        if(damage<=0||PlayerDead)return;
        int originalDamage=damage,before=PlayerHp;
        int absorbed=Math.Min(damage,PlayerTemporaryHp);PlayerTemporaryHp-=absorbed;damage-=absorbed;
        if(before==0){PlayerStable=false;if(originalDamage>=PlayerMaxHp){PlayerDead=true;return;}DeathFailures+=critical?2:1;if(DeathFailures>=3)PlayerDead=true;return;}
        if(damage==0)return;
        long overflow=(long)damage-before;
        if(overflow>=PlayerMaxHp){PlayerHp=0;PlayerDead=true;return;}
        PlayerHp=Math.Max(0,PlayerHp-damage);
        if(PlayerHp==0)PlayerReactionAvailable=false;
    }
    public void UpdatePlayerMaxHp(int constitution, PlayerClasses classes) {

        int newMaxHp = classes.CalculateMaxHp(constitution);
        if(newMaxHp != PlayerMaxHp) { PlayerMaxHp = newMaxHp; PlayerHp = newMaxHp; }

    }
    public void UpdateMonsterMaxHp(int newMaxHp) {
        if(newMaxHp < 1) throw new ArgumentOutOfRangeException(nameof(newMaxHp));
        if(newMaxHp != MaxHp) { MaxHp = newMaxHp; Hp = newMaxHp; }
    }
    public void EndRound() {ShieldActive=false;PlayerSlowed=false;MonsterSlowed=false;PlayerOpportunityBlocked=false; BattleStarted=true;Round++;PlayerReactionAvailable=true;RefreshActions(); }

}
