using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace UnityEngine {
 public struct Vector2 {public float x,y;}
 public struct Rect {public float x,y,width,height;public Rect(float a,float b,float c,float d){x=a;y=b;width=c;height=d;}public bool Contains(Vector2 p)=>false;}
 public struct Color {public Color(float a,float b,float c,float d){}}
 public enum EventType {MouseDown}public class Event {public static Event current=new Event();public EventType type;public int button=0;public void Use(){}}
 public static class GUI {public static bool enabled=true;public static void Label(Rect r,string s,object style){}}
 public static class GUILayout {public static void BeginArea(Rect r){}public static void EndArea(){}public static Vector2 BeginScrollView(Vector2 p)=>p;public static void EndScrollView(){}public static void Label(string s,object style){}public static bool Button(string s,object style)=>false;public static bool Toggle(bool b,string s,object style)=>b;public static int SelectionGrid(int value,string[] names,int columns,object style)=>value;}
 public static class Random {public static int D20=10;public static readonly Queue<int> D8=new Queue<int>(),D20Queue=new Queue<int>();public static int Range(int min,int max)=>max==21?(D20Queue.Count>0?D20Queue.Dequeue():D20):max==9&&D8.Count>0?D8.Dequeue():max-1;}
}
public static class AcidSplash {public static int Modifier(int score)=>(int)Math.Floor((score-10)/2.0);}
public static class BattleMap {
 public const int RescueId=int.MinValue;
 public static int FeetBetween(Vector2Int a,Vector2Int b)=>Mathf.RoundToInt(Vector2Int.Distance(a,b)*5);
 public static int BodyDistance(Vector2Int a,int n,Vector2Int b,int m)=>Math.Max(0,Math.Abs(a.x-b.x)-Math.Max(n,m)+1)*5;
 public static bool Overlap(Vector2Int a,int n,Vector2Int b,int m)=>n>0&&m>0&&a.x<b.x+m&&a.x+n>b.x&&a.y<b.y+m&&a.y+n>b.y;
}
public static class MonsterSpellSequence {public static int AttackBonus(int pb,int score)=>pb+AcidSplash.Modifier(score);}
public sealed partial class TrainingGame {
 sealed class Unit {public int Id=0,X=0,Y=0,Spaces=1,Armor=10,EffectiveSpeed=30,SleepStage=0;public bool Prone=false,Restrained=false,Dodging=false,Alive=true,ControllerReady=true,FeignDeath=false;}
 sealed class MapStub {
  public int Size=20,PlayerSpaces=1,MonsterSpaces=1,Scene=0,PlayerScene=0,MonsterScene=0,PointCalls,BodyCalls;public Vector2Int? RescuePosition=null;public int RescueSpaces=1;public bool CarriedPlayer=false;public bool IsCarried(bool player)=>player&&CarriedPlayer;public bool Locked=true,PlayerContained=false,MonsterContained=false;public Vector2Int LastPoint,Player=new Vector2Int(0,0),Monster=new Vector2Int(5,0);
  public bool HasContainedCreature=>PlayerContained||MonsterContained;public readonly List<Unit> SceneSummons=new List<Unit>();
  public int DistanceFeet=>BattleMap.BodyDistance(Player,1,Monster,1);public int SceneOf(int actor)=>actor==0?PlayerScene:MonsterScene;public bool SameScene(int a,int b)=>SceneOf(a)>=0&&SceneOf(a)==SceneOf(b);public bool OnScene(int actor)=>SceneOf(actor)==Scene;
  public bool IsContained(bool player)=>player?PlayerContained:MonsterContained;
  public bool SceneBody(int actor,out Vector2Int p,out int n){p=actor==0?Player:Monster;n=1;if(actor>0){var u=SceneSummons.Find(a=>a.Id==actor);if(u==null||!u.Alive)return false;p=new Vector2Int(u.X,u.Y);n=u.Spaces;}return true;}
  public bool Inside(Vector2Int p,int n)=>p.x>=0&&p.y>=0&&p.x+n<=Size&&p.y+n<=Size;
  public int CreatureCoverBonus(int source,int target){BodyCalls++;return 0;}
  public int BounceCoverCalls,LastCoverSource;public int CreatureCoverAt(int source,int target,CreatureCover.Body origin,CreatureCover.Body body){BounceCoverCalls++;LastCoverSource=source;LastPoint=new Vector2Int((int)origin.X,(int)origin.Y);return 0;}
  public int CreatureCoverFromPoint(Vector2Int p,int target){PointCalls++;LastPoint=p;return 2;}
 }
 sealed class CombatStub {
  public int Round=1,PlayerHp=100,PlayerArmorClass=10,MonsterArmorClass=10,MonsterProficiencyBonus=0,MonsterRevivalPenalty=0;public bool PlayerReactionAvailable=true;public bool PlayerDead=false,PlayerFeignDeath=false,MagicActionAvailable=true;public bool PlayerBanished=false;public bool BonusActionAvailable=true,PlayerCanAct=true,MonsterCanAct=true;
  public bool PlayerBlessed=false,MonsterBlessed=false,PlayerBaned=false,MonsterBaned=false,PlayerBladeWard=false,PlayerParalyzed=false,MonsterHelpless=false,PlayerProne=false,MonsterProne=false,PlayerPoisoned=false,MonsterPoisoned=false;
  public bool TryUseMagicAction(){if(!MagicActionAvailable)return false;MagicActionAvailable=false;return true;}public bool TryUseAftermathBonusAction()=>TryUseBonusAction();public bool TryUseBonusAction(){if(!BonusActionAvailable||!PlayerCanAct)return false;BonusActionAvailable=false;return true;}public bool ConsumeGuidingLight(bool player)=>false;
 }
 sealed class ClassStub {public int ProficiencyBonus=2;public int this[int n]=>0;}sealed class FighterStub {public int Misses;public bool ConsumeAdvantage(int level)=>false;public void RecordMiss(int level){Misses++;}}
 sealed class TurnsStub {public bool Active=false;}sealed class MonsterSpellsStub {public int Ability=4;}sealed class BreathStub {public int D20Penalty=0;}
 sealed class RayDamage {public long[] Raw;}sealed class ExtraMarkDamage {public int Raw=0;}sealed class MarkStub {public int DamageType=6;}
 readonly MapStub map=new MapStub();readonly CombatStub combat=new CombatStub();readonly ClassStub classes=new ClassStub();readonly FighterStub fighter=new FighterStub();readonly TurnsStub summonTurns=new TurnsStub();readonly MonsterSpellsStub monsterSpells=new MonsterSpellsStub();readonly BreathStub monsterBreathing=new BreathStub();readonly MarkStub mark=new MarkStub();
 readonly int[] attributes={14,14,14,14,14,14};readonly List<string> history=new List<string>();
 string selectedSpellName="",concentrationSpell="",panel="",page="battle",dice="";long worldRounds=0,concentrationExpiresWorld=-1;int directSlotLevel=1,concentrationExpiresRound,draggingToken=-1,PlayerAquaticD20Penalty=0,outcomeDeferDepth,damage,invisibilityBreaks;bool rolling=false,reactionPending=false,spellTargeting=false,PlayerTurnAllowed=true,mirrorIntercepted,intercept,visible=true;
 object paperTexture=new object(),heading=new object(),small=new object(),button=new object(),label=new object(),centered=new object();
 string IntegerField(string value,bool negative)=>value;void Surface(Rect r,object texture){}void Fill(Rect r,Color c){}bool Btn(Rect r,string s,bool enabled)=>false;void DrawMap(Rect r,bool drag){}Vector2 BoardPointer()=>new Vector2();Rect CellRect(Rect r,Vector2Int p)=>r;void StartCoroutine(IEnumerator r){}
 string TransitActorName(int id)=>id.ToString();IEnumerable<int> BreathActors(){yield return 0;yield return -1;foreach(var u in map.SceneSummons)if(u.Alive)yield return u.Id;}
 bool CommandBody(int actor,out Vector2Int p,out int n)=>map.SceneBody(actor,out p,out n);
 bool HealingHandsTarget(int actor,out Vector2Int p,out int n,out string name){name=actor.ToString();return map.SceneBody(actor,out p,out n)&&map.OnScene(actor)&&!(actor==-1&&deadMonster);}
 bool SummonTarget(int actor,out Vector2Int p,out int n,out string name){name=actor.ToString();return HealingHandsTarget(actor,out p,out n,out name)&&!((actor==0||actor==-1)&&map.IsContained(actor==0));}
 Unit FindSummon(int id)=>map.SceneSummons.Find(u=>u.Id==id);
 int SpellAbilityIndex(int caster)=>3;int EffectiveMonsterAbility(int ability)=>10;
 bool MonsterSeesPlayer=>visible;bool PlayerSeesMonster=>visible;
 int Visibility(bool viewer,bool player)=>visible?0:2;int VisibilityAt(bool viewer,Vector2Int p,int n)=>visible?0:2;bool MonsterCanSeeTarget(int target)=>visible;bool SpiritSees(Unit u,int source,Vector2Int p,int n)=>visible;
 bool FaerieAdvantage(int target,bool sees)=>false;bool Entangled(bool player)=>false;bool FearDisadvantage(bool player)=>false;bool RayAttackDisadvantage(bool player,int ability)=>false;bool ResurrectionD20Disadvantage(bool player)=>false;bool BlurDisadvantage(bool viewer,bool target)=>false;
 bool PerceivesWithSpecialSense(bool viewer,bool target)=>false;
 void BreakOrdinaryInvisibility(bool player,string context){invisibilityBreaks++;}
 IEnumerator ResolvePlayerHitDefense(int value,long total,bool sees,bool special,int cover){mirrorIntercepted=intercept;yield break;}
 IEnumerator PresentRoll(int sides,int[] values,string text){yield break;}
 IEnumerator RollMarkDamage(bool target,bool critical,ExtraMarkDamage result){yield break;}
 IEnumerator ReduceRayPacket(bool player,long[] raw,bool rolled,string title,RayDamage result){result.Raw=zeroPacket?new long[raw.Length]:raw;yield break;}
 int ApplyMappedDamagePacket(int target,int source,long[] raw,bool critical=false){int amount=packetResolver==null?(int)raw.Sum():packetResolver(target,raw);damage+=amount;hitTargets.Add(target);packetTypes.Add(Array.FindIndex(raw,v=>v>0));lastRaw=(long[])raw.Clone();if(killMonster&&target==-1)deadMonster=true;if(killCaster)combat.PlayerHp=0;return amount;}
 readonly List<int> packetTypes=new List<int>(),maintainedDamages=new List<int>();Func<int,long[],int> packetResolver=null;
 bool deadMonster=false,killMonster=false,killCaster=false,zeroPacket=false;int outcomes;readonly List<int> hitTargets=new List<int>();long[] lastRaw;
 bool PlayerCanSustainCast=>combat.PlayerHp>0&&!combat.PlayerParalyzed;bool AcidArrowIsolated(int source,int target)=>AcidArrowResidue.CrossesBoundary(source,target,map.PlayerContained,map.MonsterContained);void RefreshUndeadFollowers(){}
 int agathysValue=0;int AgathysRetaliation(bool melee)=>melee?agathysValue:0;int ApplyAgathysRetaliation(int frost,bool source)=>0;
 IEnumerator MaintainConcentration(int amount){maintainedDamages.Add(amount);yield break;}void RefreshBestialSpirit(){}void ReconcileFallenRescue(){}void CheckBattleOutcome(){outcomes++;}
#if !FIRE_SHIELD_RUNTIME
 sealed class HitRetaliation {}
 HitRetaliation CaptureHitRetaliation(int attacker,bool melee,int? distance=null)=>new HitRetaliation();
 IEnumerator ResolveHitRetaliation(int attacker,HitRetaliation result){yield break;}
 IEnumerator MaintainRetaliationConcentration(HitRetaliation result){yield break;}
#endif
 static void C(bool v,string message){if(!v)throw new Exception(message);}
 static void Run(IEnumerator r){var stack=new Stack<IEnumerator>();stack.Push(r);while(stack.Count>0){var top=stack.Peek();if(!top.MoveNext()){stack.Pop();continue;}if(top.Current is IEnumerator)stack.Push((IEnumerator)top.Current);}}
 static TrainingGame Cast(int slot=2,bool attack=true,int target=-1){var g=new TrainingGame();Run(g.CastSpiritualWeapon(new Vector2Int(4,0),8,slot,target,attack,int.MaxValue));return g;}
 public static void Main(){
  var g=Cast();C(g.damage==10&&g.concentrationSpell=="灵体武器"&&g.concentrationExpiresWorld==10&&g.concentrationExpiresRound==int.MaxValue,"initial production attack uses 1d8+casting modifier and starts one-minute concentration");
  C(g.map.PointCalls==1&&g.map.BodyCalls==0&&g.map.LastPoint==new Vector2Int(4,0),"actual shared spell attack computes cover from remote force, not caster body");
  g=Cast(3);C(g.damage==18,"third slot adds one damage die");UnityEngine.Random.D20=20;g=Cast(3);C(g.damage==34,"critical doubles dice but not casting modifier");UnityEngine.Random.D20=1;g=Cast();C(g.damage==0&&g.spiritualWeapon.Active(0)&&g.fighter.Misses==1,"natural one misses but leaves force and concentration intact");UnityEngine.Random.D20=10;
  g=Cast(2,false);C(g.damage==0&&g.spiritualWeapon.Active(0),"initial attack is optional");Vector2Int p;g.spiritualColumn="5";g.spiritualRow="1";C(g.SpiritualControlError(out p)!="","even if a bonus were restored, control cannot occur in cast turn");
  g.combat.Round=2;g.spiritualAttack=true;g.spiritualColumn="9";g.spiritualRow="1";C(g.SpiritualControlError(out p)!="","invalid target is rejected before spending bonus");g.spiritualAttack=false;Run(g.ControlSpiritualWeapon());C(!g.combat.BonusActionAvailable&&g.spiritualWeapon.Cell==new Vector2Int(8,0)&&g.spiritualWeapon.Expires==10,"later control can move twenty feet without attacking and does not refresh expiry");
  g=Cast(2,false);g.combat.Round=2;g.spiritualColumn="5";g.spiritualRow="1";g.map.PlayerScene=1;C(g.SpiritualControlError(out p).Contains("位面"),"cross-plane controls suspend");Run(g.ControlSpiritualWeapon());C(g.combat.BonusActionAvailable&&g.spiritualWeapon.Active(5),"suspended control spends nothing and does not dismiss the force");g.map.PlayerScene=0;g.map.Player=new Vector2Int(19,0);g.spiritualAttack=true;Run(g.ControlSpiritualWeapon());C(g.damage==10&&!g.combat.BonusActionAvailable,"returning to the same plane resumes control even more than sixty feet away");
  g=Cast(2,false);g.combat.Round=2;g.spiritualColumn="5";g.spiritualRow="1";g.spiritualAttack=true;g.map.Player=new Vector2Int(19,0);g.combat.MonsterHelpless=true;Run(g.ControlSpiritualWeapon());C(g.damage==10,"helpless critical proximity is measured from the actual caster, not remote force");
  g=Cast(2,false);g.map.Player=new Vector2Int(4,0);g.combat.MonsterHelpless=true;Run(g.SpiritualWeaponAttack(-1));C(g.damage==18,"adjacent actual caster qualifies for helpless automatic critical");
  g=Cast(2,false);g.intercept=true;g.map.Player=new Vector2Int(4,0);Run(g.SpiritualWeaponAttack(0));C(g.damage==0&&g.spiritualWeapon.Active(0),"mirror interception cancels hit without dismissing persistent force");
  g=Cast(2,false);g.worldRounds=10;g.combat.Round=11;g.spiritualColumn="5";g.spiritualRow="1";Run(g.ControlSpiritualWeapon());C(g.combat.BonusActionAvailable&&!g.spiritualWeapon.Active(10),"expiry continues during suspended control and rejects spending");g.EndSpiritualWeapon();C(!g.spiritualWeapon.Active(0),"reset/concentration cleanup clears effect");
  g=Cast(2,false);g.map.PlayerContained=true;g.map.Player=g.map.Monster;C(!g.SpiritualTargetAllowed(g.spiritualWeapon.Cell,0,int.MaxValue,0),"external force cannot attack through swallowing total cover");C(g.SpiritualTargetAllowed(g.spiritualWeapon.Cell,0,-1,0),"internal force can target the swallowed creature");
  var e=new SpiritualWeaponEffect();e.Begin(new Vector2Int(4,4),2,8,0,1,0);C(e.CanMove(new Vector2Int(8,4),20)&&!e.CanMove(new Vector2Int(9,4),20)&&!e.CanMove(new Vector2Int(7,7),20)&&!e.CanMove(new Vector2Int(-1,4),20),"twenty-foot movement, diagonal rounding and bounds");C(!e.CanControl(1,0,0)&&e.CanControl(2,1,0)&&!e.CanControl(2,1,1),"later turn and same-plane gates independent of scene display");
  C(SpiritualWeaponEffect.Dice(1)==0&&SpiritualWeaponEffect.Dice(9,true)==16&&!SpiritualWeaponEffect.ValidTime(long.MaxValue-9),"slot and time boundaries");
  Console.WriteLine("PASS: production Spiritual Weapon initial/repeated attacks, optional attack, bonus timing, range, remote cover, caster proximity, critical dice, mirror, planar pause/resume, expiry, reset and containment");
 }
}
