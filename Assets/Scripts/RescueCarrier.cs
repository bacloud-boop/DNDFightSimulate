using System;
// Aftermath-only manual carrier. Casting qualification remains in the external rescue flow.
public sealed class RescueCarrier
{
    public int Strength {get;private set;}=10;
    public int Dexterity {get;private set;}=10;
    public bool Prone {get;set;}
    public bool Stand(){int cost=EffectiveSpeed/2;if(!Prone||!TurnActive||EffectiveSpeed<=0||MovementRemaining<cost)return false;MovementSpent+=cost;Prone=false;return true;}
    public int DexteritySaveModifier=>(int)Math.Floor((Dexterity-10)/2.0);
    public int Size {get;private set;}=2;
    public int Speed {get;private set;}=30;
    public readonly BodyWeight Weight=new BodyWeight();
    public readonly CorpseCarryingState Load=new CorpseCarryingState();
    public bool TurnActive {get;private set;}
    public int ActionsRemaining {get;private set;}
    public int MovementSpent {get;private set;}
    public int Dashes {get;private set;}
    long lastTurn=-1;
    public int EffectiveSpeed=>Load.AllowedSpeed(Strength,Size,Weight.EquipmentPounds,Speed);
    public int MovementRemaining=>(int)Math.Max(0,Math.Min(int.MaxValue,(long)EffectiveSpeed*(1+Dashes)-MovementSpent));
    public bool Configure(int strength,int size,int speed,int bodyPounds,int equipmentPounds,int dexterity=10)
    {
        if(dexterity<3||dexterity>20||TurnActive||Load.Loaded||strength<3||strength>20||size<0||size>5||speed<0||bodyPounds<1||equipmentPounds<0)return false;
        if(!CarryingRules.CanCarry(strength,size,equipmentPounds,0))return false;
        Strength=strength;Dexterity=dexterity;Size=size;Speed=speed;Weight.TryApply(bodyPounds.ToString(),equipmentPounds.ToString());return true;
    }
    public bool BeginTurn(long sequence)
    {
        if(TurnActive||sequence<=lastTurn)return false;lastTurn=sequence;TurnActive=true;ActionsRemaining=1;MovementSpent=0;Dashes=0;return true;
    }
    public void EndTurn(){TurnActive=false;ActionsRemaining=0;}
    bool UseAction(){if(!TurnActive||ActionsRemaining<1)return false;ActionsRemaining--;return true;}
    public bool PickUp(int id,bool dead,BodyWeight corpse,bool drag)=>TurnActive&&Load.PickUp(id,dead,corpse,Strength,Size,Weight.EquipmentPounds,drag,UseAction);
    public bool Drop(Func<bool> place)=>TurnActive&&Load.Drop(place);
    public bool Move(int feet){if(!TurnActive||feet<1||feet>MovementRemaining)return false;MovementSpent+=feet;return true;}
    public bool Dash(){if(!UseAction())return false;Dashes++;return true;}
    public bool CanMove(int feet)=>TurnActive&&feet>0&&feet<=MovementRemaining;
    public void Reset(){Prone=false;EndTurn();Load.Clear();MovementSpent=0;Dashes=0;lastTurn=-1;}
}
