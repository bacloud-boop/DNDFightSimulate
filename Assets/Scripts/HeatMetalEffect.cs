using System;
using System.Collections.Generic;
public sealed class HeatMetalEffect
{
    public int ObjectId {get;private set;}
    public int Slot {get;private set;}
    public int CastRound {get;private set;}
    public long Dc {get;private set;}
    long begins,expires;
    readonly HashSet<int> penalties=new HashSet<int>();
    public int Dice=>Slot;
    public static bool Touching(EquipmentObjectKind kind,EquipmentObjectPlace place,bool held,bool melded,bool living)
        =>living&&!melded&&(kind==EquipmentObjectKind.Weapon?held:place==EquipmentObjectPlace.Equipped);
    public bool Active(long now)=>ObjectId>0&&now>=begins&&now<expires;
    public bool Begin(int id,int slot,long dc,int round,long now)
    {
        if(id<1||slot<2||slot>9||now<0||now>long.MaxValue-10)return false;
        ObjectId=id;Slot=slot;Dc=dc;CastRound=round;begins=now;expires=now+10;penalties.Clear();return true;
    }
    public bool CanPulse(int round,long now,int distance)=>Active(now)&&round>CastRound&&distance<=60;
    public void Retained(int actor,long now){if(Active(now))penalties.Add(actor);}
    public bool Disadvantage(int actor,long now)=>Active(now)&&penalties.Contains(actor);
    public void CasterTurnStarts(){penalties.Clear();}
    public void Clear(){ObjectId=Slot=0;penalties.Clear();}
}
