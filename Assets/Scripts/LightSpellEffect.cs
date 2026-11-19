using System;
public sealed class LightSpellEffect
{
    public int ObjectId {get;private set;}
    public long Expires {get;private set;}
    public int Red {get;private set;}=255;
    public int Green {get;private set;}=230;
    public int Blue {get;private set;}=170;
    public bool Active(long now)=>ObjectId>0&&now>=0&&now<Expires;
    public static bool CanTouch(int objectSize,bool inReach,int? carrier,int caster=0)=>objectSize>=0&&objectSize<=3&&inReach&&(!carrier.HasValue||carrier.Value==caster);
    public bool Cast(int objectId,int red,int green,int blue,long now)
    {
        if(objectId<1||red<0||red>255||green<0||green>255||blue<0||blue>255||now<0||now>long.MaxValue-600)return false;
        ObjectId=objectId;Red=red;Green=green;Blue=blue;Expires=now+600;return true;
    }
    public void Clear(){ObjectId=0;Expires=0;}
}
