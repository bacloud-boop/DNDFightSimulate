using System.Collections;
using UnityEngine;
public sealed partial class TrainingGame
{
    sealed class DamageReductionResult {public long[] Raw;}
    DamageReductionAllocation damageAllocation;
    string[] damageAllocationInputs;
    string damageAllocationContext;
    bool damageAllocationConfirmed;
    Vector2 damageAllocationScroll;
    IEnumerator AllocateDamageReduction(long[] raw,int penalty,string context,DamageReductionResult result,int minimumTotal=0)
    {
        var allocation=new DamageReductionAllocation(raw,penalty,minimumTotal);
        if(allocation.Complete){allocation.TryCommit(out result.Raw);yield break;}
        damageAllocation=allocation;damageAllocationContext=context;damageAllocationInputs=new string[DamageTypes.Names.Length];
        for(int i=0;i<damageAllocationInputs.Length;i++)damageAllocationInputs[i]="0";
        damageAllocationConfirmed=false;
        try {while(!damageAllocationConfirmed)yield return null;allocation.TryCommit(out result.Raw);}
        finally {damageAllocation=null;damageAllocationInputs=null;}
    }
    void DrawDamageReductionAllocation()
    {
        if(damageAllocation==null)return;
        Fill(new Rect(0,0,1600,900),new Color(0,0,0,.93f));Surface(new Rect(445,110,710,710),paperTexture);
        GUILayout.BeginArea(new Rect(475,140,650,650));GUILayout.Label("伤害减值 · 分配抵扣",heading);
        GUILayout.Label(damageAllocationContext+"\n必须抵扣合计 "+damageAllocation.Required+"。先按类型抵扣，再结算抗性／免疫；每类最低为0。",label);
        damageAllocationScroll=GUILayout.BeginScrollView(damageAllocationScroll,GUILayout.Height(420));
        var values=new int[DamageTypes.Names.Length];bool valid=true;
        for(int i=0;i<values.Length;i++)if(damageAllocation.Damage(i)>0){
            GUILayout.Label(DamageTypes.Names[i]+" · 可抵扣伤害 "+damageAllocation.Damage(i),small);
            damageAllocationInputs[i]=IntegerField(damageAllocationInputs[i],false);
            if(!int.TryParse(damageAllocationInputs[i],out values[i]))valid=false;
        }
        GUILayout.EndScrollView();valid=valid&&damageAllocation.TryAssign(values);
        GUILayout.Label(valid?"已分配 "+damageAllocation.Assigned+" / "+damageAllocation.Required:"分配无效：各类须为非负整数，不超过该类伤害，总额不超过本次减值。",small);
        bool previous=GUI.enabled;GUI.enabled=valid&&damageAllocation.Complete;
        if(GUILayout.Button("确认抵扣",button))damageAllocationConfirmed=true;
        GUI.enabled=previous;GUILayout.EndArea();
    }
}
