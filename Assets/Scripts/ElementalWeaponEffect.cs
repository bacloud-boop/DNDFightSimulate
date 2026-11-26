public sealed class ElementalWeaponEffect
{
    public int ObjectId {get;private set;}
    public int Bonus {get;private set;}
    public int DamageType {get;private set;}
    public long Expires {get;private set;}
    public static bool ValidType(int type)=>type==3||type==4||type==5||type==7||type==12;
    public static int SlotBonus(int slot)=>slot>=7?3:slot>=5?2:1;
    public bool Begin(int id,int slot,int type,long now)
    {
        if(id<=0||slot<3||slot>9||!ValidType(type)||now<0||now>long.MaxValue-600)return false;
        ObjectId=id;Bonus=SlotBonus(slot);DamageType=type;Expires=now+600;return true;
    }
    public int AttackBonus(int id,long now)=>id>0&&id==ObjectId&&now>=Expires-600&&now<Expires?Bonus:0;
    public void Clear(){ObjectId=0;Bonus=0;DamageType=0;Expires=0;}
}
