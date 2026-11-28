public sealed class CallLightningEffect
{
    public int X {get;private set;}public int Y {get;private set;}public int Scene {get;private set;}public int Dice {get;private set;}
    public long Dc {get;private set;}public long Expires {get;private set;}long begins=-1;
    public bool Active(long now)=>begins>=0&&now>=begins&&now<Expires;
    public bool Begin(int x,int y,int scene,int slot,bool storm,long dc,long now){if(x<0||y<0||scene<0||slot<3||slot>9||now<0||now>long.MaxValue-100)return false;X=x;Y=y;Scene=scene;Dice=slot+(storm?1:0);Dc=dc;begins=now;Expires=now+100;return true;}
    public void Clear(){begins=-1;Expires=0;Dice=0;}
}
