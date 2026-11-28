using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly CallLightningEffect callLightning=new CallLightningEffect();bool outdoorStorm;
    string cloudColumn="1",cloudRow="1";
    bool CloudPoint(out Vector2Int p){int x,y;bool valid=int.TryParse(cloudColumn,out x)&int.TryParse(cloudRow,out y);p=Vector2Int.zero;if(!valid||x<1||y<1||x>map.Size||y>map.Size)return false;p=new Vector2Int(x-1,y-1);return true;}
    string LightningPointError(Vector2Int cloud)
    {
        Vector2Int strike;if(!VisionPoint(out strike)||strike.x<0||strike.y<0||strike.x>=map.Size||strike.y>=map.Size)return "落雷列／行须为地图内整数。";
        if(BattleMap.FeetBetween(cloud,strike)>60)return "落雷点须位于云团60尺半径投影内。";
        return VisibilityAt(true,strike,1)>=2?"必须看见落雷点。":"";
    }
    string CallLightningError()
    {
        Vector2Int cloud;if(!CloudPoint(out cloud))return "云团投影中心须为地图内整数坐标。";
        if(map.IsContained(true))return "吞咽内部不能向外召出云团。";
        if(map.DistanceToCell(true,cloud)>120||VisibilityAt(true,cloud,1)>=2)return "云团中心须在120尺内可见。";
        if(worldRounds<0||worldRounds>long.MaxValue-100)return "世界时间超出范围。";
        return LightningPointError(cloud);
    }
    string CallLightningRepeatError()
    {
        if(!callLightning.Active(worldRounds)||concentrationSpell!="召雷术")return "召雷术已结束。";
        if(rolling||reactionPending||spellTargeting||draggingToken>=0||summonTurns.Active)return "请先完成当前操作。";
        if(terrainActor!=0||!PlayerTurnAllowed||!(combat.Aftermath?combat.AftermathMagicActionAvailable:combat.MagicActionAvailable))return "当前没有可用的魔法动作。";
        if(map.SceneOf(0)!=callLightning.Scene||map.Scene!=callLightning.Scene||map.IsContained(true))return "无法从其他位面或吞咽内部控制该云团。";
        return LightningPointError(new Vector2Int(callLightning.X,callLightning.Y));
    }
    void DrawCallLightningChoices()
    {
        GUILayout.Label("云团投影中心（列／行）",small);cloudColumn=IntegerField(cloudColumn,false);cloudRow=IntegerField(cloudRow,false);
        GUILayout.Label("落雷点（列／行）",small);visionColumn=IntegerField(visionColumn,false);visionRow=IntegerField(visionRow,false);
        GUILayout.Label("云团半径60尺，中心须在120尺内可见；落雷点须可见且在云下，5尺内所有生物敏捷豁免。三环3d10闪电，逐环＋1d10，成功半伤；施法时室外风暴额外＋1d10。专注至多10分钟。云团投影可超出棋盘边缘。",small);
        if(!callLightning.Active(worldRounds))return;
        GUILayout.Label("现存云团：("+(callLightning.X+1)+","+(callLightning.Y+1)+")，"+callLightning.Dice+"d10；剩余 "+(callLightning.Expires-worldRounds)+" 回合。再次落雷不重新施法、不发声、不刷新时间。",small);
        string error=CallLightningRepeatError();bool enabled=GUI.enabled;GUI.enabled=enabled&&error=="";if(GUILayout.Button("现存云团再次落雷（魔法动作）",button))StartCoroutine(RepeatCallLightning());GUI.enabled=enabled;if(error!="")GUILayout.Label(error,small);
    }
    IEnumerator CastCallLightning(Vector2Int cloud,Vector2Int strike,int caster,int slot)
    {
        if(!callLightning.Begin(cloud.x,cloud.y,map.SceneOf(0),slot,outdoorStorm,8L+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(caster)]),worldRounds))yield break;
        concentrationSpell="召雷术";concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=callLightning.Expires;
        history.Insert(0,"召雷术云团已出现，"+callLightning.Dice+"d10闪电"+(outdoorStorm?"（施法时室外风暴强化）":"")+"，专注至多10分钟。");yield return ResolveCallLightning(strike);
    }
    IEnumerator RepeatCallLightning()
    {
        Vector2Int strike;if(CallLightningRepeatError()!=""||!VisionPoint(out strike)||!combat.TryUseAftermathMagicAction())yield break;
        bool busy=rolling;string saved=panel;rolling=true;panel="";try{yield return ResolveCallLightning(strike);}finally{rolling=busy;panel=saved;}
    }
    IEnumerator ResolveCallLightning(Vector2Int strike)
    {
        if(!callLightning.Active(worldRounds)||map.SceneOf(0)!=callLightning.Scene||map.IsContained(true))yield break;
        var targets=new List<int>();foreach(int actor in BreathActors()){Vector2Int p;int n;string name;if(map.SceneOf(actor)!=callLightning.Scene||!HealingHandsTarget(actor,out p,out n,out name)||(actor==0||actor==-1)&&map.IsContained(actor==0))continue;if(BattleMap.BodyDistance(strike,1,p,n)<=5)targets.Add(actor);}
        int sum=0;var rolls=new int[callLightning.Dice];for(int i=0;i<rolls.Length;i++){rolls[i]=Random.Range(1,11);sum+=rolls[i];}yield return PresentRoll(10,rolls,"召雷术："+rolls.Length+"d10 = "+sum+" 闪电");
        var reduced=new RayDamage();yield return ReduceRayDamage(true,sum,"闪电","召雷术",reduced);int self=0;outcomeDeferDepth++;
        try{foreach(int actor in targets){var failed=new HashSet<int>();yield return RollMappedAbilitySave(actor,0,callLightning.Dc,failed,"召雷术",1,strike);int damage=ApplyMappedAreaDamage(actor,0,failed.Contains(actor)?reduced.Amount:reduced.Amount/2,7,"召雷术");if(actor==0)self=damage;}
            yield return MaintainConcentration(self);RefreshBestialSpirit();ReconcileFallenRescue();
        }finally{outcomeDeferDepth--;CheckBattleOutcome();}
    }
    void DrawCallLightningMap(Rect board)
    {
        Vector2Int center=Vector2Int.zero;bool preview=panel=="spells"&&selectedSpellName=="Call Lightning"&&CloudPoint(out center);if(!preview){if(!callLightning.Active(worldRounds)||callLightning.Scene!=map.Scene)return;center=new Vector2Int(callLightning.X,callLightning.Y);}
        for(int y=0;y<map.Size;y++)for(int x=0;x<map.Size;x++)if(BattleMap.FeetBetween(center,new Vector2Int(x,y))<=60)Fill(CellRect(board,new Vector2Int(x,y)),new Color(.25f,.4f,.7f,.1f));
        if(panel=="spells"&&selectedSpellName=="Call Lightning"){Vector2Int strike;if(VisionPoint(out strike))for(int y=0;y<map.Size;y++)for(int x=0;x<map.Size;x++)if(BattleMap.FeetBetween(strike,new Vector2Int(x,y))<=5)Fill(CellRect(board,new Vector2Int(x,y)),new Color(.5f,.7f,1f,.45f));}
    }
}
