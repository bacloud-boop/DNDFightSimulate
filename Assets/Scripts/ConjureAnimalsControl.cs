using System.Collections;
using UnityEngine;
public sealed partial class TrainingGame
{
    string packColumn="1",packRow="1";
    void OpenAnimalPackControl(){packColumn=(animalPack.X+1).ToString();packRow=(animalPack.Y+1).ToString();panel="animalPack";}
    bool PackInput(out Vector2Int cell){int x,y;bool valid=int.TryParse(packColumn,out x)&int.TryParse(packRow,out y);cell=Vector2Int.zero;if(!valid||x<1||y<1||x>=map.Size||y>=map.Size)return false;cell=new Vector2Int(x-1,y-1);return true;}
    string AnimalPackMoveError()
    {
        if(!animalPack.Active(worldRounds)||concentrationSpell!="咒唤兽群")return "兽群已结束。";
        if(rolling||reactionPending||spellTargeting||draggingToken>=0)return "请先完成当前操作。";
        if(terrainActor!=0||!PlayerTurnAllowed||!combat.PlayerMovementReady)return "只能在玩家自己的回合控制兽群。";
        if(!animalPack.CanMove(terrainTurn,worldRounds))return "玩家本回合须先实际移动，且兽群须仍有剩余移动额度。";
        if(map.SceneOf(0)!=animalPack.Scene||map.Scene!=animalPack.Scene||map.IsContained(true))return "请在兽群所在位面、吞咽外进行控制。";
        Vector2Int cell;if(!PackInput(out cell)||!map.CanPlaceSummon(cell,2))return "请选择地图内未被占据的2×2落点。";
        int distance=BattleMap.FeetBetween(PackCell,cell);if(distance==0||distance>animalPack.MovementRemaining)return "移动距离须为正，且不能超过兽群剩余 "+animalPack.MovementRemaining+" 尺。";
        return VisibilityAt(true,cell,2)>=2?"必须看见落点。":"";
    }
    void DrawAnimalPackControl()
    {
        Surface(new Rect(350,110,900,745),paperTexture);if(Btn(new Rect(1140,125,90,46),"关闭",!rolling)){panel="";return;}
        GUILayout.BeginArea(new Rect(375,190,390,620));GUILayout.Label("控制咒唤兽群",heading);GUILayout.Label("本回合剩余 "+animalPack.MovementRemaining+" / 30尺。可分多次移动，不消耗玩家动作或附赠动作；不刷新持续时间。兽群沿途可触发10尺范围内生物的豁免，落点须未被占据。",small);
        packColumn=IntegerField(packColumn,false);packRow=IntegerField(packRow,false);string error=AnimalPackMoveError();GUILayout.Label(error==""?"可移动至所选落点。":error,small);
        bool enabled=GUI.enabled;GUI.enabled=enabled&&error=="";if(GUILayout.Button("移动兽群",button))StartCoroutine(MoveAnimalPack());GUI.enabled=enabled;GUILayout.EndArea();
        var board=new Rect(800,250,400,400);DrawMap(board,false);Vector2Int p;if(PackInput(out p))Fill(BodyRect(board,p,2),new Color(.5f,.9f,.8f,.35f));
        var ev=Event.current;var pointer=BoardPointer();if(!rolling&&ev.type==EventType.MouseDown&&ev.button==0&&board.Contains(pointer)){float step=board.width/map.Size;packColumn=((int)((pointer.x-board.x)/step)+1).ToString();packRow=((int)((pointer.y-board.y)/step)+1).ToString();ev.Use();}
    }
    IEnumerator MoveAnimalPack()
    {
        Vector2Int target;if(AnimalPackMoveError()!=""||!PackInput(out target))yield break;
        var origin=PackCell;var previous=origin;var meter=map.RouteMeter(origin,target,false);bool busy=rolling;string previousPanel=panel;rolling=true;panel="";
        try{foreach(var p in BattleMap.MovementAnchors(origin,target)){
            int feet=meter.Next(BattleMap.FeetBetween(previous,p),false,false);previous=p;
            if(!animalPack.Move(p.x,p.y,feet,terrainTurn,worldRounds))break;
            foreach(int actor in BreathActors())QueuePackEntry(actor,"兽群移入10尺范围");
            if(PendingTerrainCount>0)yield return DrainGreaseEntries();
        }history.Insert(0,"咒唤兽群移动结束，本回合剩余 "+animalPack.MovementRemaining+"尺；未消耗动作或附赠动作。");}
        finally{rolling=busy;panel=previousPanel;}
    }
    void DrawAnimalPackMap(Rect board)
    {
        if(animalPack.Active(worldRounds)&&animalPack.Scene==map.Scene){for(int y=0;y<map.Size;y++)for(int x=0;x<map.Size;x++)if(BattleMap.BodyDistance(PackCell,2,new Vector2Int(x,y),1)<=10)Fill(CellRect(board,new Vector2Int(x,y)),new Color(.2f,.7f,.55f,.12f));
            Fill(BodyRect(board,PackCell,2),new Color(.25f,.8f,.6f,.4f));GUI.Label(BodyRect(board,PackCell,2),new[]{"幽灵狼群","幽灵蛇群","幽灵鸟群"}[animalPack.Appearance],small);
        }
        Vector2Int p;if(panel=="spells"&&selectedSpellName=="Conjure Animals"&&VisionPoint(out p))Fill(BodyRect(board,p,2),new Color(.4f,.9f,.7f,.45f));
    }
}
