using UnityEngine;

public sealed partial class BattleMap
{
    // Only two participants currently exist, so nesting and multiple passengers are impossible.
    int containedActor=-1;
    public System.Action<bool> ContainmentEnded;
    public bool HasContainedCreature=>containedActor>=0;
    public bool IsContained(bool player)=>containedActor==(player?0:1);
    public int OccupiedSpaces(bool player)=>!OnScene(player?0:-1)||IsContained(player)||IsCarried(player)?0:player?PlayerSpaces:MonsterSpaces;
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
        ContainmentEnded?.Invoke(!player);return true;
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
        public Vector2Int Host,Target;public bool Shared;
        public ReleasePlan(Vector2Int host,Vector2Int target,bool shared=false){Host=host;Target=target;Shared=shared;}
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
                var host=new Vector2Int(hx,hy);if(!player&&Riding&&host!=originalHost)continue;if(!SummonFootprintClear(host,hostSpaces,!player?riderMountId:0))continue;int dx=hx-originalHost.x,dy=hy-originalHost.y,cost=dx*dx+dy*dy;
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
    // A forced true-form change and release must be planned together: a smaller
    // host can free the very space needed by its passenger. Preview is reversible.
    public System.Collections.Generic.List<ReleasePlan> RestoredHostReleasePlans(int spaces,int sizeIndex=-1)
    {
        if(!HasContainedCreature||spaces<1||spaces>4)return new System.Collections.Generic.List<ReleasePlan>();
        bool hostPlayer=!IsContained(true);int body=playerBodySpaces,monster=MonsterSpaces,pr=playerRecoverySpaces,mr=monsterRecoverySpaces,mount=riderMountId;
        bool pd=PlayerAquatic.Submerged,md=MonsterAquatic.Submerged,rd=RescueAquatic.Submerged;
        try{if(hostPlayer){if(Riding&&!(sizeIndex>=0?sizeIndex<RiderMount.Size:spaces<RiderMount.Spaces))riderMountId=0;playerBodySpaces=spaces;playerRecoverySpaces=0;}else{MonsterSpaces=spaces;monsterRecoverySpaces=0;}var plans=MandatoryReleasePlans();return plans.Count>0?plans:SharedReleasePlans();}
        finally{playerBodySpaces=body;MonsterSpaces=monster;playerRecoverySpaces=pr;monsterRecoverySpaces=mr;riderMountId=mount;PlayerAquatic.TrySetSubmerged(pd,WaterCells(Player,PlayerSpaces),PlayerSpaces*PlayerSpaces);MonsterAquatic.TrySetSubmerged(md,WaterCells(Monster,MonsterSpaces),MonsterSpaces*MonsterSpaces);if(RescuePosition.HasValue)RescueAquatic.TrySetSubmerged(rd,WaterCells(RescuePosition.Value,RescueSpaces),RescueSpaces*RescueSpaces);}
    }
    public bool ApplyRestoredHostRelease(ReleasePlan chosen,int spaces,int sizeIndex=-1)
    {
        bool found=false;foreach(var plan in RestoredHostReleasePlans(spaces,sizeIndex))if(plan.Host==chosen.Host&&plan.Target==chosen.Target){chosen=plan;found=true;break;}if(!found)return false;
        bool player=IsContained(true),hostPlayer=!player;var oldHost=hostPlayer?Player:Monster;int oldSpaces=hostPlayer?PlayerSpaces:MonsterSpaces,body=playerBodySpaces,monster=MonsterSpaces,pr=playerRecoverySpaces,mr=monsterRecoverySpaces,mount=riderMountId;
        if(hostPlayer){if(Riding&&!(sizeIndex>=0?sizeIndex<RiderMount.Size:spaces<RiderMount.Spaces))riderMountId=0;playerBodySpaces=spaces;playerRecoverySpaces=0;}else{MonsterSpaces=spaces;monsterRecoverySpaces=0;}
        // The same validated root-footprint plan is now an ordinary release plan.
        if(chosen.Shared){containedActor=-1;if(player){Monster=chosen.Host;Player=chosen.Target;}else{Player=chosen.Host;Monster=chosen.Target;}ContainmentEnded?.Invoke(hostPlayer);}
        else if(!ApplyMandatoryReleasePlan(chosen)){playerBodySpaces=body;MonsterSpaces=monster;playerRecoverySpaces=pr;monsterRecoverySpaces=mr;riderMountId=mount;return false;}SyncWaterPositions();
        MovementObserved?.Invoke(hostPlayer?0:-1,oldHost,chosen.Host,oldSpaces,hostPlayer?PlayerSpaces:MonsterSpaces,true);
        MovementObserved?.Invoke(player?0:-1,oldHost,chosen.Target,0,player?PlayerSpaces:MonsterSpaces,true);return true;
    }
    static int SharedCells(Vector2Int a,int n,Vector2Int b,int m)=>n<1||m<1?0:System.Math.Max(0,System.Math.Min(a.x+n,b.x+m)-System.Math.Max(a.x,b.x))*System.Math.Max(0,System.Math.Min(a.y+n,b.y+m)-System.Math.Max(a.y,b.y));
    int ForcedSharingCost(int actor,Vector2Int cell,int spaces,bool includeMount=false)
    {
        int cost=0;foreach(var body in TransitBodies(actor))cost+=SharedCells(cell,spaces,body.Cell,body.Spaces);
        foreach(var body in AllRemains())cost+=SharedCells(cell,spaces,new Vector2Int(body.X,body.Y),RemainsSpaces(body));if(includeMount&&actor==0&&Riding)cost+=SharedCells(cell,spaces,new Vector2Int(RiderMount.X,RiderMount.Y),RiderMount.Spaces);return cost;
    }
    System.Collections.Generic.List<ReleasePlan> SharedReleasePlans()
    {
        var plans=new System.Collections.Generic.List<ReleasePlan>();if(!HasContainedCreature)return plans;bool passengerPlayer=IsContained(true);int hostActor=passengerPlayer?-1:0;
        var origin=passengerPlayer?Monster:Player;int hn=passengerPlayer?MonsterSpaces:PlayerSpaces,tn=passengerPlayer?PlayerSpaces:MonsterSpaces;
        int bestShared=int.MaxValue,bestMove=int.MaxValue,bestExit=int.MaxValue;var targetCosts=new int[Size-tn+1,Size-tn+1];for(int y=0;y<=Size-tn;y++)for(int x=0;x<=Size-tn;x++)targetCosts[x,y]=ForcedSharingCost(hostActor,new Vector2Int(x,y),tn);
        // Only a fallback after the complete no-sharing search failed. The two
        // released bodies remain distinct; existing bystanders may be shared.
        for(int hy=0;hy<=Size-hn;hy++)for(int hx=0;hx<=Size-hn;hx++){
            var host=new Vector2Int(hx,hy);if(hostActor==0&&Riding&&host!=origin)continue;int move=(hx-origin.x)*(hx-origin.x)+(hy-origin.y)*(hy-origin.y);int hostCost=ForcedSharingCost(hostActor,host,hn);
            for(int ty=0;ty<=Size-tn;ty++)for(int tx=0;tx<=Size-tn;tx++){
                var target=new Vector2Int(tx,ty);if(Overlap(host,hn,target,tn))continue;int shared=hostCost+targetCosts[tx,ty],exit=System.Math.Max(5,BodyDistance(host,hn,target,tn));
                if(shared>bestShared||shared==bestShared&&(move>bestMove||move==bestMove&&exit>bestExit))continue;
                if(shared<bestShared||move<bestMove||exit<bestExit){plans.Clear();bestShared=shared;bestMove=move;bestExit=exit;}plans.Add(new ReleasePlan(host,target,true));
            }
        }return plans;
    }
    public bool RestoreTrueFormFootprint(bool player,int spaces,int sizeIndex)
    {
        if(HasContainedCreature||spaces<1||spaces>4||spaces>Size)return false;var from=player?Player:Monster;int old=player?PlayerSpaces:MonsterSpaces;Vector2Int anchor=from;bool legal;
        bool stay=player&&Riding&&sizeIndex<RiderMount.Size;
        if(stay)legal=true;
        else if(player&&Riding)legal=FindDismount(spaces,out anchor);
        else legal=FindRecovery(from,old,spaces,player?Monster:Player,OccupiedSpaces(!player),out anchor);
        if(!legal){int bestShared=int.MaxValue,bestMove=int.MaxValue;for(int y=0;y<=Size-spaces;y++)for(int x=0;x<=Size-spaces;x++){
            var p=new Vector2Int(x,y);int cost=ForcedSharingCost(player?0:-1,p,spaces,player&&Riding&&!stay),move=(x-from.x)*(x-from.x)+(y-from.y)*(y-from.y);if(cost<bestShared||cost==bestShared&&move<bestMove){bestShared=cost;bestMove=move;anchor=p;}
        }}
        if(player){if(Riding&&!stay)riderMountId=0;playerBodySpaces=spaces;Player=anchor;playerRecoverySpaces=0;}else{MonsterSpaces=spaces;Monster=anchor;monsterRecoverySpaces=0;}
        SyncWaterPositions();MovementObserved?.Invoke(player?0:-1,from,anchor,old,player?PlayerSpaces:MonsterSpaces,true);return true;
    }
    public bool ApplyMandatoryReleasePlan(ReleasePlan chosen,int nearbyFeet=5)
    {
        foreach(var plan in MandatoryReleasePlans(nearbyFeet))if(plan.Host==chosen.Host&&plan.Target==chosen.Target){
            bool player=IsContained(true);containedActor=-1;
            if(player){Monster=plan.Host;Player=plan.Target;}else{Player=plan.Host;Monster=plan.Target;}
            ContainmentEnded?.Invoke(!player);
            return true;
        }
        return false;
    }
    void ClearContainment()
    {
        if(!HasContainedCreature)return;
        bool hostPlayer=!IsContained(true);containedActor=-1;ContainmentEnded?.Invoke(hostPlayer);
        // Only called when leaving combat for deployment; reset both legal starting anchors.
        Player=Vector2Int.zero;Monster=new Vector2Int(Size-MonsterSpaces,Size-MonsterSpaces);
    }
}
