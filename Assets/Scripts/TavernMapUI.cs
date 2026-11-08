using UnityEngine;

public sealed partial class TrainingGame
{
    readonly BattleMap map=new BattleMap();
    int draggingToken=-1,dragControl;
    string deploymentMessage="拖动蓝色玩家与红色木桩，选择不同格子。";
    void BeginDeployment() {map.Unlock();page="deployment";panel="";draggingToken=-1;}
    Vector2 BoardPointer() {return GUI.matrix.inverse.MultiplyPoint3x4(new Vector3(Input.mousePosition.x,Screen.height-Input.mousePosition.y,0));}
    Rect CellRect(Rect board,Vector2Int cell) {float size=board.width/map.Size;return new Rect(board.x+cell.x*size,board.y+cell.y*size,size,size);}
    void Token(Rect rect,bool player,bool preview=false)
    {
        Fill(new Rect(rect.x+2,rect.y+2,rect.width-4,rect.height-4),player?new Color(.12f,.34f,.46f,.95f):new Color(.55f,.13f,.07f,.95f));
        if(rect.width>=30)GUI.DrawTexture(rect,player?miniatures.Player:miniatures.Monster,ScaleMode.ScaleToFit);
        else GUI.Label(rect,player?"人":"桩",new GUIStyle(centered){fontSize=Mathf.Max(10,(int)(rect.width*.65f)),normal={textColor=Color.white}});
    }
    string movementMessage="拖动玩家棋子移动。";
    void DrawMap(Rect board,bool allowDrag)
    {
        bool moving=page=="battle" && map.Locked;
        Surface(new Rect(board.x-5,board.y-5,board.width+10,board.height+10),paperTexture);
        float size=board.width/map.Size;
        for(int y=0;y<map.Size;y++)for(int x=0;x<map.Size;x++) {
            var rect=new Rect(board.x+x*size,board.y+y*size,size,size);
            if((x+y)%2==0)Fill(rect,new Color(.32f,.21f,.09f,.10f));
            if(moving && allowDrag && map.CanMoveTo(new Vector2Int(x,y)))Fill(rect,new Color(.12f,.40f,.26f,.16f));
        }
        for(int i=0;i<=map.Size;i++) {
            Fill(new Rect(board.x+i*size,board.y,1,board.height),new Color(.32f,.21f,.09f,.45f));
            Fill(new Rect(board.x,board.y+i*size,board.width,1),new Color(.32f,.21f,.09f,.45f));
        }
        var playerRect=CellRect(board,map.Player);var monsterRect=CellRect(board,map.Monster);
        if(draggingToken!=0)Token(playerRect,true);
        if(draggingToken!=1)Token(monsterRect,false);
        if(!allowDrag || (map.Locked&&!moving))return;
        int id=GUIUtility.GetControlID(23891,FocusType.Passive);
        var e=Event.current;Vector2 point=BoardPointer();
        bool playerCanDrag=playerRect.Contains(point)&&(!moving||map.MovementRemaining>0);
        if(e.type==EventType.MouseDown && e.button==0 && (playerCanDrag||(!moving&&monsterRect.Contains(point)))) {
            draggingToken=playerCanDrag?0:1;dragControl=id;GUIUtility.hotControl=id;e.Use();
        }
        if(draggingToken>=0) {
            var cell=new Vector2Int(Mathf.FloorToInt((point.x-board.x)/size),Mathf.FloorToInt((point.y-board.y)/size));
            bool inside=board.Contains(point);
            bool legal=moving?inside&&map.CanMoveTo(cell):inside&&cell!=(draggingToken==0?map.Monster:map.Player);
            if(inside)Fill(CellRect(board,cell),legal?new Color(.1f,.55f,.2f,.4f):new Color(.8f,.05f,.02f,.4f));
            Token(new Rect(point.x-size/2,point.y-size/2,size,size),draggingToken==0,true);
            if(moving) {
                int cost=BattleMap.FeetBetween(map.Player,cell);
                movementMessage="本次 "+cost+" 尺\n距回合起点 "+BattleMap.FeetBetween(map.RoundStartPlayer,cell)+" 尺\n移动后剩余 "+(map.MovementRemaining-cost)+" 尺\n"+(legal?"松手落在格心":"不可移动到此位置");
            }
            if(e.type==EventType.MouseDrag)e.Use();
            if(e.type==EventType.KeyDown && e.keyCode==KeyCode.Escape) {draggingToken=-1;GUIUtility.hotControl=0;movementMessage="已取消本次移动。";e.Use();return;}
            if(e.type==EventType.MouseUp && e.button==0) {
                var origin=map.Player;
                bool valid=inside&&(moving?map.MovePlayer(cell):map.Place(draggingToken==0,cell));
                if(moving) {
                    if(valid && cell!=origin) {
                        combat.MarkBattleStarted();int cost=BattleMap.FeetBetween(origin,cell);
                        history.Insert(0,"玩家移动 "+cost+" 尺至("+(cell.x+1)+","+(cell.y+1)+")，本回合剩余 "+map.MovementRemaining+" 尺。");logScroll=Vector2.zero;
                    }
                    movementMessage=valid?"移动完成，剩余 "+map.MovementRemaining+" 尺。":"移动取消：超出移动力、越界或目标格被木桩占据。";
                } else deploymentMessage=valid?"位置已更新，可以继续拖动或开始战斗。":"该位置不可用：必须在地图内，且双方不能占同一格。";
                draggingToken=-1;if(GUIUtility.hotControl==dragControl)GUIUtility.hotControl=0;e.Use();
            }
        }
    }
    void DrawDeployment()
    {
        GUI.Label(new Rect(500,22,600,50),"选择地图与初始位置",lightTitle);
        if(Btn(new Rect(35,25,250,48),"← 酒馆桌面",!rolling&&draggingToken<0))page="home";
        DrawCard(true);DrawCard(false);
        Surface(new Rect(365,105,870,750),paperTexture);
        for(int i=0;i<3;i++)if(Btn(new Rect(390+i*275,125,255,46),(map.Selected==i?"✓ ":"")+BattleMap.Names[i]+" · "+BattleMap.Sizes[i]+"×"+BattleMap.Sizes[i],!rolling&&panel==""&&draggingToken<0))map.Select(i);
        GUI.Label(new Rect(390,180,820,40),"每格 5 尺 · 距离 "+map.DistanceFeet+" 尺 · 蓝色为玩家，红色为木桩",centered);
        DrawMap(new Rect(550,225,500,500),!rolling&&panel=="");
        GUI.Label(new Rect(390,735,820,35),deploymentMessage,centered);
        if(Btn(new Rect(590,787,420,48),"开始战斗 · 锁定位置",!rolling&&panel==""&&draggingToken<0)) {
            if(map.Lock()){page="battle";history.Insert(0,"部署完成："+BattleMap.Names[map.Selected]+"，玩家("+(map.Player.x+1)+","+(map.Player.y+1)+")，木桩("+(map.Monster.x+1)+","+(map.Monster.y+1)+")，距离 "+map.DistanceFeet+" 尺。");}
        }
        if(panel!="")DrawPanel();
    }
}
