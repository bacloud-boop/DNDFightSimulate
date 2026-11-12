public sealed class TimedSpellBuffs
{
    long divineEnd=-1,magicEnd=-1,playerStrideEnd=-1,monsterStrideEnd=-1;
    long playerBarkEnd=-1,monsterBarkEnd=-1;
    public void Barkskin(bool player,long now){if(player)playerBarkEnd=now+600;else monsterBarkEnd=now+600;}
    public int ArmorFloor(bool player,long now)=>now<(player?playerBarkEnd:monsterBarkEnd)?17:0;
    string magicWeapon="";int magicValue;
    public void DivineFavor(long now){divineEnd=now+10;}
    public bool DivineActive(long now)=>now<divineEnd;
    public void MagicWeapon(string weapon,int slot,long now){magicWeapon=weapon;magicValue=slot>=6?3:slot>=3?2:1;magicEnd=now+600;}
    public int WeaponBonus(string weapon,long now)=>now<magicEnd&&weapon==magicWeapon?magicValue:0;
    public void Longstrider(bool player,long now){if(player)playerStrideEnd=now+600;else monsterStrideEnd=now+600;}
    public int SpeedBonus(bool player,long now)=>now<(player?playerStrideEnd:monsterStrideEnd)?10:0;
    public void Clear(){playerBarkEnd=monsterBarkEnd=-1;divineEnd=magicEnd=playerStrideEnd=monsterStrideEnd=-1;magicWeapon="";magicValue=0;}
}
