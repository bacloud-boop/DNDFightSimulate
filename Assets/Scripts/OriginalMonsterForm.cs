using System;
using System.Linq;
using UnityEngine;
public sealed partial class TrainingGame {
 MonsterFormData originalMonster=new MonsterFormData(),originalDraft;
 string[] originalAbilityInputs,originalStatInputs,originalDamageInputs;string originalMessage="";Vector2 originalScroll;
 void BeginOriginalDraft(MonsterFormData form){originalDraft=form.Copy();originalAbilityInputs=form.Abilities.Select(v=>v.ToString()).ToArray();originalStatInputs=new[]{form.MaxHp,form.Armor,form.Proficiency,form.AttackRange,form.Speed,form.Darkvision}.Select(v=>v.ToString()).ToArray();originalDamageInputs=form.Dice.Concat(new[]{form.Bonus}).Select(v=>v.ToString()).ToArray();}
 MonsterFormData CurrentMonsterForm(){var form=new MonsterFormData{Type=monsterProfile.TypeIndex==13?9:monsterProfile.TypeIndex,Size=monsterProfile.SizeIndex,MaxHp=combat.MonsterBaseMaxHp,Armor=combat.MonsterBaseArmorClass,Proficiency=combat.MonsterProficiencyBonus,AttackRange=combat.MonsterAttackRange,Speed=combat.MonsterBaseSpeed,Darkvision=vision.MonsterRange,Melee=combat.MonsterAttackIsMelee,Bonus=monster.Bonus};for(int i=0;i<6;i++)form.Abilities[i]=monsterAttributes[i];Array.Copy(monsterSkillTiers,form.Skills,18);Array.Copy(monster.Counts,form.Dice,5);Array.Copy(monster.Types,form.DamageTypes,6);return form;}
 void DrawOriginalMonster(){
  if(originalDraft==null)BeginOriginalDraft(originalMonster);Surface(new Rect(770,110,480,745),paperTexture);if(Btn(new Rect(1140,125,90,46),"关闭")){panel="";return;}
  GUILayout.BeginArea(new Rect(790,185,440,635));originalScroll=GUILayout.BeginScrollView(originalScroll);GUILayout.Label("木桩生前形态 · 独立资料",heading);GUILayout.Label("仅在亡灵接受完全复生术时恢复；其他复活不切换形态。首次操作后锁定。",small);
  bool old=GUI.enabled;GUI.enabled=!combat.BattleStarted;
  if(GUILayout.Button("复制当前基础配置到草稿",button)){BeginOriginalDraft(CurrentMonsterForm());originalMessage="已复制基础数值；若当前是亡灵，生前类型暂置为类人生物，请确认后应用。";}
  GUILayout.Label("生前类型（不允许亡灵）",small);originalDraft.Type=GUILayout.SelectionGrid(originalDraft.Type,CreatureProfile.TypeNames.Take(13).ToArray(),3,new GUIStyle(button){fontSize=15,fixedHeight=32});
  GUILayout.Label("生前体型",small);originalDraft.Size=GUILayout.SelectionGrid(originalDraft.Size,CreatureProfile.SizeNames,3,new GUIStyle(button){fontSize=15,fixedHeight=32});
  for(int i=0;i<6;i++){GUILayout.Label(PlayerAttributes.Names[i],small);originalAbilityInputs[i]=IntegerField(originalAbilityInputs[i],false);}
  string[] names={"最大HP（至少1）","护甲等级（5～30）","熟练加值（可负）","攻击距离（尺）","移动力（尺）","黑暗视觉（尺）"};for(int i=0;i<6;i++){GUILayout.Label(names[i],small);originalStatInputs[i]=IntegerField(originalStatInputs[i],i==2);}
  originalDraft.Melee=GUILayout.SelectionGrid(originalDraft.Melee?0:1,new[]{"近战攻击","远程攻击"},2,button)==0;
  for(int i=0;i<6;i++){GUILayout.Label(i<5?"d"+MonsterDamage.Sides[i]+"数量":"固定伤害加值",small);originalDamageInputs[i]=IntegerField(originalDamageInputs[i],i==5);originalDraft.DamageTypes[i]=GUILayout.SelectionGrid(originalDraft.DamageTypes[i],DamageTypes.Names,4,new GUIStyle(button){fontSize=14,fixedHeight=30});}
  for(int i=0;i<18;i++){GUILayout.Label(SkillCheckRules.Names[i],small);originalDraft.Skills[i]=GUILayout.SelectionGrid(originalDraft.Skills[i],new[]{"无熟练","熟练","专精"},3,new GUIStyle(button){fontSize=15,fixedHeight=30});}
  if(GUILayout.Button("应用生前形态资料",button)){
   var form=originalDraft.Copy();var stats=new int[6];bool valid=true;
   for(int i=0;i<6;i++){int value;if(!int.TryParse(originalAbilityInputs[i],out value))valid=false;else form.Abilities[i]=value;if(!int.TryParse(originalStatInputs[i],out stats[i]))valid=false;if(!int.TryParse(originalDamageInputs[i],out value))valid=false;else if(i<5)form.Dice[i]=value;else form.Bonus=value;}
   form.MaxHp=stats[0];form.Armor=stats[1];form.Proficiency=stats[2];form.AttackRange=stats[3];form.Speed=stats[4];form.Darkvision=stats[5];string error="所有输入必须为有效整数。";
   if(valid&&form.Valid(out error)){originalMonster=form;originalMessage="生前资料已独立保存，未改变当前木桩。";}else originalMessage=error;
  }
  GUI.enabled=old;GUILayout.Label(originalMessage,small);GUILayout.EndScrollView();GUILayout.EndArea();
 }
 void DrawOriginalFormPreview(Rect board){
  if(panel!="aftermath"||!monsterCorpse.Dead||monsterCorpse.Type!=13)return;
  int spaces=CreatureProfile.SpacesForSize(originalMonster.Size);for(int y=0;y<map.Size;y++)for(int x=0;x<map.Size;x++)if(x>=map.Monster.x&&x<map.Monster.x+spaces&&y>=map.Monster.y&&y<map.Monster.y+spaces)Fill(CellRect(board,new Vector2Int(x,y)),CanRestoreOriginalMonster?new Color(.2f,.7f,.4f,.4f):new Color(.9f,.1f,.1f,.5f));
 }
 bool CanRestoreOriginalMonster=>map.CanRestoreMonsterFootprint(CreatureProfile.SpacesForSize(originalMonster.Size));
 bool RestoreOriginalMonster(){
  if(!CanRestoreOriginalMonster)return false;
  var form=originalMonster;map.RestoreMonsterFootprint(CreatureProfile.SpacesForSize(form.Size));monsterProfile.TryApply(form.Type,form.Size);monsterAttributes.TryApply(form.Abilities.Select(v=>v.ToString()).ToArray());
  combat.RestoreMonsterBase(form.MaxHp,form.Armor,form.Proficiency,form.AttackRange,form.Speed,form.Melee);vision.MonsterRange=form.Darkvision;monsterVisionInput=form.Darkvision.ToString();
  monsterTypeDraft=form.Type;monsterSizeDraft=form.Size;monsterAttackKindDraft=form.Melee?0:1;monsterHpInput=form.MaxHp.ToString();monsterArmorInput=form.Armor.ToString();monsterProficiencyInput=form.Proficiency.ToString();monsterRangeInput=form.AttackRange.ToString();monsterSpeedInput=form.Speed.ToString();
  for(int i=0;i<6;i++){monsterAttributeInputs[i]=form.Abilities[i].ToString();damageTypeInputs[i]=monster.Types[i]=form.DamageTypes[i];damageInputs[i]=(i<5?form.Dice[i]:form.Bonus).ToString();}Array.Copy(form.Dice,monster.Counts,5);monster.Bonus=form.Bonus;Array.Copy(form.Skills,monsterSkillTiers,18);Array.Copy(form.Skills,monsterSkillDraft,18);monsterSkillsDirty=false;
  history.Insert(0,"完全复生术：恢复已保存的完整生前基础资料，保持原占地左上格；当前类型 "+monsterProfile.Label+"。");return true;
 }
}
