// Standalone model check. Compile with BattleMap.cs and CreatureProfile.cs.
// Minimal Unity value-type substitutes allow checks without launching the Unity Editor.
using System;
namespace UnityEngine {
 public struct Vector2Int {
 public int x,y;public Vector2Int(int x,int y){this.x=x;this.y=y;}public static Vector2Int zero=>new Vector2Int(0,0);
 public static float Distance(Vector2Int a,Vector2Int b)=>(float)Math.Sqrt((double)(a.x-b.x)*(a.x-b.x)+(double)(a.y-b.y)*(a.y-b.y));
 public static bool operator ==(Vector2Int a,Vector2Int b)=>a.x==b.x&&a.y==b.y;public static bool operator !=(Vector2Int a,Vector2Int b)=>!(a==b);
 public override bool Equals(object o)=>o is Vector2Int&&this==(Vector2Int)o;public override int GetHashCode()=>x*31+y;
 }
 public static class Mathf {public static int RoundToInt(float n)=>(int)Math.Round(n,MidpointRounding.ToEven);}
}
class FootprintChecks {
static void Check(bool v,string n){if(!v)throw new Exception(n);}
static UnityEngine.Vector2Int V(int x,int y)=>new UnityEngine.Vector2Int(x,y);
static void Main(){var m=new BattleMap();Check(m.TrySetFootprint(false,2)&&m.Monster==V(8,8),"large bounds relocation");Check(!m.Place(true,V(9,9))&&m.Place(true,V(7,8)),"large occupied collision");Check(m.DistanceFeet==5,"nearest occupied-cell distance");Check(m.DistanceToCell(false,V(9,9))==0,"sphere includes far occupied cell");Check(!m.Place(false,V(9,8)),"footprint must fit board");Check(m.Lock()&&!m.TrySetFootprint(false,3),"locked footprint");m.Unlock();Check(m.TrySetFootprint(false,3)&&m.Inside(m.Monster,3),"huge resize");Check(!m.TrySetFootprint(false,5),"invalid footprint");m.Select(0);Check(m.Place(false,V(2,0))&&m.Place(true,V(0,0))&&m.Lock(),"path setup");Check(!m.CanMoveTo(V(5,0)),"swept path cannot cross occupied body");Check(m.CanMoveTo(V(0,4))&&m.MovePlayer(V(0,4))&&m.MovementRemaining==10,"cumulative movement cost");Check(!m.CanMoveTo(V(0,7)),"movement limit");Check(BattleMap.BodyDistance(V(0,0),2,V(2,2),2)==7,"large diagonal distance");Check(BattleMap.LeavesReach(V(3,0),V(5,0),V(0,0),5,1,3),"leave huge defender edge");Check(!BattleMap.LeavesReach(V(3,0),V(3,1),V(0,0),5,1,3),"stay within edge reach");Check(BattleMap.LeavesReach(V(1,1),V(2,2),V(0,0),7,1,1),"rounded diagonal reach");Check(CreatureProfile.SpacesForSize(2)==1&&CreatureProfile.SpacesForSize(3)==2&&CreatureProfile.SpacesForSize(4)==3&&CreatureProfile.SpacesForSize(5)==4,"size footprints");Console.WriteLine("PASS: footprints, bounds, resize, swept collisions, cumulative movement, body distances, area targeting and opportunity reach");}}
