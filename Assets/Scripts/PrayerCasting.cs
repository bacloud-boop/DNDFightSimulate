using System;
using System.Collections.Generic;
public sealed class PrayerCasting
{
    public const string Name="Prayer of Healing",Title="治疗祷言";
    readonly HashSet<int> targets;
    public int Slot {get;}
    public int PaidRound {get;private set;}
    public int Actions {get;private set;}=1;
    public long Finish {get;}
    public int Count=>targets.Count;
    public int[] Targets{get{var a=new int[targets.Count];targets.CopyTo(a);Array.Sort(a);return a;}}
    long lastWorld;
    public PrayerCasting(IEnumerable<int> selected,int slot,int round,long world)
    {
        if(slot<2||slot>9||round<1||world<0||world>long.MaxValue-101)throw new ArgumentOutOfRangeException();
        targets=new HashSet<int>(selected);if(targets.Count<1||targets.Count>5)throw new ArgumentException("Choose one to five creatures");
        Slot=slot;PaidRound=round;lastWorld=world;Finish=world+100;
    }
    public void Exclude(int target){targets.Remove(target);}
    public bool CanContinue(int round,long world)=>Actions<100&&round==(long)PaidRound+1&&world>=lastWorld&&world<Finish;
    public bool Continue(int round,long world){if(!CanContinue(round,world))return false;PaidRound=round;lastWorld=world;Actions++;return true;}
    public bool EndTurn(int round)=>round==PaidRound;
    public bool Ready(long world)=>world>=Finish&&Actions==100&&Count>0;
}
