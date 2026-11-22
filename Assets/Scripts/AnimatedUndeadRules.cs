using System;
public sealed class AnimatedUndeadRules
{
    public bool Skeleton {get;}
    static readonly int[] skeleton={10,16,15,8,6,5},zombie={13,6,16,6,3,5};
    public int Ability(int index)=>(Skeleton?skeleton:zombie)[index];
    public int Save(int index)=>!Skeleton&&index==3?0:AcidSplash.Modifier(Ability(index));
    public int Armor=>Skeleton?14:8;
    public int MaxHp=>Skeleton?13:15;
    public int Speed=>Skeleton?30:20;
    public AnimatedUndeadRules(bool bones){Skeleton=bones;}
    public SummonedUnit Create(int actor)
    {
        var unit=new SummonedUnit(actor,Skeleton?"骷髅":"僵尸",MaxHp,Armor,Speed,2,1,breathesAir:true,breathesWater:true);
        unit.CreatureType=13;unit.ChallengeEighths=2;unit.ExhaustionImmune=true;unit.NoSleep=true;unit.IntrinsicImmunity=t=>t==9;unit.IntrinsicVulnerability=t=>Skeleton&&t==0;
        return unit;
    }
    public bool FortitudeEligible(int damage,long[] types,bool critical)=>!Skeleton&&damage>0&&!critical&&(types==null||types.Length<=11||types[11]==0);
    public static long FortitudeDc(int damage)=>5L+damage;
}
