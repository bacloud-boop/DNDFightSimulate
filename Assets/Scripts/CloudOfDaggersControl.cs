using System.Collections;
using UnityEngine;
public sealed partial class TrainingGame
{
    string daggerColumn="1",daggerRow="1";
    void OpenCloudDaggersControl(){daggerColumn=(cloudDaggers.X+1).ToString();daggerRow=(cloudDaggers.Y+1).ToString();panel="cloudDaggers";}
    bool DaggersDestination(out Vector2Int p){int x,y;bool valid=int.TryParse(daggerColumn,out x)&int.TryParse(daggerRow,out y);p=new Vector2Int(x-1,y-1);return valid&&map.Inside(p,1);}
    string DaggersMoveError()
    {
        if(!cloudDaggers.Active(worldRounds))return "匕首之云已结束。";
        if(page!="battle"||!map.Locked||rolling||reactionPending||spellTargeting||draggingToken>=0)return "请先完成当前操作。";
        if(combat.Aftermath?!combat.AftermathMagicActionAvailable:!combat.MagicActionAvailable)return "当前没有可用魔法动作，或不是玩家回合。";
        Vector2Int p;if(!DaggersDestination(out p))return "列与行须为地图内整数。";
        if(combat.Round<=cloudDaggers.CastRound)return "只能在施法后的后续玩家回合传送匕首之云。";
        return BattleMap.FeetBetween(new Vector2Int(cloudDaggers.X,cloudDaggers.Y),p)>30?"落点距现存区域超过30尺。":"";
    }
    void DrawCloudDaggersControl()
    {
        Surface(new Rect(350,110,900,745),paperTexture);if(Btn(new Rect(1140,125,90,46),"关闭",!rolling)){panel="";return;}
        GUILayout.BeginArea(new Rect(375,190,390,620));GUILayout.Label("传送匕首之云",heading);
        GUILayout.Label("后续自己的回合，使用一个魔法动作传送现存5尺立方区域至30尺内落点。此操作不重新施法、不发声、不延长持续时间；不计算沿途伤害。",small);
        daggerColumn=IntegerField(daggerColumn,false);daggerRow=IntegerField(daggerRow,false);Vector2Int p;
        if(DaggersDestination(out p))GUILayout.Label("传送距离 "+BattleMap.FeetBetween(new Vector2Int(cloudDaggers.X,cloudDaggers.Y),p)+"尺",small);
        string error=DaggersMoveError();GUILayout.Label(error==""?"落点合法；同一生物每回合仍至多受伤一次。":error,small);
        bool old=GUI.enabled;GUI.enabled=old&&error=="";if(GUILayout.Button("传送区域（魔法动作）",button))StartCoroutine(MoveCloudDaggers());GUI.enabled=old;GUILayout.EndArea();
        var board=new Rect(800,250,400,400);DrawMap(board,false);if(DaggersDestination(out p))Fill(CellRect(board,p),new Color(.95f,.6f,.15f,.6f));
        var ev=Event.current;var pointer=BoardPointer();if(!rolling&&ev.type==EventType.MouseDown&&ev.button==0&&board.Contains(pointer)){float cell=board.width/map.Size;daggerColumn=((int)((pointer.x-board.x)/cell)+1).ToString();daggerRow=((int)((pointer.y-board.y)/cell)+1).ToString();ev.Use();}
    }
    IEnumerator MoveCloudDaggers()
    {
        Vector2Int p;if(DaggersMoveError()!=""||!DaggersDestination(out p))yield break;
        if(!combat.TryUseAftermathMagicAction())yield break;
        bool busy=rolling;rolling=true;panel="";bool moved=p.x!=cloudDaggers.X||p.y!=cloudDaggers.Y;
        try{
            cloudDaggers.Move(p.x,p.y,combat.Round,worldRounds,BattleMap.FeetBetween(new Vector2Int(cloudDaggers.X,cloudDaggers.Y),p));
            if(moved){daggersInsideMonster=false;QueueDaggersArea("区域传送落点");if(PendingTerrainCount>0)yield return DrainGreaseEntries();}
            history.Insert(0,"匕首之云传送完成；消耗1魔法动作，持续时间不刷新，沿途不造成伤害。"+(moved?"":"落点未改变，不新增进入伤害。"));
        }finally{rolling=busy;}
    }
}
