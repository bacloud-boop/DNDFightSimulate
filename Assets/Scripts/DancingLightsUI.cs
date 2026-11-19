using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public sealed partial class TrainingGame
{
    void DrawDancingLightsPanel(bool moving)
    {
        if(moving){RefreshDancingLights();SyncDanceMoveDraft();}
        Surface(new Rect(350,110,900,745),paperTexture);
        if(Btn(new Rect(1140,125,90,46),moving?"关闭":"返回",!rolling)){if(moving)panel="";else selectedSpellName="";return;}
        GUILayout.BeginArea(new Rect(375,185,390,630));dancingScroll=GUILayout.BeginScrollView(dancingScroll);
        var definition=DirectSpell.Find("Dancing Lights");
        GUILayout.Label(moving?"移动舞光 · 附赠动作":"舞光术",heading);
        GUILayout.Label(moving?"移动现存光点，每个至多60尺。不会重新施法或延长持续时间，无需声音、姿势或空手。":definition.Summary,small);
        if(dancingLights.Active(worldRounds))GUILayout.Label("现存 "+dancingLights.Points.Count+" 个光源 · 剩余至多 "+Math.Max(0,dancingLights.Expires-worldRounds)*6+" 秒",small);
        bool enabled=GUI.enabled;GUI.enabled=enabled&&!rolling;
        int[] casters=new int[0];
        if(!moving){
            var metadata=SelectedSpellMetadata();casters=metadata==null?new int[0]:EligibleCasters(metadata).ToArray();
            if(Array.IndexOf(casters,directCastingClass)<0&&casters.Length>0)directCastingClass=casters[0];
            foreach(int caster in casters)if(GUILayout.Button((caster==directCastingClass?"✓ ":"")+CasterLabel(caster),button))directCastingClass=caster;
            if(metadata!=null&&map.PlayerAquatic.Submerged&&!WaterBreathingAllowed(true)&&Array.IndexOf(metadata.components,"声音")>=0)GUILayout.Label("水下声音施法会结束屏息并呛水。",small);
            danceAppearance=GUILayout.SelectionGrid(danceAppearance,new[]{"光球","火把光影","灯笼光影","合并人形"},2,new GUIStyle(button){fontSize=16,fixedHeight=34});
            if(danceAppearance!=3){GUILayout.Label("独立光点数量",small);danceCount=GUILayout.SelectionGrid(danceCount-1,new[]{"1","2","3","4"},4,new GUIStyle(button){fontSize=16,fixedHeight=32})+1;}
            else GUILayout.Label("四个光点合为一个中型人形，统一移动；只是光影，不占据格子。",small);
        }
        int count=moving?dancingLights.Points.Count:danceAppearance==3?1:danceCount;
        danceSelected=Mathf.Clamp(danceSelected,0,Math.Max(0,count-1));
        for(int i=0;i<count;i++){
            int id=moving?dancingLights.Points[i].Id:i+1;
            if(GUILayout.Button((danceSelected==i?"✓ ":"")+(moving&&dancingLights.Combined||!moving&&danceAppearance==3?"合并人形位置":"光点 "+id+" · 列／行"),button))danceSelected=i;
            GUILayout.BeginHorizontal();
            if(moving){var input=danceMoveInputs[id];input[0]=IntegerField(input[0],false);input[1]=IntegerField(input[1],false);}
            else {danceColumns[i]=IntegerField(danceColumns[i],false);danceRows[i]=IntegerField(danceRows[i],false);}
            GUILayout.EndHorizontal();
        }
        GUILayout.Label("每个光源照亮10尺微光；存在多个独立光点时，各自须距至少另一个光点20尺内。单个独立光点可单独存在。点击右侧地图设置选中的位置。",small);
        string error=moving?DanceMoveError():DirectSpellInvalidReason(definition,casters.Length>0);
        GUILayout.Label(error==""?(moving?"确认后消耗1个附赠动作。":"确认后消耗1个魔法动作，戏法不消耗法术位。"):error,small);
        GUI.enabled=enabled&&error=="";
        if(GUILayout.Button(moving?"确认移动舞光":"确认施放舞光术",button)){
            if(moving)MoveDancingLights();else {panel="";StartCoroutine(ResolveDirectSpell(definition,true,directCastingClass,0));}
        }
        GUI.enabled=enabled&&!rolling;
        if(!moving&&dancingLights.Active(worldRounds)&&GUILayout.Button("调整现存舞光（附赠动作）",button))OpenDancingLightsControl();
        GUI.enabled=enabled;GUILayout.EndScrollView();GUILayout.EndArea();
        Rect board=new Rect(800,250,400,400);DrawMap(board,false);DrawDancingDraft(board,moving);
        GUI.Label(new Rect(800,670,400,100),"黄色为拟放置位置，淡黄为10尺微光预览；确认前不会更改实际光照。光影可以与生物占格重叠。",small);
        var e=Event.current;var point=BoardPointer();
        if(!rolling&&count>0&&e.type==EventType.MouseDown&&e.button==0&&board.Contains(point)){
            float side=board.width/map.Size;string column=((int)((point.x-board.x)/side)+1).ToString(),row=((int)((point.y-board.y)/side)+1).ToString();
            if(moving){int id=dancingLights.Points[danceSelected].Id;danceMoveInputs[id][0]=column;danceMoveInputs[id][1]=row;}
            else {danceColumns[danceSelected]=column;danceRows[danceSelected]=row;}e.Use();
        }
    }
    void DrawDancingDraft(Rect board,bool moving)
    {
        var points=DanceDraft(moving);if(points==null)return;
        foreach(var point in points){if(!DanceOnMap(point))continue;
            for(int y=0;y<map.Size;y++)for(int x=0;x<map.Size;x++)if(DancingLightPoint.Distance(point,new DancingLightPoint(0,x,y))<=10)Fill(CellRect(board,new Vector2Int(x,y)),new Color(.95f,.78f,.2f,.16f));
            var rect=CellRect(board,new Vector2Int(point.X,point.Y));Fill(rect,new Color(.95f,.7f,.1f,.42f));GUI.Label(rect,point.Id.ToString(),new GUIStyle(centered){fontSize=15});
        }
    }
}
