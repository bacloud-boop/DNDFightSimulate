using System.Linq;
using UnityEngine;
public sealed partial class TrainingGame
{
    Vector2 frozenScroll;
    bool HasFrozenRemains=>new[]{0,-1,BattleMap.RescueId}.Any(id=>map.BodyPresent(id)&&CurrentCorpse(id).Frozen)||map.Remains.Bodies.Any(b=>map.BodyPresent(b.Id)&&b.Frozen);
    void DrawFrozenRemainsPanel()
    {
        Surface(new Rect(350,110,540,745),paperTexture);if(Btn(new Rect(780,125,90,46),"关闭",!rolling)){panel="";return;}
        GUILayout.BeginArea(new Rect(375,185,490,635));frozenScroll=GUILayout.BeginScrollView(frozenScroll);
        GUILayout.Label("冻结遗体",heading);GUILayout.Label("寒冰锥杀死的生物冻成雕像。解冻免费且不推进时间；冻结不改变重量、占地、搬运、装备回收或复活资格，不属于石化状态。",small);
        bool enabled=GUI.enabled;GUI.enabled=enabled&&!rolling&&!reactionPending&&draggingToken<0;
        foreach(int id in new[]{0,-1,BattleMap.RescueId})if(map.BodyPresent(id)&&CurrentCorpse(id).Frozen&&GUILayout.Button("免费解冻："+TransitActorName(id)+"遗体",button)){CurrentCorpse(id).Thaw();history.Insert(0,TransitActorName(id)+"遗体已手动解冻，不消耗行动或时间。");}
        foreach(var body in map.Remains.Bodies)if(map.BodyPresent(body.Id)&&body.Frozen&&GUILayout.Button("免费解冻："+body.Name+" #"+body.Id,button)){body.Frozen=false;history.Insert(0,body.Name+"已手动解冻，不消耗行动或时间。");}
        GUI.enabled=enabled;if(!HasFrozenRemains)GUILayout.Label("当前没有冻结遗体。",small);
        GUILayout.EndScrollView();GUILayout.EndArea();
    }
    void DrawFrozenRemainsMap(Rect board)
    {
        foreach(int id in new[]{0,-1,BattleMap.RescueId}){
            if(!map.BodyPresent(id)||!CurrentCorpse(id).Frozen)continue;
            Vector2Int p;int n;string name;if(!AnimateBodyInfo(id,out p,out n,out name)||(id==0||id==-1)&&(map.IsContained(id==0)||map.IsCarried(id==0))||id==BattleMap.RescueId&&!map.RescuePosition.HasValue)continue;
            DrawFrozenBody(board,p,n);
        }
        foreach(var body in map.Remains.Bodies)if(body.Frozen&&map.RemainsSpaces(body)>0)DrawFrozenBody(board,new Vector2Int(body.X,body.Y),body.Spaces);
    }
    void DrawFrozenBody(Rect board,Vector2Int p,int n){var rect=BodyRect(board,p,n);Fill(rect,new Color(.45f,.8f,1,.65f));GUI.Label(rect,"冻",new GUIStyle(centered){fontSize=16});}
}
