using UnityEngine;
using System.Collections;

public sealed partial class TrainingGame
{
    readonly int[] hitDiceSpent=new int[PlayerClasses.Names.Length];
    bool shortRestReady,hasLongRest;
    long worldRounds,lastLongRestStart;
    string restMessage="";
    Vector2 restScroll;
    bool OutsideCombat => (!combat.BattleStarted||combat.BattleFinished)&&!combat.PlayerDead&&!rolling&&!spellTargeting&&!reactionPending&&draggingToken<0;
    void ResetRestResources(){System.Array.Clear(hitDiceSpent,0,hitDiceSpent.Length);shortRestReady=false;}
    void DrawRestPanel()
    {
        Surface(new Rect(350,110,480,745),paperTexture);
        if(Btn(new Rect(720,125,90,46),"关闭")){panel="";return;}
        GUILayout.BeginArea(new Rect(375,185,430,630));restScroll=GUILayout.BeginScrollView(restScroll);
        GUILayout.Label("休息 · 战斗外",heading);
        GUILayout.Label("短休 1 小时；长休 8 小时。活动战斗需先结束或重置。",label);
        bool previous=GUI.enabled;GUI.enabled=OutsideCombat;
        if(GUILayout.Button("完成短休",button)){worldRounds+=600;RefreshAidEffects();combat.ClearShortDurationEffects();UpdateStableRecovery();fighter.ShortRest();druid.ShortRest();RefreshWildShape();shortRestReady=true;restMessage="短休完成：回气恢复 1 次，动作如潮恢复全部，荒野形态恢复1次；可逐颗消耗生命骰治疗。";}
        bool allowed=!hasLongRest||worldRounds-lastLongRestStart>=14400;
        GUI.enabled=OutsideCombat&&allowed&&combat.PlayerHp>0;
        if(GUILayout.Button("完成长休",button)){lastLongRestStart=worldRounds;hasLongRest=true;worldRounds+=4800;RefreshAidEffects();combat.ClearShortDurationEffects();fighter.LongRest();druid.LongRest();EndWildShape("长休");combat.ClearPlayerTemporaryHp();knownFormReplacement=true;combat.HealPlayer(int.MaxValue);ResetRestResources();restMessage="长休完成：生命、生命骰及已接入的长休资源恢复。";}
        GUI.enabled=previous;
        if(!allowed)GUILayout.Label("距下一次长休尚需 "+((14400-(worldRounds-lastLongRestStart)+599)/600)+" 小时（可通过短休推进时间）。",label);
        GUILayout.Label(restMessage,label);
        for(int i=0;i<PlayerClasses.Names.Length;i++)if(classes[i]>0){
            int index=i;GUI.enabled=OutsideCombat&&shortRestReady&&hitDiceSpent[i]<classes[i];
            if(GUILayout.Button(PlayerClasses.Names[i]+" · 生命骰 d"+classes.HitDie(i)+" 剩 "+System.Math.Max(0,classes[i]-hitDiceSpent[i])+" / "+classes[i],button)){
                StartCoroutine(SpendRestDie(index));
            }
            GUI.enabled=previous;
        }
        GUILayout.EndScrollView();GUILayout.EndArea();
    }
    IEnumerator SpendRestDie(int index)
    {
        rolling=true;hitDiceSpent[index]++;
        int value=Random.Range(1,classes.HitDie(index)+1),modifier=AcidSplash.Modifier(attributes[2]);
        restMessage="生命骰 "+value+" + 体质 "+modifier+" = "+Mathf.Max(0,value+modifier)+"（结果停留2秒）";
        yield return new WaitForSecondsRealtime(2);
        int actual=combat.HealPlayer(Mathf.Max(0,value+modifier));restMessage+="，实际恢复 "+actual+"。";
        rolling=false;
    }}
