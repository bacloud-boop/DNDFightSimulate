using System;
using System.Collections.Generic;
public sealed class RegenerationEffects
{
    sealed class Effect {public long Expires,RegrowAt;}
    readonly Dictionary<int,Effect> effects=new Dictionary<int,Effect>();
    readonly Dictionary<int,string> missing=new Dictionary<int,string>();
    readonly Dictionary<int,long> lastTurn=new Dictionary<int,long>();
    public static bool ValidTime(long now)=>now>=0&&now<=long.MaxValue-600;
    public string Missing(int actor){string value;return missing.TryGetValue(actor,out value)?value:"";}
    public bool Active(int actor,long now){Effect e;return effects.TryGetValue(actor,out e)&&now>=e.Expires-600&&now<e.Expires;}
    public bool SetMissing(int actor,string value,long now)
    {
        if(now<0||now>long.MaxValue-20||value==null)return false;value=value.Trim();
        if(value==Missing(actor))return true;if(value.Length==0)missing.Remove(actor);else missing[actor]=value;
        Effect e;if(effects.TryGetValue(actor,out e)&&Active(actor,now))e.RegrowAt=value.Length==0?-1:now+20;
        return true;
    }
    public bool Begin(int actor,long now)
    {
        if(!ValidTime(now))return false;
        effects[actor]=new Effect{Expires=now+600,RegrowAt=Missing(actor)==""?-1:now+20};return true;
    }
    public bool StartTurn(int actor,long stamp,long now)
    {
        long old;if(stamp<0||!Active(actor,now)||lastTurn.TryGetValue(actor,out old)&&stamp<=old)return false;
        lastTurn[actor]=stamp;return true;
    }
    public List<int> Refresh(long now)
    {
        var restored=new List<int>();foreach(var pair in effects){var e=pair.Value;
            if(e.RegrowAt>=0&&now>=e.RegrowAt&&e.RegrowAt<e.Expires){if(Missing(pair.Key)!=""){missing.Remove(pair.Key);restored.Add(pair.Key);}e.RegrowAt=-1;}}
        foreach(int id in new List<int>(effects.Keys))if(now>=effects[id].Expires)effects.Remove(id);return restored;
    }
    public void RestoreBody(int actor){missing.Remove(actor);Effect e;if(effects.TryGetValue(actor,out e))e.RegrowAt=-1;}
    public void Clear(){effects.Clear();missing.Clear();lastTurn.Clear();}
}
