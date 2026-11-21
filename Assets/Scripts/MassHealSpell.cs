using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly Dictionary<int,string> massHealAllocation=new Dictionary<int,string>{{0,"700"},{-1,"0"}};
    void DrawMassHealEntry(int id,string name)
    {
        if(!massHealAllocation.ContainsKey(id))massHealAllocation[id]="0";
        GUILayout.Label(name,small);massHealAllocation[id]=IntegerField(massHealAllocation[id],false);
    }
    void DrawMassHealChoices()
    {
        GUILayout.Label("分配至多700点治疗，填0不选择；所有数值必须为非负整数。仅实际恢复至少1HP的目标解除目盲、耳聋、中毒。",small);
        DrawMassHealEntry(0,"玩家 · HP "+combat.PlayerHp+" / "+combat.PlayerMaxHp);
        DrawMassHealEntry(-1,"木桩 · HP "+combat.Hp+" / "+combat.MaxHp);
        var present=new HashSet<int>{0,-1};
        foreach(var unit in map.Summons)if(unit.Alive){present.Add(unit.Id);DrawMassHealEntry(unit.Id,unit.Name+" · HP "+unit.Hp+" / "+unit.MaxHp);}
        foreach(int id in new List<int>(massHealAllocation.Keys))if(!present.Contains(id))DrawMassHealEntry(id,"已失效目标 #"+id+"（请清零）");
        int total;if(MassHealRules.ValidAllocation(massHealAllocation.Values,out total))GUILayout.Label("已分配 "+total+" / 700，未分配 "+(700-total),small);
        if(GUILayout.Button("清空分配",button))foreach(int id in new List<int>(massHealAllocation.Keys))massHealAllocation[id]="0";
    }
    string MassHealError()
    {
        int total;if(!MassHealRules.ValidAllocation(massHealAllocation.Values,out total))return "每项须为0～700的整数，合计不能超过700。";
        foreach(var pair in massHealAllocation){int amount=int.Parse(pair.Value);if(amount==0)continue;string error=FixedHealingTargetError(pair.Key);if(error!="")return "目标 #"+pair.Key+"："+error;}
        return "";
    }
    void ApplyMassHeal()
    {
        string error=MassHealError();if(error!=""){history.Insert(0,"群体医疗术未生效："+error);return;}
        int playerActual=0,totalActual=0;
        foreach(var pair in massHealAllocation){int amount=int.Parse(pair.Value);if(amount==0)continue;
            int actual=pair.Key==0||pair.Key==-1?MassHealRules.Apply(combat,sensory,pair.Key==0,amount,worldRounds):FindSummon(pair.Key).Heal(amount);
            if(actual>0){feignDeath.EndBlind(pair.Key,worldRounds);SyncFeignDeath();}
            string name=pair.Key==0?"玩家":pair.Key==-1?"木桩":FindSummon(pair.Key).Name;
            if(pair.Key==0)playerActual=actual;totalActual+=actual;
            history.Insert(0,"群体医疗术："+name+"分配 "+amount+"，实际恢复 "+actual+" HP；"+(actual>0?"解除目盲、耳聋与中毒全部施法来源。":"未恢复HP，不解除状态。"));
        }
        // Apply the triggered bond after all primary recipients, so it cannot
        // fill the steed first and invalidate that recipient's condition removal.
        ApplySteedLifeBond(playerActual,9);RefreshConcentration();dice="群体医疗术 · 实际治疗合计 "+totalActual;
    }
}
