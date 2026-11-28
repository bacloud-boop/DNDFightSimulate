using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    sealed class StartStatus {public int Actor,Kind;public long Revision,Group;public Vector2Int Origin;public int Spaces;}
    readonly List<StartStatus> startStatuses=new List<StartStatus>();
    bool startStatusOrderPending,startStatusFirst;int startStatusChoice;Vector2 startStatusScroll;
    int OtherTerrainCount=>packEntries.Count+greaseEntries.Count+spikeEntries.Count+webEntries.Count+daggerEntries.Count+heroismEntries.Count+auraStarts.Count;
    void QueueSearingStart(int actor){if(searingSmite.ClaimStart(actor,terrainTurn,worldRounds))startStatuses.Add(new StartStatus{Actor=actor,Kind=1});}
    IEnumerator ChooseStartStatusOrder()
    {
        startStatusFirst=true;startStatusChoice=0;
        if(startStatuses.Count+ (OtherTerrainCount>0?1:0)<=1)yield break;
        startStatusChoice=-1;startStatusOrderPending=true;
        try{while(startStatusChoice<0)yield return null;startStatusFirst=startStatusChoice<startStatuses.Count||OtherTerrainCount==0;}
        finally{startStatusOrderPending=false;}
    }
    void DrawStartStatusOrder()
    {
        Surface(new Rect(440,150,720,600),paperTexture);GUILayout.BeginArea(new Rect(470,180,660,540));
        GUILayout.Label("持续效果 · 选择结算顺序",heading);startStatusScroll=GUILayout.BeginScrollView(startStatusScroll);
        for(int i=0;i<startStatuses.Count;i++)if(GUILayout.Button(TransitActorName(startStatuses[i].Actor)+(startStatuses[i].Kind==7?" · 塔莎狂笑术回合末感知豁免":startStatuses[i].Kind==6?" · 月华之光体质豁免与光耀":startStatuses[i].Kind==5?" · 造风术力量豁免与推离":startStatuses[i].Kind==4?" · 臭云术体质豁免":startStatuses[i].Kind==3?" · 灵体卫士感知豁免与伤害":startStatuses[i].Kind==2?" · 雪雨暴敏捷豁免":startStatuses[i].Kind==0?" · 再生术恢复1HP":" · 炽焰斩火焰与豁免"),button))startStatusChoice=i;
        if(OtherTerrainCount>0&&GUILayout.Button("先结算其他治疗／地形效果",button))startStatusChoice=startStatuses.Count;
        GUILayout.EndScrollView();GUILayout.EndArea();
    }
    IEnumerator ResolveStartStatus()
    {
        if(startStatuses.Count==0)yield break;int index=startStatusChoice>=0&&startStatusChoice<startStatuses.Count?startStatusChoice:0;
        var entry=startStatuses[index];startStatuses.RemoveAt(index);
        if(entry.Kind==7){yield return RepeatLaughterSave(entry.Actor,entry.Revision);yield break;}
        if(entry.Kind==6){yield return ResolveMoonbeamSave(entry.Actor,entry.Revision,entry.Group);yield break;}
        if(entry.Kind==5){yield return ResolveGustSave(entry.Actor,entry.Revision);yield break;}
        if(entry.Kind==4){yield return ResolveStinkingSave(entry.Actor,entry.Revision);yield break;}
        if(entry.Kind==3){yield return ResolveGuardianSave(entry.Actor,entry.Revision,entry.Origin,entry.Spaces);yield break;}
        if(entry.Kind==2){yield return ResolveSleetSave(entry.Actor,entry.Revision);yield break;}
        if(entry.Kind==1){yield return ResolveSearingStart(entry.Actor);yield break;}
        if(!regeneration.Active(entry.Actor,worldRounds))yield break;
        int actual=HealRegenerationTarget(entry.Actor,1);if(entry.Actor==0)ApplySteedLifeBond(actual,7);
        if(actual>0)history.Insert(0,"再生术："+TransitActorName(entry.Actor)+"回合开始恢复1HP。");
    }
}
