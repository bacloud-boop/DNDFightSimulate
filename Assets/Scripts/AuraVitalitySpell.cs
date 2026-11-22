using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly AuraVitalityEffect auraVitality=new AuraVitalityEffect();
    readonly Queue<long> auraStarts=new Queue<long>();
    bool auraChoicePending,auraOrderPending,auraDecided,auraSkip,auraFirst;
    int auraSelected,auraOrderChoice;
    Vector2 auraScroll;
    bool AuraVitalityTarget(int actor)
    {
        Vector2Int cell;int spaces;string name;
        if(!HealingHandsTarget(actor,out cell,out spaces,out name))return false;
        if(actor==0)return true;
        if((actor==0||actor==-1)&&map.IsContained(actor==0))return false;
        if(map.IsContained(true)&&actor!=-1)return false;
        return BattleMap.BodyDistance(map.Player,map.PlayerSpaces,cell,spaces)<=30;
    }
    string AuraVitalityError()=>worldRounds<0||worldRounds>long.MaxValue-10?"世界时间超出范围。":"";
    void DrawAuraVitalityChoices()
    {
        GUILayout.Label("创建灵光时与自己每次回合开始时，可选择30尺光环内一名生物治疗2d6，也可跳过。无需看见目标；可治疗自己、盟友或敌人。专注至多1分钟，升环不增强。不消耗后续动作或附赠动作。",small);
    }
    void EndAuraVitality()
    {
        auraVitality.Clear();auraStarts.Clear();auraChoicePending=auraOrderPending=false;
    }
    IEnumerator CastAuraVitality()
    {
        if(!auraVitality.Begin(worldRounds,terrainTurn))yield break;
        concentrationSpell="活力灵光";concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=auraVitality.Expires;
        history.Insert(0,"活力灵光：30尺光环，专注至多1分钟；创建时及每个自身回合开始，可治疗一名范围内生物2d6。");
        yield return ResolveAuraVitalityHealing(auraVitality.Revision);
    }
    void QueueAuraVitalityStart(int actor)
    {
        if(actor==0&&auraVitality.ClaimStart(terrainTurn,worldRounds))auraStarts.Enqueue(auraVitality.Revision);
    }
    IEnumerator ChooseAuraVitalityOrder()
    {
        auraFirst=true;
        if(auraStarts.Count==0||greaseEntries.Count+spikeEntries.Count+webEntries.Count+daggerEntries.Count==0)yield break;
        auraOrderChoice=0;auraOrderPending=true;
        try{while(auraOrderChoice==0&&auraStarts.Count>0)yield return null;auraFirst=auraOrderChoice!=2;}
        finally{auraOrderPending=false;}
    }
    void DrawAuraVitalityOrder()
    {
        Fill(new Rect(0,0,1600,900),new Color(0,0,0,.92f));Surface(new Rect(465,245,670,380),paperTexture);
        GUI.Label(new Rect(495,270,610,60),"回合开始 · 选择效果先后",heading);
        if(GUI.Button(new Rect(495,365,610,65),"先选择活力灵光治疗",button))auraOrderChoice=1;
        if(GUI.Button(new Rect(495,455,610,65),"先结算地形／区域效果",button))auraOrderChoice=2;
    }
    void DrawAuraVitalityHealingChoice()
    {
        Fill(new Rect(0,0,1600,900),new Color(0,0,0,.92f));Surface(new Rect(350,150,900,650),paperTexture);
        GUILayout.BeginArea(new Rect(380,180,380,580));
        GUILayout.Label("活力灵光 · 选择治疗目标",heading);
        GUILayout.Label("范围内一名生物恢复2d6生命；也可跳过本次机会。不会解除睡眠术或假死术等独立状态。",small);
        auraScroll=GUILayout.BeginScrollView(auraScroll);
        foreach(int actor in BreathActors())if(AuraVitalityTarget(actor)){
            int hp=actor==0?combat.PlayerHp:actor==-1?combat.Hp:actor==BattleMap.RescueId?rescueCarrier.Hp:FindSummon(actor).Hp;
            if(GUILayout.Button(TransitActorName(actor)+" · 当前HP "+hp,button)){auraSelected=actor;auraSkip=false;auraDecided=true;}
        }
        if(GUILayout.Button("跳过本次治疗",button)){auraSkip=true;auraDecided=true;}
        GUILayout.EndScrollView();GUILayout.EndArea();
        DrawMap(new Rect(800,250,400,400),false);
        GUI.Label(new Rect(800,680,400,70),"光环随玩家移动，30尺内无需可见；吞咽仍隔离内外。",small);
    }
    IEnumerator ResolveAuraVitalityHealing(long revision)
    {
        if(revision!=auraVitality.Revision||!auraVitality.Active(worldRounds)||concentrationSpell!="活力灵光")yield break;
        bool oldBusy=rolling;string oldPanel=panel;rolling=true;panel="";auraDecided=auraSkip=false;auraChoicePending=true;
        try{
            while(!auraDecided&&revision==auraVitality.Revision&&auraVitality.Active(worldRounds)&&concentrationSpell=="活力灵光")yield return null;
            auraChoicePending=false;
            if(revision!=auraVitality.Revision||!auraVitality.Active(worldRounds)||concentrationSpell!="活力灵光")yield break;
            if(auraSkip){history.Insert(0,"活力灵光：跳过本次治疗机会。");yield break;}
            int target=auraSelected;
            if(!AuraVitalityTarget(target)){history.Insert(0,"活力灵光：目标已不在合法范围，本次机会结束。");yield break;}
            int a=Random.Range(1,7),b=Random.Range(1,7),amount=a+b;
            dice="活力灵光 · 2d6治疗";yield return PresentRoll(6,new[]{a,b},"2d6 = "+amount+" 治疗，不附加属性调整值");
            if(revision!=auraVitality.Revision||!auraVitality.Active(worldRounds)||!AuraVitalityTarget(target))yield break;
            int rolled=amount;amount=RollHealingAmount(target,amount,12L);
            int actual=target==0?combat.HealPlayer(amount):target==-1?combat.HealMonster(amount):target==BattleMap.RescueId?rescueCarrier.Heal(amount):FindSummon(target).Heal(amount);
            if(target==0)ApplySteedLifeBond(actual,3);
            history.Insert(0,"活力灵光："+TransitActorName(target)+"治疗骰 "+rolled+(amount!=rolled?"；希望信标按最大值 "+amount+" 治疗":"")+"，实际恢复 "+actual+" HP。");
            dice="活力灵光 · 实际治疗 "+actual;ObserveDeaths();RefreshConcentration();
        }finally{auraChoicePending=false;panel=oldPanel;rolling=oldBusy;}
    }
    void DrawAuraVitalityArea(Rect board)
    {
        if(!(panel=="spells"&&selectedSpellName=="Aura of Vitality")&&!auraChoicePending)return;
        if(map.IsContained(true))return;
        for(int y=0;y<map.Size;y++)for(int x=0;x<map.Size;x++)if(BattleMap.BodyDistance(map.Player,map.PlayerSpaces,new Vector2Int(x,y),1)<=30)Fill(CellRect(board,new Vector2Int(x,y)),new Color(.3f,.8f,.45f,.18f));
    }
}
