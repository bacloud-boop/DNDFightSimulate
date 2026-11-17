using System;
// Aftermath-only manual carrier. Casting qualification remains in the external rescue flow.
public sealed class RescueCarrier
{
    public int Strength {get;private set;}=10;
    public int Dexterity {get;private set;}=10;
    public int MaxHp {get;private set;}=10;
    public int Hp {get;private set;}=10;
    public bool Dead {get;private set;}
    public bool Stable {get;private set;}
    public int DeathSuccesses {get;private set;}
    public int DeathFailures {get;private set;}
    public int RevivalPenalty {get;private set;}
    public long RecoveryAt {get;private set;}=-1;
    long lastDeathSave=-1;
    public readonly CorpseClock Corpse=new CorpseClock();
    public bool Conscious=>Hp>0&&!Dead;
    public bool CanDepart=>Conscious&&!Load.Loaded;
    public bool NeedsDeathSave=>Hp==0&&!Dead&&!Stable;
    public string LifeLabel=>Dead?"死亡":Hp>0?"清醒":Stable?"稳定 / 昏迷":"倒地 / 昏迷";
    public int TakeDamage(long amount,bool critical=false){
        if(amount<=0||Dead)return 0;
        int before=Hp,lost=(int)Math.Min((long)Hp,amount);Hp-=lost;
        if(Hp==0){Prone=true;EndTurn();Stable=false;RecoveryAt=-1;
            if(amount-before>=MaxHp)Dead=true;
            else if(before==0){DeathFailures+=critical?2:1;if(DeathFailures>=3)Dead=true;}
        }return lost;
    }
    public bool ResolveDeathSave(int die,long sequence){
        if(die<1||die>20)throw new ArgumentOutOfRangeException(nameof(die));
        if(!NeedsDeathSave||sequence<=lastDeathSave)return false;lastDeathSave=sequence;
        if(die==20){Heal(1);return true;}
        if(die==1)DeathFailures+=2;else if(die+RevivalPenalty>=10)DeathSuccesses++;else DeathFailures++;
        if(DeathFailures>=3)Dead=true;
        else if(DeathSuccesses>=3){Stable=true;DeathFailures=DeathSuccesses=0;}
        return true;
    }
    public int Heal(int amount){if(Dead||amount<=0)return 0;int before=Hp;Hp=(int)Math.Min(MaxHp,(long)Hp+amount);if(Hp>0){Stable=false;DeathFailures=DeathSuccesses=0;RecoveryAt=-1;}return Hp-before;}
    public bool ScheduleRecovery(long now,int hours){if(!Stable||RecoveryAt>=0||hours<1||hours>4||now<0||now>long.MaxValue-hours*600)return false;RecoveryAt=now+hours*600;return true;}
    public void ObserveLife(long now){if(Stable&&RecoveryAt>=0&&now>=RecoveryAt)Heal(1);Corpse.Observe(Dead,9,now);Corpse.Age(now);}
    public bool Revive(int hp,bool ordeal){if(!Dead||hp<1)return false;Dead=Stable=false;DeathFailures=DeathSuccesses=0;Hp=Math.Min(MaxHp,hp);RecoveryAt=-1;Corpse.Clear();if(ordeal)RevivalPenalty=-4;return true;}
    public void CompleteLongRest(){if(Conscious)RevivalPenalty=Math.Min(0,RevivalPenalty+1);}
    public bool Prone {get;set;}
    public bool Stand(){int cost=EffectiveSpeed/2;if(!Prone||!TurnActive||EffectiveSpeed<=0||MovementRemaining<cost)return false;MovementSpent+=cost;Prone=false;return true;}
    public int DexteritySaveModifier=>(int)Math.Floor((Dexterity-10)/2.0)+RevivalPenalty;
    public int Size {get;private set;}=2;
    public int Speed {get;private set;}=30;
    public readonly BodyWeight Weight=new BodyWeight();
    public readonly CorpseCarryingState Load=new CorpseCarryingState();
    public bool TurnActive {get;private set;}
    public int ActionsRemaining {get;private set;}
    public int MovementSpent {get;private set;}
    public int Dashes {get;private set;}
    long lastTurn=-1;
    public int EffectiveSpeed=>Load.AllowedSpeedOunces(Strength,Size,Weight.GearOunces,Speed);
    public int MovementRemaining=>(int)Math.Max(0,Math.Min(int.MaxValue,(long)EffectiveSpeed*(1+Dashes)-MovementSpent));
    public bool Configure(int strength,int size,int speed,int bodyPounds,int equipmentPounds,int dexterity=10,int maxHp=10)
    {
        if(maxHp<1||dexterity<3||dexterity>20||TurnActive||Load.Loaded||strength<3||strength>20||size<0||size>5||speed<0||bodyPounds<1||equipmentPounds<0)return false;
        if(!CarryingRules.CanCarry(strength,size,equipmentPounds,0))return false;
        Reset();MaxHp=Hp=maxHp;Strength=strength;Dexterity=dexterity;Size=size;Speed=speed;Weight.TryApply(bodyPounds.ToString(),equipmentPounds.ToString());return true;
    }
    public bool BeginTurn(long sequence)
    {
        if(!Conscious||TurnActive||sequence<=lastTurn)return false;lastTurn=sequence;TurnActive=true;ActionsRemaining=1;MovementSpent=0;Dashes=0;return true;
    }
    public void EndTurn(){TurnActive=false;ActionsRemaining=0;}
    bool UseAction(){if(!TurnActive||ActionsRemaining<1)return false;ActionsRemaining--;return true;}
    public bool PickUp(int id,bool dead,BodyWeight corpse,bool drag)=>TurnActive&&Load.PickUpOunces(id,dead,corpse,Strength,Size,Weight.GearOunces,drag,UseAction);
    public bool Drop(Func<bool> place)=>TurnActive&&Load.Drop(place);
    public bool Move(int feet){if(!TurnActive||feet<1||feet>MovementRemaining)return false;MovementSpent+=feet;return true;}
    public bool Dash(){if(!UseAction())return false;Dashes++;return true;}
    public bool CanMove(int feet)=>TurnActive&&feet>0&&feet<=MovementRemaining;
    public void Reset(){Dead=Stable=false;DeathFailures=DeathSuccesses=RevivalPenalty=0;RecoveryAt=lastDeathSave=-1;Corpse.Clear();Hp=MaxHp;Prone=false;EndTurn();Load.Clear();MovementSpent=0;Dashes=0;lastTurn=-1;}
}
