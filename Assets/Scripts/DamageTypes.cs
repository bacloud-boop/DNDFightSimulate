using System;
using System.Text;
public static class DamageTypes
{
    public static readonly string[] Names={"钝击","穿刺","挥砍","强酸","寒冷","火焰","力场","闪电","死灵","毒素","心灵","光耀","雷鸣"};
    public static int Index(string name){return Array.IndexOf(Names,name);}
    public static int[] Resolve(long[] raw,bool[] resistance=null,bool[] immunity=null,bool[] vulnerability=null)
    {
        if(raw==null||raw.Length!=Names.Length)throw new ArgumentException("Damage vector must contain 13 types.");
        foreach(var flags in new[]{resistance,immunity,vulnerability})if(flags!=null&&flags.Length!=Names.Length)throw new ArgumentException("Damage flags must contain 13 types.");
        var result=new int[Names.Length];for(int i=0;i<result.Length;i++){
            long value=Math.Max(0,raw[i]);
            if(immunity!=null&&immunity[i])value=0;
            else {if(resistance!=null&&resistance[i])value/=2;if(vulnerability!=null&&vulnerability[i])value=value>int.MaxValue?int.MaxValue:value*2;}
            result[i]=(int)Math.Min(int.MaxValue,value);
        }return result;
    }
    public static int Total(int[] values){long total=0;foreach(int value in values)total+=value;return (int)Math.Min(int.MaxValue,total);}
    public static string Describe(int[] values){var text=new StringBuilder();for(int i=0;i<values.Length;i++)if(values[i]>0)text.Append((text.Length>0?"，":"")+Names[i]+" "+values[i]);return text.Length==0?"各类伤害均为0":text.ToString();}
}
