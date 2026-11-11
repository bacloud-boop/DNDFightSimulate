public static class ConcentrationRules
{
    public static int DamageSaveDc(int damage) => System.Math.Min(30,System.Math.Max(10,damage/2));
}
