public sealed partial class TrainingGame
{
 int monsterAttackAbility,monsterAttackAbilityDraft;
 long MonsterCustomHitBonus=>EnchantedWeaponAttack(MonsterWeaponObject)+combat.MonsterHitBonus+MonsterAttackAbility.Modifier(monsterAttackAbility,monsterAttackAbility==0?10:EffectiveMonsterAbility(monsterAttackAbility-1));
}
