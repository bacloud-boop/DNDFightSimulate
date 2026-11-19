using System;
[Flags] public enum ArmorTraining {None=0,Light=1,Medium=2,Heavy=4,Shield=8}
public sealed class ArmorDefinition
{
    public readonly string Name;
    public readonly ArmorTraining Category;
    public readonly int BaseAc,DexCap,Strength,Weight;
    public readonly bool StealthDisadvantage;
    ArmorDefinition(string name,ArmorTraining category,int ac,int dexCap,int strength,int weight,bool stealth=false){Name=name;Category=category;BaseAc=ac;DexCap=dexCap;Strength=strength;Weight=weight;StealthDisadvantage=stealth;}
    public int ArmorClass(int dexterity)=>Category==ArmorTraining.Heavy?BaseAc:BaseAc+Math.Min(DexCap,(int)Math.Floor((dexterity-10)/2.0));
    public static readonly ArmorDefinition[] All={
        new ArmorDefinition("无甲",ArmorTraining.None,10,int.MaxValue,0,0),
        new ArmorDefinition("布甲",ArmorTraining.Light,11,int.MaxValue,0,8,true),
        new ArmorDefinition("皮甲",ArmorTraining.Light,11,int.MaxValue,0,10),
        new ArmorDefinition("镶钉皮甲",ArmorTraining.Light,12,int.MaxValue,0,13),
        new ArmorDefinition("兽皮甲",ArmorTraining.Medium,12,2,0,12),
        new ArmorDefinition("链甲衫",ArmorTraining.Medium,13,2,0,20),
        new ArmorDefinition("鳞甲",ArmorTraining.Medium,14,2,0,45,true),
        new ArmorDefinition("胸甲",ArmorTraining.Medium,14,2,0,20),
        new ArmorDefinition("半身板甲",ArmorTraining.Medium,15,2,0,40,true),
        new ArmorDefinition("环甲",ArmorTraining.Heavy,14,0,0,40,true),
        new ArmorDefinition("链甲",ArmorTraining.Heavy,16,0,13,55,true),
        new ArmorDefinition("条板甲",ArmorTraining.Heavy,17,0,15,60,true),
        new ArmorDefinition("全身板甲",ArmorTraining.Heavy,18,0,15,65,true)
    };
}
