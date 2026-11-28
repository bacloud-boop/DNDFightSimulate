using System;
public sealed class BestialSpiritRules
{
    public int Slot {get;}
    public int AttackBonus {get;}
    public int Armor=>11+Slot;
    public int MaxHp=>30+5*(Slot-2);
    public int Attacks=>Slot/2;
    public int FixedDamage=>4+Slot;
    public const int Reach=5,Speed=30,Darkvision=60;
    public const int DamageSides=8;
    public const string DamageType="穿刺";
    // Attribute order used by the simulator: Str, Dex, Con, Wis, Int, Cha.
    static readonly int[] scores={18,11,16,14,4,5};
    public int Ability(int index)=>scores[index];
    public BestialSpiritRules(int slot,int spellAttackBonus){if(slot<2||slot>9)throw new ArgumentOutOfRangeException(nameof(slot));Slot=slot;AttackBonus=spellAttackBonus;}
    public SummonedUnit CreateLandUnit(int id,string appearance="")=>new SummonedUnit(id,"大地野兽灵魄"+(string.IsNullOrWhiteSpace(appearance)?"":"（"+appearance+"）"),MaxHp,Armor,Speed,1,Attacks);
    public SummonedUnit CreateWaterUnit(int id,string appearance="")=>new SummonedUnit(id,"海洋野兽灵魄"+(string.IsNullOrWhiteSpace(appearance)?"":"（"+appearance+"）"),MaxHp,Armor,Speed,1,Attacks,swimSpeed:30,breathesAir:false,breathesWater:true);
    public static bool PackTactics(bool allyWithinFiveFeet,bool allyIncapacitated)=>allyWithinFiveFeet&&!allyIncapacitated;
}
