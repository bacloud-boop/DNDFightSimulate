using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly CloudOfDaggersEffect cloudDaggers=new CloudOfDaggersEffect();
    sealed class DaggersEntry {public long Revision;public int Count;public List<int> Actors;public string Reason;public long MoveEvent;public int MoveStep;}
    readonly Queue<DaggersEntry> daggerEntries=new Queue<DaggersEntry>();
    // A cube conjured inside a swallowing creature remains inside that creature
    // until it leaves that footprint or the cube is teleported elsewhere.
    bool daggersInsideMonster;
    bool DaggersBody(int actor,out Vector2Int cell,out int spaces)
    {
        string name;if(!HealingHandsTarget(actor,out cell,out spaces,out name))return false;
        if(daggersInsideMonster)return actor==0||actor==-1;
        return !((actor==0||actor==-1)&&map.IsContained(actor==0));
    }
    bool InDaggers(int actor){Vector2Int p;int n;return DaggersBody(actor,out p,out n)&&cloudDaggers.Intersects(p.x,p.y,n,worldRounds);}
    bool DaggersEndAvailable(int actor)=>GustEndAvailable(actor)||GuardianEndAvailable(actor)||PackEndAvailable(actor)||InDaggers(actor)&&cloudDaggers.Available(actor,terrainTurn,worldRounds);
    void RefreshCloudDaggers(){if(daggersInsideMonster&&(combat.Hp==0||!cloudDaggers.Intersects(map.Monster.x,map.Monster.y,map.MonsterSpaces,worldRounds)))daggersInsideMonster=false;}
    void EndCloudDaggers(){cloudDaggers.Clear();daggerEntries.Clear();daggersInsideMonster=false;}
    string CloudDaggersError()
    {
        Vector2Int p;if(!VisionPoint(out p))return "中心格须位于地图内。";
        if(map.DistanceToCell(true,p)>60)return "匕首之云中心超出60尺。";
        if(map.IsContained(true)&&!BattleMap.Overlap(p,1,map.Monster,map.MonsterSpaces))return "吞咽内部只能在容纳者内部创造区域。";
        return worldRounds<0||worldRounds>long.MaxValue-10?"世界时间超出范围。":"";
    }
    void DrawCloudDaggersChoices(){if(cloudDaggers.Active(worldRounds)&&GUILayout.Button("控制现存区域（不重新施法）",button))OpenCloudDaggersControl();GUILayout.Label("5尺立方体，中心对齐所选格心；陆地、水面与潜水单位均按现有平面范围判定。出现、进入、回合结束或区域传送入体时受伤，每名生物每回合至多一次。后续自己的回合可用魔法动作传送区域30尺，只算落点。",small);visionColumn=IntegerField(visionColumn,false);visionRow=IntegerField(visionRow,false);}
    IEnumerator CastCloudDaggers(Vector2Int p,int slot)
    {
        cloudDaggers.Begin(p.x,p.y,slot,combat.Round,worldRounds);daggersInsideMonster=map.IsContained(true);concentrationSpell="匕首之云";concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=worldRounds+10;
        QueueDaggersArea("区域出现");if(PendingTerrainCount>0)yield return DrainGreaseEntries();
    }
    void QueueDaggers(IEnumerable<int> actors,string reason,long moveEvent=-1,int moveStep=-1)
    {
        var targets=new List<int>();foreach(int actor in actors)if(cloudDaggers.Claim(actor,terrainTurn,worldRounds))targets.Add(actor);
        if(targets.Count==0)return;
        if(moveEvent>=0)foreach(var pending in daggerEntries)if(pending.Revision==cloudDaggers.Revision&&pending.MoveEvent==moveEvent&&pending.MoveStep==moveStep){pending.Actors.AddRange(targets);return;}
        daggerEntries.Enqueue(new DaggersEntry{Revision=cloudDaggers.Revision,Count=cloudDaggers.Dice,Actors=targets,Reason=reason,MoveEvent=moveEvent,MoveStep=moveStep});
    }
    void QueueDaggersArea(string reason){RefreshCloudDaggers();var ids=new List<int>();foreach(int actor in BreathActors())if(InDaggers(actor))ids.Add(actor);QueueDaggers(ids,reason);}
    IEnumerator EndDaggersTurn(int actor){if(DaggersEndAvailable(actor)){if(InDaggers(actor))QueueDaggers(new[]{actor},"回合末");QueuePackEntry(actor,"回合末");QueueGuardianEnd(actor);QueueGustEnd(actor);if(PendingTerrainCount>0)yield return DrainGreaseEntries();}}
    void ObserveDaggersMovement(int actor,Vector2Int from,Vector2Int to,int oldSpaces,int spaces,bool landing)
    {
        if(actor==BattleMap.RescueId&&map.RescueCargoPlayer.HasValue){bool player=map.RescueCargoPlayer.Value;var cargo=player?map.Player:map.Monster;int size=player?map.PlayerSpaces:map.MonsterSpaces;ObserveDaggersMovement(player?0:-1,new Vector2Int(cargo.x-(to.x-from.x),cargo.y-(to.y-from.y)),cargo,size,size,landing);}
        RefreshCloudDaggers();Vector2Int actual;int n;if(!cloudDaggers.Active(worldRounds)||!DaggersBody(actor,out actual,out n))return;
        var path=landing?new List<Vector2Int>{to}:BattleMap.MovementAnchors(from,to);if(from==to&&oldSpaces!=spaces)path.Add(to);
        var previous=from;int previousSpaces=oldSpaces,step=0;
        foreach(var p in path){if(!cloudDaggers.Intersects(previous.x,previous.y,previousSpaces,worldRounds)&&cloudDaggers.Intersects(p.x,p.y,spaces,worldRounds))QueueDaggers(new[]{actor},"进入区域",terrainMovementEvent,step);previous=p;previousSpaces=spaces;step++;}
    }
    bool DaggersPathNeeded(Vector2Int from,Vector2Int to,int spaces)
    {
        if(PackPathNeeded(from,to,spaces))return true;if(!cloudDaggers.Active(worldRounds))return false;foreach(var p in BattleMap.MovementAnchors(from,to))if(cloudDaggers.Intersects(p.x,p.y,spaces,worldRounds))return true;return false;
    }
    IEnumerator ResolveDaggersEntry(DaggersEntry entry)
    {
        if(entry.Revision!=cloudDaggers.Revision||!cloudDaggers.Active(worldRounds))yield break;
        int amount=0;var rolls=new int[entry.Count];for(int i=0;i<rolls.Length;i++){rolls[i]=UnityEngine.Random.Range(1,5);amount+=rolls[i];}
        string saved=panel;panel="";outcomeDeferDepth++;
        try{
            dice="匕首之云 · "+entry.Reason;yield return PresentRoll(4,rolls,entry.Count+"d4 = "+amount+" 挥砍；无攻击检定及豁免");
            var reduced=new RayDamage();yield return ReduceRayDamage(true,amount,"挥砍","匕首之云",reduced);
            int selfDamage=0;foreach(int actor in entry.Actors){Vector2Int p;int n;string name;if(!HealingHandsTarget(actor,out p,out n,out name))continue;int applied=ApplyMappedAreaDamage(actor,0,reduced.Amount,2,"匕首之云 · "+entry.Reason);if(actor==0)selfDamage=applied;}
            if(selfDamage>0)yield return MaintainConcentration(selfDamage);ReconcileFallenRescue();RefreshBestialSpirit();NoticeCastingInterruption();
        }finally{outcomeDeferDepth--;panel=saved;CheckBattleOutcome();}
    }
    void DrawCloudDaggersArea(Rect board)
    {
        if(cloudDaggers.Active(worldRounds))Fill(CellRect(board,new Vector2Int(cloudDaggers.X,cloudDaggers.Y)),new Color(.65f,.25f,.85f,.48f));
        Vector2Int p;if(panel=="spells"&&selectedSpellName=="Cloud of Daggers"&&VisionPoint(out p))Fill(CellRect(board,p),new Color(.8f,.4f,.95f,.5f));
    }
}
