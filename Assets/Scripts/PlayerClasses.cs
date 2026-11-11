public sealed class PlayerClasses
{
    public static readonly string[] Names = {"战士", "法师", "游侠", "术士", "契术师", "吟游诗人", "牧师", "游荡者", "武僧", "圣武士", "野蛮人", "德鲁伊"};
    readonly int[] levels = CreateLevels();
    static int[] CreateLevels(){var values=new int[Names.Length];values[0]=1;return values;}
    readonly int[] subclasses = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Repeat(-1,Names.Length));
    public int Subclass(int index) => levels[index]>=3?subclasses[index]:-1;
    public bool TryApplySubclasses(int[] choices) {
        if(choices==null || choices.Length!=levels.Length)return false;
        for(int i=0;i<levels.Length;i++)if(levels[i]>=3 && (choices[i]<0 || choices[i]>=Subclasses2024.Names[i].Length))return false;
        for(int i=0;i<levels.Length;i++)subclasses[i]=levels[i]>=3?choices[i]:-1;return true;
    }
    public bool SavingThrowProficient(int ability) {
        switch(InitialClass){case 0:case 10:return ability==0||ability==2;case 1:case 11:return ability==4||ability==3;case 2:case 8:return ability==0||ability==1;case 3:return ability==2||ability==5;case 5:return ability==1||ability==5;case 7:return ability==1||ability==4;default:return ability==3||ability==5;}
    }
    public int HitDie(int index) => FirstLevelHp(index);
    public int InitialClass { get; private set; }
    public int this[int index] => levels[index];
    public int TotalLevel => System.Linq.Enumerable.Sum(levels);
    public int ProficiencyBonus => 2 + (TotalLevel - 1) / 4;
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
    static int FirstLevelHp(int index) => index==10?12:(index==1||index==3)?6:(index==0||index==2||index==9)?10:8;
    static int LaterLevelHp(int index) => FirstLevelHp(index)/2+1;
}