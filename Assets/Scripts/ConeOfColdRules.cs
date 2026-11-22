public static class ConeOfColdRules
{
    public const int LengthFeet=60;
    public static int DiceCount(int slot)=>slot>=5&&slot<=9?slot+3:0;
    public static int AfterSave(int damage,bool saved)=>System.Math.Max(0,damage)/(saved?2:1);
}
