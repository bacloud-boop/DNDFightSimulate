public static class HellishRebukeRules
{
    public const string Name="Hellish Rebuke";
    public static int DiceCount(int slot)=>slot>=1&&slot<=9?slot+1:0;
    public static bool CanTrigger(int damage,int? creature,int distance,bool visible,bool canReact)=>damage>0&&creature.HasValue&&distance>=0&&distance<=60&&visible&&canReact;
}
