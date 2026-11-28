using System;
using System.Collections.Generic;
// One concentration instance; each target can escape independently. Revision invalidates queued saves.
public sealed class LaughterEffect
{
    public const string Name="Tasha’s Hideous Laughter",Title="塔莎狂笑术";
    readonly HashSet<int> targets=new HashSet<int>();
    public long Revision {get;private set;}
    public long Expires {get;private set;}
    public long Dc {get;private set;}
    public int Count=>targets.Count;
    public void Begin(long now,long dc){Clear();Expires=checked(now+10);Dc=dc;}
    public void Clear(){targets.Clear();Expires=0;Revision++;}
    public bool Active(int actor,long now)=>now<Expires&&targets.Contains(actor);
    public void Add(int actor){targets.Add(actor);}
    public bool Remove(int actor)=>targets.Remove(actor);
    public int[] Targets(){var result=new int[targets.Count];targets.CopyTo(result);return result;}
}
