using UnityEngine;

public sealed class BattleMap
{
    public static readonly string[] Names={"小地图","中地图","大地图"};
    public static readonly int[] Sizes={10,15,20};
    public int Selected {get;private set;}
    public int Size => Sizes[Selected];
    // Positions identify the top-left occupied cell; footprint centers may lie on grid intersections.
    public Vector2Int Player {get;private set;} = Vector2Int.zero;
    public Vector2Int Monster {get;private set;} = new Vector2Int(9,9);
    public int PlayerSpaces {get;private set;}=1;
    public int MonsterSpaces {get;private set;}=1;
    public bool Locked {get;private set;}
    public const int MovementPerRound=30;
    public int PlayerSpeed {get;private set;}=MovementPerRound;
    public int MovementSpent {get;private set;}
    public int MovementRemaining {get;private set;} = MovementPerRound;
    public void SetPlayerSpeed(int speed){PlayerSpeed=System.Math.Max(0,speed);MovementRemaining=System.Math.Max(0,PlayerSpeed-MovementSpent);}
    public void SpendMonsterMovement(int feet){MonsterMovementRemaining=System.Math.Max(0,MonsterMovementRemaining-System.Math.Max(0,feet));}
    public int MonsterMovementRemaining {get;private set;} = 30;
    public void StartMonsterMovementTurn(int speed=30) {MonsterMovementRemaining=System.Math.Max(0,speed);}
    public int TacticalMovementRemaining {get;private set;}
    public void GrantTacticalMovement() {TacticalMovementRemaining+=PlayerSpeed/2;}
    public int MovementBudget(bool tactical) => tactical?TacticalMovementRemaining:MovementRemaining;
    public Vector2Int RoundStartPlayer {get;private set;}
    public static int FeetBetween(Vector2Int a,Vector2Int b) => Mathf.RoundToInt(Vector2Int.Distance(a,b)*5);
    public static bool Overlap(Vector2Int a,int aSize,Vector2Int b,int bSize) => a.x<b.x+bSize&&a.x+aSize>b.x&&a.y<b.y+bSize&&a.y+aSize>b.y;
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
    public int DistanceToCell(bool player,Vector2Int cell) => BodyDistance(player?Player:Monster,player?PlayerSpaces:MonsterSpaces,cell,1);
    public bool Inside(Vector2Int cell,int spaces) => cell.x>=0&&cell.y>=0&&cell.x+spaces<=Size&&cell.y+spaces<=Size;
    public bool CanPlace(bool player,Vector2Int cell) => Inside(cell,player?PlayerSpaces:MonsterSpaces)&&!Overlap(cell,player?PlayerSpaces:MonsterSpaces,player?Monster:Player,player?MonsterSpaces:PlayerSpaces);
    // Open-rectangle intersection: touching a boundary is legal, crossing occupied space is not.
    static bool SegmentCrossesBody(Vector2Int from,Vector2Int to,int moverSize,Vector2Int obstacle,int obstacleSize)
    {
        double t0=0,t1=1;
        for(int axis=0;axis<2;axis++){
            double start=axis==0?from.x:from.y,delta=(axis==0?to.x:to.y)-start;
            double low=(axis==0?obstacle.x:obstacle.y)-moverSize+.00001,high=(axis==0?obstacle.x:obstacle.y)+obstacleSize-.00001;
            if(delta==0){if(start<low||start>high)return false;}
            else {double enter=(low-start)/delta,leave=(high-start)/delta;if(enter>leave){double tmp=enter;enter=leave;leave=tmp;}t0=System.Math.Max(t0,enter);t1=System.Math.Min(t1,leave);if(t0>t1)return false;}
        }
        return t0<=t1;
    }
    public bool CanMoveTo(Vector2Int cell,bool tactical=false) => Locked&&cell!=Player&&MovementBudget(tactical)>0&&CanPlace(true,cell)&&FeetBetween(Player,cell)<=MovementBudget(tactical)&&!SegmentCrossesBody(Player,cell,PlayerSpaces,Monster,MonsterSpaces);
    public bool MovePlayer(Vector2Int cell,bool tactical=false) {
        if(!CanMoveTo(cell,tactical))return false;
        if(tactical)TacticalMovementRemaining-=FeetBetween(Player,cell);else {int cost=FeetBetween(Player,cell);MovementSpent+=cost;MovementRemaining-=cost;}
        Player=cell;return true;
    }
    public bool CanMoveMonsterTo(Vector2Int cell) => Locked&&cell!=Monster&&CanPlace(false,cell)&&FeetBetween(Monster,cell)<=MonsterMovementRemaining&&!SegmentCrossesBody(Monster,cell,MonsterSpaces,Player,PlayerSpaces);
    public bool MoveMonster(Vector2Int cell) {if(!CanMoveMonsterTo(cell))return false;MonsterMovementRemaining-=FeetBetween(Monster,cell);Monster=cell;return true;}
    public void RefreshMovement() {MovementSpent=0;MovementRemaining=PlayerSpeed;TacticalMovementRemaining=0;RoundStartPlayer=Player;}
    public int DistanceFeet => BodyDistance(Player,PlayerSpaces,Monster,MonsterSpaces);
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
    public bool CanResizePlayerAt(int spaces,Vector2Int anchor)
    {
        if(spaces<1||spaces>4||!Inside(anchor,spaces)||Overlap(anchor,spaces,Monster,MonsterSpaces))return false;
        if(spaces>=PlayerSpaces)return anchor.x<=Player.x&&anchor.y<=Player.y&&anchor.x+spaces>=Player.x+PlayerSpaces&&anchor.y+spaces>=Player.y+PlayerSpaces;
        return anchor.x>=Player.x&&anchor.y>=Player.y&&anchor.x+spaces<=Player.x+PlayerSpaces&&anchor.y+spaces<=Player.y+PlayerSpaces;
    }
    public bool TryResizePlayerAt(int spaces,Vector2Int anchor){if(!CanResizePlayerAt(spaces,anchor))return false;PlayerSpaces=spaces;Player=anchor;return true;}
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
    public void Unlock(){Locked=false;TacticalMovementRemaining=0;MonsterMovementRemaining=30;}
}
