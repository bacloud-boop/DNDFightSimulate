using System;
using System.Collections.Generic;
public sealed class IceStormFields
{
    sealed class Field {public int Caster,Size;public long Begins,EndWorld,EndTurn;public HashSet<int> Cells;}
    readonly List<Field> fields=new List<Field>();
    readonly Dictionary<int,long> ended=new Dictionary<int,long>();
    readonly Dictionary<int,long> lastEnd=new Dictionary<int,long>();
    long Turns(int caster)=>ended.ContainsKey(caster)?ended[caster]:0;
    public bool CanAdd(int caster,long now)=>now>=0&&now<=long.MaxValue-2&&Turns(caster)<=long.MaxValue-2;
    bool Active(Field f,long now)=>now>=f.Begins&&now<f.EndWorld&&Turns(f.Caster)<f.EndTurn;
    public static bool Covers(int cx,int cy,int x,int y,int spaces=1)
    {
        if(spaces<1||spaces>4)return false;
        long dx=Math.Max(0,Math.Max((long)cx-x-spaces+1,(long)x-cx)),dy=Math.Max(0,Math.Max((long)cy-y-spaces+1,(long)y-cy));
        return dx<=4&&dy<=4&&dx*dx+dy*dy<=16;
    }
    public bool Add(int caster,int cx,int cy,int size,Func<int,int,bool> land,long now)
    {
        if(!CanAdd(caster,now)||size<1||size>20||cx<0||cy<0||cx>=size||cy>=size||land==null)return false;
        var cells=new HashSet<int>();for(int y=0;y<size;y++)for(int x=0;x<size;x++)if(Covers(cx,cy,x,y)&&land(x,y))cells.Add(y*size+x);
        Refresh(now);fields.Add(new Field{Caster=caster,Size=size,Cells=cells,Begins=now,EndWorld=now+2,EndTurn=Turns(caster)+2});return true;
    }
    public bool Cell(int x,int y,long now){Refresh(now);return fields.Exists(f=>Active(f,now)&&x>=0&&y>=0&&x<f.Size&&y<f.Size&&f.Cells.Contains(y*f.Size+x));}
    public void EndTurn(int caster,long stamp=-1){long previous;if(stamp>=0){if(lastEnd.TryGetValue(caster,out previous)&&stamp<=previous)return;lastEnd[caster]=stamp;}long count=Turns(caster);if(count<long.MaxValue)ended[caster]=count+1;fields.RemoveAll(f=>f.Caster==caster&&Turns(caster)>=f.EndTurn);}
    public void Refresh(long now){fields.RemoveAll(f=>now>=f.EndWorld||Turns(f.Caster)>=f.EndTurn);}
    public void Clear(){fields.Clear();ended.Clear();lastEnd.Clear();}
}
public static class IceStormRules
{
    public static int BludgeoningDice(int slot)=>slot>=4&&slot<=9?slot-2:0;
    public static long[] Packet(int blunt,int cold){var raw=new long[13];raw[0]=Math.Max(0,blunt);raw[4]=Math.Max(0,cold);return raw;}
    public static long[] AfterSave(long[] raw,bool saved){var result=(long[])raw.Clone();for(int i=0;i<result.Length;i++)result[i]=Math.Max(0,result[i])/(saved?2:1);return result;}
}
