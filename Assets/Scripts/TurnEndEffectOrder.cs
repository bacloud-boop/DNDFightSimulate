using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    Vector2 unitEndOrderScroll;
    bool unitEndOrderPending;int unitEndOrderChoice=-1,unitEndOrderActor;string[] unitEndOrderLabels;
    void DrawUnitEndOrder()
    {
        if(!unitEndOrderPending)return;Fill(new Rect(0,0,1600,900),new Color(0,0,0,.92f));Surface(new Rect(465,150,670,630),paperTexture);
        GUILayout.BeginArea(new Rect(495,180,610,570));GUILayout.Label(TransitActorName(unitEndOrderActor)+"回合末 · 选择结算顺序",heading);unitEndOrderScroll=GUILayout.BeginScrollView(unitEndOrderScroll);
        for(int i=0;i<unitEndOrderLabels.Length;i++)if(GUILayout.Button(unitEndOrderLabels[i],button))unitEndOrderChoice=i;
        GUILayout.EndScrollView();GUILayout.EndArea();
    }
    IEnumerator FinishUnitEndEffects(int actor,SummonedUnit unit=null)
    {
        bool sleep=unit!=null&&unit.Alive&&unit.SleepStage==1&&combat.Round>=unit.SleepRepeatRound;
        var arrows=acidArrows.Pending(actor);var greaseEnd=ActorGrease(actor);bool cloud=DaggersEndAvailable(actor),sphere=SphereEndAvailable(actor);
        while(sleep||greaseEnd!=null||cloud||sphere||arrows.Count>0){
            if(actor==BattleMap.RescueId?rescueCarrier.Dead:unit==null||!unit.Alive)yield break;
            var keys=new List<long>();var names=new List<string>();if(sleep){keys.Add(0);names.Add("睡眠术 · 感知豁免");}if(greaseEnd!=null){keys.Add(1);names.Add("油腻术 · 敏捷豁免");}if(cloud){keys.Add(2);names.Add("区域回合末效果 · 匕首之云／咒唤兽群");}
            if(sphere){keys.Add(3);names.Add("炽焰法球 · 敏捷豁免与火焰伤害");}
            foreach(var entry in arrows){keys.Add(entry.Id+4);names.Add(entry.Title+"残留 #"+entry.Id+" · "+entry.Dice+"d4强酸");}
            long choice=keys[0];if(keys.Count>1){unitEndOrderActor=actor;unitEndOrderLabels=names.ToArray();unitEndOrderChoice=-1;unitEndOrderPending=true;while(unitEndOrderChoice<0)yield return null;choice=keys[unitEndOrderChoice];unitEndOrderPending=false;}
            if(choice>=5){var entry=arrows.Find(e=>e.Id==choice-4);arrows.Remove(entry);if(entry!=null)yield return ResolveAcidArrowResidue(entry);continue;}
            if(choice==0){sleep=false;yield return RepeatSummonSleep(unit);}else if(choice==1){var field=greaseEnd;greaseEnd=null;yield return ResolveGreaseSave(actor,field,"回合末油脂");}else if(choice==2){cloud=false;yield return EndDaggersTurn(actor);}else{sphere=false;yield return EndFlamingSphereTurn(actor);}
        }
    }
}
