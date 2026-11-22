using System;
using System.Collections.Generic;
public sealed class AcidArrowResidue
{
    public sealed class Entry
    {
        public long Id {get;internal set;}public int Source {get;internal set;}public int Target {get;internal set;}
        public int Dice {get;internal set;}public long DueStart {get;internal set;}
    }
    readonly List<Entry> entries=new List<Entry>();readonly Dictionary<int,long> starts=new Dictionary<int,long>();long sequence;
    public static int InitialDice(int slot,bool critical=false)=>slot>=2&&slot<=9?(slot+2)*(critical?2:1):0;
    public static int LaterDice(int slot)=>slot>=2&&slot<=9?slot:0;
    public static int InitialDamage(int rolled,bool hit)=>Math.Max(0,rolled)/(hit?1:2);
    public static bool CrossesBoundary(int source,int target,bool playerContained,bool monsterContained)
    {
        if(source==target)return false;
        if(source==0&&playerContained)return target!=-1;
        if(source==-1&&monsterContained)return target!=0;
        return target==0&&playerContained||target==-1&&monsterContained;
    }
    public long Starts(int actor){long value;return starts.TryGetValue(actor,out value)?value:0;}
    public bool CanAdd(int target)=>sequence<long.MaxValue-7&&Starts(target)<long.MaxValue;
    public Entry Add(int source,int target,int slot)
    {
        if(!CanAdd(target)||LaterDice(slot)==0)return null;
        var entry=new Entry{Id=++sequence,Source=source,Target=target,Dice=LaterDice(slot),DueStart=Starts(target)+1};entries.Add(entry);return entry;
    }
    public void StartTurn(int actor){long now=Starts(actor);if(now<long.MaxValue)starts[actor]=now+1;}
    public List<Entry> Pending(int target)=>entries.FindAll(e=>e.Target==target&&Starts(target)>=e.DueStart);
    public bool Take(long id){int at=entries.FindIndex(e=>e.Id==id);if(at<0)return false;entries.RemoveAt(at);return true;}
    public void RemoveTarget(int target)=>entries.RemoveAll(e=>e.Target==target);
    public void RemoveUnavailable(Func<int,bool> available)=>entries.RemoveAll(e=>!available(e.Target));
    public void Clear(){entries.Clear();starts.Clear();sequence=0;}
}
