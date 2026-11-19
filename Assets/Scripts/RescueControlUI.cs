using System.Collections;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly RescueCarrier rescueCarrier=new RescueCarrier();
    string rescueStrength="10",rescueDexterity="10",rescueMaxHp="10",rescueSpeed="30",rescueBody="150",rescueEquipment="0",rescueMessage="选择地图边缘的合法空位入场。";
    bool rescueDropMode;int rescueSize=2;Vector2 rescueScroll;
    bool RescueConfiguration(out int strength,out int speed,out int body,out int gear)
    {
        strength=speed=body=gear=0;int dexterity,maxHp;
        return int.TryParse(rescueMaxHp,out maxHp)&&maxHp>=1&&int.TryParse(rescueDexterity,out dexterity)&&dexterity>=3&&dexterity<=20&&int.TryParse(rescueStrength,out strength)&&strength>=3&&strength<=20&&int.TryParse(rescueSpeed,out speed)&&speed>=0
            &&int.TryParse(rescueBody,out body)&&body>0&&int.TryParse(rescueEquipment,out gear)&&gear>=0&&EquipmentLoadRules.Allowed(strength,rescueSize,gear,0);
    }
    void DrawRescueMarker(Rect board)
    {
        if(!map.RescuePosition.HasValue)return;
        var rect=BodyRect(board,map.RescuePosition.Value,map.RescueSpaces);Fill(rect,new Color(.12f,.5f,.31f,.9f));
        GUI.Label(rect,"援",new GUIStyle(centered){fontSize=Mathf.Max(12,(int)(rect.width*.6f)),normal={textColor=Color.white}});
    }
    void DrawRescueControl()
    {
        Surface(new Rect(350,110,900,745),paperTexture);if(Btn(new Rect(1140,125,90,46),"关闭")){panel="";return;}
        GUILayout.BeginArea(new Rect(375,185,390,630));rescueScroll=GUILayout.BeginScrollView(rescueScroll);
        GUILayout.Label("救援搬运者",heading);GUILayout.Label("从地图边缘入场；点击地图中的合法目标格移动。资源属于救援者。",small);
        bool old=GUI.enabled;GUI.enabled=!rolling&&combat.Aftermath&&!map.RescuePosition.HasValue;
        GUILayout.Label("力量（3～20整数）",small);rescueStrength=IntegerField(rescueStrength,false);
        GUILayout.Label("敏捷（3～20整数，豁免无熟练）",small);rescueDexterity=IntegerField(rescueDexterity,false);
        GUILayout.Label("最大HP（至少1的整数）",small);rescueMaxHp=IntegerField(rescueMaxHp,false);
        GUILayout.Label("体型",small);rescueSize=GUILayout.SelectionGrid(rescueSize,CreatureProfile.SizeNames,3,new GUIStyle(button){fontSize=15,fixedHeight=32});
        GUILayout.Label("移动力（非负整数尺）",small);rescueSpeed=IntegerField(rescueSpeed,false);
        GUILayout.Label("体重（正整数磅）",small);rescueBody=IntegerField(rescueBody,false);
        GUILayout.Label("装备总重（非负整数磅）",small);rescueEquipment=IntegerField(rescueEquipment,false);
        GUI.enabled=old;
        if(map.RescuePosition.HasValue){
            DrawRescueLifeControls();DrawRescueCargoControls();
            GUILayout.Label("HP "+rescueCarrier.Hp+" / "+rescueCarrier.MaxHp, label);
            GUILayout.Label("动作 "+rescueCarrier.ActionsRemaining+" · 剩余移动 "+rescueCarrier.MovementRemaining+"尺 · 当前速度 "+rescueCarrier.EffectiveSpeed+"尺",label);
            GUI.enabled=!rolling&&combat.Aftermath&&rescueCarrier.TurnActive&&rescueCarrier.ActionsRemaining>0;
            if(GUILayout.Button("疾走（1动作）",button))rescueCarrier.Dash();
            GUI.enabled=!rolling&&combat.Aftermath&&rescueCarrier.TurnActive&&rescueCarrier.Prone;
            if(GUILayout.Button("起身（半速移动）",button))rescueCarrier.Stand();
            GUI.enabled=!rolling&&combat.Aftermath;
            if(GUILayout.Button("结束救援回合 / 推进6秒",button))StartCoroutine(NextRescueTurn());
            GUI.enabled=old;
        }
        GUILayout.Label(rescueMessage,small);GUILayout.EndScrollView();GUILayout.EndArea();
        var board=new Rect(800,250,400,400);Surface(new Rect(795,245,410,410),paperTexture);
        float size=board.width/map.Size;int strength,speed,body,gear;bool config=RescueConfiguration(out strength,out speed,out body,out gear);
        for(int y=0;y<map.Size;y++)for(int x=0;x<map.Size;x++){
            var cell=new Vector2Int(x,y);var rect=CellRect(board,cell);
            Fill(rect,map.IsWater(cell)?new Color(.06f,.38f,.62f,.58f):new Color(.5f,.35f,.12f,.12f));
            bool legal=map.RescuePosition.HasValue?(rescueDropMode?map.CanDropRescueCorpse(cell):map.CanMoveRescue(cell)&&rescueCarrier.CanMove(map.RescueMovementCost(cell))):config&&map.CanDeployRescue(cell,CreatureProfile.SpacesForSize(rescueSize),true);
            if(legal)Fill(rect,new Color(.1f,.65f,.25f,.25f));
        }
        for(int i=0;i<=map.Size;i++){Fill(new Rect(board.x+i*size,board.y,1,board.height),new Color(.3f,.2f,.1f,.5f));Fill(new Rect(board.x,board.y+i*size,board.width,1),new Color(.3f,.2f,.1f,.5f));}
        if(!map.IsContained(true)&&!map.IsCarried(true))Token(BodyRect(board,map.Player,map.PlayerSpaces),true);
        if(!map.IsContained(false)&&!map.IsCarried(false))Token(BodyRect(board,map.Monster,map.MonsterSpaces),false);
        DrawRetainedRemains(board);DrawRescueMarker(board);
        var e=Event.current;var point=BoardPointer();
        if(!rolling&&combat.Aftermath&&e.type==EventType.MouseDown&&e.button==0&&board.Contains(point)){
            var cell=new Vector2Int((int)((point.x-board.x)/size),(int)((point.y-board.y)/size));
            if(!map.RescuePosition.HasValue){
                if(config&&map.CanDeployRescue(cell,CreatureProfile.SpacesForSize(rescueSize),true)&&rescueCarrier.Configure(strength,rescueSize,speed,body,gear,int.Parse(rescueDexterity),int.Parse(rescueMaxHp))&&map.DeployRescue(cell,CreatureProfile.SpacesForSize(rescueSize),true)){
                    rescueCarrier.BeginTurn(worldRounds);rescueMessage="救援者已从边缘入场，可点击合法位置移动。";
                    history.Insert(0,"救援搬运者从地图边缘入场，力量"+strength+"，敏捷"+rescueCarrier.Dexterity+"，移动力"+speed+"尺。");
                }else rescueMessage="不能入场：请检查配置，选择边缘且能容纳完整占地的空位。";
            }else if(rescueDropMode){if(rescueCarrier.Drop(()=>map.DropRescueCorpse(cell))){rescueDropMode=false;rescueMessage="对象已放在所选合法占地，未消耗动作。";}else rescueMessage="此处无法放下完整占地，请另选绿色位置。";}
            else if(map.CanMoveRescue(cell)&&rescueCarrier.CanMove(map.RescueMovementCost(cell)))StartCoroutine(MoveRescueThroughGrease(cell));
            else rescueMessage="移动失败：距离超出额度、占地冲突或路径被阻挡。";
            e.Use();
        }
        GUI.Label(new Rect(800,665,400,60),"绿色格可入场或移动；绿色“援”为救援者。",small);
    }
    void DrawRescueCargoControls()
    {
        bool old=GUI.enabled;
        if(rescueCarrier.Load.Loaded){
            GUILayout.Label((rescueCarrier.Load.Dragging?"拖行":"肩扛")+"："+(rescueCarrier.Load.TargetId==0?"玩家":rescueCarrier.Load.TargetId==-1?"木桩":map.Remains.Find(rescueCarrier.Load.TargetId.Value)?.Name??"旧躯体")+"，总重 "+WeightUnits.Format(rescueCarrier.Load.WeightOunces),small);
            GUI.enabled=!rolling&&rescueCarrier.TurnActive;
            if(GUILayout.Button(rescueDropMode?"取消放置选择":"放下搬运对象（免费）",button))rescueDropMode=!rescueDropMode;
        }else foreach(bool player in new[]{true,false}){
            var weight=player?playerWeight.Weight:monsterWeight.Weight;bool dead=player?combat.PlayerDead:combat.Hp==0;
            foreach(bool drag in new[]{false,true}){
                bool fits=CarryingRules.CanCarryOunces(rescueCarrier.Strength,rescueCarrier.Size,rescueCarrier.Weight.GearOunces,0)&&weight.Known&&(drag?CarryingRules.CanDragOunces(rescueCarrier.Strength,rescueCarrier.Size,weight.TotalOunces):CarryingRules.CanCarryOunces(rescueCarrier.Strength,rescueCarrier.Size,rescueCarrier.Weight.GearOunces,weight.TotalOunces));
                GUI.enabled=!rolling&&dead&&map.CanAttachRescueCorpse(player)&&rescueCarrier.ActionsRemaining>0&&fits;
                if(GUILayout.Button((drag?"拖行":"肩扛")+(player?"玩家":"木桩")+"遗体（1动作）",button)){
                    if(rescueCarrier.PickUp(player?0:-1,dead,weight,drag)&&map.AttachRescueCorpse(player,drag)){
                        rescueMessage="已开始"+(drag?"拖行":"肩扛")+"，消耗救援者1个动作。";history.Insert(0,rescueMessage);
                    }
                }
            }
        }
        GUI.enabled=old;DrawRescueRetainedCargo();
    }
    IEnumerator NextRescueTurn()
    {
        rolling=true;if(rescueCarrier.Conscious)yield return ResolveGreaseSave(BattleMap.RescueId,ActorGrease(BattleMap.RescueId),"救援回合末油脂");rescueCarrier.EndTurn();yield return AftermathTick();
        if(map.RescuePosition.HasValue)rescueCarrier.BeginTurn(worldRounds);
    }
}
