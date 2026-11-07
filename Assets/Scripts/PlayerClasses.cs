public sealed class PlayerClasses
{
    public static readonly string[] Names = {"战士", "法师", "游侠"};
    readonly int[] levels = {1,0,0};
    public int InitialClass { get; private set; }
    public int this[int index] => levels[index];
    public int TotalLevel => levels[0]+levels[1]+levels[2];
    public bool TryApply(string[] inputs, int initialClass)
    {
        if(inputs==null || inputs.Length!=3 || initialClass<0 || initialClass>=3) return false;
        var parsed=new int[3];
        for(int i=0;i<3;i++) if(!int.TryParse(inputs[i],out parsed[i]) || parsed[i]<0 || parsed[i]>20) return false;
        if(parsed[0]+parsed[1]+parsed[2]<1 || parsed[initialClass]<1) return false;
        System.Array.Copy(parsed,levels,3); InitialClass=initialClass; return true;
    }
    public int CalculateMaxHp(int constitution)
    {
        int modifier=(int)System.Math.Floor((constitution-10)/2.0);
        int hp=(InitialClass==1?6:10)+modifier;
        for(int i=0;i<3;i++) {
            int additionalLevels=levels[i]-(i==InitialClass?1:0);
            hp+=additionalLevels*((i==1?4:6)+modifier);
        }
        return hp;
    }
}
