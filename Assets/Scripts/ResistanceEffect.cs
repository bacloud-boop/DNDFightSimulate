using System;
using System.Collections.Generic;
public sealed class ResistanceEffect
{
    public int Target{get;private set;}public int Type{get;private set;}
    public long Expires{get;private set;}=-1;readonly Dictionary<int,long> benefited=new Dictionary<int,long>();
    public bool Active(long now)=>Expires>=0&&now>=0&&now<Expires;
    public static bool Allowed(int type)=>type>=0&&type<13&&type!=6&&type!=10;
    public bool Begin(int target,int type,long now){if(!Allowed(type)||now<0||now>long.MaxValue-10)return false;Target=target;Type=type;Expires=now+10;return true;}
    public bool Available(int actor,long turn,long now){long last;return Active(now)&&Target==actor&&(!benefited.TryGetValue(actor,out last)||last!=turn);}
    public long[] Reduce(int actor,long[] raw,int die,long turn,long now)
    {
        if(raw==null||die<1||die>4)throw new ArgumentException();
        if(!Available(actor,turn,now)||Type>=raw.Length||raw[Type]<=0)return raw;
        benefited[actor]=turn;var result=(long[])raw.Clone();result[Type]=Math.Max(0,result[Type]-die);return result;
    }
    public void Clear(bool resetClaims=false){Expires=-1;if(resetClaims)benefited.Clear();}
}
