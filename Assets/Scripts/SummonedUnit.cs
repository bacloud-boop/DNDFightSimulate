using System;

public enum SummonCommandKind {None,AssistCombat,Dodge}
public struct SummonCommand : IEquatable<SummonCommand>
{
    public SummonCommandKind Kind {get;}
    SummonCommand(SummonCommandKind kind){Kind=kind;}
    public static SummonCommand AssistCombat=>new SummonCommand(SummonCommandKind.AssistCombat);
    public static SummonCommand Dodge=>new SummonCommand(SummonCommandKind.Dodge);
    public bool Equals(SummonCommand other)=>Kind==other.Kind;
    public override bool Equals(object obj)=>obj is SummonCommand&&Equals((SummonCommand)obj);
    public override int GetHashCode()=>(int)Kind;
}

// A summon owns its resources; spending them never spends its caster's action or movement.
public sealed class SummonedUnit
{
    public int Id {get;}
    public string Name {get;}
    public int MaxHp {get;}
    public int Hp {get;private set;}
    public int TemporaryHp {get;private set;}
    public void ChooseTemporaryHp(int amount,bool takeNew){if(amount<0)throw new ArgumentOutOfRangeException(nameof(amount));if(Alive&&takeNew)TemporaryHp=amount;}
    readonly int baseArmor;public int Armor=>baseArmor+(WardBonus==null?0:WardBonus());
    public Func<int> WardBonus;public Action<long[]> DamageApplied;
    public int Speed {get;}
    public int Size {get;}
    public int X {get;internal set;}
    public int Y {get;internal set;}
    public int Spaces=>CreatureProfile.SpacesForSize(Size);
    public bool Alive=>Hp>0;
    public bool TurnActive {get;private set;}
    public bool ReactionAvailable {get;private set;}
    long movementSpent;
    int dashes;
    public bool FeignDeath,FeignBlind;
    public void SetFeignDeath(bool active,bool blind){FeignDeath=active;FeignBlind=blind;if(active){Dodging=false;AttacksRemaining=0;}}
    public bool ResistsType(int type)=>type>=0&&((WardBonus!=null&&WardBonus()>0)||FeignDeath&&type!=10||Stoneskin&&type<=2);
    public int SleepStage {get;set;}
    public int SleepRepeatRound {get;set;}
    public bool Stoneskin {get;set;}
    public bool NoSleep {get;set;}
    public bool ExhaustionImmune {get;set;}
    public bool Prone {get;set;}
    public bool Stand(){int cost=EffectiveSpeed/2;if(!Prone||!Alive||!TurnActive||EffectiveSpeed<=0||MovementRemaining<cost)return false;movementSpent+=cost;Prone=false;return true;}
    public bool Restrained {get;set;}
    public int LoadSpeedLimit {get;set;}=int.MaxValue;
    public int EffectiveSpeed=>FeignDeath||Restrained||SleepStage==2?0:Math.Max(0,Math.Min(Speed,LoadSpeedLimit)-Breathing.SpeedPenalty);
    public int MovementRemaining=>Alive&&TurnActive?(int)Math.Max(0,Math.Min(int.MaxValue,(long)EffectiveSpeed*(1+dashes)-movementSpent)):0;
    public int AttacksRemaining {get;private set;}
    public int AttacksPerAction {get;}
    bool actionAvailable;
    public bool ActionAvailable=>actionAvailable&&!FeignDeath&&SleepStage==0;
    public bool Dodging {get;private set;}
    public bool Disengaged {get;private set;}
    public bool BonusAvailable {get;private set;}
    public bool TryUseBonus(){if(!Alive||!TurnActive||FeignDeath||SleepStage>0||!BonusAvailable)return false;BonusAvailable=false;return true;}
    public bool Disengage(){if(!TryUseAction())return false;Disengaged=true;return true;}
    public SummonCommand Orders {get;private set;}
    public bool HasVerbalOrders=>Orders.Kind!=SummonCommandKind.None;
    public bool ReceiveVerbalOrders(SummonCommand command,out bool changed)
    {
        changed=false;
        if(FeignDeath||SleepStage==2)return false;
        if(!Alive||(command.Kind!=SummonCommandKind.AssistCombat&&command.Kind!=SummonCommandKind.Dodge))return false;
        changed=!Orders.Equals(command);if(changed)Orders=command;
        return true;
    }
    // The currently supported spirits have neither a CR nor class levels.
    public bool CanBePolymorphTarget=>Alive&&PolymorphEffect.TargetChallengeLimit(null,0)>=0;
    public int SwimSpeed {get;}
    public bool BreathesAir {get;}
    public bool BreathesWater {get;}
    public long WaterBreathingExpires {get;set;}
    public readonly AquaticState Aquatic=new AquaticState();
    public readonly BreathingState Breathing=new BreathingState();
    public SummonedUnit(int id,string name,int hp,int armor,int speed,int size,int attacks,int swimSpeed=0,bool breathesAir=true,bool breathesWater=false)
    {
        if(id<1||string.IsNullOrEmpty(name)||hp<1||armor<0||speed<0||swimSpeed<0||size<0||size>5||attacks<1)throw new ArgumentException("Invalid summon");
        SwimSpeed=swimSpeed;BreathesAir=breathesAir;BreathesWater=breathesWater;
        ReactionAvailable=true;Id=id;Name=name;MaxHp=Hp=hp;baseArmor=armor;Speed=speed;Size=size;AttacksPerAction=attacks;
    }
    long lastTurn=-1;
    public bool BeginTurn(long sequence,bool notify=true){if(!Alive||TurnActive||sequence<=lastTurn)return false;lastTurn=sequence;TurnActive=true;ReactionAvailable=true;movementSpent=0;dashes=0;actionAvailable=true;AttacksRemaining=0;Dodging=Disengaged=false;BonusAvailable=true;if(notify)TurnStarted?.Invoke();return true;}
    public Action TurnEnded,TurnStarted,FireExposure;
    public void EndTurn(){bool active=TurnActive;BonusAvailable=false;TurnActive=false;actionAvailable=false;AttacksRemaining=0;if(active)TurnEnded?.Invoke();}
    public bool CanSpendMovement(int cost)=>Alive&&TurnActive&&cost>0&&cost<=MovementRemaining;
    public bool SpendMovement(int cost){if(!CanSpendMovement(cost))return false;movementSpent+=cost;return true;}
    public bool SwitchWaterState(bool submerged,int waterCells)=>Aquatic.TrySwitch(submerged,waterCells,Spaces*Spaces,SpendMovement);
    public bool Attack()
    {
        if(!Alive||FeignDeath||SleepStage>0||!TurnActive)return false;
        if(AttacksRemaining==0){if(!ActionAvailable)return false;actionAvailable=false;AttacksRemaining=AttacksPerAction;}
        AttacksRemaining--;return true;
    }
    public bool Dodge(){if(!Alive||!TurnActive||!ActionAvailable)return false;actionAvailable=false;Dodging=true;return true;}
    public bool Dash(){if(!Alive||!TurnActive||!ActionAvailable)return false;actionAvailable=false;dashes++;return true;}
    public bool TryUseAction(){if(!Alive||!TurnActive||!ActionAvailable)return false;actionAvailable=false;return true;}
    public bool UseReaction(){if(!Alive||FeignDeath||SleepStage>0||!ReactionAvailable)return false;ReactionAvailable=false;return true;}
    public void Dismiss(){Hp=0;TemporaryHp=0;EndTurn();ReactionAvailable=false;Dodging=false;movementSpent=0;dashes=0;}
    public int Damage(int amount,int type=-1,long[] resolvedTypes=null){if(amount<0)throw new ArgumentOutOfRangeException(nameof(amount));bool wasAlive=Alive;if(type==5)FireExposure?.Invoke();if(resolvedTypes==null&&ResistsType(type))amount/=2;if(amount>0)SleepStage=0;int received=amount;int absorbed=Math.Min(amount,TemporaryHp);TemporaryHp-=absorbed;amount-=absorbed;int before=Hp;Hp=Math.Max(0,Hp-amount);if(!Alive){TemporaryHp=0;EndTurn();ReactionAvailable=false;Dodging=false;movementSpent=0;dashes=0;}if(wasAlive&&received>0){var packet=resolvedTypes;if(packet==null){packet=new long[14];packet[type>=0&&type<13?type:13]=received;}DamageApplied?.Invoke(packet);}return before-Hp;}
    public int Heal(int amount){if(!Alive||amount<=0)return 0;int before=Hp;Hp=(int)Math.Min(MaxHp,(long)Hp+amount);return Hp-before;}
}
