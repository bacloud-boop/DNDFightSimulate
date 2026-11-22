using System;
using System.Collections.Generic;
public sealed class FingerOfDeathRaising
{
    public struct Pending {public int Target;public long Generation,Due;}
    readonly List<Pending> pending=new List<Pending>();
    public long CasterStarts {get;private set;}
    public static int Damage(int diceTotal,bool saved)=>(int)(Math.Max(0L,Math.Min(int.MaxValue,(long)diceTotal+30))/(saved?2:1));
    public bool Schedule(int target,CorpseClock corpse)
    {
        if(corpse==null||!corpse.Dead||corpse.Type!=9||corpse.Generation==long.MaxValue||CasterStarts==long.MaxValue)return false;
        foreach(var old in pending)if(old.Target==target&&old.Generation==corpse.Generation)return false;
        pending.Add(new Pending{Target=target,Generation=corpse.Generation,Due=CasterStarts+1});return true;
    }
    public List<Pending> StartTurn()
    {
        if(CasterStarts<long.MaxValue)CasterStarts++;var result=pending.FindAll(p=>p.Due<=CasterStarts);pending.RemoveAll(p=>p.Due<=CasterStarts);return result;
    }
    public static bool SameCorpse(Pending pending,CorpseClock corpse)=>corpse!=null&&corpse.Dead&&corpse.Type==9&&corpse.Generation==pending.Generation;
    public void Clear(){pending.Clear();CasterStarts=0;}
}
