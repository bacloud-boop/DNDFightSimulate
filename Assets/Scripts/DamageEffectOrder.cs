using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    bool drainingDamageEffects,damageOrderPending;int damageOrderChoice=-1;
    readonly List<int> damageOrderKeys=new List<int>();readonly List<string> damageOrderLabels=new List<string>();
    bool PendingDamageEffects=>laughterSaves.Count>0||undeadSaves.Count>0||hellishDamage.Count>0||bondConcentration.Count>0;
    IEnumerator DrainDamageEffects()
    {
        if(drainingDamageEffects||!PendingDamageEffects)yield break;
        bool busy=rolling;string previous=dice;drainingDamageEffects=true;rolling=true;
        try{while(PendingDamageEffects){
            // Reduction and fortitude rolls are already resolved numerically;
            // show those first. Only unresolved consequences need an order choice.
            yield return DrainResistanceRolls();yield return DrainUndeadSaves();
            damageOrderKeys.Clear();damageOrderLabels.Clear();
            while(laughterSaves.Count>0&&(laughterSaves.Peek().Revision!=laughter.Revision||!Laughing(laughterSaves.Peek().Actor)||!LaughterAlive(laughterSaves.Peek().Actor)))laughterSaves.Dequeue();
            if(laughterSaves.Count>0){damageOrderKeys.Add(0);damageOrderLabels.Add(TransitActorName(laughterSaves.Peek().Actor)+" · 狂笑受伤感知豁免（优势）");}
            if(hellishDamage.Count>0){damageOrderKeys.Add(1);damageOrderLabels.Add("炼狱叱喝 · 反应施法选择");}
            if(bondConcentration.Count>0){damageOrderKeys.Add(2);damageOrderLabels.Add("守护之链转移伤害 · 专注豁免");}
            if(damageOrderKeys.Count==0)continue;int chosen=damageOrderKeys[0];
            if(damageOrderKeys.Count>1){damageOrderChoice=-1;damageOrderPending=true;try{while(damageOrderChoice<0)yield return null;chosen=damageOrderKeys[damageOrderChoice];}finally{damageOrderPending=false;}}
            if(chosen==0){var save=laughterSaves.Dequeue();yield return RepeatLaughterSave(save.Actor,save.Revision,true);}
            else if(chosen==1)yield return DrainHellishRebukes();else yield return DrainBondConcentration();
        }}finally{damageOrderPending=false;drainingDamageEffects=false;rolling=busy;dice=previous;}
    }
    void DrawDamageEffectOrder()
    {
        Fill(new Rect(0,0,1600,900),new Color(0,0,0,.93f));Surface(new Rect(440,210,720,480),paperTexture);
        GUILayout.BeginArea(new Rect(470,240,660,420));GUILayout.Label("受伤同时效果 · 选择顺序",heading);
        for(int i=0;i<damageOrderLabels.Count;i++)if(GUILayout.Button(damageOrderLabels[i],button))damageOrderChoice=i;
        GUILayout.EndArea();
    }
}
