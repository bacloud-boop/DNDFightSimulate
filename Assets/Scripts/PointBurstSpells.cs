using System.Collections;
using UnityEngine;
public sealed partial class TrainingGame {
 bool burstHealPlayer=true,burstHealMonster;
 static bool IsPointBurst(DirectSpell spell)=>spell.BurstRadius>0;
 void DrawPointBurstChoices(DirectSpell spell){if(spell.Healing){burstHealPlayer=GUILayout.Toggle(burstHealPlayer,"选择玩家",label);burstHealMonster=GUILayout.Toggle(burstHealMonster,"选择木桩",label);}GUILayout.Label("中心格列、行（1～"+map.Size+"）；半径 "+spell.BurstRadius+" 尺。"+(spell.Healing?"至多选择6名生物（目前场上2名）。":"范围包含自身。"),small);visionColumn=IntegerField(visionColumn,false);visionRow=IntegerField(visionRow,false);Vector2Int cell;if(VisionPoint(out cell)){GUILayout.Label("中心距玩家 "+map.DistanceToCell(true,cell)+" 尺",small);GUILayout.Label("范围内："+(map.DistanceToCell(true,cell)<=spell.BurstRadius?"玩家 ":"")+(map.DistanceToCell(false,cell)<=spell.BurstRadius?"木桩":""),small);}}
 string PointBurstError(DirectSpell spell){Vector2Int cell;return !VisionPoint(out cell)?"中心须位于地图内。":spell.Healing&&VisibilityAt(true,cell,1)==2?"此法术需要看见范围中心。":map.DistanceToCell(true,cell)>spell.Range?"中心超出施法距离。":"";}
 void DrawPointBurstPreview(Rect board){if(panel!="spells")return;var spell=DirectSpell.Find(selectedSpellName);if(spell==null||!IsPointBurst(spell))return;Vector2Int center;if(!VisionPoint(out center))return;for(int y=0;y<map.Size;y++)for(int x=0;x<map.Size;x++){var cell=new Vector2Int(x,y);if(BattleMap.FeetBetween(center,cell)<=spell.BurstRadius)Fill(CellRect(board,cell),spell.Healing?new Color(.2f,.8f,.3f,.35f):new Color(.85f,.3f,.12f,.35f));}}
 IEnumerator ResolvePointBurst(DirectSpell spell,int caster,int slot){
  Vector2Int center;if(!VisionPoint(out center))yield break;
  bool[] targets={combat.Hp>0&&map.DistanceToCell(false,center)<=spell.BurstRadius&&(!spell.Healing||burstHealMonster),!combat.PlayerDead&&map.DistanceToCell(true,center)<=spell.BurstRadius&&(!spell.Healing||burstHealPlayer)};bool[] failed=new bool[2];
  if(!targets[0]&&!targets[1]){history.Insert(0,spell.Chinese+"：范围内没有生物，已消耗动作。");yield break;}
  int dc=8+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(caster)]);
  for(int i=0;i<2;i++)if(targets[i]&&!spell.Healing){
   bool player=i==1,automatic=spell.SaveAbility==1&&(player?combat.PlayerHp==0||combat.PlayerParalyzed:combat.MonsterParalyzed),disadvantage=spell.Name=="Shatter"&&!player&&monsterProfile.TypeIndex==3;
   int first=Random.Range(1,21),second=disadvantage?Random.Range(1,21):0,value=disadvantage?Mathf.Min(first,second):first,modifier=player?PlayerSavingThrowModifier(spell.SaveAbility):AcidSplash.Modifier(monsterAttributes[spell.SaveAbility]);
   dice=spell.Chinese+" · "+(player?"玩家":"木桩")+PlayerAttributes.Names[spell.SaveAbility]+"豁免";var adjustment=new SaveAdjustment();yield return ApplySaveAdjustments(player,automatic,adjustment);modifier+=adjustment.Value;
   if(!automatic)yield return PresentRoll(20,disadvantage?new[]{first,second}:new[]{first},(disadvantage?"构装生物劣势，采用 ":"采用 ")+value+" + "+modifier+" / DC "+dc);failed[i]=automatic||value+modifier<dc;
  }
  int[] rolls=new int[spell.DiceCount(classes.TotalLevel,slot)];int damage=0;for(int i=0;i<rolls.Length;i++){rolls[i]=Random.Range(1,spell.Sides+1);damage+=rolls[i];}
  if(spell.Healing){int modifier=AcidSplash.Modifier(attributes[SpellAbilityIndex(caster)]);damage=Mathf.Max(0,damage+modifier);dice=spell.Chinese+" · 共享治疗骰";yield return PresentRoll(spell.Sides,rolls,"骰值 + 施法调整值 "+modifier+" = "+damage+" 治疗");int playerHeal=targets[1]?combat.HealPlayer(damage):0,monsterHeal=targets[0]?combat.HealMonster(damage):0;history.Insert(0,spell.Chinese+"：玩家实际恢复 "+playerHeal+"，木桩实际恢复 "+monsterHeal+"；不能恢复生命的目标仍受原有限制。");yield break;}
  dice=spell.Chinese+" · 共享范围伤害";yield return PresentRoll(spell.Sides,rolls,"失败伤害 "+damage+"，成功 "+damage/2+"，类型："+spell.DamageType);
  int monsterDamage=targets[0]?combat.AdjustTypedDamage(false,failed[0]?damage:damage/2,DamageTypes.Index(spell.DamageType)):0,playerDamage=targets[1]?combat.AdjustTypedDamage(true,failed[1]?damage:damage/2,DamageTypes.Index(spell.DamageType)):0;
  if(targets[0])combat.ApplyDamage(monsterDamage);if(targets[1])combat.ApplyPlayerDamage(playerDamage,sourcePlayer:true);
  history.Insert(0,spell.Chinese+"：木桩 "+monsterDamage+"，玩家 "+playerDamage+" 点"+spell.DamageType+"伤害。");if(targets[1])yield return MaintainConcentration(playerDamage);CheckBattleOutcome();
 }
}
