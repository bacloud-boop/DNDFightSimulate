using UnityEngine;
public sealed partial class TrainingGame
{
    Vector2 summonScroll;
    void DrawSummonMarkers(Rect board)
    {
        foreach(var unit in map.SceneSummons)if(unit.Alive){var rect=BodyRect(board,new Vector2Int(unit.X,unit.Y),unit.Spaces);Fill(rect,new Color(.22f,.48f,.25f,.95f));GUI.Label(rect,IsSteed(unit)?(map.Riding?"骑手／驹":"驹"):Undead(unit)!=null?(Undead(unit).Rules.Skeleton?"骷":"尸"):"兽",new GUIStyle(centered){fontSize=Mathf.Max(12,(int)(rect.width*.6f)),normal={textColor=Color.white}});}
    }
    void DrawSummonControl()
    {
        if(combat.Aftermath){panel="aftermath";DrawAftermath();return;}
        if(summonActing||greaseResolving)return;
        bool phase=summonTurns.Active;Surface(new Rect(350,110,900,745),paperTexture);
        if(!phase&&Btn(new Rect(1140,125,90,46),"关闭")){panel="";return;}
        GUILayout.BeginArea(new Rect(375,180,380,650));summonScroll=GUILayout.BeginScrollView(summonScroll);
        GUILayout.Label(phase?"召唤物回合":"召唤物指令",heading);
        GUILayout.Label("协助战斗指令跨回合保留；骑乘时在玩家回合操控坐骑移动、疾走、撤离或闪避。坐骑以心灵感应接受指挥，不需开口。",small);
        DrawRidingControls();
        foreach(var unit in map.SceneSummons)if(unit.Alive){
            GUILayout.Label(unit.Name+(NeutralUndead(unit.Id)?" · 中立失控":"")+(PassTraceBenefit(unit.Id)?" · 无踪步":"")+(BeaconHope(unit.Id)?" · 希望信标":"")+(MovementFreedom(unit.Id)?" · 行动自如":"")+(ActorBlinded(unit.Id)?" · 目盲":"")+(unit.SleepStage==2?" · 昏迷":unit.SleepStage==1?" · 失能":"")+(unit.Prone?" · 倒地":"")+(unit.Restrained?" · 束缚":"")+(SteedFear(unit.Id)?" · 恐慌":"")+" · HP "+unit.Hp+" / "+unit.MaxHp+" · 临时生命 "+unit.TemporaryHp+" · AC "+unit.Armor,heading);
            if(!phase&&unit.SleepStage>0){bool enabled=GUI.enabled;GUI.enabled=!rolling&&combat.PlayerCanAct&&BattleMap.BodyDistance(map.Player,map.PlayerSpaces,new Vector2Int(unit.X,unit.Y),unit.Spaces)<=5;if(GUILayout.Button("摇醒 "+unit.Name+"（动作）",button)&&combat.TryUseAction()){unit.SleepStage=0;RefreshSleepConcentration();}GUI.enabled=enabled;}
            if(!phase&&unit!=steed){bool old=GUI.enabled;GUI.enabled=!rolling&&combat.PlayerCanAct&&!NeutralUndead(unit.Id)&&(!PlayerInBeastForm||Undead(unit)!=null&&!Undead(unit).Verbal);
                if(Undead(unit)!=null)GUILayout.Label(Undead(unit).Verbal?"更改指令需要口头传达，不消耗动作；旧指令持续有效。":"亡灵更改指令消耗附赠动作，通过精神传达。",small);
                if(GUILayout.Button(unit.Orders.Kind==SummonCommandKind.AssistCombat?"✓ 协助战斗（持续）":"下令：协助战斗",button))IssueSummonVerbalCommand(unit,SummonCommand.AssistCombat);
                if(GUILayout.Button(unit.Orders.Kind==SummonCommandKind.Dodge?"✓ 闪避（持续）":"下令：闪避",button))IssueSummonVerbalCommand(unit,SummonCommand.Dodge);
                GUI.enabled=old;
            }else if(summonTurns.Current==unit||!phase&&ControlledSteed&&unit==steed){
                if(!IsSteed(unit)||!ControlledSteed)DrawSummonWakeOptions(unit);
                if(Undead(unit)!=null)DrawUndeadEquipment(Undead(unit));
                GUILayout.Label("移动剩余 "+unit.MovementRemaining+"尺 · 动作 "+(unit.ActionAvailable?1:0)+" · 连击剩余 "+unit.AttacksRemaining,small);
                bool old=GUI.enabled;GUI.enabled=!summonActing;
                if(GUILayout.Button((summonTarget==-1?"✓ ":"")+"攻击目标：木桩",button))summonTarget=-1;
                if(GUILayout.Button((summonTarget==0?"✓ ":"")+"攻击目标：玩家",button))summonTarget=0;
                if(IsSteed(unit)&&GUILayout.Button((summonTarget==unit.Id?"✓ ":"")+"能力目标：坐骑自身",button))summonTarget=unit.Id;
                foreach(var other in map.SceneSummons)if(other.Alive&&other.Id!=unit.Id&&GUILayout.Button((summonTarget==other.Id?"✓ ":"")+"目标："+other.Name,button))summonTarget=other.Id;
                Vector2Int target;int spaces;string name;bool legal=SummonTarget(summonTarget,out target,out spaces,out name)&&BattleMap.BodyDistance(new Vector2Int(unit.X,unit.Y),unit.Spaces,target,spaces)<=(Undead(unit)==null?5:UndeadReach(Undead(unit)));
                GUI.enabled=!summonActing&&summonTarget!=unit.Id&&legal&&(!IsSteed(unit)||OtherworldlySteedRules.CanAttack(map.Riding,combat.PlayerHp==0||combat.PlayerParalyzed))&&(unit.ActionAvailable||unit.AttacksRemaining>0);
                if(GUILayout.Button(legal?(IsSteed(unit)?"异界猛击 · 逐次结算":Undead(unit)!=null?"亡灵攻击 · 逐次结算":"撕裂攻击 · 逐次结算"):"无法攻击 · 目标不合法或超过5尺",button))StartCoroutine(ResolveSpiritAttack(unit,summonTarget));
                GUI.enabled=!summonActing&&unit.Prone;if(GUILayout.Button("起身（半速移动）",button))unit.Stand();
                GUI.enabled=!summonActing&&unit.ActionAvailable;if(unit.Restrained&&(!IsSteed(unit)||!ControlledSteed)&&GUILayout.Button("挣脱束缚（动作）",button))StartCoroutine(EscapeSpiritEntangle(unit));
                if(GUILayout.Button("疾走（动作）",button))unit.Dash();
                if(GUILayout.Button("闪避（动作）",button))unit.Dodge();
                if(GUILayout.Button("撤离（动作）",button))unit.Disengage();
                DrawSteedSpecial(unit);DrawDragonBreathActionButton(unit.Id,"summons");
                GUI.enabled=!summonActing;
                int water=map.WaterCells(new Vector2Int(unit.X,unit.Y),unit.Spaces);
                if(water>0&&GUILayout.Button((unit.Aquatic.Submerged?"浮出水面 · ":"潜入水下 · ")+WaterLayerCost(unit.Id),button)){map.SwitchSummonWaterState(unit.Id,!unit.Aquatic.Submerged);ObserveWaterBreathing();}
                if(phase&&GUILayout.Button("结束召唤物回合",button))summonEndRequested=true;
                GUI.enabled=old;
            }
        }
        GUILayout.EndScrollView();GUILayout.EndArea();
        Rect board=new Rect(800,230,400,400);DrawMap(board,false);
        GUI.Label(new Rect(800,650,400,80),phase?"点击绿色格移动；每次确认累计消耗移动力。":"玩家回合发令，结束玩家回合后手动操控。",small);
        var current=phase?summonTurns.Current:ControlledSteed?steed:null;if(summonActing||current==null)return;
        float size=board.width/map.Size;for(int y=0;y<map.Size;y++)for(int x=0;x<map.Size;x++){var cell=new Vector2Int(x,y);if(map.CanMoveSummon(current.Id,cell))Fill(CellRect(board,cell),new Color(.1f,.6f,.2f,.2f));}
        var e=Event.current;Vector2 pointer=BoardPointer();if(e.type==EventType.MouseDown&&e.button==0&&board.Contains(pointer)){
            var cell=new Vector2Int((int)((pointer.x-board.x)/size),(int)((pointer.y-board.y)/size));
            if(map.CanMoveSummon(current.Id,cell))StartCoroutine(MoveSpiritWithReactions(current,cell));e.Use();
        }
    }
}
