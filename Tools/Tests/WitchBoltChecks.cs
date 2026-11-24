using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace UnityEngine {
 public struct Vector2Int {public int x,y;public Vector2Int(int a,int b){x=a;y=b;}}
 public static class Random {public static int Range(int a,int b)=>b-1;}
 public static class GUI {public static bool enabled=true;}
 public static class GUILayout {public static bool Button(string text,object style)=>false;public static void Label(string text,object style){}}
}
public static class BattleMap {public const int RescueId=int.MinValue;public static int BodyDistance(UnityEngine.Vector2Int a,int n,UnityEngine.Vector2Int b,int m)=>Math.Abs(a.x-b.x)*5;}
public sealed partial class TrainingGame
{
 sealed class MapStub {
  public UnityEngine.Vector2Int Player;public int PlayerSpaces=1,Distance=1;public bool Same=true,Exists=true;
  public bool BodyPresent(int actor)=>Exists;public bool SameScene(int a,int b)=>Same;
  public bool SceneBody(int id,out UnityEngine.Vector2Int p,out int n){p=new UnityEngine.Vector2Int(id==0?0:Distance,0);n=1;return Exists;}
 }
 sealed class CombatStub {public int Round=1;public bool PlayerCanAct=true,BonusActionAvailable=true;public bool TryUseBonusAction(){if(!PlayerCanAct||!BonusActionAvailable)return false;BonusActionAvailable=false;return true;}}
 sealed class BestialStub {public bool Active=false;public UnitStub Unit=new UnitStub();}sealed class UnitStub {public int Id=7;}
 sealed class TurnsStub {public bool Active=false;}sealed class SpellAttackResult {public bool Hit,Critical,Intercepted;}
 sealed class RayDamage {public long[] Raw;}sealed class ExtraMarkDamage {public int Raw=0;}sealed class MarkStub {public int DamageType=8;}
 readonly MapStub map=new MapStub();readonly CombatStub combat=new CombatStub();readonly BestialStub bestialSpirit=new BestialStub();readonly TurnsStub summonTurns=new TurnsStub();readonly MarkStub mark=new MarkStub();
 string concentrationSpell="",panel="spells",dice;long worldRounds,concentrationExpiresWorld;int concentrationExpiresRound,directSlotLevel=1,outcomeDeferDepth,draggingToken=-1;
 bool rolling,spellTargeting=false,reactionPending=false,PlayerTurnAllowed=true,available=true,isolated=false,hit=true,critical,intercepted;
 int damage,attacks,markCalls,diceCount,bonusPulses;object button=new object(),small=new object();readonly List<string> history=new List<string>();
 bool AcidArrowTargetAvailable(int id)=>available;bool AcidArrowIsolated(int a,int b)=>isolated;string TransitActorName(int id)=>id.ToString();List<int> BreathActors()=>new List<int>{0,-1};
 void EndConcentration(string reason){witchBolt.Clear();concentrationSpell="";}
 IEnumerator RollMappedSpellAttack(int source,int target,int caster,string title,SpellAttackResult result){attacks++;result.Hit=hit&&!intercepted;result.Critical=critical;result.Intercepted=intercepted;yield break;}
 IEnumerator PresentRoll(int sides,int[] rolls,string text){diceCount=rolls.Length;yield break;}
 IEnumerator RollMarkDamage(bool p,bool crit,ExtraMarkDamage result){markCalls++;yield break;}
 IEnumerator ReduceRayPacket(bool p,long[] raw,bool rolled,string label,RayDamage result){result.Raw=raw;yield break;}
 int ApplyMappedDamagePacket(int target,int source,long[] raw,bool crit){damage+=(int)raw.Sum();if(!crit)bonusPulses++;return (int)raw.Sum();}
 IEnumerator MaintainConcentration(int damage){yield break;}void RefreshUndeadFollowers(){}void RefreshBestialSpirit(){}void ReconcileFallenRescue(){}void CheckBattleOutcome(){}
 void StartCoroutine(IEnumerator routine)=>Run(routine);
 static void Run(IEnumerator routine){var stack=new Stack<IEnumerator>();stack.Push(routine);while(stack.Count>0){var top=stack.Peek();if(!top.MoveNext()){stack.Pop();continue;}if(top.Current is IEnumerator)stack.Push((IEnumerator)top.Current);}}
 static void C(bool v,string text){if(!v)throw new Exception(text);}
 public static void Main()
 {
  for(int slot=1;slot<=9;slot++)C(WitchBoltEffect.InitialDice(slot)==slot+1&&WitchBoltEffect.InitialDice(slot,true)==2*(slot+1),"initial dice scale +1d12; critical only doubles initial dice");
  C(WitchBoltEffect.InitialDice(0)==0&&WitchBoltEffect.InitialDice(10)==0&&!WitchBoltEffect.ValidTime(long.MaxValue-9),"invalid slot and timer overflow");
  var g=new TrainingGame();g.hit=false;Run(g.CastWitchBolt(1,1,-1));C(g.damage==0&&g.witchBolt.Active(0)&&g.attacks==1,"initial miss still binds original target");
  g.combat.Round=2;g.worldRounds=1;Run(g.PulseWitchBolt());C(g.damage==12&&g.attacks==1&&g.markCalls==0&&g.diceCount==1&&!g.combat.BonusActionAvailable,"subsequent turn pays bonus, deals automatic one die and never invokes attack/hit extras");
  Run(g.PulseWitchBolt());C(g.damage==12,"spent bonus cannot repeat damage");
  g=new TrainingGame();g.intercepted=true;Run(g.CastWitchBolt(1,5,-1));C(g.damage==0&&g.witchBolt.Active(0),"confirmed mirror interception causes zero initial body damage but preserves binding");
  Run(g.PulseWitchBolt());C(g.combat.BonusActionAvailable&&g.damage==0,"cannot pulse during initial casting turn");
  g.combat.Round=4;g.worldRounds=3;Run(g.PulseWitchBolt());C(g.damage==12&&g.witchBolt.Expires==10,"skipped turns do not break link, and upcasting never increases or extends automatic damage");
  g=new TrainingGame();g.critical=true;Run(g.CastWitchBolt(1,3,-1));C(g.damage==96&&g.diceCount==8&&g.markCalls==1,"third-slot critical rolls eight d12, with initial hit extras once");
  g.map.Distance=13;g.RefreshWitchBolt();g.map.Distance=1;C(g.concentrationSpell==""&&!g.witchBolt.Active(1),"crossing 60 feet permanently ends link; returning does not restore it");
  g=new TrainingGame();g.map.Distance=12;Run(g.CastWitchBolt(1,1,-1));C(g.witchBolt.Active(0),"exactly sixty feet is legal");g.isolated=true;g.RefreshWitchBolt();C(g.concentrationSpell=="","new total-cover swallow boundary ends the link");
  g=new TrainingGame();Run(g.CastWitchBolt(1,1,-1));g.map.Same=false;g.RefreshWitchBolt();C(g.concentrationSpell=="","cross-plane transport ends link");
  g=new TrainingGame();Run(g.CastWitchBolt(1,1,-1));g.worldRounds=10;g.RefreshWitchBolt();C(g.concentrationSpell==""&&g.outcomeDeferDepth==0&&!g.rolling,"exact one-minute expiry and coroutine cleanup");
  Console.WriteLine("PASS: production Witch Bolt initial miss/mirror binding, later bonus payment, fixed automatic damage, initial-only critical/upcasting, skipped-turn persistence, range/cover/plane break and expiry");
 }
}
