using UnityEngine;

public sealed class BattleMap
{
    public static readonly string[] Names={"小地图","中地图","大地图"};
    public static readonly int[] Sizes={10,15,20};
    public int Selected {get;private set;}
    public int Size => Sizes[Selected];
    public Vector2Int Player {get;private set;} = Vector2Int.zero;
    public Vector2Int Monster {get;private set;} = new Vector2Int(9,9);
    public bool Locked {get;private set;}
    public const int MovementPerRound=30;
    public int MovementRemaining {get;private set;} = MovementPerRound;
    public Vector2Int RoundStartPlayer {get;private set;}
    public static int FeetBetween(Vector2Int a,Vector2Int b) => Mathf.RoundToInt(Vector2Int.Distance(a,b)*5);
    public bool CanMoveTo(Vector2Int cell) => Locked && MovementRemaining>0 && cell.x>=0 && cell.y>=0 && cell.x<Size && cell.y<Size && cell!=Monster && FeetBetween(Player,cell)<=MovementRemaining;
    public bool MovePlayer(Vector2Int cell) {
        if(!CanMoveTo(cell))return false;
        MovementRemaining-=FeetBetween(Player,cell);Player=cell;return true;
    }
    public void RefreshMovement() {MovementRemaining=MovementPerRound;RoundStartPlayer=Player;}
    public int DistanceFeet => Mathf.RoundToInt(Vector2Int.Distance(Player,Monster)*5);
    public void Select(int index) {if(Locked || index<0 || index>=Sizes.Length)return;Selected=index;Player=Vector2Int.zero;Monster=new Vector2Int(Size-1,Size-1);}
    public bool Place(bool player,Vector2Int cell) {
        if(Locked || cell.x<0 || cell.y<0 || cell.x>=Size || cell.y>=Size || cell==(player?Monster:Player))return false;
        if(player)Player=cell;else Monster=cell;return true;
    }
    public bool Lock() {if(Player==Monster)return false;Locked=true;RefreshMovement();return true;}
    public void Unlock(){Locked=false;}
}
