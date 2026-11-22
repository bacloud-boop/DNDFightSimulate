using System;
using System.Collections.Generic;
public static class DissonantWhispersRules
{
    public static int DiceCount(int slot)=>slot>=1&&slot<=9?slot+2:0;
    public static int AfterSave(int damage,bool saved)=>Math.Max(0,damage)/(saved?2:1);
    public static List<int> SafeRoute(int width,int start,int speed,Func<int,int,int> cost,Func<int,int> distance,Func<int,bool> safe,Func<int,bool> canEnd)
        =>CommandRoute.Plan(width,start,speed,true,(a,b)=>safe(b)?cost(a,b):int.MaxValue,distance,c=>safe(c)&&canEnd(c));
}
// This movement belongs to a reaction, not a creature's turn movement or actions.
public sealed class ReactionMovementBudget
{
    public int Spent {get;private set;}
    public int Remaining(int speed)=>(int)Math.Max(0L,(long)speed-Spent);
    public bool Spend(int cost,int speed){if(cost<1||cost>Remaining(speed))return false;Spent+=cost;return true;}
}
