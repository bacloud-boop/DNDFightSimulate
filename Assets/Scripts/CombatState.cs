using System;
public sealed class CombatState
{
    public Func<bool,bool> DeathWardGuard {get;set;}
    bool PreventDeath(bool player)=>DeathWardGuard!=null&&DeathWardGuard(player);
    public bool PlayerBladeWard {get;set;}
    public int PlayerEnergyResistance {get;set;}=-1;
    public int MonsterEnergyResistance {get;set;}=-1;
    public int AdjustTypedDamage(bool player,int amount,int type){return type>=0&&(player?PlayerEnergyResistance:MonsterEnergyResistance)==type?amount/2:amount;}
    public int AdjustPlayerPacket(long[] packet){long total=0;for(int i=0;i<packet.Length;i++)total+=Math.Min(int.MaxValue,PlayerEnergyResistance==i?Math.Max(0,packet[i])/2:Math.Max(0,packet[i]));return (int)Math.Min(int.MaxValue,total);}
    public bool PlayerBaned {get;set;}
    public bool MonsterBaned {get;set;}
    public bool PlayerBlessed {get;set;}
    public bool MonsterBlessed {get;set;}
    int playerSliverUntil,monsterSliverUntil;
    public void ApplySliver(bool player){if(player)playerSliverUntil=Round+1;else monsterSliverUntil=Round+1;}
    public bool ConsumeSliver(bool player){bool active=player?playerSliverUntil>0:monsterSliverUntil>0;if(player)playerSliverUntil=0;else monsterSliverUntil=0;return active;}
    public bool MonsterAttackIsMelee {get;set;}=true;
    public bool PlayerParalyzed {get;set;}
    public bool PlayerSlowed {get;private set;}
    public bool MonsterSlowed {get;private set;}
    public bool PlayerOpportunityBlocked {get;private set;}
    public bool MonsterOpportunityBlocked {get;private set;}
    public int MonsterSpeedBonus {get;set;}
    public int MonsterSpeed => Math.Max(0,30+MonsterSpeedBonus-(MonsterSlowed?10:0));
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
    public void ResolveDeathSave(int die,int adjustment=0) {
        if(!NeedsDeathSave)return;
        if(die<1||die>20)throw new ArgumentOutOfRangeException(nameof(die));
        if(die==20){HealPlayer(1);return;}
        if(die==1)DeathFailures+=2;else if(die+adjustment>=10)DeathSuccesses++;else DeathFailures++;
        if(DeathFailures>=3)PlayerDead=true;
        else if(DeathSuccesses>=3){PlayerStable=true;DeathSuccesses=0;DeathFailures=0;}
    }
    int playerGuidedUntil,monsterGuidedUntil;
    public void ApplyGuidingLight(bool player){if(player)playerGuidedUntil=Round+1;else monsterGuidedUntil=Round+1;}
    public bool ConsumeGuidingLight(bool player){bool active=player?playerGuidedUntil>0:monsterGuidedUntil>0;if(player)playerGuidedUntil=0;else monsterGuidedUntil=0;return active;}
    int playerMockeryUntil;bool monsterMockery;
    public void ApplyMockery(bool player){if(player)playerMockeryUntil=Round+1;else monsterMockery=true;}
    public bool ConsumeMockery(bool player){bool active=player?playerMockeryUntil>0:monsterMockery;if(player)playerMockeryUntil=0;else monsterMockery=false;return active;}
    public void FinishMonsterTurnEffects(){monsterMockery=false;}
    int playerHealingBlockedUntil,monsterHealingBlockedUntil;
    public bool PlayerHealingBlocked => playerHealingBlockedUntil>0;
    public bool MonsterHealingBlocked => monsterHealingBlockedUntil>0;
    public void BlockHealing(bool player){if(player)playerHealingBlockedUntil=Round+1;else monsterHealingBlockedUntil=Round+1;}
    public void ExpirePlayerTurnEffects(){if(Round>=playerSliverUntil)playerSliverUntil=0;if(Round>=monsterSliverUntil)monsterSliverUntil=0;if(Round>=playerGuidedUntil)playerGuidedUntil=0;if(Round>=monsterGuidedUntil)monsterGuidedUntil=0;if(Round>=playerMockeryUntil)playerMockeryUntil=0;if(Round>=playerHealingBlockedUntil)playerHealingBlockedUntil=0;if(Round>=monsterHealingBlockedUntil)monsterHealingBlockedUntil=0;}
    public void ClearShortDurationEffects(){playerSliverUntil=monsterSliverUntil=0;playerGuidedUntil=0;monsterGuidedUntil=0;ShieldActive=false;PlayerSlowed=false;MonsterSlowed=false;PlayerOpportunityBlocked=false;MonsterOpportunityBlocked=false;playerMockeryUntil=0;monsterMockery=false;playerHealingBlockedUntil=0;monsterHealingBlockedUntil=0;}
    public int MonsterTemporaryHp {get;private set;}
    public void GrantMonsterTemporaryHp(int amount){if(Hp>0)MonsterTemporaryHp=Math.Max(MonsterTemporaryHp,Math.Max(0,amount));}
    public void ClearMonsterTemporaryHp(){MonsterTemporaryHp=0;}
    public int PlayerTemporaryHp {get;private set;}
    public void GrantPlayerTemporaryHp(int amount){PlayerTemporaryHp=Math.Max(PlayerTemporaryHp,Math.Max(0,amount));}
    public void ClearPlayerTemporaryHp(){PlayerTemporaryHp=0;}
    int? playerArmorOverride;
    public bool ShieldActive {get;private set;}
    public void ActivateShield(){ShieldActive=true;}
    public int PlayerArmorBonus {get;set;}
    public int PlayerArmorFloor {get;set;}
    public int MonsterArmorBonus {get;set;}
    public int MonsterArmorFloor {get;set;}
    public int PlayerArmorClass => Math.Max(PlayerArmorFloor,(playerArmorOverride??10)+(ShieldActive?5:0)+PlayerArmorBonus);
    public void SetPlayerArmorOverride(int? armor){playerArmorOverride=armor;}
    public int MonsterProficiencyBonus { get; set; } = 0;
    public int MonsterAttackRange { get; private set; } = 5;
    public void UpdateMonsterAttackRange(int rangeFeet) {
        if(rangeFeet<0)throw new ArgumentOutOfRangeException(nameof(rangeFeet));
        MonsterAttackRange=rangeFeet;
    }
    int monsterBaseArmor=10;
    public int MonsterArmorClass => Math.Max(MonsterArmorFloor,monsterBaseArmor+MonsterArmorBonus);
    public void UpdateMonsterArmorClass(int armorClass) {
        if(armorClass < 5 || armorClass > 30) throw new ArgumentOutOfRangeException(nameof(armorClass));
        monsterBaseArmor = armorClass;
    }
    int monsterBaseMaxHp=1000,playerBaseMaxHp=10;
    public int PlayerAidBonus {get;private set;}
    public int MonsterAidBonus {get;private set;}
    readonly bool[] playerAidCredits=new bool[40],monsterAidCredits=new bool[40];
    public int MaxHp => (int)Math.Min(int.MaxValue,(long)monsterBaseMaxHp+MonsterAidBonus);
    public int PlayerMaxHp => (int)Math.Min(int.MaxValue,(long)playerBaseMaxHp+PlayerAidBonus);
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
    public void KillMonster(){if(Hp>0&&PreventDeath(false))return;Hp=0;MonsterTemporaryHp=0;}
    public void KillPlayer(){if(!PlayerDead&&PreventDeath(true))return;PlayerTemporaryHp=0;PlayerHp=0;PlayerDead=true;PlayerStable=false;PlayerReactionAvailable=false;}
    public int HealMonster(int amount){if(Hp==0||MonsterHealingBlocked)return 0;int before=Hp;Hp=(int)Math.Min(MaxHp,(long)Hp+Math.Max(0,amount));return Hp-before;}
    public bool StabilizePlayer(){if(PlayerHp!=0||PlayerDead)return false;PlayerStable=true;DeathSuccesses=0;DeathFailures=0;return true;}
    public void SetAidBonus(bool player,int bonus)
    {
        if(bonus<0||bonus>40)throw new ArgumentOutOfRangeException(nameof(bonus));
        int old=player?PlayerAidBonus:MonsterAidBonus;var credits=player?playerAidCredits:monsterAidCredits;
        if(old==bonus)return;
        if(player)PlayerAidBonus=bonus;else MonsterAidBonus=bonus;
        if(bonus>old){int gained=player?HealPlayer(bonus-old):HealMonster(bonus-old);for(int i=old;i<bonus;i++)credits[i]=i-old<gained;}
        else {
            int remove=0;for(int i=bonus;i<old;i++){if(credits[i])remove++;credits[i]=false;}
            if(player){int before=PlayerHp;PlayerHp=Math.Min(PlayerMaxHp,Math.Max(0,PlayerHp-remove));if(before>0&&PlayerHp==0){if(PreventDeath(true))PlayerHp=1;else {PlayerStable=false;PlayerReactionAvailable=false;}}}
            else {int before=Hp;Hp=Math.Min(MaxHp,Math.Max(0,Hp-remove));if(before>0&&Hp==0&&PreventDeath(false))Hp=1;}
        }
    }
    void RefreshActions() {ActionsRemaining=1;ActionsThisRound=1;RestrictedActionsRemaining=0;BonusActionAvailable=true;}
    public void ResetBattle() {PlayerBaned=MonsterBaned=false;PlayerBlessed=MonsterBlessed=false;MonsterTemporaryHp=0;PlayerAidBonus=MonsterAidBonus=0;Array.Clear(playerAidCredits,0,40);Array.Clear(monsterAidCredits,0,40);PlayerArmorBonus=MonsterArmorBonus=PlayerArmorFloor=MonsterArmorFloor=0;PlayerBladeWard=false;PlayerEnergyResistance=MonsterEnergyResistance=-1;MonsterSpeedBonus=0;ClearShortDurationEffects();PlayerTemporaryHp=0;playerArmorOverride=null;PlayerParalyzed=false;MonsterParalyzed=false;MonsterProne=false;PlayerDead=false;PlayerStable=false;DeathSuccesses=0;DeathFailures=0;Hp=MaxHp;PlayerHp=PlayerMaxHp;Round=1;BattleStarted=false;PlayerReactionAvailable=true;MonsterReactionAvailable=true;RefreshActions();}
    public void ApplyDamage(int damage) { if(damage < 0) throw new ArgumentOutOfRangeException(nameof(damage)); int absorbed=Math.Min(damage,MonsterTemporaryHp);MonsterTemporaryHp-=absorbed;int remaining=damage-absorbed;if(Hp>0&&remaining>=Hp&&PreventDeath(false))Hp=1;else Hp = Math.Max(0, Hp - remaining); }
    public void ApplyPlayerDamage(int damage,bool critical=false) {
        if(damage<=0||PlayerDead)return;
        int originalDamage=damage,before=PlayerHp;
        int absorbed=Math.Min(damage,PlayerTemporaryHp);PlayerTemporaryHp-=absorbed;damage-=absorbed;
        if(before==0){PlayerStable=false;if(originalDamage>=PlayerMaxHp){PlayerDead=true;return;}DeathFailures+=critical?2:1;if(DeathFailures>=3)PlayerDead=true;return;}
        if(damage==0)return;
        if(damage>=before&&PreventDeath(true)){PlayerHp=1;return;}
        long overflow=(long)damage-before;
        if(overflow>=PlayerMaxHp){PlayerHp=0;PlayerDead=true;return;}
        PlayerHp=Math.Max(0,PlayerHp-damage);
        if(PlayerHp==0)PlayerReactionAvailable=false;
    }
    public void UpdatePlayerMaxHp(int constitution, PlayerClasses classes) {

        int newMaxHp = classes.CalculateMaxHp(constitution);
        if(newMaxHp != playerBaseMaxHp) { playerBaseMaxHp = newMaxHp; PlayerHp = PlayerMaxHp; }

    }
    public void UpdateMonsterMaxHp(int newMaxHp) {
        if(newMaxHp < 1) throw new ArgumentOutOfRangeException(nameof(newMaxHp));
        if(newMaxHp != monsterBaseMaxHp) { monsterBaseMaxHp = newMaxHp; Hp = MaxHp; }
    }
    public void EndRound() {ShieldActive=false;PlayerSlowed=false;MonsterSlowed=false;PlayerOpportunityBlocked=false; BattleStarted=true;Round++;PlayerReactionAvailable=true;RefreshActions(); }

}
