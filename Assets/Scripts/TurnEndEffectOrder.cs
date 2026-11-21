using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    bool unitEndOrderPending;int unitEndOrderChoice=-1,unitEndOrderActor;string[] unitEndOrderLabels;
    void DrawUnitEndOrder()
    {
        if(!unitEndOrderPending)return;Fill(new Rect(0,0,1600,900),new Color(0,0,0,.92f));Surface(new Rect(465,220,670,440),paperTexture);
        GUI.Label(new Rect(495,245,610,55),TransitActorName(unitEndOrderActor)+"回合末 · 选择结算顺序",heading);
        for(int i=0;i<unitEndOrderLabels.Length;i++)if(GUI.Button(new Rect(495,320+i*85,610,65),unitEndOrderLabels[i],button))unitEndOrderChoice=i;
    }
    IEnumerator FinishUnitEndEffects(int actor,SummonedUnit unit=null)
    {
        bool sleep=unit!=null&&unit.Alive&&unit.SleepStage==1&&combat.Round>=unit.SleepRepeatRound;
        var greaseEnd=ActorGrease(actor);bool cloud=DaggersEndAvailable(actor);
        while(sleep||greaseEnd!=null||cloud){
            if(actor==BattleMap.RescueId?rescueCarrier.Dead:unit==null||!unit.Alive)yield break;
            var keys=new List<int>();var names=new List<string>();if(sleep){keys.Add(0);names.Add("睡眠术 · 感知豁免");}if(greaseEnd!=null){keys.Add(1);names.Add("油腻术 · 敏捷豁免");}if(cloud){keys.Add(2);names.Add("匕首之云 · 挥砍伤害");}
            int choice=keys[0];if(keys.Count>1){unitEndOrderActor=actor;unitEndOrderLabels=names.ToArray();unitEndOrderChoice=-1;unitEndOrderPending=true;while(unitEndOrderChoice<0)yield return null;choice=keys[unitEndOrderChoice];unitEndOrderPending=false;}
            if(choice==0){sleep=false;yield return RepeatSummonSleep(unit);}else if(choice==1){var field=greaseEnd;greaseEnd=null;yield return ResolveGreaseSave(actor,field,"回合末油脂");}else{cloud=false;yield return EndDaggersTurn(actor);}
        }
    }
}
