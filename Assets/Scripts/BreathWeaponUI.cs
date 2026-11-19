using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    sealed class BreathChoice {public int Shape,Edge,Offset=-1;public double AimX=-1,AimY;public bool IncludeOrigin;}
    readonly BreathChoice playerBreathChoice=new BreathChoice(),rescueBreathChoice=new BreathChoice();Vector2 breathScroll;
    void DrawBreathPanel(int caster)
    {
        Surface(new Rect(350,110,900,745),paperTexture);if(Btn(new Rect(1140,125,90,46),"关闭",!rolling)){panel=caster==0?"":"rescue";return;}
        Vector2Int source;int spaces;string name;if(!HealingHandsTarget(caster,out source,out spaces,out name)){GUI.Label(new Rect(380,200,780,90),"当前没有可行动的龙裔身体。",label);return;}
        var choice=caster==0?playerBreathChoice:rescueBreathChoice;
        if(choice.Offset<0)choice.Offset=spaces;choice.Offset=Mathf.Clamp(choice.Offset,0,spaces*2);
        if(choice.AimX<0){choice.AimX=Mathf.Min(map.Size-1,source.x+spaces+1)+.5;choice.AimY=source.y+.5;}
        GUILayout.BeginArea(new Rect(375,185,390,630));breathScroll=GUILayout.BeginScrollView(breathScroll);
        GUILayout.Label(name+" · 龙裔吐息",heading);
        int type=BreathWeaponRules.DamageType(BreathSpecies(caster).Lineage),proficiency=BreathProficiency(caster);
        GUILayout.Label("替换攻击动作中的一次攻击，每个攻击动作至多一次；不消耗附赠动作，不触发额外的攻击或攻击检定。每长休恢复次数。",small);
        GUILayout.Label(BreathWeaponRules.DiceCount(BreathLevel(caster))+"d10 "+DamageTypes.Names[type]+" · 敏捷DC "+BreathWeaponRules.SaveDc(BreathConstitution(caster),proficiency)+"\n剩余 "+BreathState(caster).Remaining(Species2024.Dragonborn,proficiency)+" / "+Mathf.Max(0,proficiency)+" 次；豁免成功伤害减半向下取整。",label);
        bool enabled=GUI.enabled;GUI.enabled=enabled&&!rolling;
        choice.Shape=GUILayout.SelectionGrid(choice.Shape,new[]{"15尺锥形","30尺×5尺线形"},1,new GUIStyle(button){fontSize=16,fixedHeight=34});
        GUILayout.Label("选择源点；边缘源点须朝外，中心源点可自由转向",small);choice.Edge=GUILayout.SelectionGrid(choice.Edge,new[]{"右边","左边","下边","上边","自身中心"},2,new GUIStyle(button){fontSize=16,fixedHeight=32});
        var offsets=new string[spaces*2+1];for(int i=0;i<offsets.Length;i++)offsets[i]=i==spaces?"居中":i==0?"起端":i==spaces*2?"末端":"位置 "+i;
        if(choice.Edge!=4)choice.Offset=GUILayout.SelectionGrid(choice.Offset,offsets,3,new GUIStyle(button){fontSize=15,fixedHeight=30});
        choice.IncludeOrigin=GUILayout.Toggle(choice.IncludeOrigin,"将源点纳入范围",label);
        BreathTemplate template;bool valid=BreathTemplate.TryCreate(source.x,source.y,spaces,choice.Edge,choice.Offset,choice.AimX,choice.AimY,choice.Shape==1,out template,choice.IncludeOrigin);
        var targets=valid?BreathTargets(caster,template):new List<int>();var names=new List<string>();foreach(int id in targets)names.Add(TransitActorName(id));
        GUILayout.Label("格心进入范围才生效，自身和盟友也会受伤，多格目标最多结算一次。瞄准点可落在格心、格边中点或交点。\n覆盖："+(names.Count==0?"无生物":string.Join("、",names)),small);
        if(!valid)GUILayout.Label("请选择从源点所在边朝外的方向。",small);
        if((caster==0||caster==-1)&&map.IsContained(caster==0))GUILayout.Label("被吞咽时吐息仅作用于容纳者，不能穿过其身体波及外界。",small);
        if(caster==0&&fighter.AttacksRemaining>0)GUILayout.Label("当前攻击动作剩余 "+fighter.AttacksRemaining+" 次；吐息"+(fighter.BreathUsedThisAttackAction?"已替换过一次攻击":"可替换一次攻击")+"，武器保持锁定。",small);
        if(caster==0&&!SpeciesPhysicalActive(true))GUILayout.Label("当前兽形不能使用原种族的吐息。",small);
        GUI.enabled=enabled&&!rolling&&!spellTargeting&&!reactionPending&&draggingToken<0&&valid&&BreathReady(caster);
        if(GUILayout.Button("确认吐息（替换一次攻击）",button))StartCoroutine(UseBreath(caster,template));GUI.enabled=enabled;
        GUILayout.EndScrollView();GUILayout.EndArea();
        Rect board=new Rect(800,250,400,400);DrawMap(board,false);DrawBreathArea(board,template,caster);
        GUI.Label(new Rect(800,670,400,90),"橙色为吐息范围，黄点为源点；点击地图调整方向。",small);
        var e=Event.current;var point=BoardPointer();if(!rolling&&e.type==EventType.MouseDown&&e.button==0&&board.Contains(point)){
            float size=board.width/map.Size;choice.AimX=System.Math.Round((point.x-board.x)/size*2)*.5;choice.AimY=System.Math.Round((point.y-board.y)/size*2)*.5;e.Use();
        }
    }
}
