using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly SleetStormEffect sleetStorm=new SleetStormEffect();
    void EndSleetStorm(){sleetStorm.Clear();startStatuses.RemoveAll(e=>e.Kind==2);}
    bool SleetBody(int actor,out Vector2Int p,out int n)
    {
        string name;if(!HealingHandsTarget(actor,out p,out n,out name)||map.SceneOf(actor)!=sleetStorm.Scene)return false;
        return !((actor==0||actor==-1)&&map.IsContained(actor==0));
    }
    bool SleetLandCell(int x,int y)=>sleetStorm.Scene==map.Scene&&sleetStorm.Cell(x,y,worldRounds)&&!map.IsWater(new Vector2Int(x,y));
    string SleetStormError()
    {
        Vector2Int p;if(!VisionPoint(out p)||p.x<0||p.y<0||p.x>=map.Size||p.y>=map.Size)return "区域中心须为地图内整数坐标。";
        if(map.DistanceToCell(true,p)>150)return "雪雨暴中心超出150尺。";
        if(map.IsContained(true))return "吞咽内部不能在外部制造雪雨暴。";
        return worldRounds<0||worldRounds>long.MaxValue-10?"世界时间超出范围。":"";
    }
    void DrawSleetStormChoices()
    {
        visionColumn=IntegerField(visionColumn,false);visionRow=IntegerField(visionRow,false);
        GUILayout.Label("150尺内指定20尺半径区域，专注至多1分钟。区域重度遮蔽，陆地为困难地形；遮蔽与豁免也影响水面和潜水单位。出现时不立即豁免；每回合首次进入或在其中开始回合时，敏捷豁免失败倒地并失去专注。升环不增强。",small);
        GUILayout.Label("按项目规则，只熄灭普通火焰，魔法火焰维持；普通环境火焰对象暂未建立，熄火说明保留。",small);
    }
    void CastSleetStorm(Vector2Int p,int caster)
    {
        if(!sleetStorm.Begin(p.x,p.y,map.SceneOf(0),8L+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(caster)]),worldRounds))return;
        concentrationSpell="雪雨暴";concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=sleetStorm.Expires;SyncTimedBuffs();
        history.Insert(0,"雪雨暴：20尺半径重度遮蔽，陆地困难地形；进入或开始回合敏捷豁免失败倒地、失去专注，专注至多1分钟。魔法火焰保持原效果。");
    }
    void QueueSleetSave(int actor,Vector2Int p,int n,bool starting)
    {
        if(sleetStorm.Intersects(p.x,p.y,n,worldRounds)&&sleetStorm.Claim(actor,terrainTurn,starting,worldRounds))startStatuses.Add(new StartStatus{Actor=actor,Kind=2,Revision=sleetStorm.Revision});
    }
    void QueueSleetStart(int actor){Vector2Int p;int n;if(SleetBody(actor,out p,out n))QueueSleetSave(actor,p,n,true);}
    void ObserveSleetMovement(int actor,Vector2Int from,Vector2Int to,int oldSpaces,int spaces,bool landing)
    {
        if(!sleetStorm.Active(worldRounds))return;
        if(actor==BattleMap.RescueId&&map.RescueCargoPlayer.HasValue){bool player=map.RescueCargoPlayer.Value;var p=player?map.Player:map.Monster;int n=player?map.PlayerSpaces:map.MonsterSpaces;ObserveSleetMovement(player?0:-1,new Vector2Int(p.x-(to.x-from.x),p.y-(to.y-from.y)),p,n,n,landing);}
        Vector2Int body;int size;if(!SleetBody(actor,out body,out size))return;
        var path=landing?new List<Vector2Int>{to}:BattleMap.MovementAnchors(from,to);var previous=from;int n0=oldSpaces;if(from==to&&oldSpaces!=spaces)path.Add(to);
        foreach(var p in path){if(!sleetStorm.Intersects(previous.x,previous.y,n0,worldRounds)&&sleetStorm.Intersects(p.x,p.y,spaces,worldRounds))QueueSleetSave(actor,p,spaces,false);previous=p;n0=spaces;}
    }
    bool SleetPathNeeded(Vector2Int from,Vector2Int to,int spaces)
    {
        if(sleetStorm.Scene!=map.Scene||!sleetStorm.Active(worldRounds))return false;foreach(var p in BattleMap.MovementAnchors(from,to))if(sleetStorm.Intersects(p.x,p.y,spaces,worldRounds))return true;return false;
    }
    IEnumerator ResolveSleetSave(int actor,long revision)
    {
        if(revision!=sleetStorm.Revision||!sleetStorm.Active(worldRounds))yield break;
        Vector2Int p;int n;if(!SleetBody(actor,out p,out n))yield break;
        var failed=new HashSet<int>();yield return RollMappedAbilitySave(actor,0,sleetStorm.Dc,failed,"雪雨暴",1,new Vector2Int(sleetStorm.X,sleetStorm.Y));
        if(!failed.Contains(actor)){history.Insert(0,TransitActorName(actor)+"通过雪雨暴敏捷豁免。");yield break;}
        if(actor==0){combat.PlayerProne=true;EndConcentration("雪雨暴敏捷豁免失败，失去专注");}
        else if(actor==-1)combat.MonsterProne=true;
        else if(actor==BattleMap.RescueId)rescueCarrier.Prone=true;
        else {var unit=FindSummon(actor);if(unit!=null)unit.Prone=true;}
        SyncTimedBuffs();history.Insert(0,TransitActorName(actor)+"雪雨暴豁免失败，倒地；若正在专注则立即失去专注，不另作体质豁免。");
    }
    void DrawSleetStormArea(Rect board)
    {
        Vector2Int p=Vector2Int.zero;bool preview=panel=="spells"&&selectedSpellName=="Sleet Storm"&&VisionPoint(out p);
        for(int y=0;y<map.Size;y++)for(int x=0;x<map.Size;x++){if(sleetStorm.Scene==map.Scene&&sleetStorm.Cell(x,y,worldRounds))Fill(CellRect(board,new Vector2Int(x,y)),new Color(.55f,.75f,.9f,.25f));long dx=(long)x-p.x,dy=(long)y-p.y;if(preview&&dx*dx+dy*dy<=16)Fill(CellRect(board,new Vector2Int(x,y)),new Color(.7f,.85f,1f,.3f));}
    }
}
