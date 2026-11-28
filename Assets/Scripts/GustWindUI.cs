using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public sealed partial class TrainingGame
{
    int gustDirection=-1;
    void AimGustDirection(int direction)
    {
        int spaces=map.PlayerSpaces,offset=gustChoice.Offset<0?spaces:Mathf.Clamp(gustChoice.Offset,0,spaces*2);double x,y;
        if(!LightningBoltRules.DirectionAim(map.Player.x,map.Player.y,spaces,gustChoice.Edge,offset,direction,out x,out y))return;
        gustChoice.AimX=x;gustChoice.AimY=y;
    }
    void DrawGustWindPanel()
    {
        var spell=DirectSpell.Find("Gust of Wind");var metadata=SelectedSpellMetadata();if(spell==null||metadata==null){selectedSpellName="";return;}
        Surface(new Rect(350,110,900,745),paperTexture);if(Btn(new Rect(1140,125,90,46),"返回",!rolling)){selectedSpellName="";return;}
        var casters=EligibleCasters(metadata).ToArray();if(Array.IndexOf(casters,directCastingClass)<0&&casters.Length>0)directCastingClass=casters[0];
        BreathTemplate template;PlayerGustTemplate(out template);
        GUILayout.BeginArea(new Rect(375,185,390,630));gustScroll=GUILayout.BeginScrollView(gustScroll);
        GUILayout.Label("造风术",heading);GUILayout.Label(spell.Summary,small);bool enabled=GUI.enabled;GUI.enabled=enabled&&!rolling;
        foreach(int caster in casters)if(GUILayout.Button((caster==directCastingClass?"✓ ":"")+CasterLabel(caster),button))directCastingClass=caster;
        DrawSpeciesSpellResource(spell);GUILayout.Label("施法环阶（法术位资源暂不限）",small);
        directSlotLevel=Mathf.Clamp(directSlotLevel,2,9);directSlotLevel=GUILayout.SelectionGrid(directSlotLevel-2,new[]{"2","3","4","5","6","7","8","9"},4,new GUIStyle(button){fontSize=16,fixedHeight=30})+2;
        long dc=8L+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(directCastingClass)]);
        GUILayout.Label("力量DC "+dc+"；失败沿风线推离15尺，成功不推离。升环不增强。",small);
        if(map.PlayerAquatic.Submerged&&!WaterBreathingAllowed(true))GUILayout.Label("声音施法会立即结束屏息并呛水；潜水单位不受风影响。",small);
        GUILayout.Label("源点：边缘或自身中心；点击地图确定朝向",small);
        gustChoice.Edge=GUILayout.SelectionGrid(gustChoice.Edge,new[]{"右边","左边","下边","上边","自身中心"},2,new GUIStyle(button){fontSize=16,fixedHeight=32});
        int spaces=map.PlayerSpaces;var offsets=new string[spaces*2+1];for(int i=0;i<offsets.Length;i++)offsets[i]=i==spaces?"居中":i==0?"起端":i==spaces*2?"末端":"位置 "+i;
        if(gustChoice.Edge!=4)gustChoice.Offset=GUILayout.SelectionGrid(gustChoice.Offset,offsets,3,new GUIStyle(button){fontSize=15,fixedHeight=30});
        int direction=GUILayout.SelectionGrid(gustDirection,new[]{"东","东北","北","西北","西","西南","南","东南"},4,new GUIStyle(button){fontSize=16,fixedHeight=30});
        if(direction>=0){gustDirection=direction;AimGustDirection(direction);}
        PlayerGustTemplate(out template);
        var targets=template.Valid?BreathTargets(0,template):new List<int>();targets.RemoveAll(id=>id==0||ActorDiving(id));
        GUILayout.Label("格心进入60尺×10尺线形即受影响，包括盟友，施法者与潜水单位除外；多格生物仅结算一次。\n覆盖："+(targets.Count==0?"无生物":string.Join("、",targets.Select(TransitActorName).ToArray())),small);
        if(map.IsContained(true))GUILayout.Label("吞咽内部不能向外制造风线。",small);
        GUILayout.Label("创建与目标回合末豁免；进入、开始回合或转向不立即豁免。向施法者靠近额外耗移动力，风线与云雾／臭云相交就驱散整团。环境普通火焰保留说明。",small);
        if(gustWind.Active(worldRounds)){string turnError=GustTurnError();GUI.enabled=enabled&&turnError=="";if(GUILayout.Button("改变现存风线方向（附赠动作）",button))TurnGustWind();GUI.enabled=enabled;if(turnError!="")GUILayout.Label(turnError,small);}
        string error=DirectSpellInvalidReason(spell,casters.Length>0);GUILayout.Label(error==""?"消耗1个魔法动作，不进行攻击检定。":error,small);
        GUI.enabled=enabled&&error=="";if(GUILayout.Button("确认施放造风术",button)){panel="";StartCoroutine(ResolveDirectSpell(spell,true,directCastingClass,directSlotLevel));}GUI.enabled=enabled;
        GUILayout.EndScrollView();GUILayout.EndArea();
        var board=new Rect(800,250,400,400);DrawMap(board,false);DrawBreathArea(board,template,0);
        GUI.Label(new Rect(800,670,400,90),"橙色为60尺×10尺线形范围，黄点为源点。边缘源点需朝外；中心源点可自由转向。范围超出地图的部分不阻止施法。",small);
        var e=Event.current;var point=BoardPointer();if(!rolling&&e.type==EventType.MouseDown&&e.button==0&&board.Contains(point)){
            float side=board.width/map.Size;gustChoice.AimX=Math.Round((point.x-board.x)/side*2)*.5;gustChoice.AimY=Math.Round((point.y-board.y)/side*2)*.5;gustDirection=-1;e.Use();
        }
    }
}
