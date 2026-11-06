public sealed class PlayerClasses
{
    public static readonly string[] Names = {"战士", "法师"};
    readonly int[] levels = {1,0};
    public int this[int index] => levels[index];
    public int TotalLevel => levels[0]+levels[1];
    public bool TryApply(string[] inputs)
    {
        if(inputs==null || inputs.Length!=2) return false;
        var parsed=new int[2];
        for(int i=0;i<2;i++) if(!int.TryParse(inputs[i],out parsed[i]) || parsed[i]<0 || parsed[i]>20) return false;
        if(parsed[0]+parsed[1]<1) return false;
        System.Array.Copy(parsed,levels,2); return true;
    }
}
