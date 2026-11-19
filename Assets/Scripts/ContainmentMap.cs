using UnityEngine;

public sealed partial class BattleMap
{
    // Only two participants currently exist, so nesting and multiple passengers are impossible.
    int containedActor=-1;
    public bool HasContainedCreature=>containedActor>=0;
    public bool IsContained(bool player)=>containedActor==(player?0:1);
    public int OccupiedSpaces(bool player)=>IsContained(player)||IsCarried(player)?0:player?PlayerSpaces:MonsterSpaces;
    void SyncContainedPosition(){if(IsContained(true))Player=Monster;else if(IsContained(false))Monster=Player;SyncWaterPositions();}
    public bool BeginContainment(bool targetPlayer)
    {
        if(!Locked||HasContainedCreature||DistanceFeet>5)return false;
        if(targetPlayer&&Riding&&!DismountRider())return false;
        containedActor=targetPlayer?0:1;SyncContainedPosition();return true;
    }
    // This checks geometry only. Escape actions, movement costs and creature-size permissions
    // belong to the particular swallow effect, not to map placement.
    public bool CanReleaseContainedAt(Vector2Int anchor,int rangeFeet)
    {
        if(!HasContainedCreature||rangeFeet<0)return false;
        bool player=IsContained(true);int spaces=player?PlayerSpaces:MonsterSpaces;
        var host=player?Monster:Player;int hostSpaces=player?MonsterSpaces:PlayerSpaces;
        if(!Inside(anchor,spaces)||!SummonFootprintClear(anchor,spaces)||Overlap(anchor,spaces,host,hostSpaces)||BodyDistance(anchor,spaces,host,hostSpaces)>rangeFeet)return false;
        int saved=containedActor;containedActor=-1;
        try{return RecoverySafe(player?anchor:Player,player?Monster:anchor);}
        finally{containedActor=saved;}
    }
    public bool ReleaseContainedAt(Vector2Int anchor,int rangeFeet)
    {
        if(!CanReleaseContainedAt(anchor,rangeFeet))return false;
        bool player=IsContained(true);containedActor=-1;
        if(player)Player=anchor;else Monster=anchor;
        return true;
    }
    // Mandatory release may exceed the normal range, but only to the nearest legal footprint.
    public int MandatoryReleaseRange(int nearbyFeet=5)
    {
        if(!HasContainedCreature||nearbyFeet<0)return -1;
        bool player=IsContained(true);int spaces=player?PlayerSpaces:MonsterSpaces;
        var host=player?Monster:Player;int hostSpaces=player?MonsterSpaces:PlayerSpaces;
        int nearest=int.MaxValue;
        for(int y=0;y<=Size-spaces;y++)for(int x=0;x<=Size-spaces;x++){
            var cell=new Vector2Int(x,y);
            if(!CanReleaseContainedAt(cell,int.MaxValue))continue;
            nearest=System.Math.Min(nearest,BodyDistance(cell,spaces,host,hostSpaces));
        }
        return nearest==int.MaxValue?-1:System.Math.Max(nearbyFeet,nearest);
    }
    public bool CanMandatoryReleaseAt(Vector2Int anchor,int nearbyFeet=5)
    {
        int range=MandatoryReleaseRange(nearbyFeet);
        return range>=0&&CanReleaseContainedAt(anchor,range);
    }
    public bool MandatoryReleaseAt(Vector2Int anchor,int nearbyFeet=5)
    {
        int range=MandatoryReleaseRange(nearbyFeet);
        return range>=0&&ReleaseContainedAt(anchor,range);
    }
    public struct ReleasePlan
    {
        public Vector2Int Host,Target;
        public ReleasePlan(Vector2Int host,Vector2Int target){Host=host;Target=target;}
    }
    public System.Collections.Generic.List<ReleasePlan> MandatoryReleasePlans(int nearbyFeet=5)
    {
        var plans=new System.Collections.Generic.List<ReleasePlan>();
        if(!HasContainedCreature||nearbyFeet<0)return plans;
        bool player=IsContained(true);var oldPlayer=Player;var oldMonster=Monster;
        var originalHost=player?Monster:Player;int hostSpaces=player?MonsterSpaces:PlayerSpaces,targetSpaces=player?PlayerSpaces:MonsterSpaces;
        int bestMove=int.MaxValue,bestExit=int.MaxValue;
        try{
            for(int hy=0;hy<=Size-hostSpaces;hy++)for(int hx=0;hx<=Size-hostSpaces;hx++){
                var host=new Vector2Int(hx,hy);if(!SummonFootprintClear(host,hostSpaces))continue;int dx=hx-originalHost.x,dy=hy-originalHost.y,cost=dx*dx+dy*dy;
                if(cost>bestMove)continue;
                if(player)Monster=host;else Player=host;SyncContainedPosition();
                for(int ty=0;ty<=Size-targetSpaces;ty++)for(int tx=0;tx<=Size-targetSpaces;tx++){
                    var target=new Vector2Int(tx,ty);if(!CanReleaseContainedAt(target,int.MaxValue))continue;
                    int exit=System.Math.Max(nearbyFeet,BodyDistance(target,targetSpaces,host,hostSpaces));
                    if(cost<bestMove||exit<bestExit){plans.Clear();bestMove=cost;bestExit=exit;}
                    if(cost==bestMove&&exit==bestExit)plans.Add(new ReleasePlan(host,target));
                }
            }
        }finally{Player=oldPlayer;Monster=oldMonster;}
        return plans;
    }
    public bool ApplyMandatoryReleasePlan(ReleasePlan chosen,int nearbyFeet=5)
    {
        foreach(var plan in MandatoryReleasePlans(nearbyFeet))if(plan.Host==chosen.Host&&plan.Target==chosen.Target){
            bool player=IsContained(true);containedActor=-1;
            if(player){Monster=plan.Host;Player=plan.Target;}else{Player=plan.Host;Monster=plan.Target;}
            return true;
        }
        return false;
    }
    void ClearContainment()
    {
        if(!HasContainedCreature)return;
        containedActor=-1;
        // Only called when leaving combat for deployment; reset both legal starting anchors.
        Player=Vector2Int.zero;Monster=new Vector2Int(Size-MonsterSpaces,Size-MonsterSpaces);
    }
}
