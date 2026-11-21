public sealed class WardingBondEffect
{
    public int Target {get;private set;}
    long begins=-1,expires;
    public bool Begin(int target,long now){if(target==0||now<0||now>long.MaxValue-600)return false;Target=target;begins=now;expires=now+600;return true;}
    public bool Active(long now)=>begins>=0&&now>=begins&&now<expires;
    public bool Validate(long now,bool casterAlive,int distance){if(!Active(now)||!casterAlive||distance>60){Clear();return false;}return true;}
    public bool Protects(int target,long now)=>Active(now)&&Target==target;
    public void Clear(){begins=-1;}
}
