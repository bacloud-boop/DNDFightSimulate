public sealed class AgathysEffect
{
    long ends=-1;int damage;
    public void Cast(int slot,long now){if(slot<1||slot>9)throw new System.ArgumentOutOfRangeException(nameof(slot));damage=slot*5;ends=now+600;}
    public void Refresh(long now,int temporaryHp){if(now>=ends||temporaryHp<=0)Clear();}
    public int Retaliation(long now,int temporaryHp,bool melee){Refresh(now,temporaryHp);return melee?damage:0;}
    public void Clear(){damage=0;ends=-1;}
}
