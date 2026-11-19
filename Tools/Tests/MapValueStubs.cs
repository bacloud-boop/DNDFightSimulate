// Standalone model check. Compile with BattleMap.cs, CreatureProfile.cs and ChallengeRatings.cs.
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
