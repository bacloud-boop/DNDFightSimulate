public sealed partial class TrainingGame
{
    // Zero is Bludgeoning; kept independent from each existing damage group.
    int monsterBaseWeaponType,monsterBaseWeaponTypeDraft;
    int MonsterBaseWeaponDamageType=>MonsterPolymorphed?DamageTypes.Index(BeastAttack.Find(polymorph.Form).DamageType):monsterBaseWeaponType;
}
