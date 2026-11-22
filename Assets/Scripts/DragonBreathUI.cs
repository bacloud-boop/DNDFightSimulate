using System;
using System.Linq;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly BreathChoice dragonChoice=new BreathChoice();Vector2 dragonScroll;int dragonActor;string dragonReturn="";
    bool DragonBreathUIFree(int actor)=>!summonActing&&(!rolling||actor>0&&summonTurns.Current==FindSummon(actor));
    void OpenDragonBreath(int actor,string back=""){dragonActor=actor;dragonReturn=back;OpenPanel("dragonBreath");}
    void DrawDragonBreathActionButton(int actor,string back)
    {
        if(!dragonBreath.Active(worldRounds)||dragonBreath.Target!=actor)return;bool old=GUI.enabled;
        GUI.enabled=old&&DragonBreathUIFree(actor)&&DragonBreathReady(actor);
        if(GUILayout.Button("龙息术喷吐（魔法动作）",button))OpenDragonBreath(actor,back);GUI.enabled=old;
    }
    void DrawDragonBreathPanel()
    {
        Surface(new Rect(350,110,900,745),paperTexture);if(Btn(new Rect(1140,125,90,46),"关闭",DragonBreathUIFree(dragonActor))){panel=dragonReturn;return;}
        Vector2Int source;int n;string name;if(!HealingHandsTarget(dragonActor,out source,out n,out name)){GUI.Label(new Rect(380,200,780,90),"目标已经离场，无法喷吐。",label);return;}
        if(dragonChoice.Offset<0)dragonChoice.Offset=n;dragonChoice.Offset=Mathf.Clamp(dragonChoice.Offset,0,n*2);
        if(dragonChoice.AimX<0){dragonChoice.AimX=source.x+n+2;dragonChoice.AimY=source.y+n*.5;}
        GUILayout.BeginArea(new Rect(375,185,390,630));dragonScroll=GUILayout.BeginScrollView(dragonScroll);
        GUILayout.Label(name+" · 龙息术喷吐",heading);GUILayout.Label(DragonBreathEffect.DiceCount(dragonBreath.Slot)+"d6 "+DamageTypes.Names[dragonBreath.Type]+" · 敏捷DC "+dragonBreath.Dc+"；成功减半。\n消耗1个魔法动作，不是攻击动作或再次施法。",small);
        bool enabled=GUI.enabled;GUI.enabled=enabled&&DragonBreathUIFree(dragonActor);
        dragonChoice.Edge=GUILayout.SelectionGrid(dragonChoice.Edge,new[]{"右边","左边","下边","上边","自身中心"},2,button);
        var offsets=Enumerable.Range(0,n*2+1).Select(i=>i==n?"居中":"位置 "+i).ToArray();
        if(dragonChoice.Edge!=4)dragonChoice.Offset=GUILayout.SelectionGrid(dragonChoice.Offset,offsets,3,button);
        dragonChoice.IncludeOrigin=GUILayout.Toggle(dragonChoice.IncludeOrigin,"将源点纳入范围",label);
        BreathTemplate template;bool valid=BreathTemplate.TryCreate(source.x,source.y,n,dragonChoice.Edge,dragonChoice.Offset,dragonChoice.AimX,dragonChoice.AimY,false,out template,dragonChoice.IncludeOrigin);
        GUILayout.Label("覆盖："+string.Join("、",BreathTargets(dragonActor,template).Select(TransitActorName).ToArray())+"\n所有范围内生物均受影响，不要求可见，不排除盟友。",small);
        GUI.enabled=enabled&&DragonBreathUIFree(dragonActor)&&valid&&DragonBreathReady(dragonActor);
        if(GUILayout.Button("确认喷吐（魔法动作）",button))StartCoroutine(UseDragonBreath(dragonActor,template));GUI.enabled=enabled;
        GUILayout.EndScrollView();GUILayout.EndArea();var board=new Rect(800,250,400,400);DrawMap(board,false);DrawBreathArea(board,template,dragonActor);
        GUI.Label(new Rect(800,670,400,80),"15尺锥形：点击地图选择方向，格心进入范围即受影响，多格目标只结算一次。",small);
        var e=Event.current;var point=BoardPointer();if(DragonBreathUIFree(dragonActor)&&e.type==EventType.MouseDown&&e.button==0&&board.Contains(point)){
            float side=board.width/map.Size;dragonChoice.AimX=Math.Round((point.x-board.x)/side*2)*.5;dragonChoice.AimY=Math.Round((point.y-board.y)/side*2)*.5;e.Use();
        }
    }
}
