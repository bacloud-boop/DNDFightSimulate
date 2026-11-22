using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public sealed partial class TrainingGame
{
    void DrawBurningHandsPanel()
    {
        Surface(new Rect(350,110,900,745),paperTexture);if(Btn(new Rect(1140,125,90,46),"返回",!rolling)){selectedSpellName="";return;}
        bool color=selectedSpellName=="Color Spray",cold=selectedSpellName=="Cone of Cold";int reach=cold?60:15,minimum=cold?5:1;
        var spell=DirectSpell.Find(cold?"Cone of Cold":color?"Color Spray":"Burning Hands");var metadata=SelectedSpellMetadata();var casters=metadata==null?new int[0]:EligibleCasters(metadata).ToArray();
        if(Array.IndexOf(casters,directCastingClass)<0&&casters.Length>0)directCastingClass=casters[0];
        BreathTemplate template;PlayerBurningTemplate(out template,reach);
        GUILayout.BeginArea(new Rect(375,185,390,630));burningScroll=GUILayout.BeginScrollView(burningScroll);
        GUILayout.Label(spell.Chinese,heading);GUILayout.Label(spell.Summary,small);
        bool enabled=GUI.enabled;GUI.enabled=enabled&&!rolling;
        foreach(int caster in casters)if(GUILayout.Button((caster==directCastingClass?"✓ ":"")+CasterLabel(caster),button))directCastingClass=caster;
        DrawSpeciesSpellResource(spell);GUILayout.Label("施法环阶（法术位资源暂不限）",small);
        directSlotLevel=Mathf.Clamp(directSlotLevel,minimum,9);directSlotLevel=GUILayout.SelectionGrid(directSlotLevel-minimum,Enumerable.Range(minimum,10-minimum).Select(v=>v.ToString()).ToArray(),5,new GUIStyle(button){fontSize=16,fixedHeight=30})+minimum;
        long dc=8L+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(directCastingClass)]);
        GUILayout.Label(color?"体质DC "+dc+"；失败目盲至施法者下个回合结束，不造成伤害。":spell.DiceCount(classes.TotalLevel,directSlotLevel)+(cold?"d8 寒冷 · 体质DC ":"d6 火焰 · 敏捷DC ")+dc+"；成功减半。",small);
        if(map.PlayerAquatic.Submerged&&!WaterBreathingAllowed(true))GUILayout.Label("声音施法会结束屏息并呛水。"+(color||cold?"":"水下生物按已有火焰抗性结算。"),small);
        GUILayout.Label("选择锥形源点，再点击地图确定朝向",small);
        burningChoice.Edge=GUILayout.SelectionGrid(burningChoice.Edge,new[]{"右边","左边","下边","上边","自身中心"},2,new GUIStyle(button){fontSize=16,fixedHeight=32});
        int spaces=map.PlayerSpaces;var offsets=new string[spaces*2+1];for(int i=0;i<offsets.Length;i++)offsets[i]=i==spaces?"居中":i==0?"起端":i==spaces*2?"末端":"位置 "+i;
        if(burningChoice.Edge!=4)burningChoice.Offset=GUILayout.SelectionGrid(burningChoice.Offset,offsets,3,new GUIStyle(button){fontSize=15,fixedHeight=30});
        burningChoice.IncludeOrigin=GUILayout.Toggle(burningChoice.IncludeOrigin,"将源点纳入范围",label);PlayerBurningTemplate(out template,reach);
        var targets=template.Valid?BreathTargets(0,template):new List<int>();var names=targets.Select(TransitActorName).ToArray();
        GUILayout.Label("格心进入范围即受影响，包括自身和盟友；多格生物只结算一次。\n覆盖："+(names.Length==0?"无生物":string.Join("、",names)),small);
        if(map.IsContained(true))GUILayout.Label("吞咽内部的法术只作用于容纳者，不能波及外界。",small);
        string error=DirectSpellInvalidReason(spell,casters.Length>0);GUILayout.Label(error==""?"消耗1个魔法动作，不触发额外攻击。":error,small);
        GUI.enabled=enabled&&error=="";if(GUILayout.Button("确认施放"+spell.Chinese,button)){panel="";StartCoroutine(ResolveDirectSpell(spell,true,directCastingClass,directSlotLevel));}GUI.enabled=enabled;
        GUILayout.EndScrollView();GUILayout.EndArea();
        Rect board=new Rect(800,250,400,400);DrawMap(board,false);DrawBreathArea(board,template,0);
        GUI.Label(new Rect(800,670,400,85),"橙色为"+reach+"尺锥形范围，黄点为源点。边缘源点须朝外；中心源点可自由转向。",small);
        var e=Event.current;var point=BoardPointer();if(!rolling&&e.type==EventType.MouseDown&&e.button==0&&board.Contains(point)){
            float side=board.width/map.Size;burningChoice.AimX=Math.Round((point.x-board.x)/side*2)*.5;burningChoice.AimY=Math.Round((point.y-board.y)/side*2)*.5;e.Use();
        }
    }
}
