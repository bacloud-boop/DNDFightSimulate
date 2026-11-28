using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly GustWindEffect gustWind=new GustWindEffect();
    readonly BreathChoice gustChoice=new BreathChoice();Vector2 gustScroll;
    readonly Dictionary<int,long> gustEndTurns=new Dictionary<int,long>();
    void InitializeGustWind(){map.AgainstGust=(actor,from,to,n)=>actor!=0&&(!map.Riding||map.RiderMount.Id!=actor)&&!(actor==BattleMap.RescueId&&map.RescueCargoId==0)&&GustMover(actor,true)&&GustWindEffect.Against(CurrentGust(),from.x,from.y,to.x,to.y,n,new BreathBody(0,map.Player.x,map.Player.y,map.PlayerSpaces));}
    void EndGustWind(){gustWind.Clear();gustEndTurns.Clear();startStatuses.RemoveAll(e=>e.Kind==5);}
    BreathTemplate CurrentGust()=>gustWind.Template(map.Player.x,map.Player.y,map.PlayerSpaces,worldRounds);
    bool GustMover(int actor,bool ignoreDiving=false)=>gustWind.Active(worldRounds)&&(ignoreDiving||!ActorDiving(actor))&&map.SameScene(0,actor)&&!map.IsContained(true)&&!((actor==0||actor==-1)&&(map.IsContained(actor==0)||map.IsCarried(actor==0)));
    bool InGust(int actor)
    {
        Vector2Int p;int n;string name;return actor!=0&&GustMover(actor)&&HealingHandsTarget(actor,out p,out n,out name)&&CurrentGust().Intersects(p.x,p.y,n);
    }
    bool PlayerGustTemplate(out BreathTemplate line)
    {
        int n=map.PlayerSpaces;if(gustChoice.Offset<0)gustChoice.Offset=n;gustChoice.Offset=Math.Max(0,Math.Min(gustChoice.Offset,n*2));
        if(gustChoice.AimX<0){gustChoice.AimX=map.Player.x+n+20;gustChoice.AimY=map.Player.y+n*.5;}
        return BreathTemplate.TryCreate(map.Player.x,map.Player.y,n,gustChoice.Edge,gustChoice.Offset,gustChoice.AimX,gustChoice.AimY,true,out line,false,60,10);
    }
    string GustWindError()
    {
        BreathTemplate line;if(map.IsContained(true))return "吞咽内部无法向外制造风线。";
        if(!PlayerGustTemplate(out line))return "请选择朝外的有效风线方向。";
        return worldRounds<0||worldRounds>long.MaxValue-10?"世界时间超出范围。":"";
    }
    IEnumerator CastGustWind(BreathTemplate line,int edge,int offset,int caster)
    {
        if(!gustWind.Begin(line,edge,offset,8L+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(caster)]),worldRounds,terrainTurn))yield break;
        concentrationSpell="造风术";concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=gustWind.Expires;
        RefreshGustClouds();foreach(int actor in BreathActors())if(InGust(actor))QueueGust(actor);
        if(PendingTerrainCount>0)yield return DrainGreaseEntries();
        history.Insert(0,"造风术：60尺×10尺风线，创建与目标回合末力量豁免，失败沿风线推离15尺；进入和回合开始不另作豁免。潜水不受影响。");
    }
    bool GustEndAvailable(int actor)=>InGust(actor)&&(!gustEndTurns.ContainsKey(actor)||gustEndTurns[actor]!=terrainTurn);
    void QueueGust(int actor){startStatuses.Add(new StartStatus{Actor=actor,Kind=5,Revision=gustWind.Revision});}
    void QueueGustEnd(int actor){if(GustEndAvailable(actor)){gustEndTurns[actor]=terrainTurn;QueueGust(actor);}}
    IEnumerator ResolveGustSave(int actor,long revision)
    {
        if(revision!=gustWind.Revision||!gustWind.Active(worldRounds)||!GustMover(actor))yield break;
        double dx=gustWind.Dx,dy=gustWind.Dy;var failed=new HashSet<int>();yield return RollMappedAbilitySave(actor,0,gustWind.Dc,failed,"造风术",0);
        if(!failed.Contains(actor))yield break;
        if(actor==0&&map.Riding&&!map.DismountRider())yield break;
        Vector2Int start;int n;if(!map.MovementBody(actor,out start,out n))yield break;
        var last=start;int pushed=0;outcomeDeferDepth++;
        try{for(int feet=1;feet<=15;feet++){
            var cell=new Vector2Int(start.x+Mathf.RoundToInt((float)(dx*feet/5)),start.y+Mathf.RoundToInt((float)(dy*feet/5)));
            if(cell==last||BattleMap.FeetBetween(start,cell)>15)continue;
            if(!map.ForceWindStep(actor,cell))break;last=cell;pushed=BattleMap.FeetBetween(start,last);
            // Resolve terrain before the next push step, even when the current
            // save came from the shared dispatcher. Restore its outer guard.
            if(PendingTerrainCount>0){bool draining=greaseResolving;greaseResolving=false;try{yield return DrainGreaseEntries();}finally{greaseResolving=draining;}}
            if(!BanishLiving(actor))break;
        }history.Insert(0,TransitActorName(actor)+"造风术力量豁免失败，沿风线推离"+pushed+"尺；遇阻停止，无碰撞伤害、移动消耗或借机攻击。");
        }finally{outcomeDeferDepth--;CheckBattleOutcome();}
    }
    void RefreshGustClouds()
    {
        if(!gustWind.Active(worldRounds)||map.IsContained(true))return;var line=CurrentGust();int scene=map.SceneOf(0);
        if(scene==0){int dispersed=vision.Zones.RemoveAll(z=>z.Fog&&z.End>worldRounds&&GustWindEffect.OverlapsCloud(line,z.X/5+.5,z.Y/5+.5,z.Radius/5));if(dispersed>0)history.Insert(0,"造风术与云雾相交，整片云雾消散。");}
        if(stinkingCloud.Scene==scene&&stinkingCloud.Active(worldRounds)&&GustWindEffect.OverlapsCloud(line,stinkingCloud.X+.5,stinkingCloud.Y+.5,4)){EndStinkingCloud();history.Insert(0,"造风术与臭云相交，整片臭云消散；已产生的中毒仍持续到本回合结束。");}
    }
    bool GustPathNeeded(Vector2Int from,Vector2Int to)=>gustWind.Active(worldRounds)&&from!=to;
    string GustTurnError()
    {
        if(!gustWind.Active(worldRounds)||concentrationSpell!="造风术")return "没有现存的造风术。";
        if(terrainTurn==gustWind.CastTurn)return "只能在施法后的回合改变方向。";
        if(rolling||reactionPending||spellTargeting||draggingToken>=0||summonTurns.Active)return "请先完成当前操作。";
        if(terrainActor!=0||!PlayerTurnAllowed||!combat.BonusActionAvailable)return "当前没有可用的附赠动作。";
        return GustWindError();
    }
    void TurnGustWind()
    {
        BreathTemplate line;if(GustTurnError()!=""||!PlayerGustTemplate(out line)||!combat.TryUseBonusAction())return;
        gustWind.Turn(line,gustChoice.Edge,gustChoice.Offset,worldRounds,terrainTurn);RefreshGustClouds();history.Insert(0,"消耗附赠动作改变造风术方向；不重新施法、不刷新持续时间、不立即追加豁免。");
    }
    void DrawGustArea(Rect board)
    {
        if(!gustWind.Active(worldRounds)||!map.OnScene(0)||map.IsContained(true))return;var line=CurrentGust();
        for(int y=0;y<map.Size;y++)for(int x=0;x<map.Size;x++)if(line.Cell(x,y))Fill(CellRect(board,new Vector2Int(x,y)),new Color(.4f,.85f,.9f,.22f));
    }
}
