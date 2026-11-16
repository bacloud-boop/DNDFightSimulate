public static class MonsterAttackAbility
{
 public static readonly string[] Names={"无属性","力量","敏捷","体质","感知","智力","魅力"};
 public static bool Valid(int selection)=>selection>=0&&selection<=6;
 public static int Modifier(int selection,int score)=>selection==0?0:AcidSplash.Modifier(score);
}
public sealed partial class TrainingGame
{
 int monsterAttackAbility,monsterAttackAbilityDraft;
 long MonsterCustomHitBonus=>combat.MonsterHitBonus+MonsterAttackAbility.Modifier(monsterAttackAbility,monsterAttackAbility==0?10:EffectiveMonsterAbility(monsterAttackAbility-1));
}
