using System;
public sealed class CreatureProfile
{
    public static readonly string[] TypeNames={"异怪","野兽","天界","构装生物","龙","元素","妖精","邪魔","巨人","类人生物","怪兽","泥怪","植物","亡灵"};
    public static readonly string[] SizeNames={"微型","小型","中型","大型","巨型","超巨型"};
    public int TypeIndex {get;private set;}=9;
    public int SizeIndex {get;private set;}=2;
    public static int SpacesForSize(int size) => size<=2?1:size==3?2:size==4?3:4;
    public int Spaces => SpacesForSize(SizeIndex);
    public bool IsHumanoid => TypeIndex==9;
    public bool IsBeast => TypeIndex==1;
    public bool TryApply(int type,int size){if(type<0||type>=TypeNames.Length||size<0||size>=SizeNames.Length)return false;TypeIndex=type;SizeIndex=size;return true;}
    public string Label => SizeNames[SizeIndex]+" · "+TypeNames[TypeIndex];
}
