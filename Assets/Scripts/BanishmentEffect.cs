using System;
using System.Collections.Generic;
// The spell lifecycle is separate from map placement: returning entries remain
// unavailable until the map has actually restored their complete footprint.
public sealed class BanishmentEffect
{
    public enum Location {Demiplane,Returning,Departed}
    public sealed class Target
    {
        public int Actor {get;}public int CreatureType {get;}
        public Target(int actor,int type){Actor=actor;CreatureType=type;}
    }
    public sealed class Entry
    {
        public int Actor {get;internal set;}public int CreatureType {get;internal set;}
        public Location State {get;internal set;}
    }
    readonly List<Entry> entries=new List<Entry>();
    public IReadOnlyList<Entry> Entries=>entries.AsReadOnly();
    public int Caster {get;private set;}public int Slot {get;private set;}
    public long Begins {get;private set;}=-1;public long Expires {get;private set;}=-1;
    public bool Concentrating {get;private set;}
    public static int MaximumTargets(int slot)=>slot>=4&&slot<=9?slot-3:0;
    public static bool ValidTime(long now)=>now>=0&&now<=long.MaxValue-10;
    public static bool PermanentType(int type)=>type==0||type==2||type==5||type==6||type==7;
    public bool Away(int actor)=>entries.Exists(e=>e.Actor==actor);
    public bool PermanentlyDeparted(int actor)=>entries.Exists(e=>e.Actor==actor&&e.State==Location.Departed);
    public bool Begin(int caster,int slot,long now,IEnumerable<Target> targets)
    {
        if(Concentrating||entries.Exists(e=>e.State==Location.Returning)||MaximumTargets(slot)==0||!ValidTime(now)||targets==null)return false;
        var draft=new List<Entry>();var ids=new HashSet<int>();
        foreach(var target in targets){
            if(target==null||target.CreatureType<0||target.CreatureType>13||!ids.Add(target.Actor)||Away(target.Actor)||draft.Count>=MaximumTargets(slot))return false;
            draft.Add(new Entry{Actor=target.Actor,CreatureType=target.CreatureType,State=Location.Demiplane});
        }
        if(draft.Count==0)return false;
        entries.AddRange(draft);Caster=caster;Slot=slot;Begins=now;Expires=now+10;Concentrating=true;return true;
    }
    public bool End(long now,Func<int,bool> living=null)
    {
        if(!Concentrating||now<Begins)return false;
        bool completed=now>=Expires;
        foreach(var entry in entries)if(entry.State==Location.Demiplane)entry.State=completed&&(living==null||living(entry.Actor))&&PermanentType(entry.CreatureType)?Location.Departed:Location.Returning;
        Concentrating=false;return true;
    }
    public bool FinishReturn(int actor)
    {
        int at=entries.FindIndex(e=>e.Actor==actor&&e.State==Location.Returning);
        if(at<0)return false;entries.RemoveAt(at);return true;
    }
    public void Clear(){entries.Clear();Begins=Expires=-1;Concentrating=false;Caster=Slot=0;}
}
