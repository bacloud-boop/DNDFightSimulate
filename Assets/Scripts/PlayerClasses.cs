public sealed class PlayerClasses
{
    public static readonly string[] Names = {"战士", "法师", "游侠", "术士", "契术师", "吟游诗人", "牧师", "游荡者", "武僧", "圣武士", "野蛮人"};
    readonly int[] levels = {1,0,0,0,0,0,0,0,0,0,0};
    public int InitialClass { get; private set; }
    public int this[int index] => levels[index];
    public int TotalLevel => System.Linq.Enumerable.Sum(levels);
    public bool TryApply(string[] inputs, int initialClass)
    {
        if(inputs==null || inputs.Length!=Names.Length || initialClass<0 || initialClass>=Names.Length) return false;
        var parsed=new int[Names.Length];
        for(int i=0;i<Names.Length;i++) if(!int.TryParse(inputs[i],out parsed[i]) || parsed[i]<0 || parsed[i]>20) return false;
        int totalLevel=System.Linq.Enumerable.Sum(parsed);
        if(totalLevel<1 || totalLevel>20 || parsed[initialClass]<1) return false;
        System.Array.Copy(parsed,levels,Names.Length); InitialClass=initialClass; return true;
    }
    public int CalculateMaxHp(int constitution)
    {
        int modifier=(int)System.Math.Floor((constitution-10)/2.0);
        int hp=FirstLevelHp(InitialClass)+modifier;
        for(int i=0;i<Names.Length;i++) {
            int additionalLevels=levels[i]-(i==InitialClass?1:0);
            hp+=additionalLevels*(LaterLevelHp(i)+modifier);
        }
        return hp;
    }
    static int FirstLevelHp(int index) => index==10?12:(index>=4?8:(index==1 || index==3?6:10));
    static int LaterLevelHp(int index) => index==10?7:(index>=4?5:(index==1 || index==3?4:6));
}
