using System;
public sealed class CombatState
{
    public bool PlayerBanished,MonsterBanished,MonsterDeparted;
    public bool PlayerProne {get;set;}
    public bool PlayerFeignDeath,MonsterFeignDeath;
    public Func<bool,int,long,int> ExtraConditionRemoval;
    public bool Aftermath {get;set;}
    public bool AftermathPlayerTurn {get;set;}=true;
    public int PlayerRevivalPenalty {get;private set;}
    public int MonsterRevivalPenalty {get;private set;}
    public int RevivalPenalty(bool player)=>player?PlayerRevivalPenalty:MonsterRevivalPenalty;
    public void CompleteLongRest(bool player){if(player)PlayerMaxHpReduction=0;else MonsterMaxHpReduction=0;if(player)PlayerRevivalPenalty=Math.Min(0,PlayerRevivalPenalty+1);else MonsterRevivalPenalty=Math.Min(0,MonsterRevivalPenalty+1);}
    public void ClearPoison(bool player){if(player)playerPoisonUntil=0;else monsterPoisonUntil=0;}
    public bool Revive(bool player,int hp,bool ordeal){
        if(hp<1)return false;
        if(player){if(!PlayerDead)return false;PlayerDead=false;PlayerStable=false;DeathFailures=DeathSuccesses=0;PlayerHp=Math.Min(PlayerMaxHp,hp);if(ordeal)PlayerRevivalPenalty=-4;}
        else {if(Hp>0)return false;Hp=Math.Min(MaxHp,hp);if(ordeal)MonsterRevivalPenalty=-4;}return true;
    }
    public Action<bool> DamageDealt {get;set;}
    public Func<bool,int,bool> DamageImmunity {get;set;}
    public Func<bool,int,bool> DamageResistance {get;set;}
    public Func<bool,bool> PoisonConditionSaveAdvantage {get;set;}
    public bool HasPoisonConditionSaveAdvantage(bool player)=>PoisonConditionSaveAdvantage!=null&&PoisonConditionSaveAdvantage(player);
    public Func<bool,bool> DeathWardGuard {get;set;}
    bool PreventDeath(bool player)=>DeathWardGuard!=null&&DeathWardGuard(player);
    public bool PlayerBladeWard {get;set;}
    public int PlayerEnergyResistance {get;set;}=-1;
    public int MonsterEnergyResistance {get;set;}=-1;
    public bool PlayerUnderwater {get;set;}
    public bool MonsterUnderwater {get;set;}
    bool ResistsDamage(bool player,int type)=>type>=0&&((UniversalResistance!=null&&UniversalResistance(player))||((player?PlayerFeignDeath:MonsterFeignDeath)&&type!=10)||(player?PlayerEnergyResistance:MonsterEnergyResistance)==type||(DamageResistance!=null&&DamageResistance(player,type))||(type==5&&(player?PlayerUnderwater:MonsterUnderwater)));
    public Func<bool,int> WardBonus;
    public Func<bool,bool> UniversalResistance;public Func<bool,int,bool> DamageVulnerability;
    public Action<bool,long[]> DamageApplied;
    public Action<bool,int?,int> DamageReceived;
    readonly long[][] pendingTypes=new long[2][];
    public long[] CaptureAdjustedDamage(bool player)=>pendingTypes[player?0:1]==null?null:(long[])pendingTypes[player?0:1].Clone();
    public long[] ResolveTypedPacket(bool player,long[] raw){var result=new long[raw.Length];long remaining=int.MaxValue;for(int i=0;i<raw.Length;i++){long value=System.Math.Max(0,raw[i]);if(i<13){if(DamageImmunity!=null&&DamageImmunity(player,i))value=0;else {if(ResistsDamage(player,i))value/=2;if(DamageVulnerability!=null&&DamageVulnerability(player,i))value=value>int.MaxValue/2?int.MaxValue:value*2;}}result[i]=System.Math.Min(remaining,value);remaining-=result[i];}return result;}
    long[] DamageEvent(bool player,int amount,long[] explicitTypes){
        if(explicitTypes!=null)return (long[])explicitTypes.Clone();int index=player?0:1;var packet=pendingTypes[index];pendingTypes[index]=null;long total=0;if(packet!=null)foreach(long v in packet)total+=v;
        if(packet!=null&&total==amount)return packet;var untyped=new long[14];untyped[13]=amount;return untyped;
    }
    public int AdjustTypedDamage(bool player,int amount,int type){var raw=new long[14];raw[type>=0&&type<13?type:13]=System.Math.Max(0,amount);return AdjustDamagePacket(player,raw);}
    public int AdjustPlayerPacket(long[] packet)=>AdjustDamagePacket(true,packet);
    public int AdjustDamagePacket(bool player,long[] packet){if(packet.Length>5&&packet[5]>0)FireExposure?.Invoke(player);var resolved=ResolveTypedPacket(player,packet);pendingTypes[player?0:1]=resolved;long total=0;foreach(long v in resolved)total+=v;return (int)total;}
    public bool PlayerBaned {get;set;}
    public bool MonsterBaned {get;set;}
    public bool PlayerBlessed {get;set;}
    public bool MonsterBlessed {get;set;}
    int playerPoisonUntil,monsterPoisonUntil;
    public bool PlayerPoisoned=>!PlayerFeignDeath&&playerPoisonUntil>0;
    public bool MonsterPoisoned=>!MonsterFeignDeath&&monsterPoisonUntil>0;
    public void ApplyPoison(bool player){if(player?PlayerFeignDeath:MonsterFeignDeath)return;if(player)playerPoisonUntil=Round+1;else monsterPoisonUntil=Round+1;}
    int playerSliverUntil,monsterSliverUntil;
    public void ApplySliver(bool player){if(player)playerSliverUntil=Round+1;else monsterSliverUntil=Round+1;}
    public bool ConsumeSliver(bool player){bool active=player?playerSliverUntil>0:monsterSliverUntil>0;if(player)playerSliverUntil=0;else monsterSliverUntil=0;return active;}
    public bool MonsterAttackIsMelee {get;set;}=true;
    public bool MonsterRestrained {get;set;}
    public bool MonsterAttackIsWeapon {get;set;}=true;
    public bool MonsterWeaponIsHeld {get;set;}
    public int MonsterWeaponOunces {get;set;}=16;
    public Func<bool,bool> MovementFreedom;
    bool playerParalysisEffect,monsterParalysisEffect,playerSlowEffect,monsterSlowEffect;
    bool HasMovementFreedom(bool player)=>MovementFreedom!=null&&MovementFreedom(player);
    // Suppression does not end the original spell, but a new magical effect
    // cannot be added while the target is protected. Clearing always removes it.
    public bool PlayerParalysisEffect=>playerParalysisEffect;
    public bool MonsterParalysisEffect=>monsterParalysisEffect;
    public bool PlayerParalyzed {get=>playerParalysisEffect&&!HasMovementFreedom(true);set{if(!value||!HasMovementFreedom(true))playerParalysisEffect=value;}}
    public bool PlayerSlowed {get=>playerSlowEffect&&!HasMovementFreedom(true);private set=>playerSlowEffect=value;}
    public bool MonsterSlowed {get=>monsterSlowEffect&&!HasMovementFreedom(false);private set=>monsterSlowEffect=value;}
    public bool PlayerReactionsBlocked {get;private set;}
    public bool MonsterReactionsBlocked {get;private set;}
    public void BlockReactions(bool player){if(player)PlayerReactionsBlocked=true;else MonsterReactionsBlocked=true;}
    public bool PlayerOpportunityBlocked {get;private set;}
    public bool MonsterOpportunityBlocked {get;private set;}
    public int MonsterSpeedBonus {get;set;}
    public int MonsterBaseSpeed {get;private set;}=30;
    public long MonsterHitBonus=>(long)MonsterProficiencyBonus+MonsterRevivalPenalty;
    public int MonsterBaseMaxHp=>monsterBaseMaxHp;
    public int MonsterBaseArmorClass=>monsterBaseArmor;
    public void UpdateMonsterBaseSpeed(int speed){if(speed<0)throw new ArgumentOutOfRangeException(nameof(speed));MonsterBaseSpeed=speed;}
    public void RestoreMonsterBase(int hp,int armor,int proficiency,int range,int speed,bool melee){
        if(hp<1||armor<5||armor>30||range<0||speed<0)throw new ArgumentException("Invalid monster form");
        monsterBaseMaxHp=hp;monsterBaseArmor=armor;MonsterProficiencyBonus=proficiency;MonsterAttackRange=range;MonsterBaseSpeed=speed;MonsterAttackIsMelee=melee;
    }
    public int? MonsterFormSpeed {get;set;}
    public int? MonsterFormArmor {get;set;}
    public Func<int,int> MonsterSpeedTransform;
    public int MonsterSpeed=>MonsterSpeedTransform==null?MonsterUnmodifiedSpeed:Math.Max(0,MonsterSpeedTransform(MonsterUnmodifiedSpeed));
    public int MonsterUnmodifiedSpeed => MonsterFeignDeath&&!HasMovementFreedom(false)||MonsterRestrained||MonsterSleepStage==2?0:(int)Math.Min(int.MaxValue,Math.Max(0,(long)(MonsterFormSpeed??MonsterBaseSpeed)+MonsterSpeedBonus-(MonsterSlowed?10:0)));
    public void ApplyFrostSlow(bool player){if(HasMovementFreedom(player))return;if(player)PlayerSlowed=true;else MonsterSlowed=true;}
    public void BlockOpportunity(bool player){if(player)PlayerOpportunityBlocked=true;else MonsterOpportunityBlocked=true;}
    public bool MonsterProne {get;set;}
    public bool MonsterParalyzed {get=>monsterParalysisEffect&&!HasMovementFreedom(false);set{if(!value||!HasMovementFreedom(false))monsterParalysisEffect=value;}}
    public int MonsterSleepStage {get;set;}
    public bool MonsterHelpless=>MonsterParalyzed||MonsterSleepStage==2;
    public Action MonsterDamaged {get;set;}
    public bool MonsterCanMove => !MonsterDeparted&& !Aftermath&&Hp>0&&!MonsterParalyzed&&(!MonsterFeignDeath||HasMovementFreedom(false))&&MonsterSleepStage<2;
    public bool MonsterCanAct => !MonsterBanished&&!MonsterDeparted&& MonsterSleepStage==0&& !Aftermath&&Hp>0&&!MonsterParalyzed&&!MonsterFeignDeath;
    public bool PlayerDead {get;private set;}
    public bool PlayerStable {get;private set;}
    public int DeathSuccesses {get;private set;}
    public int DeathFailures {get;private set;}
    public bool NeedsDeathSave => PlayerHp==0&&!PlayerDead&&!PlayerStable;
    public bool PlayerMovementReady=>PlayerHp>0&&!PlayerDead&&!PlayerParalyzed&&(!PlayerFeignDeath||HasMovementFreedom(true));
    public bool PlayerCanMove=>!Aftermath&&Hp>0&&PlayerMovementReady;
    public bool PlayerCanAct => !PlayerBanished&&!MonsterDeparted&& !Aftermath&&PlayerHp>0&&!PlayerDead&&!PlayerParalyzed&&!PlayerFeignDeath&&Hp>0;
    public bool BattleFinished => MonsterDeparted||PlayerDead || (Hp==0 && (PlayerHp>0||PlayerStable));
    public string PlayerLifeLabel => PlayerDead?"死亡":PlayerFeignDeath?"假死 / 失能":PlayerParalyzed?"麻痹":PlayerHp>0?"清醒":PlayerStable?"稳定 / 昏迷":"倒地 / 昏迷";
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
    public Action<int> TurnEnded,TurnStarted;
    public Action<bool> FireExposure;
    public void FinishMonsterTurnEffects(){monsterMockery=false;TurnEnded?.Invoke(-1);}
    int playerHealingBlockedUntil,monsterHealingBlockedUntil;
    public bool PlayerHealingBlocked => playerHealingBlockedUntil>0;
    public bool MonsterHealingBlocked => monsterHealingBlockedUntil>0;
    public void BlockHealing(bool player){if(player)playerHealingBlockedUntil=Round+1;else monsterHealingBlockedUntil=Round+1;}
    public void ExpirePlayerTurnEffects(){if(Round>=playerPoisonUntil)playerPoisonUntil=0;if(Round>=monsterPoisonUntil)monsterPoisonUntil=0;if(Round>=playerSliverUntil)playerSliverUntil=0;if(Round>=monsterSliverUntil)monsterSliverUntil=0;if(Round>=playerGuidedUntil)playerGuidedUntil=0;if(Round>=monsterGuidedUntil)monsterGuidedUntil=0;if(Round>=playerMockeryUntil)playerMockeryUntil=0;if(Round>=playerHealingBlockedUntil)playerHealingBlockedUntil=0;if(Round>=monsterHealingBlockedUntil)monsterHealingBlockedUntil=0;}
    public void ClearShortDurationEffects(){PlayerReactionsBlocked=MonsterReactionsBlocked=false;playerPoisonUntil=monsterPoisonUntil=0;playerSliverUntil=monsterSliverUntil=0;playerGuidedUntil=0;monsterGuidedUntil=0;ShieldActive=false;PlayerSlowed=false;MonsterSlowed=false;PlayerOpportunityBlocked=false;MonsterOpportunityBlocked=false;playerMockeryUntil=0;monsterMockery=false;playerHealingBlockedUntil=0;monsterHealingBlockedUntil=0;}
    public int MonsterTemporaryHp {get;private set;}
    public void GrantMonsterTemporaryHp(int amount){if(Hp>0)MonsterTemporaryHp=Math.Max(MonsterTemporaryHp,Math.Max(0,amount));}
    public void ClearMonsterTemporaryHp(){MonsterTemporaryHp=0;}
    public int PlayerTemporaryHp {get;private set;}
    public void GrantPlayerTemporaryHp(int amount){PlayerTemporaryHp=Math.Max(PlayerTemporaryHp,Math.Max(0,amount));}
    public void ClearPlayerTemporaryHp(){PlayerTemporaryHp=0;}
    // A choice replaces the single pool; it never adds the two grants together.
    public void ChooseTemporaryHp(bool player,int amount,bool takeNew)
    {
        if(amount<0)throw new ArgumentOutOfRangeException(nameof(amount));
        if(player){if(takeNew)PlayerTemporaryHp=amount;}
        else if(takeNew&&Hp>0)MonsterTemporaryHp=amount;
    }
    int? playerArmorOverride;
    public bool ShieldActive {get;private set;}
    public void ActivateShield(){ShieldActive=true;}
    public int PlayerArmorBonus {get;set;}
    public int PlayerArmorFloor {get;set;}
    public int MonsterArmorBonus {get;set;}
    public int MonsterArmorFloor {get;set;}
    public int? PlayerEquipmentArmor {get;set;}
    public int? MonsterEquipmentArmor {get;set;}
    public int PlayerArmorClass => (WardBonus==null?0:WardBonus(true))+Math.Max(PlayerArmorFloor,(PlayerEquipmentArmor??playerArmorOverride??10)+(ShieldActive?5:0)+PlayerArmorBonus);
    public void SetPlayerArmorOverride(int? armor){playerArmorOverride=armor;}
    public int MonsterProficiencyBonus { get; set; } = 0;
    public int MonsterAttackRange { get; private set; } = 5;
    public void UpdateMonsterAttackRange(int rangeFeet) {
        if(rangeFeet<0)throw new ArgumentOutOfRangeException(nameof(rangeFeet));
        MonsterAttackRange=rangeFeet;
    }
    int monsterBaseArmor=10;
    public int MonsterArmorClass => (WardBonus==null?0:WardBonus(false))+Math.Max(MonsterArmorFloor,(MonsterEquipmentArmor??MonsterFormArmor??monsterBaseArmor)+MonsterArmorBonus);
    public void UpdateMonsterArmorClass(int armorClass) {
        if(armorClass < 5 || armorClass > 30) throw new ArgumentOutOfRangeException(nameof(armorClass));
        monsterBaseArmor = armorClass;
    }
    int monsterBaseMaxHp=1000,playerBaseMaxHp=10;
    public int PlayerAidBonus {get;private set;}
    public int MonsterAidBonus {get;private set;}
    readonly bool[] playerAidCredits=new bool[40],monsterAidCredits=new bool[40];
    public int MonsterMaxHpReduction {get;private set;}
    public int PlayerMaxHpReduction {get;private set;}
    public int PlayerSpeciesHpBonus {get;private set;}
    public int MonsterSpeciesHpBonus {get;private set;}
    public void SetSpeciesHpBonus(bool player,int bonus,bool refill){if(bonus<0)throw new ArgumentOutOfRangeException(nameof(bonus));int old=player?PlayerMaxHp:MaxHp;if(player)PlayerSpeciesHpBonus=bonus;else MonsterSpeciesHpBonus=bonus;int current=player?PlayerMaxHp:MaxHp;if(old!=current){if(player)PlayerHp=refill?current:Math.Min(PlayerHp,current);else Hp=refill?current:Math.Min(Hp,current);}}
    public int MaxHp => (int)Math.Max(1,Math.Min(int.MaxValue,(long)monsterBaseMaxHp+MonsterAidBonus+MonsterSpeciesHpBonus)-MonsterMaxHpReduction);
    public int PlayerMaxHp => (int)Math.Max(1,Math.Min(int.MaxValue,(long)playerBaseMaxHp+PlayerAidBonus+PlayerSpeciesHpBonus)-PlayerMaxHpReduction);
    public int ReduceMaxHp(bool player,int amount)
    {
        if(amount<0)throw new ArgumentOutOfRangeException(nameof(amount));
        int reduction=Math.Min(amount,(player?PlayerMaxHp:MaxHp)-1);
        if(player){PlayerMaxHpReduction=(int)Math.Min(int.MaxValue,(long)PlayerMaxHpReduction+reduction);PlayerHp=Math.Min(PlayerHp,PlayerMaxHp);}
        else{MonsterMaxHpReduction=(int)Math.Min(int.MaxValue,(long)MonsterMaxHpReduction+reduction);Hp=Math.Min(Hp,MaxHp);}
        return reduction;
    }
    public int Hp { get; private set; } = 1000;
    public int PlayerHp { get; private set; } = 10;
    public int Round { get; private set; } = 1;
    public bool BattleStarted { get; private set; }
    public bool PlayerReactionAvailable { get; private set; } = true;
    public bool MonsterReactionAvailable { get; private set; } = true;
    public bool TryUseReaction(bool player) {
        if(player) {if(!PlayerCanAct||PlayerReactionsBlocked||!PlayerReactionAvailable)return false;PlayerReactionAvailable=false;}
        else {if(!MonsterCanAct||MonsterReactionsBlocked||!MonsterReactionAvailable)return false;MonsterReactionAvailable=false;}
        BattleStarted=true;return true;
    }
    public bool TryUseCompelledReaction(bool player){
        bool ready=player?PlayerHp>0&&!PlayerDead&&!PlayerParalyzed&&!PlayerFeignDeath&&!PlayerReactionsBlocked&&PlayerReactionAvailable:Hp>0&&!MonsterParalyzed&&!MonsterFeignDeath&&MonsterSleepStage==0&&!MonsterReactionsBlocked&&MonsterReactionAvailable;
        if(!ready)return false;if(player)PlayerReactionAvailable=false;else MonsterReactionAvailable=false;BattleStarted=true;return true;
    }
    public bool CanPlayerDamageReact=>PlayerHp>0&&!PlayerDead&&!PlayerParalyzed&&!PlayerFeignDeath&&!PlayerReactionsBlocked&&PlayerReactionAvailable;
    public bool TryUseDamageReaction(){if(!CanPlayerDamageReact)return false;PlayerReactionAvailable=false;BattleStarted=true;return true;}
    public bool MonsterActionAvailable {get;private set;}=true;
    public bool TryUseMonsterAction(){if(!MonsterCanAct||!MonsterActionAvailable)return false;MonsterActionAvailable=false;BattleStarted=true;return true;}
    public bool MonsterBonusActionAvailable {get;private set;}=true;
    public bool TryUseMonsterBonusAction(){if(!MonsterCanAct||!MonsterBonusActionAvailable)return false;MonsterBonusActionAvailable=false;BattleStarted=true;return true;}
    public void StartMonsterTurn(bool notify=true) {MonsterActionAvailable=true;MonsterBonusActionAvailable=true;MonsterReactionsBlocked=false;MonsterOpportunityBlocked=false;MonsterReactionAvailable=true;if(notify)TurnStarted?.Invoke(-1);}
    public void MarkBattleStarted() { BattleStarted=true; }
    public int ActionsRemaining { get; private set; } = 1;
    public int ActionsThisRound { get; private set; } = 1;
    public int RestrictedActionsRemaining {get;private set;}
    public bool MagicActionAvailable => PlayerCanAct&&ActionsRemaining>RestrictedActionsRemaining;
    public bool TryUseMagicAction(){if(!MagicActionAvailable)return false;BattleStarted=true;ActionsRemaining--;return true;}
    public bool AftermathMagicActionAvailable=>Aftermath&&AftermathPlayerTurn&&PlayerHp>0&&!PlayerDead&&!PlayerParalyzed&&!PlayerFeignDeath&&ActionsRemaining>RestrictedActionsRemaining;
    public bool TryUseAftermathMagicAction(){if(!Aftermath)return TryUseMagicAction();if(!AftermathMagicActionAvailable)return false;ActionsRemaining--;BattleStarted=true;return true;}
    public bool ActionAvailable => PlayerCanAct&&ActionsRemaining>0;
    public bool BonusActionAvailable { get; private set; } = true;
    public bool TryUseAction() {
        if(!ActionAvailable) return false;
        BattleStarted=true;ActionsRemaining--;if(RestrictedActionsRemaining>0)RestrictedActionsRemaining--;return true;
    }
    public bool TryUseObjectAction(){if(!Aftermath)return TryUseAction();if(!AftermathPlayerTurn||PlayerHp<=0||PlayerDead||PlayerParalyzed||PlayerFeignDeath||ActionsRemaining<1)return false;ActionsRemaining--;if(RestrictedActionsRemaining>0)RestrictedActionsRemaining--;BattleStarted=true;return true;}
    public bool TryUseBonusAction() {
        if(!PlayerCanAct||!BonusActionAvailable) return false;
        BattleStarted=true;BonusActionAvailable=false;return true;
    }
    public bool TryUseAftermathBonusAction(){if(!Aftermath)return TryUseBonusAction();if(!AftermathPlayerTurn||PlayerHp<=0||PlayerDead||PlayerParalyzed||PlayerFeignDeath||!BonusActionAvailable)return false;BonusActionAvailable=false;BattleStarted=true;return true;}
    public void AddSurgeAction() {if(!PlayerCanAct)return; BattleStarted=true;ActionsRemaining++;ActionsThisRound++;RestrictedActionsRemaining++; }
    public int HealPlayer(int amount) {if(PlayerDead||PlayerHealingBlocked)return 0;int before=PlayerHp;PlayerHp=(int)Math.Min(PlayerMaxHp,(long)PlayerHp+Math.Max(0,amount));if(PlayerHp>0){PlayerStable=false;DeathSuccesses=0;DeathFailures=0;}return PlayerHp-before;}
    public void KillMonster(){if(Hp>0&&PreventDeath(false))return;Hp=0;MonsterTemporaryHp=0;}
    // Existing lethal exhaustion is a persistent condition, not a new increment
    // for Death Ward to negate. Its protective check occurs before level six.
    public bool ApplyFatalExhaustion(bool player,int level)
    {
        if(level<6)return false;
        if(player){PlayerTemporaryHp=0;PlayerHp=0;PlayerDead=true;PlayerStable=false;PlayerReactionAvailable=false;}
        else {Hp=0;MonsterTemporaryHp=0;}
        return true;
    }
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
    public void ResetBattle() {PlayerBanished=MonsterBanished=MonsterDeparted=false;pendingTypes[0]=pendingTypes[1]=null;PlayerFeignDeath=MonsterFeignDeath=false;MonsterActionAvailable=true;MonsterBonusActionAvailable=true;MonsterSleepStage=0;PlayerUnderwater=MonsterUnderwater=false;PlayerMaxHpReduction=MonsterMaxHpReduction=0;PlayerProne=false;MonsterFormSpeed=MonsterFormArmor=null;Aftermath=false;AftermathPlayerTurn=true;PlayerRevivalPenalty=MonsterRevivalPenalty=0;PlayerBaned=MonsterBaned=false;PlayerBlessed=MonsterBlessed=false;MonsterTemporaryHp=0;PlayerAidBonus=MonsterAidBonus=0;Array.Clear(playerAidCredits,0,40);Array.Clear(monsterAidCredits,0,40);PlayerArmorBonus=MonsterArmorBonus=PlayerArmorFloor=MonsterArmorFloor=0;PlayerBladeWard=false;PlayerEnergyResistance=MonsterEnergyResistance=-1;MonsterSpeedBonus=0;ClearShortDurationEffects();PlayerTemporaryHp=0;playerArmorOverride=null;PlayerParalyzed=false;MonsterParalyzed=false;MonsterProne=false;PlayerDead=false;PlayerStable=false;DeathSuccesses=0;DeathFailures=0;Hp=MaxHp;PlayerHp=PlayerMaxHp;Round=1;BattleStarted=false;PlayerReactionAvailable=true;MonsterReactionAvailable=true;RefreshActions();}
    public void ApplyDamage(int damage,bool? sourcePlayer=true,long[] resolvedTypes=null,int? sourceActor=null) { if(damage < 0) throw new ArgumentOutOfRangeException(nameof(damage)); var eventTypes=DamageEvent(false,damage,resolvedTypes);int absorbed=Math.Min(damage,MonsterTemporaryHp);MonsterTemporaryHp-=absorbed;int remaining=damage-absorbed;if(Hp>0&&remaining>=Hp&&PreventDeath(false))Hp=1;else Hp = Math.Max(0, Hp - remaining);if(damage>0)DamageApplied?.Invoke(false,eventTypes);if(damage>0)DamageReceived?.Invoke(false,sourceActor??(sourcePlayer.HasValue?(int?)(sourcePlayer.Value?0:-1):null),damage);if(damage>0)MonsterDamaged?.Invoke();if(damage>0&&sourcePlayer.HasValue)DamageDealt?.Invoke(sourcePlayer.Value); }
    public void ApplyPlayerDamage(int damage,bool critical=false,bool? sourcePlayer=false,long[] resolvedTypes=null,int? sourceActor=null) {
        if(damage<=0||PlayerDead)return;
        var eventTypes=DamageEvent(true,damage,resolvedTypes);int received=damage;
        try{
        int originalDamage=damage,before=PlayerHp;
        int absorbed=Math.Min(damage,PlayerTemporaryHp);PlayerTemporaryHp-=absorbed;damage-=absorbed;
        if(before==0){PlayerStable=false;if(originalDamage>=PlayerMaxHp){PlayerDead=true;return;}DeathFailures+=critical?2:1;if(DeathFailures>=3)PlayerDead=true;return;}
        if(damage==0)return;
        if(damage>=before&&PreventDeath(true)){PlayerHp=1;return;}
        long overflow=(long)damage-before;
        if(overflow>=PlayerMaxHp){PlayerHp=0;PlayerDead=true;return;}
        PlayerHp=Math.Max(0,PlayerHp-damage);
        if(PlayerHp==0)PlayerReactionAvailable=false;
        }finally{DamageApplied?.Invoke(true,eventTypes);DamageReceived?.Invoke(true,sourceActor??(sourcePlayer.HasValue?(int?)(sourcePlayer.Value?0:-1):null),received);if(sourcePlayer.HasValue)DamageDealt?.Invoke(sourcePlayer.Value);}
    }
    public void UpdatePlayerMaxHp(int constitution, PlayerClasses classes) {

        int newMaxHp = classes.CalculateMaxHp(constitution);
        if(newMaxHp != playerBaseMaxHp) { playerBaseMaxHp = newMaxHp; PlayerHp = PlayerMaxHp; }

    }
    public void UpdateMonsterMaxHp(int newMaxHp) {
        if(newMaxHp < 1) throw new ArgumentOutOfRangeException(nameof(newMaxHp));
        if(newMaxHp != monsterBaseMaxHp) { monsterBaseMaxHp = newMaxHp; Hp = MaxHp; }
    }
    public void EndRound(bool notify=true) {PlayerReactionsBlocked=false;ShieldActive=false;PlayerSlowed=false;MonsterSlowed=false;PlayerOpportunityBlocked=false; BattleStarted=true;Round++;PlayerReactionAvailable=true;RefreshActions();if(notify)TurnStarted?.Invoke(0); }

}
