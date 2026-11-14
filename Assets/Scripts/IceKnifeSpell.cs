using System.Collections;
using UnityEngine;
public sealed partial class TrainingGame {
 void DrawIceKnifePreview(Rect board){if(panel!="spells"||selectedSpellName!="Ice Knife")return;var origin=directTargetPlayer?map.Player:map.Monster;int spaces=directTargetPlayer?map.PlayerSpaces:map.MonsterSpaces;for(int y=0;y<map.Size;y++)for(int x=0;x<map.Size;x++){var cell=new Vector2Int(x,y);if(BattleMap.BodyDistance(origin,spaces,cell,1)<=5)Fill(CellRect(board,cell),new Color(.2f,.7f,.9f,.35f));}}
 IEnumerator ResolveIceKnife(bool targetPlayer,int caster,int slot){
  outcomeDeferDepth++;try{
  var origin=targetPlayer?map.Player:map.Monster;int spaces=targetPlayer?map.PlayerSpaces:map.MonsterSpaces;
  var impact=new DirectSpell("Ice Knife Impact","冰刃 · 穿刺攻击","",1,60,10,attack:true,damageType:"穿刺",baseDice:1);
  yield return ResolveDirectSpell(impact,targetPlayer,caster,1,true);
  bool[] targets={combat.Hp>0&&BattleMap.BodyDistance(origin,spaces,map.Monster,map.MonsterSpaces)<=5,!combat.PlayerDead&&BattleMap.BodyDistance(origin,spaces,map.Player,map.PlayerSpaces)<=5};bool[] failed=new bool[2];int dc=8+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(caster)]);
  for(int i=0;i<2;i++)if(targets[i]){
   bool player=i==1,automatic=player?combat.PlayerHp==0||combat.PlayerParalyzed:combat.MonsterParalyzed;
   int value=Random.Range(1,21),modifier=player?PlayerSavingThrowModifier(1):AcidSplash.Modifier(monsterAttributes[1]);dice="冰刃爆炸 · "+(player?"玩家":"木桩")+"敏捷豁免";var adjustment=new SaveAdjustment();yield return ApplySaveAdjustments(player,automatic,adjustment);modifier+=adjustment.Value;
   if(!automatic)yield return PresentRoll(20,new[]{value},value+" + "+modifier+" / DC "+dc);failed[i]=automatic||value+modifier<dc;
  }
  if(!failed[0]&&!failed[1]){history.Insert(0,"冰刃爆炸：范围内生物均豁免成功或没有目标，寒冷伤害0。");yield break;}
  int[] rolls=new int[DirectSpell.Find("Ice Knife").DiceCount(classes.TotalLevel,slot)];int damage=0;for(int i=0;i<rolls.Length;i++){rolls[i]=Random.Range(1,7);damage+=rolls[i];}
  dice="冰刃 · 5尺爆炸";yield return PresentRoll(6,rolls,"豁免失败 "+damage+" 寒冷，成功0；不因前段攻击暴击翻倍");
  int monsterDamage=failed[0]?combat.AdjustTypedDamage(false,damage,DamageTypes.Index("寒冷")):0,playerDamage=failed[1]?combat.AdjustTypedDamage(true,damage,DamageTypes.Index("寒冷")):0;
  if(failed[0])combat.ApplyDamage(monsterDamage);if(failed[1])combat.ApplyPlayerDamage(playerDamage);history.Insert(0,"冰刃爆炸：木桩 "+monsterDamage+"，玩家 "+playerDamage+" 寒冷伤害（无论前段命中与否均爆炸）。");if(failed[1])yield return MaintainConcentration(playerDamage);CheckBattleOutcome();
  }finally{outcomeDeferDepth--;CheckBattleOutcome();}
 }
}
