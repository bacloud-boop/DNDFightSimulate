public sealed class DetectEvilGood
{
    public long Expires {get;private set;}=-1;
    public bool Begin(long now){if(now<0||now>long.MaxValue-100)return false;Expires=now+100;return true;}
    public bool Active(long now)=>now>=0&&now<Expires;
    public bool Detects(int type,int distance,bool alive,long now,bool blocked=false)=>Active(now)&&alive&&!blocked&&distance>=0&&distance<=30&&(type==0||type==2||type==5||type==6||type==7||type==13);
    public void Clear(){Expires=-1;}
}
