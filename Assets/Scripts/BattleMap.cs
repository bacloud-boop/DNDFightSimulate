using UnityEngine;

public sealed partial class BattleMap
{
    readonly System.Collections.Generic.Dictionary<int,int> actorScenes=new System.Collections.Generic.Dictionary<int,int>();
    public int Scene {get;private set;}
    public int SceneOf(int actor){int scene;return actorScenes.TryGetValue(actor,out scene)?scene:0;}
    public bool SameScene(int a,int b)=>SceneOf(a)>=0&&SceneOf(a)==SceneOf(b);
    public bool OnScene(int actor)=>BodyPresent(actor)&&SceneOf(actor)>=0&&SceneOf(actor)==Scene;
    public void ViewScene(int scene){if(scene>=0)Scene=scene;}
    public void ViewActorScene(int actor){ViewScene(SceneOf(actor));}
    public void SetActorScene(int actor,int scene){if(scene==0)actorScenes.Remove(actor);else actorScenes[actor]=scene;}
    public void ClearScenes(){actorScenes.Clear();Scene=0;}
    public System.Collections.Generic.IEnumerable<SummonedUnit> SceneSummons {get{foreach(var u in summons)if(OnScene(u.Id))yield return u;}}
    public bool SceneBody(int actor,out Vector2Int cell,out int spaces)
    {
        if(actor==0){cell=Player;spaces=playerBodySpaces;return BodyPresent(0);}
        if(actor==-1){cell=Monster;spaces=MonsterSpaces;return BodyPresent(-1);}
        if(actor==RescueId){cell=RescuePosition??Vector2Int.zero;spaces=RescueSpaces;return BodyPresent(actor)&&RescuePosition.HasValue;}
        foreach(var u in summons)if(u.Id==actor){cell=new Vector2Int(u.X,u.Y);spaces=u.Spaces;return true;}
        cell=Vector2Int.zero;spaces=0;return false;
    }
    public bool RestoreSceneBody(int actor,int scene,Vector2Int home,out bool shared)
    {
        shared=false;Vector2Int old;int n;if(!SceneBody(actor,out old,out n)||scene<0)return false;
        int viewed=Scene;Scene=scene;Vector2Int landing=home;long best=long.MaxValue;
        try{
            for(int y=0;y<=Size-n;y++)for(int x=0;x<=Size-n;x++){
                var cell=new Vector2Int(x,y);if(!CanPlaceSummon(cell,n,actor))continue;
                long dx=x-home.x,dy=y-home.y,d=dx*dx+dy*dy;
                if(d<best){best=d;landing=cell;}
            }
            if(best==long.MaxValue){landing=new Vector2Int(System.Math.Max(0,System.Math.Min(Size-n,home.x)),System.Math.Max(0,System.Math.Min(Size-n,home.y)));shared=true;}
            SetActorScene(actor,scene);
            if(actor==0)Player=landing;else if(actor==-1)Monster=landing;else if(actor==RescueId)RescuePosition=landing;
            else foreach(var u in summons)if(u.Id==actor){u.X=landing.x;u.Y=landing.y;break;}
            SyncContainedPosition();SyncWaterPositions();return true;
        }finally{Scene=viewed;}
    }
    public void SeparatePlanarContainment(){if(HasContainedCreature){bool hostPlayer=!IsContained(true);containedActor=-1;ContainmentEnded?.Invoke(hostPlayer);}}
    public void SeparatePlanarRider(){riderMountId=0;}
    readonly System.Collections.Generic.HashSet<int> consumedBodies=new System.Collections.Generic.HashSet<int>();
    public bool BodyPresent(int actor)=>!consumedBodies.Contains(actor);
    public bool CanRestoreBodyAt(int actor,Vector2Int cell,int spaces){if(BodyPresent(actor)||!Inside(cell,spaces)||!CanPlaceSummon(cell,spaces,actor))return false;Vector2Int old;int n;if(actor==0){old=Player;n=PlayerSpaces;}else if(actor==-1){old=Monster;n=MonsterSpaces;}else if(actor==RescueId&&RescuePosition.HasValue){old=RescuePosition.Value;n=RescueSpaces;}else return false;SetReincarnationBody(actor,cell,spaces);consumedBodies.Remove(actor);try{return RecoverySafe(Player,Monster);}finally{consumedBodies.Add(actor);SetReincarnationBody(actor,old,n);}}
    public bool RestoreBodyAt(int actor,Vector2Int cell,int spaces){if(!CanRestoreBodyAt(actor,cell,spaces))return false;SetReincarnationBody(actor,cell,spaces);consumedBodies.Remove(actor);SyncWaterPositions();MovementObserved?.Invoke(actor,new Vector2Int(-100,-100),cell,0,spaces,true);return true;}
    public bool CanAnimateBody(SummonedUnit unit,int body){if(!Locked||unit==null||!BodyPresent(body)||RescueCargoId==body||(body==0||body==-1)&&IsContained(body==0))return false;Vector2Int cell;int n;if(!CargoBody(body,out cell,out n))return false;consumedBodies.Add(body);try{return CanAddSummon(unit,cell);}finally{consumedBodies.Remove(body);}}
    public bool AnimateBody(SummonedUnit unit,int body){if(!CanAnimateBody(unit,body))return false;Vector2Int cell;int n;CargoBody(body,out cell,out n);consumedBodies.Add(body);if(AddSummon(unit,cell))return true;consumedBodies.Remove(body);return false;}
    public static readonly string[] Names={"小地图","中地图","大地图"};
    public static readonly int[] Sizes={10,15,20};
    public System.Func<int,Vector2Int,bool> VoluntaryMovement;
    bool VoluntaryMovementAllowed(int id,Vector2Int cell)=>VoluntaryMovement==null||VoluntaryMovement(id,cell);
    public int Selected {get;private set;}
    public int Size => Sizes[Selected];
    // Positions identify the top-left occupied cell; footprint centers may lie on grid intersections.
    public Vector2Int Player {get;private set;} = Vector2Int.zero;
    public Vector2Int Monster {get;private set;} = new Vector2Int(9,9);
    int playerBodySpaces=1;
    public int PlayerSpaces {get=>Riding?RiderMount.Spaces:playerBodySpaces;private set=>playerBodySpaces=value;}
    public int MonsterSpaces {get;private set;}=1;
    public bool Locked {get;private set;}
    public const int MovementPerRound=30;
    public int PlayerSpeed {get;private set;}=MovementPerRound;
    public int MovementSpent {get;private set;}
    public int DashesThisTurn {get;private set;}
    public int MovementMaximum=>PlayerSpeed*(1+DashesThisTurn);
    public bool PlayerCrawling {get;set;}
    public int PlayerMovementCost(Vector2Int to)=>WaterMovementCost(Player,to,PlayerSpaces,PlayerSwimmingSpeed,PlayerCrawling,0);
    public bool SpendPlayerMovement(int feet){if(feet<0||MovementRemaining<feet)return false;MovementSpent+=feet;MovementRemaining-=feet;return true;}
    public void Dash(){DashesThisTurn++;MovementRemaining=System.Math.Max(0,MovementMaximum-MovementSpent);}
    public int MovementRemaining {get;private set;} = MovementPerRound;
    public void SetPlayerSpeed(int speed){PlayerSpeed=System.Math.Max(0,speed);MovementRemaining=System.Math.Max(0,MovementMaximum-MovementSpent);}
    public System.Func<int> MonsterTurnSpeed;
    long monsterMovementSpent,monsterDashGranted;int monsterMovementBase=30,monsterDashes;
    public int MonsterMovementRemaining {get=>(int)System.Math.Max(0,System.Math.Min(int.MaxValue,(MonsterTurnSpeed==null?(long)monsterMovementBase+monsterDashGranted:(long)System.Math.Max(0,MonsterTurnSpeed())*(1L+monsterDashes))-monsterMovementSpent));private set{monsterMovementBase=System.Math.Max(0,value);monsterMovementSpent=monsterDashGranted=0;monsterDashes=0;}}
    public void SpendMonsterMovement(int feet){monsterMovementSpent+=System.Math.Min(MonsterMovementRemaining,System.Math.Max(0,feet));}
    public void GrantMonsterDash(int speed){monsterDashes++;monsterDashGranted+=System.Math.Max(0,speed);}
    public void StartMonsterMovementTurn(int speed=30){MonsterMovementRemaining=speed;}
    public int TacticalMovementRemaining {get;private set;}
    public void GrantTacticalMovement() {TacticalMovementRemaining+=PlayerSpeed/2;}
    public int MovementBudget(bool tactical) => PlayerSpeed<=0?0:tactical?TacticalMovementRemaining:MovementRemaining;
    public Vector2Int RoundStartPlayer {get;private set;}
    public static int FeetBetween(Vector2Int a,Vector2Int b) => Mathf.RoundToInt(Vector2Int.Distance(a,b)*5);
    public static bool Overlap(Vector2Int a,int aSize,Vector2Int b,int bSize) => aSize>0&&bSize>0&&a.x<b.x+bSize&&a.x+aSize>b.x&&a.y<b.y+bSize&&a.y+aSize>b.y;
    public static int BodyDistance(Vector2Int a,int aSize,Vector2Int b,int bSize)
    {
        int dx=System.Math.Max(0,System.Math.Max(a.x-(b.x+bSize-1),b.x-(a.x+aSize-1)));
        int dy=System.Math.Max(0,System.Math.Max(a.y-(b.y+bSize-1),b.y-(a.y+aSize-1)));
        return Mathf.RoundToInt((float)System.Math.Sqrt((double)dx*dx+(double)dy*dy)*5);
    }
    public static bool LeavesReach(Vector2Int from,Vector2Int to,Vector2Int defender,int rangeFeet,int movingSpaces,int defenderSpaces)
    {
        if(BodyDistance(to,movingSpaces,defender,defenderSpaces)<=rangeFeet)return false;
        double radius=(rangeFeet+.49999)/5.0;
        for(int my=0;my<movingSpaces;my++)for(int mx=0;mx<movingSpaces;mx++)
        for(int dy=0;dy<defenderSpaces;dy++)for(int dx=0;dx<defenderSpaces;dx++){
            double ax=from.x+mx-defender.x-dx,ay=from.y+my-defender.y-dy;
            double vx=to.x-from.x,vy=to.y-from.y,length=vx*vx+vy*vy;
            if(length==0)continue;
            double t=System.Math.Max(0,System.Math.Min(1,-(ax*vx+ay*vy)/length));
            double nearX=ax+t*vx,nearY=ay+t*vy;
            if(nearX*nearX+nearY*nearY<=radius*radius)return true;
        }
        return false;
    }
    public int DistanceToCell(bool player,Vector2Int cell) => !OnScene(player?0:-1)?int.MaxValue:BodyDistance(player?Player:Monster,player?PlayerSpaces:MonsterSpaces,cell,1);
    public bool Inside(Vector2Int cell,int spaces) => cell.x>=0&&cell.y>=0&&cell.x+spaces<=Size&&cell.y+spaces<=Size;
    public bool CanPlace(bool player,Vector2Int cell) => !IsContained(player)&&!IsCarried(player)&&Inside(cell,player?PlayerSpaces:MonsterSpaces)&&SummonFootprintClear(cell,player?PlayerSpaces:MonsterSpaces,player?riderMountId:0)&&!Overlap(cell,player?PlayerSpaces:MonsterSpaces,player?Monster:Player,OccupiedSpaces(!player));
    // Open-rectangle intersection: touching a boundary is legal, crossing occupied space is not.
    static bool SegmentCrossesBody(Vector2Int from,Vector2Int to,int moverSize,Vector2Int obstacle,int obstacleSize)
    {
        if(obstacleSize==0)return false;
        double t0=0,t1=1;
        for(int axis=0;axis<2;axis++){
            double start=axis==0?from.x:from.y,delta=(axis==0?to.x:to.y)-start;
            double low=(axis==0?obstacle.x:obstacle.y)-moverSize+.00001,high=(axis==0?obstacle.x:obstacle.y)+obstacleSize-.00001;
            if(delta==0){if(start<low||start>high)return false;}
            else {double enter=(low-start)/delta,leave=(high-start)/delta;if(enter>leave){double tmp=enter;enter=leave;leave=tmp;}t0=System.Math.Max(t0,enter);t1=System.Math.Min(t1,leave);if(t0>t1)return false;}
        }
        return t0<=t1;
    }
    int playerRecoverySpaces,monsterRecoverySpaces;
    bool FindRecovery(Vector2Int body,int bodySpaces,int recovery,Vector2Int other,int otherSpaces,out Vector2Int result,int exceptId=0){
        result=body;if(recovery==0)return true;double best=double.MaxValue;
        for(int y=body.y-recovery+1;y<body.y+bodySpaces;y++)for(int x=body.x-recovery+1;x<body.x+bodySpaces;x++){
            var cell=new Vector2Int(x,y);if(!Inside(cell,recovery)||!SummonFootprintClear(cell,recovery,exceptId)||Overlap(cell,recovery,other,otherSpaces))continue;
            bool fits=recovery>=bodySpaces?x<=body.x&&y<=body.y&&x+recovery>=body.x+bodySpaces&&y+recovery>=body.y+bodySpaces:x>=body.x&&y>=body.y&&x+recovery<=body.x+bodySpaces&&y+recovery<=body.y+bodySpaces;if(!fits)continue;
            double dx=x+recovery*.5-body.x-bodySpaces*.5,dy=y+recovery*.5-body.y-bodySpaces*.5,d=dx*dx+dy*dy;if(d<best){best=d;result=cell;}
        }return best<double.MaxValue;
    }
    bool RecoverySafe(Vector2Int player,Vector2Int monster){Vector2Int ignored;return (!OnScene(0)||IsContained(true)||FindRecovery(player,PlayerSpaces,playerRecoverySpaces,monster,OccupiedSpaces(false),out ignored,riderMountId))&&(!OnScene(-1)||IsContained(false)||FindRecovery(monster,MonsterSpaces,monsterRecoverySpaces,player,OccupiedSpaces(true),out ignored));}
    bool RecoveryPathSafe(bool player,Vector2Int from,Vector2Int to){
        if(playerRecoverySpaces==0&&monsterRecoverySpaces==0)return true;
        int steps=System.Math.Max(System.Math.Abs(to.x-from.x),System.Math.Abs(to.y-from.y))*2;
        for(int i=0;i<=steps;i++){double t=steps==0?0:(double)i/steps;var point=new Vector2Int(Mathf.RoundToInt((float)(from.x+(to.x-from.x)*t)),Mathf.RoundToInt((float)(from.y+(to.y-from.y)*t)));if(!RecoverySafe(player?point:Player,player?Monster:point))return false;}return true;
    }
    public bool TrySetRecoverySize(bool player,int spaces){if(spaces<0||spaces>4)return false;int old=player?playerRecoverySpaces:monsterRecoverySpaces;if(player)playerRecoverySpaces=spaces;else monsterRecoverySpaces=spaces;if(RecoverySafe(Player,Monster))return true;if(player)playerRecoverySpaces=old;else monsterRecoverySpaces=old;return false;}
    public bool FindRecoveryPlacement(bool player,out Vector2Int result)=>FindRecovery(player?Player:Monster,player?PlayerSpaces:MonsterSpaces,player?playerRecoverySpaces:monsterRecoverySpaces,player?Monster:Player,player?MonsterSpaces:PlayerSpaces,out result,player?riderMountId:0);
    public void ClearRecoveryConstraints(){playerRecoverySpaces=monsterRecoverySpaces=0;}
    public bool CanTransformAt(bool player,int spaces,Vector2Int anchor,int recoverySpaces)
    {
        if(!Locked||spaces<1||spaces>4||recoverySpaces<1||recoverySpaces>4)return false;
        var body=player?Player:Monster;int oldSpaces=player?PlayerSpaces:MonsterSpaces;
        var other=player?Monster:Player;int otherSpaces=OccupiedSpaces(!player);
        Vector2Int mountedCell;if(player&&Riding&&spaces>=RiderMount.Spaces)return MountedSizeChange(spaces,out mountedCell);
        if(HasContainedCreature||!Inside(anchor,spaces)||!SummonFootprintClear(anchor,spaces,player?riderMountId:0)||!Overlap(anchor,spaces,body,oldSpaces)||Overlap(anchor,spaces,other,otherSpaces))return false;
        Vector2Int ignored;
        // Both the target and the other participant must remain able to revert.
        return FindRecovery(anchor,spaces,recoverySpaces,other,otherSpaces,out ignored,player?riderMountId:0)
            &&FindRecovery(other,otherSpaces,player?monsterRecoverySpaces:playerRecoverySpaces,anchor,spaces,out ignored);
    }
    public bool TransformAt(bool player,int spaces,Vector2Int anchor,int recoverySpaces)
    {
        var body=player?Player:Monster;int oldSpaces=player?PlayerSpaces:MonsterSpaces;
        if(!CanTransformAt(player,spaces,anchor,recoverySpaces))return false;
        if(player){CommitMountedBody(spaces,ref anchor);Player=anchor;PlayerSpaces=spaces;playerRecoverySpaces=recoverySpaces;}
        else {Monster=anchor;MonsterSpaces=spaces;monsterRecoverySpaces=recoverySpaces;}
        SyncWaterPositions();MovementObserved?.Invoke(player?0:-1,body,anchor,oldSpaces,player?PlayerSpaces:MonsterSpaces,true);
        return true;
    }
    public bool RestoreForm(bool player)
    {
        var observedFrom=player?Player:Monster;int observedSpaces=player?PlayerSpaces:MonsterSpaces;
        if(HasContainedCreature)return false;
        int spaces=player?playerRecoverySpaces:monsterRecoverySpaces;if(spaces==0)return false;
        Vector2Int anchor;if(!FindRecoveryPlacement(player,out anchor))return false;
        var other=player?Monster:Player;int otherSpaces=OccupiedSpaces(!player);Vector2Int ignored;
        if(!FindRecovery(other,otherSpaces,player?monsterRecoverySpaces:playerRecoverySpaces,anchor,spaces,out ignored))return false;
        if(player){CommitMountedBody(spaces,ref anchor);Player=anchor;PlayerSpaces=spaces;playerRecoverySpaces=0;}
        else {Monster=anchor;MonsterSpaces=spaces;monsterRecoverySpaces=0;}
        SyncWaterPositions();
        MovementObserved?.Invoke(player?0:-1,observedFrom,anchor,observedSpaces,player?PlayerSpaces:MonsterSpaces,true);return true;
    }
    public bool CanMoveTo(Vector2Int cell,bool tactical=false) => OnScene(0)&&Locked&&VoluntaryMovementAllowed(0,cell)&&!Riding&&!ActorRestrained(true)&&!IsContained(true)&&cell!=Player&&MovementBudget(tactical)>0&&CanOccupyForMove(0,Player,cell,PlayerSpaces)&&RecoveryPathSafe(true,Player,cell)&&PlayerMovementCost(cell)<=MovementBudget(tactical)&&CreaturePathClear(0,Player,cell,PlayerSpaces);
    public bool MovePlayer(Vector2Int cell,bool tactical=false) {
        if(!CanMoveTo(cell,tactical))return false;
        if(tactical)TacticalMovementRemaining-=PlayerMovementCost(cell);else {int cost=PlayerMovementCost(cell);MovementSpent+=cost;MovementRemaining-=cost;}
        var observedFrom=Player;Player=cell;SyncWaterWalkRoute(0,observedFrom,cell,PlayerSpaces);SyncContainedPosition();MovementObserved?.Invoke(0,observedFrom,cell,PlayerSpaces,PlayerSpaces,false);return true;
    }
    public bool CanTeleportPlayer(Vector2Int cell,int range)=>Locked&&VoluntaryMovementAllowed(0,cell)&&!IsContained(true)&&Inside(cell,playerBodySpaces)&&SummonFootprintClear(cell,playerBodySpaces)&&!Overlap(cell,playerBodySpaces,Monster,OccupiedSpaces(false))&&RecoverySafe(cell,Monster)&&FeetBetween(Player,cell)<=range;
    public bool TeleportPlayer(Vector2Int cell,int range){if(!CanTeleportPlayer(cell,range))return false;var observedFrom=Player;int old=PlayerSpaces;riderMountId=0;Player=cell;SyncContainedPosition();MovementObserved?.Invoke(0,observedFrom,cell,old,PlayerSpaces,true);return true;}
    public bool CanMoveMonsterTo(Vector2Int cell) =>OnScene(-1)&& Locked&&VoluntaryMovementAllowed(-1,cell)&&!ActorRestrained(false)&&!IsContained(false)&&cell!=Monster&&CanOccupyForMove(-1,Monster,cell,MonsterSpaces)&&RecoveryPathSafe(false,Monster,cell)&&MonsterMovementCost(cell)<=MonsterMovementRemaining&&CreaturePathClear(-1,Monster,cell,MonsterSpaces);
    public bool MoveMonster(Vector2Int cell) {if(!CanMoveMonsterTo(cell))return false;SpendMonsterMovement(MonsterMovementCost(cell));var observedFrom=Monster;Monster=cell;SyncWaterWalkRoute(-1,observedFrom,cell,MonsterSpaces);SyncContainedPosition();MovementObserved?.Invoke(-1,observedFrom,cell,MonsterSpaces,MonsterSpaces,false);return true;}
    public void RefreshMovement() {DashesThisTurn=0;MovementSpent=0;MovementRemaining=PlayerSpeed;TacticalMovementRemaining=0;RoundStartPlayer=Player;}
    public int DistanceFeet => !SameScene(0,-1)?int.MaxValue:HasContainedCreature?0:BodyDistance(Player,PlayerSpaces,Monster,MonsterSpaces);
    public bool TrySetFootprint(bool player,int spaces)
    {
        if(Locked||spaces<1||spaces>4)return false;
        var current=player?Player:Monster;var other=player?Monster:Player;int otherSize=player?MonsterSpaces:PlayerSpaces;
        Vector2Int best=current;int bestCost=int.MaxValue;
        for(int y=0;y<=Size-spaces;y++)for(int x=0;x<=Size-spaces;x++){
            var cell=new Vector2Int(x,y);if(Overlap(cell,spaces,other,otherSize))continue;
            int cost=FeetBetween(current,cell);if(cost<bestCost){best=cell;bestCost=cost;}
        }
        if(bestCost==int.MaxValue)return false;
        if(player){PlayerSpaces=spaces;Player=best;}else {MonsterSpaces=spaces;Monster=best;}return true;
    }
    public int ForceMoveAway(bool player,Vector2Int origin,int originSpaces,int feet)
        => ForceMoveOnLine(player,origin,originSpaces,feet,false);
    public int ForceMoveToward(bool player,Vector2Int origin,int originSpaces,int feet)
        => ForceMoveOnLine(player,origin,originSpaces,feet,true);
    int ForceMoveOnLine(bool player,Vector2Int origin,int originSpaces,int feet,bool toward)
    {
        if(player&&Riding&&!DismountRider())return 0;
        var start=player?Player:Monster;int spaces=player?PlayerSpaces:MonsterSpaces;
        double dx=start.x+spaces*.5-origin.x-originSpaces*.5,dy=start.y+spaces*.5-origin.y-originSpaces*.5,length=System.Math.Sqrt(dx*dx+dy*dy);
        if(IsContained(player)||length==0||feet<=0)return 0;
        if(toward){dx=-dx;dy=-dy;}
        var last=start;var obstacle=player?Monster:Player;int obstacleSpaces=OccupiedSpaces(!player);
        for(int distance=1;distance<=feet;distance++){
            var candidate=new Vector2Int(start.x+Mathf.RoundToInt((float)(dx/length*distance/5)),start.y+Mathf.RoundToInt((float)(dy/length*distance/5)));
            if(candidate==last)continue;
            if(FeetBetween(start,candidate)>feet)continue;
            if(toward&&BodyDistance(candidate,spaces,origin,originSpaces)>=BodyDistance(last,spaces,origin,originSpaces))continue;
            if(!CanPlace(player,candidate)||!RecoveryPathSafe(player,last,candidate)||SegmentCrossesBody(last,candidate,spaces,obstacle,obstacleSpaces)||(!player&&CrossesEnemySummon(last,candidate,spaces)))break;
            // Commit each pushed/pulled step before observing it so breathing,
            // spell tethers and terrain see the actual position and water layer.
            if(player)Player=candidate;else Monster=candidate;SyncContainedPosition();
            ApplyWaterWalkFall(player?0:-1,last,candidate,spaces,false);
            MovementObserved?.Invoke(player?0:-1,last,candidate,spaces,spaces,false);last=candidate;
        }
        if(player)Player=last;else Monster=last;SyncContainedPosition();return FeetBetween(start,last);
    }
    public bool CanRestoreMonsterFootprint(int spaces)=>!HasContainedCreature&&spaces>=1&&spaces<=4&&Inside(Monster,spaces)&&SummonFootprintClear(Monster,spaces)&&!Overlap(Monster,spaces,Player,PlayerSpaces);
    public bool RestoreMonsterFootprint(int spaces){if(!CanRestoreMonsterFootprint(spaces))return false;MonsterSpaces=spaces;return true;}
    public bool CanResizePlayerAt(int spaces,Vector2Int anchor)
    {
        Vector2Int mountedCell;if(Riding&&spaces>=RiderMount.Spaces)return MountedSizeChange(spaces,out mountedCell);
        if(HasContainedCreature||spaces<1||spaces>4||!Inside(anchor,spaces)||!SummonFootprintClear(anchor,spaces,riderMountId)||Overlap(anchor,spaces,Monster,MonsterSpaces))return false;
        if(spaces>=PlayerSpaces)return anchor.x<=Player.x&&anchor.y<=Player.y&&anchor.x+spaces>=Player.x+PlayerSpaces&&anchor.y+spaces>=Player.y+PlayerSpaces;
        return anchor.x>=Player.x&&anchor.y>=Player.y&&anchor.x+spaces<=Player.x+PlayerSpaces&&anchor.y+spaces<=Player.y+PlayerSpaces;
    }
    public bool TryResizePlayerAt(int spaces,Vector2Int anchor){if(!CanResizePlayerAt(spaces,anchor))return false;var from=Player;int old=PlayerSpaces;CommitMountedBody(spaces,ref anchor);PlayerSpaces=spaces;Player=anchor;SyncWaterPositions();MovementObserved?.Invoke(0,from,anchor,old,PlayerSpaces,true);return true;}
    public bool FindPlayerResizePlacement(int spaces,out Vector2Int result,Vector2Int? referenceCell=null)
    {
        result=Player;double best=double.MaxValue;
        for(int y=Player.y-spaces+1;y<Player.y+PlayerSpaces;y++)for(int x=Player.x-spaces+1;x<Player.x+PlayerSpaces;x++){
            var cell=new Vector2Int(x,y);if(!CanResizePlayerAt(spaces,cell))continue;
            if(referenceCell.HasValue&&(referenceCell.Value.x<x||referenceCell.Value.y<y||referenceCell.Value.x>=x+spaces||referenceCell.Value.y>=y+spaces))continue;
            double dx=x+spaces*.5-(Player.x+PlayerSpaces*.5),dy=y+spaces*.5-(Player.y+PlayerSpaces*.5),distance=dx*dx+dy*dy;
            if(distance<best){best=distance;result=cell;}
        }
        return best<double.MaxValue;
    }
    public void Select(int index) {if(Locked || index<0 || index>=Sizes.Length)return;Selected=index;Player=Vector2Int.zero;Monster=new Vector2Int(Size-MonsterSpaces,Size-MonsterSpaces);}
    public bool Place(bool player,Vector2Int cell) {
        if(Locked||!CanPlace(player,cell))return false;
        if(player)Player=cell;else Monster=cell;return true;
    }
    public bool Lock() {if(!CanPlace(true,Player)||!CanPlace(false,Monster))return false;Locked=true;RefreshMovement();StartMonsterMovementTurn();return true;}
    public void Unlock(){ClearScenes();consumedBodies.Clear();riderMountId=0;RemoveRescue(true);PlayerAquatic.TrySetSubmerged(false,0,1);MonsterAquatic.TrySetSubmerged(false,0,1);ClearSummons();PlayerCrawling=false;ClearContainment();ClearRecoveryConstraints();Locked=false;TacticalMovementRemaining=0;MonsterMovementRemaining=30;}
}
