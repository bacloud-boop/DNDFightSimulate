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
    public int DistanceFeet => Mathf.RoundToInt(Vector2Int.Distance(Player,Monster)*5);
    public void Select(int index) {if(Locked || index<0 || index>=Sizes.Length)return;Selected=index;Player=Vector2Int.zero;Monster=new Vector2Int(Size-1,Size-1);}
    public bool Place(bool player,Vector2Int cell) {
        if(Locked || cell.x<0 || cell.y<0 || cell.x>=Size || cell.y>=Size || cell==(player?Monster:Player))return false;
        if(player)Player=cell;else Monster=cell;return true;
    }
    public bool Lock() {if(Player==Monster)return false;Locked=true;return true;}
    public void Unlock(){Locked=false;}
}
