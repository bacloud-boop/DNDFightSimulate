public sealed class Weapon
{
    public string Name { get; }
    public int DiceSides { get; }
    public bool HasFinesse { get; }
    public Weapon(string name, int diceSides, bool hasFinesse = false) { Name = name; DiceSides = diceSides; HasFinesse = hasFinesse; }
    public bool UsesDexterity(PlayerAttributes attributes) => HasFinesse && attributes.DexterityModifier > attributes.StrengthModifier;
    public int AttackModifier(PlayerAttributes attributes) => UsesDexterity(attributes) ? attributes.DexterityModifier : attributes.StrengthModifier;
    public int RollDamage() { return UnityEngine.Random.Range(1, DiceSides + 1); }
    public static readonly Weapon[] Available = {
        new Weapon("长剑（双手）", 10),
        new Weapon("长剑（单手）", 8),
        new Weapon("短剑", 6, true),
        new Weapon("匕首", 4, true),
        new Weapon("刺剑", 8, true),
        new Weapon("弯刀", 6, true),
        new Weapon("鞭", 4, true)
    };
}
