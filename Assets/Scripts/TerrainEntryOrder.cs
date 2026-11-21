using System.Collections;
using UnityEngine;
public sealed partial class TrainingGame
{
    long terrainMovementEvent;
    bool terrainEntryOrderPending;int terrainEntryOrderChoice,terrainEntryOrderActor;
    void DrawTerrainEntryOrder()
    {
        if(!terrainEntryOrderPending)return;Fill(new Rect(0,0,1600,900),new Color(0,0,0,.92f));Surface(new Rect(465,245,670,380),paperTexture);
        GUI.Label(new Rect(495,270,610,60),TransitActorName(terrainEntryOrderActor)+" · 同时进入地形效果",heading);
        if(GUI.Button(new Rect(495,365,610,65),"先结算油腻术敏捷豁免",button))terrainEntryOrderChoice=1;
        if(GUI.Button(new Rect(495,455,610,65),"先结算匕首之云伤害",button))terrainEntryOrderChoice=2;
    }
    bool terrainDaggersFirst;
    IEnumerator ChooseDaggersEntryOrder()
    {
        terrainDaggersFirst=false;if(greaseEntries.Count==0||daggerEntries.Count==0)yield break;
        var g=greaseEntries.Peek();var d=daggerEntries.Peek();if(d.MoveEvent<0||!d.Actors.Contains(g.Actor))yield break;
        if(d.MoveEvent!=g.MoveEvent){terrainDaggersFirst=d.MoveEvent<g.MoveEvent;yield break;}
        if(d.MoveStep!=g.MoveStep){terrainDaggersFirst=d.MoveStep<g.MoveStep;yield break;}
        terrainEntryOrderActor=g.Actor;terrainEntryOrderChoice=0;terrainEntryOrderPending=true;try{while(terrainEntryOrderChoice==0)yield return null;terrainDaggersFirst=terrainEntryOrderChoice==2;}finally{terrainEntryOrderPending=false;}
    }
}
