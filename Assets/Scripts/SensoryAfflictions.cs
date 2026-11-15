using System;
using System.Collections.Generic;
public sealed class SensoryAffliction
{
    public readonly long Id,Dc,Begins,Expires;
    public readonly bool Player,Blind;
    internal SensoryAffliction(long id,bool player,bool blind,long dc,long now){Id=id;Player=player;Blind=blind;Dc=dc;Begins=now;Expires=now+10;}
    public bool Active(long now)=>now>=Begins&&now<Expires;
}
public sealed class SensoryAfflictions
{
    readonly List<SensoryAffliction> effects=new List<SensoryAffliction>();
    long sequence;
    public bool CanAdd(long now,int count)=>now>=0&&now<=long.MaxValue-10&&count>0&&sequence<=long.MaxValue-count;
    void Prune(long now){effects.RemoveAll(effect=>now>=effect.Expires);}
    public SensoryAffliction Add(bool player,bool blind,long dc,long now)
    {
        if(!CanAdd(now,1))return null;Prune(now);
        var effect=new SensoryAffliction(++sequence,player,blind,dc,now);effects.Add(effect);return effect;
    }
    public SensoryAffliction Current(bool player,long now)
    {
        Prune(now);SensoryAffliction best=null;
        foreach(var effect in effects)if(effect.Player==player&&effect.Active(now)&&(best==null||effect.Dc>best.Dc||effect.Dc==best.Dc&&effect.Id>best.Id))best=effect;
        return best;
    }
    public bool Blinded(bool player,long now){var effect=Current(player,now);return effect!=null&&effect.Blind;}
    public bool Deafened(bool player,long now){var effect=Current(player,now);return effect!=null&&!effect.Blind;}
    public List<SensoryAffliction> Pending(bool player,long now){Prune(now);return effects.FindAll(effect=>effect.Player==player&&effect.Active(now));}
    public bool End(long id)=>effects.RemoveAll(effect=>effect.Id==id)>0;
    public void Clear(){effects.Clear();sequence=0;}
    public static bool BlocksSight(bool blinded,int blindsight,double distance)=>blinded&&!(blindsight>0&&Math.Round(distance)<=blindsight);
}
