using System;
public sealed class SizeChangeEffect
{
    public bool Present {get;private set;}
    public bool Player {get;private set;}
    public int Delta {get;private set;}
    public long Expires {get;private set;}
    public static int ChangedSize(int size,int delta)=>size>=0&&size<=5&&(delta==1||delta==-1)&&size+delta>=0&&size+delta<=5?size+delta:-1;
    public void Begin(bool player,int delta,long now){if((delta!=1&&delta!=-1)||now<0||now>long.MaxValue-10)throw new ArgumentOutOfRangeException();Present=true;Player=player;Delta=delta;Expires=now+10;}
    public bool Applies(bool player)=>Present&&Player==player;
    public void Clear(){Present=false;Delta=0;}
}
