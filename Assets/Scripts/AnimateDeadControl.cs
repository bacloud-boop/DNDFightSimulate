using System;
using System.Collections.Generic;

// Control is separate from a creature's existence: expiry never destroys it.
// Orders use 0 = none, 1 = assist combat, 2 = Dodge, matching the follower UI.
public sealed class AnimateDeadControl
{
    public const long DayRounds=14400;
    public sealed class Entry
    {
        public int Actor {get;}
        public bool Destroyed {get;internal set;}
        public long ControlExpires {get;internal set;}
        internal int Order;
        internal Entry(int actor,long until){Actor=actor;ControlExpires=until;}
    }
    readonly Dictionary<int,Entry> entries=new Dictionary<int,Entry>();
    public Entry Find(int actor){Entry value;return entries.TryGetValue(actor,out value)?value:null;}
    public static int CreationLimit(int slot)=>slot>=3&&slot<=9?1+2*(slot-3):0;
    public static int RenewalLimit(int slot)=>slot>=3&&slot<=9?4+2*(slot-3):0;
    public static bool ValidTime(long now)=>now>=0&&now<=long.MaxValue-DayRounds;
    public bool Controlled(int actor,long now)
    {
        var entry=Find(actor);return entry!=null&&!entry.Destroyed&&now>=0&&now<entry.ControlExpires;
    }
    public bool Neutral(int actor,long now){var entry=Find(actor);return entry!=null&&!entry.Destroyed&&!Controlled(actor,now);}
    public int OrderFor(int actor,long now)=>Controlled(actor,now)?Find(actor).Order:0;
    static bool Selection(IEnumerable<int> actors,int limit,out List<int> chosen)
    {
        chosen=new List<int>();if(actors==null||limit<1)return false;
        var seen=new HashSet<int>();foreach(int actor in actors){if(actor<1||!seen.Add(actor)||chosen.Count>=limit)return false;chosen.Add(actor);}
        return chosen.Count>0;
    }
    // Registration happens only after actual corpse conversion has committed.
    public bool Register(IEnumerable<int> actors,int slot,long now)
    {
        List<int> chosen;if(!ValidTime(now)||!Selection(actors,CreationLimit(slot),out chosen))return false;
        foreach(int actor in chosen)if(entries.ContainsKey(actor))return false;
        foreach(int actor in chosen)entries.Add(actor,new Entry(actor,now+DayRounds));return true;
    }
    // Project rule: recasting may regain an expired, neutral creature as well
    // as renew a currently controlled creature. Neither operation heals it.
    public bool Reassert(IEnumerable<int> actors,int slot,long now,Func<int,bool> withinTenFeet)
    {
        List<int> chosen;if(!ValidTime(now)||withinTenFeet==null||!Selection(actors,RenewalLimit(slot),out chosen))return false;
        foreach(int actor in chosen){var entry=Find(actor);if(entry==null||entry.Destroyed||!withinTenFeet(actor))return false;}
        foreach(int actor in chosen){var entry=Find(actor);if(!Controlled(actor,now))entry.Order=0;entry.ControlExpires=now+DayRounds;}return true;
    }
    // One mental Bonus Action gives the same order to the whole chosen group.
    // The caller validates the caster's consciousness and other action locks.
    public bool Command(IEnumerable<int> actors,int order,long now,Func<int,bool> withinSixtyFeet,Func<bool> payBonusAction)
    {
        List<int> chosen;if(order<1||order>2||withinSixtyFeet==null||payBonusAction==null||!Selection(actors,int.MaxValue,out chosen))return false;
        foreach(int actor in chosen)if(!Controlled(actor,now)||!withinSixtyFeet(actor))return false;
        if(!payBonusAction())return false;
        foreach(int actor in chosen)Find(actor).Order=order;return true;
    }
    public List<int> Refresh(long now)
    {
        var lost=new List<int>();foreach(var entry in entries.Values)if(!entry.Destroyed&&!Controlled(entry.Actor,now)&&entry.Order!=0){entry.Order=0;lost.Add(entry.Actor);}return lost;
    }
    public bool Destroy(int actor){var entry=Find(actor);if(entry==null||entry.Destroyed)return false;entry.Destroyed=true;entry.Order=0;return true;}
    public void Clear()=>entries.Clear();
}

public sealed class AnimateDeadCasting
{
    public int PaidRound {get;private set;}
    public int Actions {get;private set;}=1;
    public long Finish {get;}
    long lastWorld;
    public AnimateDeadCasting(int round,long world)
    {
        if(round<1||world<0||world>long.MaxValue-10)throw new ArgumentOutOfRangeException();
        PaidRound=round;lastWorld=world;Finish=world+10;
    }
    public bool CanContinue(int round,long world)=>Actions<10&&round==(long)PaidRound+1&&world>=lastWorld&&world<Finish;
    public bool Continue(int round,long world){if(!CanContinue(round,world))return false;PaidRound=round;lastWorld=world;Actions++;return true;}
    public bool EndTurn(int round)=>round==PaidRound;
    public bool Ready(long world)=>world>=Finish&&Actions==10;
}
