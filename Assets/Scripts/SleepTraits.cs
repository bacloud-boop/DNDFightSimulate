using UnityEngine;
public sealed partial class TrainingGame
{
 bool playerNoSleep,playerExhaustionImmune,monsterNoSleep,monsterExhaustionImmune;
 bool playerNoSleepDraft,playerExhaustionImmuneDraft,monsterNoSleepDraft,monsterExhaustionImmuneDraft;
 void DrawSleepTraitConfiguration(bool player)
 {
  if(player){playerNoSleepDraft=GUILayout.Toggle(playerNoSleepDraft,"无需睡眠",label);playerExhaustionImmuneDraft=GUILayout.Toggle(playerExhaustionImmuneDraft,"力竭免疫",label);}
  else {monsterNoSleepDraft=GUILayout.Toggle(monsterNoSleepDraft,"无需睡眠",label);monsterExhaustionImmuneDraft=GUILayout.Toggle(monsterExhaustionImmuneDraft,"力竭免疫",label);}
 }
 void ApplySleepTraitConfiguration(bool player)
 {
  if(player){playerNoSleep=playerNoSleepDraft;playerExhaustionImmune=playerExhaustionImmuneDraft;}
  else {monsterNoSleep=monsterNoSleepDraft;monsterExhaustionImmune=monsterExhaustionImmuneDraft;}
 }
 bool EffectiveNoSleep(bool player){var beast=player?(PlayerInBeastForm?PlayerBeast:null):(MonsterPolymorphed?PolymorphBeast:null);return beast!=null?beast.noSleep:player?playerNoSleep||playerSpecies.NoSleep:monsterNoSleep||monsterSpecies.NoSleep;}
 bool EffectiveExhaustionImmune(bool player){var beast=player?(PlayerInBeastForm?PlayerBeast:null):(MonsterPolymorphed?PolymorphBeast:null);return beast!=null?beast.exhaustionImmune:player?playerExhaustionImmune:monsterExhaustionImmune;}
 bool AutomaticSleepSave(bool player)=>EffectiveNoSleep(player)||EffectiveExhaustionImmune(player);
}
