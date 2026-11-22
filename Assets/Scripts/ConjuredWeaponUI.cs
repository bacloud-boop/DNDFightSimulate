using System;
using System.Linq;
using UnityEngine;
public sealed partial class TrainingGame
{
    void DrawBarragePanel()
    {
        var spell=DirectSpell.Find("Conjure Barrage");var metadata=SelectedSpellMetadata();if(metadata==null){selectedSpellName="";return;}
        Surface(new Rect(350,110,900,745),paperTexture);if(Btn(new Rect(1140,125,90,46),"返回",!rolling)){selectedSpellName="";return;}
        var casters=EligibleCasters(metadata).ToArray();if(Array.IndexOf(casters,directCastingClass)<0&&casters.Length>0)directCastingClass=casters[0];
        BreathTemplate template;PlayerBarrageTemplate(out template);
        GUILayout.BeginArea(new Rect(375,185,390,630));barrageScroll=GUILayout.BeginScrollView(barrageScroll);
        GUILayout.Label(spell.Chinese,heading);GUILayout.Label(spell.Summary,small);bool enabled=GUI.enabled;GUI.enabled=enabled&&!rolling;
        foreach(int caster in casters)if(GUILayout.Button((caster==directCastingClass?"✓ ":"")+CasterLabel(caster),button))directCastingClass=caster;
        GUILayout.Label("施法环阶（法术位资源暂不限）",small);directSlotLevel=Mathf.Clamp(directSlotLevel,3,9);
        directSlotLevel=GUILayout.SelectionGrid(directSlotLevel-3,new[]{"3","4","5","6","7","8","9"},4,new GUIStyle(button){fontSize=16,fixedHeight=30})+3;
        GUILayout.Label(ConjuredWeaponRules.DiceCount(false,directSlotLevel)+"d8力场 · 敏捷DC "+(8L+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(directCastingClass)]))+"；成功减半。",small);
        if(map.PlayerAquatic.Submerged&&!WaterBreathingAllowed(true))GUILayout.Label("声音施法会立即结束屏息并呛水。",small);
        GUILayout.Label("选择60尺锥形源点，再点击地图确定朝向",small);
        barrageChoice.Edge=GUILayout.SelectionGrid(barrageChoice.Edge,new[]{"右边","左边","下边","上边","自身中心"},2,new GUIStyle(button){fontSize=16,fixedHeight=32});
        int n=map.PlayerSpaces;var offsets=new string[n*2+1];for(int i=0;i<offsets.Length;i++)offsets[i]=i==n?"居中":i==0?"起端":i==n*2?"末端":"位置 "+i;
        if(barrageChoice.Edge!=4)barrageChoice.Offset=GUILayout.SelectionGrid(barrageChoice.Offset,offsets,3,new GUIStyle(button){fontSize=15,fixedHeight=30});
        barrageChoice.IncludeOrigin=GUILayout.Toggle(barrageChoice.IncludeOrigin,"将源点纳入范围",label);PlayerBarrageTemplate(out template);
        DrawConjuredTargets(false,BreathTargets(0,template));
        if(map.IsContained(true))GUILayout.Label("吞咽内部仅能影响容纳者，不穿透到外界。",small);
        string error=DirectSpellInvalidReason(spell,casters.Length>0);GUILayout.Label(error==""?"消耗1个魔法动作，不触发额外攻击。":error,small);
        GUI.enabled=enabled&&error=="";if(GUILayout.Button("确认施放箭如雨下",button)){panel="";StartCoroutine(ResolveDirectSpell(spell,true,directCastingClass,directSlotLevel));}GUI.enabled=enabled;
        GUILayout.EndScrollView();GUILayout.EndArea();
        var board=new Rect(800,250,400,400);DrawMap(board,false);DrawBreathArea(board,template,0);
        GUI.Label(new Rect(800,670,400,90),"橙色为60尺锥形，黄点为源点。格心进入范围才纳入，多格单位只结算一次；仅伤害勾选且可见的目标。",small);
        var e=Event.current;var pointer=BoardPointer();if(!rolling&&e.type==EventType.MouseDown&&e.button==0&&board.Contains(pointer)){
            float side=board.width/map.Size;barrageChoice.AimX=Math.Round((pointer.x-board.x)/side*2)*.5;barrageChoice.AimY=Math.Round((pointer.y-board.y)/side*2)*.5;e.Use();
        }
    }
}
