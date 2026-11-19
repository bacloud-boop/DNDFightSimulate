using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    sealed class WeightDraft
    {
        public readonly BodyWeight Weight;
        public string Body,Equipment="0",Message="";
        public WeightDraft(bool unknown){Weight=new BodyWeight(unknown?(int?)null:150);Body=unknown?"":"150";}
    }
    readonly WeightDraft playerWeight=new WeightDraft(false),monsterWeight=new WeightDraft(false);
    readonly Dictionary<string,WeightDraft> beastWeights=new Dictionary<string,WeightDraft>();
    readonly CorpseCarryingState playerCorpseLoad=new CorpseCarryingState(),monsterCorpseLoad=new CorpseCarryingState();
    long PlayerEquipmentWeight=>PlayerInBeastForm?0:playerWeight.Weight.GearOunces;
    long MonsterEquipmentWeight=>MonsterPolymorphed?0:monsterWeight.Weight.GearOunces;
    static int BeastSizeIndex(BeastRecord form)=>form.size=="Tiny"?0:form.size=="Small"?1:form.size=="Large"?3:form.size=="Huge"?4:form.size=="Gargantuan"?5:2;
    bool CanKeepCorpseInForm(bool player,BeastRecord form,bool keepSizeChange=false)=>(player?playerCorpseLoad:monsterCorpseLoad).FitsForm(form.abilities[0],keepSizeChange&&sizeChange.Applies(player)?SizeChangeEffect.ChangedSize(BeastSizeIndex(form),sizeChange.Delta):BeastSizeIndex(form),player?playerWeight.Weight.EquipmentPounds:monsterWeight.Weight.EquipmentPounds,true);
    WeightDraft BeastWeight(string name){WeightDraft value;if(!beastWeights.TryGetValue(name,out value)){value=new WeightDraft(true);beastWeights.Add(name,value);}return value;}
    void DrawWeightInputs(WeightDraft draft,bool beast=false)
    {
        bool old=GUI.enabled;GUI.enabled=old&&(!combat.BattleStarted||!draft.Weight.Known);
        GUILayout.Label(beast?"形态体重（整数磅，未设置时不能搬运）":"体重（整数磅，至少1）",small);draft.Body=IntegerField(draft.Body,false);
        if(!beast){GUILayout.Label("其他装备重量（整数磅，不含护甲、盾牌和武器）",small);draft.Equipment=IntegerField(draft.Equipment,false);GUILayout.Label("护甲与盾牌 "+draft.Weight.ArmorPounds+" 磅 · 回收背包装备 "+WeightUnits.Format(draft.Weight.StoredGearOunces)+" · 装备合计 "+WeightUnits.Format(draft.Weight.GearOunces),small);}
        if(GUILayout.Button("应用重量资料",button))draft.Message=!WeightConfigurationFits(draft,beast)?"拒绝配置：装备重量超出携带上限，或输入无效。":draft.Weight.TryApply(draft.Body,beast?"0":draft.Equipment)?"重量已保存。":"体重须为正整数，装备重量须为非负整数。";
        GUI.enabled=old;if(draft.Message!="")GUILayout.Label(draft.Message,small);
    }
    void DrawBeastWeight(BeastRecord beast){if(beast!=null){DrawWeightInputs(BeastWeight(beast.name),true);GUILayout.Label("融入装备暂不计负重，复原恢复；搬运的遗体不融入。",small);}}
}
