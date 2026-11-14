using System.Collections;
using UnityEngine;
public sealed partial class TrainingGame {
 static bool IsPointBurst(DirectSpell spell)=>spell.BurstRadius>0;
 void DrawPointBurstChoices(DirectSpell spell){GUILayout.Label("中心格列、行（1～"+map.Size+"）；半径 "+spell.BurstRadius+" 尺，范围包含自身。",small);visionColumn=IntegerField(visionColumn,false);visionRow=IntegerField(visionRow,false);Vector2Int cell;if(VisionPoint(out cell)){GUILayout.Label("中心距玩家 "+map.DistanceToCell(true,cell)+" 尺",small);GUILayout.Label("范围内："+(map.DistanceToCell(true,cell)<=spell.BurstRadius?"玩家 ":"")+(map.DistanceToCell(false,cell)<=spell.BurstRadius?"木桩":""),small);}}
 string PointBurstError(DirectSpell spell){Vector2Int cell;return !VisionPoint(out cell)?"中心须位于地图内。":map.DistanceToCell(true,cell)>spell.Range?"中心超出施法距离。":"";}
 void DrawPointBurstPreview(Rect board){if(panel!="spells")return;var spell=DirectSpell.Find(selectedSpellName);if(spell==null||!IsPointBurst(spell))return;Vector2Int center;if(!VisionPoint(out center))return;for(int y=0;y<map.Size;y++)for(int x=0;x<map.Size;x++){var cell=new Vector2Int(x,y);if(BattleMap.FeetBetween(center,cell)<=spell.BurstRadius)Fill(CellRect(board,cell),new Color(.85f,.3f,.12f,.35f));}}
 IEnumerator ResolvePointBurst(DirectSpell spell,int caster,int slot){
  Vector2Int center;if(!VisionPoint(out center))yield break;
  bool[] targets={combat.Hp>0&&map.DistanceToCell(false,center)<=spell.BurstRadius,!combat.PlayerDead&&map.DistanceToCell(true,center)<=spell.BurstRadius};bool[] failed=new bool[2];
  if(!targets[0]&&!targets[1]){history.Insert(0,spell.Chinese+"：范围内没有生物，已消耗动作。");yield break;}
  int dc=8+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(caster)]);
  for(int i=0;i<2;i++)if(targets[i]){
   bool player=i==1,automatic=spell.SaveAbility==1&&(player?combat.PlayerHp==0||combat.PlayerParalyzed:combat.MonsterParalyzed),disadvantage=spell.Name=="Shatter"&&!player&&monsterProfile.TypeIndex==3;
   int first=Random.Range(1,21),second=disadvantage?Random.Range(1,21):0,value=disadvantage?Mathf.Min(first,second):first,modifier=player?PlayerSavingThrowModifier(spell.SaveAbility):AcidSplash.Modifier(monsterAttributes[spell.SaveAbility]);
   dice=spell.Chinese+" · "+(player?"玩家":"木桩")+PlayerAttributes.Names[spell.SaveAbility]+"豁免";var adjustment=new SaveAdjustment();yield return ApplySaveAdjustments(player,automatic,adjustment);modifier+=adjustment.Value;
   if(!automatic)yield return PresentRoll(20,disadvantage?new[]{first,second}:new[]{first},(disadvantage?"构装生物劣势，采用 ":"采用 ")+value+" + "+modifier+" / DC "+dc);failed[i]=automatic||value+modifier<dc;
  }
  int[] rolls=new int[spell.DiceCount(classes.TotalLevel,slot)];int damage=0;for(int i=0;i<rolls.Length;i++){rolls[i]=Random.Range(1,spell.Sides+1);damage+=rolls[i];}
  dice=spell.Chinese+" · 共享范围伤害";yield return PresentRoll(spell.Sides,rolls,"失败伤害 "+damage+"，成功 "+damage/2+"，类型："+spell.DamageType);
  int monsterDamage=targets[0]?combat.AdjustTypedDamage(false,failed[0]?damage:damage/2,DamageTypes.Index(spell.DamageType)):0,playerDamage=targets[1]?combat.AdjustTypedDamage(true,failed[1]?damage:damage/2,DamageTypes.Index(spell.DamageType)):0;
  if(targets[0])combat.ApplyDamage(monsterDamage);if(targets[1])combat.ApplyPlayerDamage(playerDamage);
  history.Insert(0,spell.Chinese+"：木桩 "+monsterDamage+"，玩家 "+playerDamage+" 点"+spell.DamageType+"伤害。");if(targets[1])yield return MaintainConcentration(playerDamage);CheckBattleOutcome();
 }
}
