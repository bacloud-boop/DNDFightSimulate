using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public sealed partial class TrainingGame
{
    void AimLightningDirection(int direction)
    {
        int spaces=map.PlayerSpaces,offset=lightningChoice.Offset<0?spaces:Mathf.Clamp(lightningChoice.Offset,0,spaces*2);double x,y;
        if(!LightningBoltRules.DirectionAim(map.Player.x,map.Player.y,spaces,lightningChoice.Edge,offset,direction,out x,out y))return;
        lightningAimInitialized=true;lightningChoice.AimX=x;lightningChoice.AimY=y;
    }
    void DrawLightningBoltPanel()
    {
        var spell=DirectSpell.Find("Lightning Bolt");var metadata=SelectedSpellMetadata();if(spell==null||metadata==null){selectedSpellName="";return;}
        Surface(new Rect(350,110,900,745),paperTexture);if(Btn(new Rect(1140,125,90,46),"返回",!rolling)){selectedSpellName="";return;}
        var casters=EligibleCasters(metadata).ToArray();if(Array.IndexOf(casters,directCastingClass)<0&&casters.Length>0)directCastingClass=casters[0];
        BreathTemplate template;PlayerLightningTemplate(out template);
        GUILayout.BeginArea(new Rect(375,185,390,630));lightningScroll=GUILayout.BeginScrollView(lightningScroll);
        GUILayout.Label("闪电束",heading);GUILayout.Label(spell.Summary,small);bool enabled=GUI.enabled;GUI.enabled=enabled&&!rolling;
        foreach(int caster in casters)if(GUILayout.Button((caster==directCastingClass?"✓ ":"")+CasterLabel(caster),button))directCastingClass=caster;
        DrawSpeciesSpellResource(spell);GUILayout.Label("施法环阶（法术位资源暂不限）",small);
        directSlotLevel=Mathf.Clamp(directSlotLevel,3,9);directSlotLevel=GUILayout.SelectionGrid(directSlotLevel-3,new[]{"3","4","5","6","7","8","9"},4,new GUIStyle(button){fontSize=16,fixedHeight=30})+3;
        long dc=8L+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(directCastingClass)]);
        GUILayout.Label(LightningBoltRules.DiceCount(directSlotLevel)+"d6 闪电 · 敏捷DC "+dc+"；成功减半。",small);
        if(map.PlayerAquatic.Submerged&&!WaterBreathingAllowed(true))GUILayout.Label("声音施法会立即结束屏息并呛水；水域不扩大闪电束范围。",small);
        GUILayout.Label("源点：边缘或自身中心；点击地图确定朝向",small);
        lightningChoice.Edge=GUILayout.SelectionGrid(lightningChoice.Edge,new[]{"右边","左边","下边","上边","自身中心"},2,new GUIStyle(button){fontSize=16,fixedHeight=32});
        int spaces=map.PlayerSpaces;var offsets=new string[spaces*2+1];for(int i=0;i<offsets.Length;i++)offsets[i]=i==spaces?"居中":i==0?"起端":i==spaces*2?"末端":"位置 "+i;
        if(lightningChoice.Edge!=4)lightningChoice.Offset=GUILayout.SelectionGrid(lightningChoice.Offset,offsets,3,new GUIStyle(button){fontSize=15,fixedHeight=30});
        int direction=GUILayout.SelectionGrid(lightningDirection,new[]{"东","东北","北","西北","西","西南","南","东南"},4,new GUIStyle(button){fontSize=16,fixedHeight=30});
        if(direction>=0){lightningDirection=direction;AimLightningDirection(direction);}
        lightningChoice.IncludeOrigin=GUILayout.Toggle(lightningChoice.IncludeOrigin,"将源点纳入范围",label);PlayerLightningTemplate(out template);
        var targets=template.Valid?BreathTargets(0,template):new List<int>();
        GUILayout.Label("格心进入100尺×5尺线形即受影响，包括自身和盟友；多格生物仅结算一次。\n覆盖："+(targets.Count==0?"无生物":string.Join("、",targets.Select(TransitActorName).ToArray())),small);
        if(map.IsContained(true))GUILayout.Label("吞咽内部仅能影响容纳者，不穿透到外界。",small);
        string error=DirectSpellInvalidReason(spell,casters.Length>0);GUILayout.Label(error==""?"消耗1个魔法动作，不进行攻击检定。":error,small);
        GUI.enabled=enabled&&error=="";if(GUILayout.Button("确认施放闪电束",button)){panel="";StartCoroutine(ResolveDirectSpell(spell,true,directCastingClass,directSlotLevel));}GUI.enabled=enabled;
        GUILayout.EndScrollView();GUILayout.EndArea();
        var board=new Rect(800,250,400,400);DrawMap(board,false);DrawBreathArea(board,template,0);
        GUI.Label(new Rect(800,670,400,90),"橙色为100尺×5尺线形范围，黄点为源点。边缘源点需朝外；中心源点可自由转向。范围超出地图的部分不阻止施法。",small);
        var e=Event.current;var point=BoardPointer();if(!rolling&&e.type==EventType.MouseDown&&e.button==0&&board.Contains(point)){
            float side=board.width/map.Size;lightningChoice.AimX=Math.Round((point.x-board.x)/side*2)*.5;lightningChoice.AimY=Math.Round((point.y-board.y)/side*2)*.5;lightningDirection=-1;lightningAimInitialized=true;e.Use();
        }
    }
}
