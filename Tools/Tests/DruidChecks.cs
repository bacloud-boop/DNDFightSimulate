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
class DruidChecks {
static void Check(bool value,string message){if(!value)throw new Exception(message);}
static UnityEngine.Vector2Int V(int x,int y)=>new UnityEngine.Vector2Int(x,y);
static void Main(){
Check(DruidFeatures.Maximum(1)==0&&DruidFeatures.Maximum(2)==2&&DruidFeatures.Maximum(6)==3&&DruidFeatures.Maximum(17)==4,"wild shape uses");
Check(DruidFeatures.KnownFormLimit(2)==4&&DruidFeatures.KnownFormLimit(4)==6&&DruidFeatures.KnownFormLimit(8)==8&&DruidFeatures.DurationRounds(3)==900,"known forms and fractional hours");
var c=new CombatState();var map=new BattleMap();var d=new DruidFeatures();map.Place(false,V(1,0));map.Lock();UnityEngine.Vector2Int anchor;
Check(!d.TryBeginWildShape(2,c,map,2,out anchor)&&d.WildShapesUsed==0&&c.BonusActionAvailable&&map.PlayerSpaces==1,"insufficient room is atomic and free");
map.Unlock();map.Select(0);map.Place(true,V(5,5));map.Lock();var original=map.Player;
Check(d.TryBeginWildShape(2,c,map,2,out anchor,original)&&map.PlayerSpaces==2&&!c.BonusActionAvailable&&d.WildShapesUsed==1,"legal shape consumes bonus and use");
Check(!d.TryBeginWildShape(2,c,map,2,out anchor,original)&&d.WildShapesUsed==1,"cannot reuse bonus");c.EndRound();
Check(d.TryBeginWildShape(2,c,map,1,out anchor,original)&&map.Player==original,"shape reversal retains reference cell");c.EndRound();
Check(!d.TryBeginWildShape(2,c,map,2,out anchor)&&c.BonusActionAvailable,"no remaining uses costs nothing");d.ShortRest();Check(d.Remaining(2)==1,"short rest restores one");d.LongRest();Check(d.Remaining(2)==2,"long rest restores all");
map.Unlock();map.Select(0);map.Lock();Check(map.MovePlayer(V(2,0)),"move before shape");map.SetPlayerSpeed(60);Check(map.MovementRemaining==50,"speed change preserves spent movement");Check(map.MovePlayer(V(6,0)),"move in fast form");map.SetPlayerSpeed(30);Check(map.MovementRemaining==0&&map.MovementSpent==30,"revert cannot refund spent movement");
c.ResetBattle();c.GrantPlayerTemporaryHp(5);c.GrantPlayerTemporaryHp(3);Check(c.PlayerTemporaryHp==5,"temporary hp do not stack");c.ApplyPlayerDamage(7);Check(c.PlayerHp==8&&c.PlayerTemporaryHp==0,"temporary hp absorb first");c.SetPlayerArmorOverride(12);Check(c.PlayerArmorClass==12,"beast armor override");c.SetPlayerArmorOverride(null);Check(c.PlayerArmorClass==10,"armor reverts");
c.ResetBattle();c.GrantPlayerTemporaryHp(5);c.ApplyPlayerDamage(20);Check(c.PlayerHp==0&&!c.PlayerDead,"temporary hp reduce massive overflow");c.GrantPlayerTemporaryHp(5);c.ApplyPlayerDamage(1,true);Check(c.DeathFailures==2&&c.PlayerTemporaryHp==4,"damage while downed counts despite temporary hp");c.ResetBattle();c.GrantPlayerTemporaryHp(5);c.ApplyPlayerDamage(25);Check(c.PlayerDead,"massive damage after absorption");
Check(BeastAttack.Supported.Length==10&&BeastAttack.Find("Spider").ExtraSides==4&&BeastAttack.Find("Wolf").KnockProne&&BeastAttack.Find("Rat").Agile,"natural attacks and traits");
Console.WriteLine("PASS: atomic transformation costs, insufficient space, uses, rests, known limits, reference-cell restoration, speed changes, temporary HP, armor and natural attacks");}}
