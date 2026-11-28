public static class Alignment2024
{
    public const int Default=4;
    public static readonly string[] Names={"守序善良","中立善良","混乱善良","守序中立","绝对中立","混乱中立","守序邪恶","中立邪恶","混乱邪恶"};
    public static bool Valid(int value)=>value>=0&&value<Names.Length;
    public static int GuardianDamageType(int value)=>value>=6&&Valid(value)?8:11;
}
