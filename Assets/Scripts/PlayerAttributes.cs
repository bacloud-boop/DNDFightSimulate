using System;
public sealed class PlayerAttributes
{
    public static readonly string[] Names = {"力量", "敏捷", "体质", "感知", "智力", "魅力"};
    readonly int[] values = {10,10,10,10,10,10};
    public int this[int index] => values[index];
    public bool TryApply(string[] inputs)
    {
        if(inputs == null || inputs.Length != 6) return false;
        var parsed = new int[6];
        for(int i=0;i<6;i++)
            if(!int.TryParse(inputs[i], out parsed[i]) || parsed[i]<3 || parsed[i]>20) return false;
        Array.Copy(parsed, values, 6);
        return true;
    }
}
