public sealed class Weapon
{
    public string Name { get; }
    public int DiceSides { get; }
    public int DiceCount { get; }
    public int FixedDamage { get; }
    public string DamageDice => DiceCount==0?FixedDamage.ToString():DiceCount+"d"+DiceSides;
    public bool HasFinesse { get; }
    public bool HasLight { get; }
    public bool IsMelee { get; }
    public int ReachFeet { get; }
    public int MaxRangeFeet { get; }
    public string RangeText => IsMelee?ReachFeet+"尺":ReachFeet+"/"+MaxRangeFeet+"尺";
    public bool CanReach(int distanceFeet) => distanceFeet<=MaxRangeFeet;
    public bool HasRangeDisadvantage(int distanceFeet) => !IsMelee && distanceFeet>ReachFeet && CanReach(distanceFeet);
    public string Traits => (IsMelee?"近战武器":"远程武器")+" · "+RangeText+" · "+(HasFinesse?"灵巧":"无灵巧")+(HasLight?" · 轻型":"");
    public Weapon(string name, int diceSides, bool hasFinesse = false, int diceCount = 1, bool hasLight = false, bool isMelee = true, int rangeFeet = 5, int maxRangeFeet = 5, int fixedDamage = 0) { Name = name; DiceSides = diceSides; HasFinesse = hasFinesse; DiceCount = diceCount; HasLight = hasLight; IsMelee = isMelee; ReachFeet = rangeFeet; MaxRangeFeet = maxRangeFeet; FixedDamage = fixedDamage; }
    public bool UsesDexterity(PlayerAttributes attributes) => HasFinesse?attributes.DexterityModifier>attributes.StrengthModifier:!IsMelee;
    public int AttackModifier(PlayerAttributes attributes) => UsesDexterity(attributes) ? attributes.DexterityModifier : attributes.StrengthModifier;
    public int[] RollDamageDice(bool critical = false) {
        var values=new int[DiceCount*(critical?2:1)];
        for(int i=0;i<values.Length;i++)values[i]=UnityEngine.Random.Range(1,DiceSides+1);
        return values;
    }
    public int RollDamage() { return System.Linq.Enumerable.Sum(RollDamageDice())+FixedDamage; }
    public static readonly Weapon[] Available = {
        new Weapon("长剑（双手）", 10),
        new Weapon("长剑（单手）", 8),
        new Weapon("短剑", 6, true, hasLight: true),
        new Weapon("匕首", 4, true, hasLight: true),
        new Weapon("刺剑", 8, true),
        new Weapon("弯刀", 6, true, hasLight: true),
        new Weapon("鞭", 4, true, rangeFeet: 10, maxRangeFeet: 10),
        new Weapon("短棒", 4, hasLight: true),
        new Weapon("巨棒", 8),
        new Weapon("手斧", 6, hasLight: true),
        new Weapon("标枪", 6),
        new Weapon("轻锤", 4, hasLight: true),
        new Weapon("硬头锤", 6),
        new Weapon("长棍", 6),
        new Weapon("镰刀", 4, hasLight: true),
        new Weapon("矛", 6),
        new Weapon("战斧", 8),
        new Weapon("链枷", 8),
        new Weapon("长柄刀", 10, rangeFeet: 10, maxRangeFeet: 10),
        new Weapon("巨斧", 12),
        new Weapon("巨剑", 6, diceCount: 2),
        new Weapon("戟", 10, rangeFeet: 10, maxRangeFeet: 10),
        new Weapon("骑枪", 10, rangeFeet: 10, maxRangeFeet: 10),
        new Weapon("巨锤", 6, diceCount: 2),
        new Weapon("钉头锤", 8),
        new Weapon("长矛", 10, rangeFeet: 10, maxRangeFeet: 10),
        new Weapon("三叉戟", 8),
        new Weapon("战镐", 8),
        new Weapon("战锤", 8),
        new Weapon("长弓", 8, isMelee: false, rangeFeet: 150, maxRangeFeet: 600),
        new Weapon("飞镖", 4, hasFinesse: true, isMelee: false, rangeFeet: 20, maxRangeFeet: 60),
        new Weapon("轻弩", 8, isMelee: false, rangeFeet: 80, maxRangeFeet: 320),
        new Weapon("短弓", 6, isMelee: false, rangeFeet: 80, maxRangeFeet: 320),
        new Weapon("投石索", 4, isMelee: false, rangeFeet: 30, maxRangeFeet: 120),
        new Weapon("吹箭筒", 0, diceCount: 0, isMelee: false, rangeFeet: 25, maxRangeFeet: 100, fixedDamage: 1),
        new Weapon("手弩", 6, hasLight: true, isMelee: false, rangeFeet: 30, maxRangeFeet: 120),
        new Weapon("重弩", 10, isMelee: false, rangeFeet: 100, maxRangeFeet: 400),
        new Weapon("火铳", 12, isMelee: false, rangeFeet: 40, maxRangeFeet: 120),
        new Weapon("手铳", 10, isMelee: false, rangeFeet: 30, maxRangeFeet: 90)
    };
}
