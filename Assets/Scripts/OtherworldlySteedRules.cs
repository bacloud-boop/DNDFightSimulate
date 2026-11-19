using System;
public enum SteedKind {Celestial,Fey,Fiend}
public sealed class OtherworldlySteedRules
{
    public int Slot {get;}
    public SteedKind Kind {get;}
    public int Armor=>10+Slot;
    public int MaxHp=>5+10*Slot;
    public int Speed=>60;
    public int DamageBonus=>Slot;
    public int DamageType=>Kind==SteedKind.Celestial?11:Kind==SteedKind.Fey?10:8;
    public bool SpecialAvailable {get;private set;}=true;
    public OtherworldlySteedRules(int slot,SteedKind kind)
    {
        if(slot<2||slot>9||!Enum.IsDefined(typeof(SteedKind),kind))throw new ArgumentOutOfRangeException();
        Slot=slot;Kind=kind;
    }
    public int Ability(int index)
    {
        int[] scores={18,12,14,12,6,8};
        if(index<0||index>=scores.Length)throw new ArgumentOutOfRangeException(nameof(index));
        return scores[index];
    }
    public SummonedUnit CreateUnit(int id)=>new SummonedUnit(id,"异界坐骑",MaxHp,Armor,Speed,3,1);
    public bool TryUseSpecial(Func<bool> operation)
    {
        if(!SpecialAvailable||operation==null||!operation())return false;
        SpecialAvailable=false;return true;
    }
    public void LongRest(){SpecialAvailable=true;}
    public static bool CanAttack(bool mounted,bool riderIncapacitated)=>!mounted||riderIncapacitated;
    public static int SharedSpaces(int steedSpaces,bool mounted,int riderSpaces)=>mounted?steedSpaces:riderSpaces;
}
