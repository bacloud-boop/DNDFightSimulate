using System;
using System.Collections.Generic;
public enum Species2024 {None=-1,Aasimar,Dragonborn,Dwarf,Elf,Gnome,Goliath,Halfling,Human,Orc,Tiefling}
public sealed class SpeciesProfile
{
    public static readonly string[] Names={"阿斯莫","龙裔","矮人","精灵","侏儒","歌利亚","半身人","人类","兽人","提夫林"};
    public static readonly string[] OriginFeats={"警觉","巧匠","医者","幸运","魔法学徒","音乐家","凶蛮打击者","熟习","酒馆斗殴者","健壮"};
    public Species2024 Kind=Species2024.None;
    public int Size=2,Lineage,SpellAbility=4,Level=1,ChosenSkill;
    public int OriginFeat=7;
    public int BonusPriority; // SpeciesTurnPolicy: before spells, after spells, disabled.
    public int[] SkilledChoices={4,5,6};
    public static SpeciesProfile Human()=>new SpeciesProfile{Kind=Species2024.Human};
    public SpeciesProfile Copy(){var p=(SpeciesProfile)MemberwiseClone();p.SkilledChoices=SkilledChoices==null?null:(int[])SkilledChoices.Clone();return p;}
    public string Name=>Kind==Species2024.None?"无种族特性":Names[(int)Kind];
    public string[] Lineages=>Kind==Species2024.Dragonborn?new[]{"黑龙","蓝龙","黄铜龙","青铜龙","赤铜龙","金龙","绿龙","红龙","银龙","白龙"}:Kind==Species2024.Elf?new[]{"卓尔","高等精灵","木精灵"}:Kind==Species2024.Gnome?new[]{"森林侏儒","岩侏儒"}:Kind==Species2024.Goliath?new[]{"云巨人","火巨人","霜巨人","山丘巨人","石巨人","风暴巨人"}:Kind==Species2024.Tiefling?new[]{"深渊","冥界","炼狱"}:new[]{"标准"};
    public bool SizeChoice=>Kind==Species2024.Aasimar||Kind==Species2024.Human||Kind==Species2024.Tiefling;
    public int BodySize=>Kind==Species2024.Gnome||Kind==Species2024.Halfling?1:SizeChoice?Size:2;
    public int Speed=>Kind==Species2024.Goliath||Kind==Species2024.Elf&&Lineage==2?35:30;
    public int Darkvision=>Kind==Species2024.Dwarf||Kind==Species2024.Orc||Kind==Species2024.Elf&&Lineage==0?120:Kind==Species2024.Aasimar||Kind==Species2024.Dragonborn||Kind==Species2024.Elf||Kind==Species2024.Gnome||Kind==Species2024.Tiefling?60:0;
    public bool NoSleep=>Kind==Species2024.Elf;
    public int LongRestRounds=>NoSleep?2400:4800;
    public int HpBonus(int level)=>Kind==Species2024.Dwarf?level:Kind==Species2024.Human&&OriginFeat==9?2*level:0;
    public int CapacitySize(int bodySize)=>Kind==Species2024.Goliath?Math.Min(5,bodySize+1):bodySize;
    public bool Resist(int type)
    {
        if(Kind==Species2024.Aasimar)return type==8||type==11;
        if(Kind==Species2024.Dwarf)return type==9;
        if(Kind==Species2024.Tiefling)return type==(Lineage==0?9:Lineage==1?8:5);
        if(Kind==Species2024.Dragonborn){int[] types={3,7,5,7,3,5,9,5,4,4};return type==types[Lineage];}
        return false;
    }
    public bool SaveAdvantage(int ability,bool poison=false,bool charm=false,bool fear=false)
        =>Kind==Species2024.Gnome&&(ability==3||ability==4||ability==5)||Kind==Species2024.Dwarf&&poison||Kind==Species2024.Elf&&charm||Kind==Species2024.Halfling&&fear;
    public IEnumerable<int> Proficiencies()
    {
        if(Kind==Species2024.Human||Kind==Species2024.Elf)yield return ChosenSkill;
        if(Kind==Species2024.Human&&OriginFeat==7)foreach(int id in SkilledChoices)yield return id;
    }
    public bool Valid()
    {
        if(!SpeciesTurnPolicy.Valid(BonusPriority))return false;
        if((int)Kind< -1||(int)Kind>9||Level<1||Level>20||(SpellAbility!=3&&SpellAbility!=4&&SpellAbility!=5)||Lineage<0||Lineage>=Lineages.Length||(Size!=1&&Size!=2)||OriginFeat<0||OriginFeat>=OriginFeats.Length)return false;
        if(ChosenSkill<0||ChosenSkill>=18||Kind==Species2024.Elf&&ChosenSkill!=10&&ChosenSkill!=12&&ChosenSkill!=13)return false;
        if(SkilledChoices==null||SkilledChoices.Length!=3)return false;
        var seen=new HashSet<int>();if(Kind==Species2024.Human)seen.Add(ChosenSkill);
        foreach(int id in SkilledChoices)if(id<0||id>=55||Kind==Species2024.Human&&OriginFeat==7&&!seen.Add(id))return false;
        return true;
    }
    public long ProficiencyUses(int proficiency)=>Math.Max(0L,proficiency);
}
