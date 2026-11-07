public sealed class Weapon
{
    public string Name { get; }
    public int DiceSides { get; }
    public Weapon(string name, int diceSides) { Name = name; DiceSides = diceSides; }
    public int RollDamage() { return UnityEngine.Random.Range(1, DiceSides + 1); }
    public static readonly Weapon[] Available = {
        new Weapon("长剑（双手）", 10),
        new Weapon("长剑（单手）", 8),
        new Weapon("短剑", 6),
        new Weapon("匕首", 4)
    };
}
