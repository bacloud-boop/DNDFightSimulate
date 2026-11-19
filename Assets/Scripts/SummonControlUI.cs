using UnityEngine;
public sealed partial class TrainingGame
{
    Vector2 summonScroll;
    void DrawSummonMarkers(Rect board)
    {
        foreach(var unit in map.Summons)if(unit.Alive){var rect=BodyRect(board,new Vector2Int(unit.X,unit.Y),unit.Spaces);Fill(rect,new Color(.22f,.48f,.25f,.95f));GUI.Label(rect,"兽",new GUIStyle(centered){fontSize=Mathf.Max(12,(int)(rect.width*.6f)),normal={textColor=Color.white}});}
    }
    void DrawSummonControl()
    {
        if(combat.Aftermath){panel="aftermath";DrawAftermath();return;}
        if(summonActing||greaseResolving)return;
        bool phase=summonTurns.Active;Surface(new Rect(350,110,900,745),paperTexture);
        if(!phase&&Btn(new Rect(1140,125,90,46),"关闭")){panel="";return;}
        GUILayout.BeginArea(new Rect(375,180,380,650));summonScroll=GUILayout.BeginScrollView(summonScroll);
        GUILayout.Label(phase?"召唤物回合":"召唤物指令",heading);
        GUILayout.Label("协助战斗指令持续保留，手动移动或更换目标不需再次开口。新指令仅在玩家回合发出。",small);
        foreach(var unit in map.Summons)if(unit.Alive){
            GUILayout.Label(unit.Name+(unit.SleepStage==2?" · 昏迷":unit.SleepStage==1?" · 失能":"")+(unit.Prone?" · 倒地":"")+(unit.Restrained?" · 束缚":"")+" · HP "+unit.Hp+" / "+unit.MaxHp+" · AC "+unit.Armor,heading);
            if(!phase&&unit.SleepStage>0){bool enabled=GUI.enabled;GUI.enabled=!rolling&&combat.PlayerCanAct&&BattleMap.BodyDistance(map.Player,map.PlayerSpaces,new Vector2Int(unit.X,unit.Y),unit.Spaces)<=5;if(GUILayout.Button("摇醒 "+unit.Name+"（动作）",button)&&combat.TryUseAction()){unit.SleepStage=0;RefreshSleepConcentration();}GUI.enabled=enabled;}
            if(!phase){bool old=GUI.enabled;GUI.enabled=!rolling&&combat.PlayerCanAct&&!PlayerInBeastForm;
                if(GUILayout.Button(unit.Orders.Kind==SummonCommandKind.AssistCombat?"✓ 协助战斗（持续）":"下令：协助战斗",button))IssueSummonVerbalCommand(unit,SummonCommand.AssistCombat);
                if(GUILayout.Button(unit.Orders.Kind==SummonCommandKind.Dodge?"✓ 闪避（持续）":"下令：闪避",button))IssueSummonVerbalCommand(unit,SummonCommand.Dodge);
                GUI.enabled=old;
            }else if(summonTurns.Current==unit){
                DrawSummonWakeOptions(unit);
                GUILayout.Label("移动剩余 "+unit.MovementRemaining+"尺 · 动作 "+(unit.ActionAvailable?1:0)+" · 连击剩余 "+unit.AttacksRemaining,small);
                bool old=GUI.enabled;GUI.enabled=!summonActing;
                if(GUILayout.Button((summonTarget==-1?"✓ ":"")+"攻击目标：木桩",button))summonTarget=-1;
                if(GUILayout.Button((summonTarget==0?"✓ ":"")+"攻击目标：玩家",button))summonTarget=0;
                foreach(var other in map.Summons)if(other.Alive&&other.Id!=unit.Id&&GUILayout.Button((summonTarget==other.Id?"✓ ":"")+"目标："+other.Name,button))summonTarget=other.Id;
                Vector2Int target;int spaces;string name;bool legal=SummonTarget(summonTarget,out target,out spaces,out name)&&BattleMap.BodyDistance(new Vector2Int(unit.X,unit.Y),unit.Spaces,target,spaces)<=5;
                GUI.enabled=!summonActing&&legal&&(unit.ActionAvailable||unit.AttacksRemaining>0);
                if(GUILayout.Button(legal?"撕裂攻击 · 逐次结算":"无法攻击 · 目标不合法或超过5尺",button))StartCoroutine(ResolveSpiritAttack(unit,summonTarget));
                GUI.enabled=!summonActing&&unit.Prone;if(GUILayout.Button("起身（半速移动）",button))unit.Stand();
                GUI.enabled=!summonActing&&unit.ActionAvailable;if(unit.Restrained&&GUILayout.Button("挣脱纠缠（动作）",button))StartCoroutine(EscapeSpiritEntangle(unit));
                if(GUILayout.Button("疾走（动作）",button))unit.Dash();
                if(GUILayout.Button("闪避（动作）",button))unit.Dodge();
                GUI.enabled=!summonActing;
                int water=map.WaterCells(new Vector2Int(unit.X,unit.Y),unit.Spaces);
                if(water>0&&GUILayout.Button(unit.Aquatic.Submerged?"浮出水面 · 5尺":"潜入水下 · 5尺",button))unit.SwitchWaterState(!unit.Aquatic.Submerged,water);
                if(GUILayout.Button("结束召唤物回合",button))summonEndRequested=true;
                GUI.enabled=old;
            }
        }
        GUILayout.EndScrollView();GUILayout.EndArea();
        Rect board=new Rect(800,230,400,400);DrawMap(board,false);
        GUI.Label(new Rect(800,650,400,80),phase?"点击绿色格移动；每次确认累计消耗移动力。":"玩家回合发令，结束玩家回合后手动操控。",small);
        var current=summonTurns.Current;if(!phase||summonActing||current==null)return;
        float size=board.width/map.Size;for(int y=0;y<map.Size;y++)for(int x=0;x<map.Size;x++){var cell=new Vector2Int(x,y);if(map.CanMoveSummon(current.Id,cell))Fill(CellRect(board,cell),new Color(.1f,.6f,.2f,.2f));}
        var e=Event.current;Vector2 pointer=BoardPointer();if(e.type==EventType.MouseDown&&e.button==0&&board.Contains(pointer)){
            var cell=new Vector2Int((int)((pointer.x-board.x)/size),(int)((pointer.y-board.y)/size));
            if(map.CanMoveSummon(current.Id,cell))StartCoroutine(MoveSpiritWithReactions(current,cell));e.Use();
        }
    }
}
