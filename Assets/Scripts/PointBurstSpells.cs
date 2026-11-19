using System.Collections;
using UnityEngine;
public sealed partial class TrainingGame {
 bool burstHealPlayer=true,burstHealMonster;
 readonly System.Collections.Generic.HashSet<int> burstHealSummons=new System.Collections.Generic.HashSet<int>();
 static bool IsPointBurst(DirectSpell spell)=>spell.BurstRadius>0;
 void DrawPointBurstChoices(DirectSpell spell){if(spell.Healing){burstHealPlayer=GUILayout.Toggle(burstHealPlayer,"选择玩家",label);burstHealMonster=GUILayout.Toggle(burstHealMonster,"选择木桩",label);foreach(var unit in map.Summons)if(unit.Alive){bool chosen=GUILayout.Toggle(burstHealSummons.Contains(unit.Id),"选择："+unit.Name,label);if(chosen)burstHealSummons.Add(unit.Id);else burstHealSummons.Remove(unit.Id);}}GUILayout.Label("中心格列、行（1～"+map.Size+"）；半径 "+spell.BurstRadius+" 尺。"+(spell.Healing?"至多选择6名生物，范围内的召唤物也可选。":"范围包含自身。"),small);visionColumn=IntegerField(visionColumn,false);visionRow=IntegerField(visionRow,false);Vector2Int cell;if(VisionPoint(out cell)){GUILayout.Label("中心距玩家 "+map.DistanceToCell(true,cell)+" 尺",small);GUILayout.Label("范围内："+(map.DistanceToCell(true,cell)<=spell.BurstRadius?"玩家 ":"")+(map.DistanceToCell(false,cell)<=spell.BurstRadius?"木桩":""),small);}}
 string PointBurstError(DirectSpell spell){
  Vector2Int cell;if(!VisionPoint(out cell))return "中心须位于地图内。";
  if(spell.Healing&&VisibilityAt(true,cell,1)==2)return "此法术需要看见范围中心。";
  if(map.DistanceToCell(true,cell)>spell.Range)return "中心超出施法距离。";
  if(spell.Healing){int selected=(burstHealPlayer&&!combat.PlayerDead&&map.DistanceToCell(true,cell)<=spell.BurstRadius?1:0)+(burstHealMonster&&combat.Hp>0&&map.DistanceToCell(false,cell)<=spell.BurstRadius?1:0);foreach(var unit in SummonsInBurst(cell,spell.BurstRadius))if(burstHealSummons.Contains(unit.Id))selected++;if(selected>6)return "最多选择6名范围内目标。";}
  return "";
 }
 void DrawPointBurstPreview(Rect board){if(panel!="spells")return;var spell=DirectSpell.Find(selectedSpellName);if(spell==null||!IsPointBurst(spell))return;Vector2Int center;if(!VisionPoint(out center))return;for(int y=0;y<map.Size;y++)for(int x=0;x<map.Size;x++){var cell=new Vector2Int(x,y);if(BattleMap.FeetBetween(center,cell)<=spell.BurstRadius)Fill(CellRect(board,cell),spell.Healing?new Color(.2f,.8f,.3f,.35f):new Color(.85f,.3f,.12f,.35f));}}
 IEnumerator ResolvePointBurst(DirectSpell spell,int caster,int slot){
  Vector2Int center;if(!VisionPoint(out center))yield break;
  bool[] targets={combat.Hp>0&&map.DistanceToCell(false,center)<=spell.BurstRadius&&(!spell.Healing||burstHealMonster),!combat.PlayerDead&&map.DistanceToCell(true,center)<=spell.BurstRadius&&(!spell.Healing||burstHealPlayer)};bool[] failed=new bool[2];
  var summonTargets=SummonsInBurst(center,spell.BurstRadius);if(spell.Healing)summonTargets.RemoveAll(x=>!burstHealSummons.Contains(x.Id));
  var failedSummons=new System.Collections.Generic.HashSet<int>();
  if(!targets[0]&&!targets[1]&&summonTargets.Count==0){history.Insert(0,spell.Chinese+"：范围内没有生物，已消耗动作。");yield break;}
  int dc=8+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(caster)]);
  for(int i=0;i<2;i++)if(targets[i]&&!spell.Healing){
   bool player=i==1,automatic=spell.SaveAbility==1&&(player?combat.PlayerHp==0||combat.PlayerParalyzed:combat.MonsterHelpless),disadvantage=spell.Name=="Shatter"&&!player&&monsterProfile.TypeIndex==3;
   var armorSave=RollArmorSave(player,spell.SaveAbility,disadvantage);int value=armorSave.Value,modifier=player?PlayerSavingThrowModifier(spell.SaveAbility):MonsterSavingThrowModifier(spell.SaveAbility);
   dice=spell.Chinese+" · "+(player?"玩家":"木桩")+PlayerAttributes.Names[spell.SaveAbility]+"豁免";var adjustment=new SaveAdjustment();yield return ApplySaveAdjustments(player,automatic,adjustment,spell.SaveAbility,center);modifier+=adjustment.Value;
   if(!automatic)yield return PresentRoll(20,armorSave.Dice,(disadvantage?"构装生物劣势，采用 ":"采用 ")+value+" + "+modifier+" / DC "+dc);failed[i]=automatic||value+modifier<dc;
  }
  if(!spell.Healing)foreach(var unit in summonTargets){var save=new SummonSave();yield return RollSummonSave(unit,spell.SaveAbility,dc,center,save);if(save.Failed)failedSummons.Add(unit.Id);}
  int[] rolls=new int[spell.DiceCount(classes.TotalLevel,slot)];int damage=0;for(int i=0;i<rolls.Length;i++){rolls[i]=Random.Range(1,spell.Sides+1);damage+=rolls[i];}
  if(spell.Healing){int modifier=AcidSplash.Modifier(attributes[SpellAbilityIndex(caster)]);damage=Mathf.Max(0,damage+modifier);dice=spell.Chinese+" · 共享治疗骰";yield return PresentRoll(spell.Sides,rolls,"骰值 + 施法调整值 "+modifier+" = "+damage+" 治疗");int playerHeal=targets[1]?combat.HealPlayer(damage):0,monsterHeal=targets[0]?combat.HealMonster(damage):0;ApplySteedLifeBond(playerHeal,spell.Level);history.Insert(0,spell.Chinese+"：玩家实际恢复 "+playerHeal+"，木桩实际恢复 "+monsterHeal+"；不能恢复生命的目标仍受原有限制。");foreach(var unit in summonTargets)history.Insert(0,spell.Chinese+"："+unit.Name+"实际恢复 "+unit.Heal(damage)+" HP。");yield break;}
  dice=spell.Chinese+" · 共享范围伤害";yield return PresentRoll(spell.Sides,rolls,"失败伤害 "+damage+"，成功 "+damage/2+"，类型："+spell.DamageType);
  var rayDamage=new RayDamage();yield return ReduceRayDamage(true,damage,spell.DamageType,"范围法术共享伤害",rayDamage);damage=rayDamage.Amount;
        int monsterDamage=targets[0]?combat.AdjustTypedDamage(false,failed[0]?damage:damage/2,DamageTypes.Index(spell.DamageType)):0,playerDamage=targets[1]?combat.AdjustTypedDamage(true,failed[1]?damage:damage/2,DamageTypes.Index(spell.DamageType)):0;
  if(targets[0])combat.ApplyDamage(monsterDamage);if(targets[1])combat.ApplyPlayerDamage(playerDamage,sourcePlayer:true);
  foreach(var unit in summonTargets)ApplySummonSpellDamage(unit,failedSummons.Contains(unit.Id)?damage:damage/2,spell.DamageType);
  history.Insert(0,spell.Chinese+"：木桩 "+monsterDamage+"，玩家 "+playerDamage+" 点"+spell.DamageType+"伤害。");if(targets[1])yield return MaintainConcentration(playerDamage);RefreshBestialSpirit();CheckBattleOutcome();
 }
}
