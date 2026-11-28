using System;
// Aftermath-only manual carrier. Casting qualification remains in the external rescue flow.
public sealed partial class RescueCarrier
{
    public int Strength {get=>abilityScores[0];private set=>abilityScores[0]=value;}
    public int Dexterity {get=>abilityScores[1];private set=>abilityScores[1]=value;}
    public int BaseMaxHp {get;private set;}=10;
    public int MaxHp=>(int)Math.Min(int.MaxValue,(long)BaseMaxHp+Species.HpBonus(Species.Level));
    public int Hp {get;private set;}=10;
    public bool Dead {get;private set;}
    public bool Stable {get;private set;}
    public int DeathSuccesses {get;private set;}
    public int DeathFailures {get;private set;}
    public int RevivalPenalty {get;private set;}
    public long RecoveryAt {get;private set;}=-1;
    long lastDeathSave=-1;
    public readonly CorpseClock Corpse=new CorpseClock();
    public readonly BreathingState Breathing=new BreathingState();
    public bool FeignDeath,Restrained;
    public Func<bool> MovementFreedom;
    bool HasMovementFreedom=>MovementFreedom!=null&&MovementFreedom();
    public bool MovementReady=>Hp>0&&!Dead&&(!FeignDeath||HasMovementFreedom);
    public Func<int> WardBonus;public int ArmorClass=>10+(int)Math.Floor((Dexterity-10)/2.0)+(WardBonus==null?0:WardBonus());
    public Func<int,bool> ExtraResistance;public Action<int,int> DamageApplied;
    public bool ResistsType(int type)=>(ExtraResistance!=null&&ExtraResistance(type))||Species.Resist(type)||FeignDeath&&type>=0&&type!=10;
    public bool Conscious=>Hp>0&&!Dead&&!FeignDeath;
    public bool CanDepart=>Conscious&&!Load.Loaded;
    public bool NeedsDeathSave=>Hp==0&&!Dead&&!Stable;
    public string LifeLabel=>Dead?"死亡":Hp>0?(FeignDeath?"假死 / 失能":"清醒"):Stable?"稳定 / 昏迷":"倒地 / 昏迷";
    public Func<long[],long[]> IncomingDamage;
    public long[] PrepareIncomingDamage(long[] raw)=>Dead||IncomingDamage==null?raw:IncomingDamage(raw);
    public int TakeDamage(long amount,bool critical=false,int damageType=-1){
        if(amount<=0||Dead)return 0;if(damageType==5)FireExposure?.Invoke();
        var raw=new long[14];int index=damageType>=0&&damageType<13?damageType:13;raw[index]=amount;amount=PrepareIncomingDamage(raw)[index];
        if(ResistsType(damageType))amount/=2;if(amount<=0)return 0;
        long incoming=amount;if(incoming>0)DamageApplied?.Invoke((int)Math.Min(int.MaxValue,incoming),damageType);
        int lost=ApplyResolvedDamage(amount,critical);ConditionDamageReceived?.Invoke((int)Math.Min(int.MaxValue,incoming));return lost;
    }
    int ApplyResolvedDamage(long amount,bool critical){long incoming=amount;int before=Hp;
        int absorbed=(int)Math.Min(TemporaryHp,amount);TemporaryHp-=absorbed;amount-=absorbed;
        if(before==0){Stable=false;RecoveryAt=-1;EndTurn();if(incoming>=MaxHp)Dead=true;else {DeathFailures+=critical?2:1;if(DeathFailures>=3)Dead=true;}return 0;}
        if(amount<=0)return 0;int lost=(int)Math.Min((long)Hp,amount);Hp-=lost;
        if(Hp==0){Prone=true;EndTurn();Stable=false;RecoveryAt=-1;
            if(amount-before>=MaxHp)Dead=true;
        }return lost;
    }
    public Action<long[]> TypedDamageApplied;
    public int TakeDamagePacket(long[] raw){
        if(raw==null)throw new ArgumentNullException(nameof(raw));if(Dead)return 0;if(raw.Length>5&&raw[5]>0)FireExposure?.Invoke();raw=PrepareIncomingDamage(raw);
        var resolved=new long[raw.Length];long total=0;
        for(int i=0;i<raw.Length;i++){long value=Math.Max(0,raw[i]);if(ResistsType(i))value/=2;resolved[i]=Math.Min(int.MaxValue-total,value);total+=resolved[i];}
        if(total==0)return 0;
        if(TypedDamageApplied!=null)TypedDamageApplied(resolved);else if(DamageApplied!=null)for(int i=0;i<resolved.Length;i++)if(resolved[i]>0)DamageApplied((int)resolved[i],i);
        ApplyResolvedDamage(total,false);ConditionDamageReceived?.Invoke((int)total);return (int)total;
    }
    public bool ResolveDeathSave(int die,long sequence,int bonus=0){
        if(die<1||die>20)throw new ArgumentOutOfRangeException(nameof(die));
        if(!NeedsDeathSave||sequence<=lastDeathSave)return false;lastDeathSave=sequence;
        if(die==20){Heal(1);return true;}
        if(die==1)DeathFailures+=2;else if(die+RevivalPenalty+Breathing.D20Penalty+bonus>=10)DeathSuccesses++;else DeathFailures++;
        if(DeathFailures>=3)Dead=true;
        else if(DeathSuccesses>=3){Stable=true;DeathFailures=DeathSuccesses=0;}
        return true;
    }
    public int Heal(int amount){if(Dead||amount<=0)return 0;int before=Hp;Hp=(int)Math.Min(MaxHp,(long)Hp+amount);if(Hp>0){Stable=false;DeathFailures=DeathSuccesses=0;RecoveryAt=-1;}return Hp-before;}
    public bool ScheduleRecovery(long now,int hours){if(!Stable||RecoveryAt>=0||hours<1||hours>4||now<0||now>long.MaxValue-hours*600)return false;RecoveryAt=now+hours*600;return true;}
    public void ObserveLife(long now){if(Stable&&RecoveryAt>=0&&now>=RecoveryAt)Heal(1);Corpse.Observe(Dead,9,now);Corpse.Age(now);}
    public bool Revive(int hp,bool ordeal){if(!Dead||hp<1)return false;Dead=Stable=false;DeathFailures=DeathSuccesses=0;Hp=Math.Min(MaxHp,hp);RecoveryAt=-1;Corpse.Clear();if(ordeal)RevivalPenalty=-4;return true;}
    public bool ApplyFatalSuffocation(){
        if(!Breathing.Fatal||Dead)return false;
        Hp=TemporaryHp=0;Dead=true;Stable=false;Prone=true;DeathFailures=DeathSuccesses=0;RecoveryAt=-1;BonusActionAvailable=ReactionAvailable=false;EndTurn();return true;
    }
    public bool ReconcileRevivalBreathing(bool canBreathe){
        if(Dead)return false;if(Breathing.ReconcileRevival(canBreathe))return true;ApplyFatalSuffocation();return false;
    }
    public void CompleteLongRest(){if(!Conscious)return;RevivalPenalty=Math.Min(0,RevivalPenalty+1);SpeciesAbilities.LongRest();SpeciesSpells.LongRest();TemporaryHp=0;Heal(int.MaxValue);}
    public Func<bool> ExtraIncapacitated,ExtraProne;public Action<int> ConditionDamageReceived;
    public bool ConditionIncapacitated=>ExtraIncapacitated!=null&&ExtraIncapacitated();
    public bool ConditionProne=>ExtraProne!=null&&ExtraProne();
    bool prone;public bool Prone {get=>prone||ConditionProne;set=>prone=value;}
    public bool Stand(){int cost=EffectiveSpeed/2;if(ConditionProne||!Prone||!TurnActive||EffectiveSpeed<=0||MovementRemaining<cost)return false;MovementSpent+=cost;Prone=false;return true;}
    public int DexteritySaveModifier=>(int)Math.Floor((Dexterity-10)/2.0)+RevivalPenalty+Breathing.D20Penalty;
    public int Size {get;private set;}=2;
    public int Speed {get;private set;}=30;
    public readonly BodyWeight Weight=new BodyWeight();
    public readonly CorpseCarryingState Load=new CorpseCarryingState();
    public bool TurnActive {get;private set;}
    public Func<bool> ActionBlock;int actionBudget;public int ActionsRemaining {get=>ConditionIncapacitated||ActionBlock!=null&&ActionBlock()?0:actionBudget;private set=>actionBudget=value;}
    public int MovementSpent {get;private set;}
    public int Dashes {get;private set;}
    long lastTurn=-1;
    public Func<int,int> SpeedTransform;
    public int EffectiveSpeed=>SpeedTransform==null?UnmodifiedEffectiveSpeed:Math.Max(0,SpeedTransform(UnmodifiedEffectiveSpeed));
    public int UnmodifiedEffectiveSpeed=>FeignDeath&&!HasMovementFreedom||Restrained?0:Math.Max(0,Load.AllowedSpeedOunces(Strength,Species.CapacitySize(Size),Weight.GearOunces,Speed)-Breathing.SpeedPenalty);
    public int MovementRemaining=>(int)Math.Max(0,Math.Min(int.MaxValue,(long)EffectiveSpeed*(1+Dashes)-MovementSpent));
    public bool Configure(int strength,int size,int speed,int bodyPounds,int equipmentPounds,int dexterity=10,int maxHp=10)
    {
        return ConfigureProfile(new[]{strength,dexterity,10,10,10,10},new SpeciesProfile(),0,size,speed,bodyPounds,equipmentPounds,maxHp);
    }
    public bool BeginTurn(long sequence)
    {
        if(!MovementReady||TurnActive||sequence<=lastTurn)return false;lastTurn=sequence;TurnActive=true;ActionsRemaining=1;BonusActionAvailable=true;ReactionAvailable=true;MovementSpent=0;Dashes=0;TurnStarted?.Invoke();return true;
    }
    public Action TurnEnded,TurnStarted,FireExposure;
    public void EndTurn(bool notify=true){bool active=TurnActive;TurnActive=false;ActionsRemaining=0;if(active&&notify)TurnEnded?.Invoke();}
    public bool UseAction(){if(!Conscious||!TurnActive||ActionsRemaining<1)return false;ActionsRemaining--;return true;}
    public bool PickUp(int id,bool dead,BodyWeight corpse,bool drag)=>TurnActive&&Load.PickUpOunces(id,dead,corpse,Strength,Species.CapacitySize(Size),Weight.GearOunces,drag,UseAction);
    public bool Drop(Func<bool> place)=>TurnActive&&Load.Drop(place);
    public bool Move(int feet){if(!MovementReady||!TurnActive||feet<1||feet>MovementRemaining)return false;MovementSpent+=feet;return true;}
    public bool Dash(){if(!UseAction())return false;Dashes++;return true;}
    public bool CanMove(int feet)=>MovementReady&&TurnActive&&feet>0&&feet<=MovementRemaining;
    public void Reset(){Breathing.Reset();FeignDeath=Restrained=false;SpeciesAbilities.Reset();SpeciesSpells.LongRest();TemporaryHp=0;BonusActionAvailable=ReactionAvailable=true;Dead=Stable=false;DeathFailures=DeathSuccesses=RevivalPenalty=0;RecoveryAt=lastDeathSave=-1;Corpse.Clear();Hp=MaxHp;Prone=false;EndTurn(false);Load.Clear();MovementSpent=0;Dashes=0;lastTurn=-1;}
}
