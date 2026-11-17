using UnityEngine;
public sealed partial class TrainingGame {
 sealed class WeightDraft {public readonly BodyWeight Weight=new BodyWeight();public string Body="150",Equipment="0",Message="";}
 readonly WeightDraft playerWeight=new WeightDraft(),monsterWeight=new WeightDraft();
 long PlayerEquipmentWeight=>PlayerInBeastForm?0:playerWeight.Weight.GearPounds;
 long MonsterEquipmentWeight=>MonsterPolymorphed?0:monsterWeight.Weight.GearPounds;
 static int BeastSizeIndex(BeastRecord form)=>form.size=="Tiny"?0:form.size=="Small"?1:form.size=="Large"?3:form.size=="Huge"?4:form.size=="Gargantuan"?5:2;
 bool ConfiguredLoadAllowed(bool player,string[] values,int size){int strength;return values!=null&&values.Length==6&&int.TryParse(values[0],out strength)&&EquipmentLoadRules.Allowed(strength,size,(player?playerWeight:monsterWeight).Weight.GearPounds,0);}
 void DrawWeightInputs(WeightDraft draft){
  GUILayout.Label("体重（整数磅，至少1）",small);draft.Body=IntegerField(draft.Body,false);GUILayout.Label("装备总重量（整数磅，不含自身体重）",small);draft.Equipment=IntegerField(draft.Equipment,false);
  if(GUILayout.Button("应用重量资料",button)){bool player=object.ReferenceEquals(draft,playerWeight);int weight;if(!int.TryParse(draft.Equipment,out weight)||!EquipmentLoadRules.Allowed(player?attributes[0]:monsterAttributes[0],player?2:monsterProfile.SizeIndex,weight,0))draft.Message="装备超出携带上限，或重量输入无效。";else draft.Message=draft.Weight.TryApply(draft.Body,draft.Equipment)?"已保存重量资料。":"体重须为正整数，装备重量须为非负整数。";}GUILayout.Label(draft.Message,small);
 }
}
