using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UnityEngine {
 public struct Vector2 {public float x,y;}
 public struct Rect {public float x,y,width,height;public Rect(float a,float b,float c,float d){x=a;y=b;width=c;height=d;}public bool Contains(Vector2 p)=>false;}
 public struct Color {public Color(float r,float g,float b,float a){}}
 public enum EventType {MouseDown}public class Event {public static Event current=new Event();public EventType type;public int button;public void Use(){}}
 public static class GUI {public static bool enabled=true;public static void Label(Rect r,string text,object style){}}
 public static class GUILayout {public static int SelectionGrid(int a,string[] b,int c,object d)=>a;public static bool Button(string text,object style)=>false;public static void Label(string text,object style){}public static void BeginArea(Rect r){}public static void EndArea(){}}
 public static class Random {public static int D20=1;public static readonly Queue<int> Dice=new Queue<int>();public static int Range(int min,int max)=>max==21?(Dice.Count>0?Dice.Dequeue():D20):max-1;}
}
public static class AcidSplash {public static int Modifier(int score)=>(int)Math.Floor((score-10)/2.0);}
public static class PlayerAttributes {public static readonly string[] Names={"力量","敏捷","体质","感知","智力","魅力"};}
public sealed partial class TrainingGame
{
 sealed class Breath {public int D20Penalty;}
 sealed class Unit {public int Id=7,X=8,Y=4,Spaces=1,SleepStage;public bool Alive=true;public readonly Breath Breathing=new Breath();}
 sealed class Rescue {public bool Dead;public int Hp=10,RevivalPenalty;public readonly Breath Breathing=new Breath();public int Ability(int a)=>10;}
 sealed class MapStub {
  public Vector2Int Player=new Vector2Int(0,0),Monster=new Vector2Int(7,0);public int PlayerSpaces=1,MonsterSpaces=1,Size=30,Scene,PlayerScene,MonsterScene,LastCoverSpaces,Cover;public bool Contained;public bool? RescueCargoPlayer=null;public Vector2Int LastCover;
  public readonly List<Unit> Units=new List<Unit>();public bool IsContained(bool player)=>player&&Contained;public int SceneOf(int actor)=>actor==0?PlayerScene:MonsterScene;
  public bool Inside(Vector2Int p,int n)=>p.x>=0&&p.y>=0&&p.x<=Size-n&&p.y<=Size-n;
  public bool CanPlaceSummon(Vector2Int p,int n)=>Inside(p,n)&&!BattleMap.Overlap(p,n,Player,PlayerSpaces)&&!BattleMap.Overlap(p,n,Monster,MonsterSpaces);
  public int DistanceToCell(bool player,Vector2Int cell)=>BattleMap.BodyDistance(player?Player:Monster,player?PlayerSpaces:MonsterSpaces,cell,1);
  public RouteCostMeter RouteMeter(Vector2Int a,Vector2Int b,bool c){int total=0;var old=a;foreach(var p in BattleMap.MovementAnchors(a,b)){total+=BattleMap.FeetBetween(old,p);old=p;}return new RouteCostMeter(BattleMap.FeetBetween(a,b),total,c);}
  public int CreatureCoverFromPoint(Vector2Int p,int actor,int spaces){LastCover=p;LastCoverSpaces=spaces;return Cover;}public int CreatureCoverBonus(int a,int b)=>0;
 }
 sealed class Classes {public int ProficiencyBonus=2;}
 sealed class RayDamage {public int Amount;}sealed class ArmorSaveRoll {public int Value;public int[] Dice;}sealed class SaveAdjustment {public int Value;}
 sealed class GreaseEntry {public int Actor,Field;}
 readonly MapStub map=new MapStub();readonly CombatState combat=new CombatState();readonly Classes classes=new Classes();readonly Rescue rescueCarrier=new Rescue();readonly int[] attributes={14,14,14,14,14,14};
 readonly List<string> history=new List<string>(),order=new List<string>();readonly List<int> damageCounts=new List<int>();readonly List<int> damageActors=new List<int>();
 readonly Queue<GreaseEntry> greaseEntries=new Queue<GreaseEntry>();readonly Queue<int> spikeEntries=new Queue<int>(),webEntries=new Queue<int>(),daggerEntries=new Queue<int>(),heroismEntries=new Queue<int>(),auraStarts=new Queue<int>();readonly List<int> startStatuses=new List<int>();
 bool packConfirm=true,packPreferFirst=true,greaseResolving,rolling,reactionPending,spellTargeting,visible=true,startStatusFirst,auraFirst,heroismFirst,terrainDaggersFirst,weak;
 int terrainActor=0,draggingToken=-1,outcomeDeferDepth,concentrationExpiresRound;long worldRounds,terrainTurn=1,concentrationExpiresWorld;string concentrationSpell="",panel="",selectedSpellName="",dice="",page="battle",visionColumn="4",visionRow="1";
 object button=new object(),small=new object(),heading=new object(),paperTexture=new object();bool PlayerTurnAllowed=true;
 string IntegerField(string x,bool negative)=>x;void Surface(Rect r,object t){}void Fill(Rect r,Color c){}bool Btn(Rect r,string s,bool enabled)=>false;void DrawMap(Rect r,bool drag){}Vector2 BoardPointer()=>new Vector2();Rect CellRect(Rect r,Vector2Int p)=>r;Rect BodyRect(Rect r,Vector2Int p,int n)=>r;void StartCoroutine(IEnumerator routine){}
 Unit FindSummon(int id)=>map.Units.Find(u=>u.Id==id);int SpellAbilityIndex(int caster)=>3;
 bool VisionPoint(out Vector2Int p){int x,y;bool valid=int.TryParse(visionColumn,out x)&int.TryParse(visionRow,out y);p=new Vector2Int(x-1,y-1);return valid&&map.Inside(p,1);}
 int VisibilityAt(bool viewer,Vector2Int cell,int spaces)=>visible?0:2;
 bool ConjuredTargetVisible(bool player,int actor)=>visible;string TransitActorName(int actor)=>actor.ToString();
 bool HealingHandsTarget(int actor,out Vector2Int p,out int n,out string name){p=actor==0?map.Player:map.Monster;n=actor==0?map.PlayerSpaces:map.MonsterSpaces;name=actor.ToString();if(actor==0)return !combat.PlayerDead;if(actor==-1)return combat.Hp>0;var u=FindSummon(actor);if(u==null||!u.Alive)return false;p=new Vector2Int(u.X,u.Y);n=u.Spaces;return true;}
 IEnumerable<int> BreathActors(){yield return 0;if(combat.Hp>0)yield return -1;foreach(var u in map.Units)if(u.Alive)yield return u.Id;}
 IEnumerator PresentRoll(int sides,int[] values,string label){if(sides==10)damageCounts.Add(values.Length);yield break;}
 IEnumerator ReduceRayDamage(bool player,int amount,string type,string title,RayDamage result){result.Amount=amount;yield break;}
 int ApplyMappedAreaDamage(int actor,int caster,int amount,int type,string title){order.Add("pack");damageActors.Add(actor);var raw=new long[13];raw[type]=amount;var packet=combat.ResolveTypedPacket(actor==0,raw);int value=(int)packet[type];if(actor==0)combat.ApplyPlayerDamage(value,sourcePlayer:true);else if(actor==-1)combat.ApplyDamage(value);return value;}
 IEnumerator MaintainConcentration(int damage){if(combat.PlayerHp<=0){EndAnimalPack();concentrationSpell="";}yield break;}
 void ReconcileFallenRescue(){}void RefreshBestialSpirit(){}void NoticeCastingInterruption(){}void CheckBattleOutcome(){}
 bool SpeciesSaveAdvantage(bool p,int ability)=>false;bool BeaconHope(int actor)=>false;bool ResurrectionD20Disadvantage(bool player)=>weak;
 ArmorSaveRoll RollArmorSave(bool player,int ability,bool avoidsOrEndsFrightened=false)=>RollD20Test(player,ability:ability);
 int PlayerSavingThrowModifier(int a)=>2;int MonsterSavingThrowModifier(int a)=>0;int BondBonus(int actor)=>0;int SummonSaveModifier(Unit unit,int a)=>0;
 ArmorSaveRoll RollRescueSave(int a)=>RollSavingThrowDice(false,false);ArmorSaveRoll RollSummonSavingThrow(Unit u,int a)=>RollSavingThrowDice(false,false);
 IEnumerator ApplySaveAdjustments(bool player,bool automatic,SaveAdjustment result,int ability=-1,bool ignoreCover=false){yield break;}
 IEnumerator ChooseStartStatusOrder(){startStatusFirst=true;yield break;}IEnumerator ResolveStartStatus(){startStatuses.RemoveAt(0);yield break;}
 IEnumerator ChooseAuraVitalityOrder(){auraFirst=true;yield break;}IEnumerator ResolveAuraVitalityHealing(int x){yield break;}IEnumerator ChooseHeroismStartOrder(){heroismFirst=true;yield break;}IEnumerator ResolveHeroismStart(int x){yield break;}
 IEnumerator ChooseDaggersEntryOrder(){terrainDaggersFirst=false;yield break;}IEnumerator ResolveDaggersEntry(int x){yield break;}IEnumerator ResolveSpikeEntry(int x){yield break;}IEnumerator ResolveWebEntry(int x){yield break;}IEnumerator ResolveGreaseSave(int a,int f,string s){order.Add("grease");yield break;}
 static void C(bool ok,string reason){if(!ok)throw new Exception(reason);}
 static void Run(TrainingGame g,IEnumerator routine){int ticks=0;var stack=new Stack<IEnumerator>();stack.Push(routine);while(stack.Count>0){if(++ticks>10000)throw new Exception("Coroutine did not complete");var r=stack.Peek();if(!r.MoveNext()){stack.Pop();continue;}if(g.packPromptPending)g.packDecision=g.packConfirm?1:2;if(g.packOrderPending)g.packDecision=g.packPreferFirst?1:2;if(r.Current is IEnumerator)stack.Push((IEnumerator)r.Current);}}
 static TrainingGame New(int slot=3){var g=new TrainingGame();g.CastConjureAnimals(new Vector2Int(3,0),0,slot,0);return g;}
 public static void CheckPack()
 {
  var g=New();C(g.animalPack.Active(0)&&g.packEntries.Count==0&&g.damageCounts.Count==0,"creation causes no immediate save or damage");C(!g.animalPack.CanMove(1,0),"must actually move after casting before controlling pack");
  g.map.Player=new Vector2Int(1,0);g.animalPack.NoticePlayerMovement(1,0);g.packColumn="5";g.packRow="1";int actions=g.combat.ActionsRemaining;bool bonus=g.combat.BonusActionAvailable;
  Run(g,g.MoveAnimalPack());C(g.animalPack.X==4&&g.animalPack.MovementRemaining==25&&g.combat.ActionsRemaining==actions&&g.combat.BonusActionAvailable==bonus,"moving pack spends only independent movement");C(g.damageCounts.Count==1&&g.damageCounts[0]==3&&g.combat.Hp==970,"movement near visible target causes 3d10 on failed Dexterity save");
  C(g.map.LastCover==new Vector2Int(4,0)&&g.map.LastCoverSpaces==2,"Dexterity cover originates from complete pack footprint");C(!g.PackEndAvailable(-1),"same target cannot save again on same global turn, even at turn end");
  g.packColumn="10";Run(g,g.MoveAnimalPack());C(g.animalPack.MovementRemaining==0&&g.animalPack.X==9,"multiple controls share cumulative thirty feet");g.animalPack.NoticePlayerMovement(1,0);C(g.animalPack.MovementRemaining==0,"additional player movement does not replenish pack movement");
  g.terrainTurn=2;g.animalPack.StartPlayerTurn(2);C(g.animalPack.MovementRemaining==30&&!g.animalPack.CanMove(2,0),"next player turn resets thirty feet but requires fresh player movement");
  g=New(5);g.map.Monster=new Vector2Int(6,0);g.QueuePackEntry(-1,"回合末");g.packConfirm=false;Run(g,g.DrainGreaseEntries());C(g.combat.Hp==1000&&g.animalPack.Available(-1,1,0),"declining a trigger causes no save and does not use once-per-turn allowance");
  g.packConfirm=true;g.QueuePackEntry(-1,"回合末");g.QueuePackEntry(-1,"重复进入");C(g.packEntries.Count==1,"pending same-target triggers are deduplicated");Run(g,g.DrainGreaseEntries());C(g.damageCounts[0]==5&&g.combat.Hp==950,"upcasting adds one d10 per slot");
  g.terrainTurn=3;g.QueuePackEntry(-1,"下一生物回合");Run(g,g.DrainGreaseEntries());C(g.combat.Hp==900,"once per global creature turn rather than once per round");
  g=New();g.map.Monster=new Vector2Int(6,0);UnityEngine.Random.D20=20;g.QueuePackEntry(-1,"成功");Run(g,g.DrainGreaseEntries());C(g.combat.Hp==1000&&g.damageCounts.Count==0&&!g.animalPack.Available(-1,1,0),"successful save causes zero damage and uses the allowance");UnityEngine.Random.D20=1;
  foreach(bool first in new[]{true,false}){g=New();g.map.Monster=new Vector2Int(6,0);g.packPreferFirst=first;g.QueuePackEntry(-1,"进入");g.greaseEntries.Enqueue(new GreaseEntry());Run(g,g.DrainGreaseEntries());C(g.order.Count==2&&g.order[0]==(first?"pack":"grease"),"actual shared terrain dispatcher respects player-selected order");}
  g=New();g.map.Monster=new Vector2Int(8,0);var from=g.map.Monster;g.map.Monster=new Vector2Int(6,0);g.ObservePackMovement(-1,from,g.map.Monster,1,1,false);C(g.packEntries.Count==1&&g.PackPathNeeded(from,g.map.Monster,1),"entering range queues a trigger and requests stepped movement");
  g=New();g.map.Monster=new Vector2Int(5,0);g.ObservePackMovement(-1,new Vector2Int(6,0),g.map.Monster,1,1,false);C(g.packEntries.Count==1,"entering a new space already inside ten feet also triggers");
  g=New();g.map.Monster=new Vector2Int(6,0);g.visible=false;g.QueuePackEntry(-1,"不可见");C(g.packEntries.Count==0,"unseen targets do not trigger");g.visible=true;g.map.MonsterScene=1;g.QueuePackEntry(-1,"其他位面");C(g.packEntries.Count==0,"different-plane target excluded");
  g=New();g.map.Player=new Vector2Int(2,0);C(g.PackStrengthAdvantage,"caster within five feet gains Strength saving throw advantage");UnityEngine.Random.Dice.Enqueue(3);UnityEngine.Random.Dice.Enqueue(18);var save=g.RollD20Test(true,ability:0);C(save.Dice.Length==2&&save.Value==18,"actual shared saving-throw runtime rolls advantage for Strength");g.weak=true;UnityEngine.Random.Dice.Enqueue(7);save=g.RollD20Test(true,ability:0);C(save.Dice.Length==1&&save.Value==7,"advantage cancels resurrection disadvantage");g.map.Contained=true;C(!g.PackStrengthAdvantage,"swallowed caster isolated from external pack");
  g=New();g.packColumn=int.MaxValue.ToString();g.packRow="1";Vector2Int target;C(!g.PackInput(out target),"overflow-sized coordinates rejected");g.worldRounds=100;C(!g.animalPack.Active(100)&&!g.PackEndAvailable(-1),"ten-minute expiry prevents triggers and movement");
  g=New();g.map.Monster=new Vector2Int(6,0);g.QueuePackEntry(-1,"旧施法");var old=g.packEntries.Dequeue();g.animalPack.Begin(3,0,0,3,12,1,0,1);Run(g,g.ResolvePackEntry(old));C(g.damageCounts.Count==0,"old queued entries cannot affect a replacement casting");g.EndAnimalPack();C(!g.animalPack.Active(0)&&g.packEntries.Count==0,"ending concentration/reset clears state and queues");
  C(!new ConjureAnimalsEffect().Begin(0,0,0,3,10,0,long.MaxValue-99,0),"overflowing duration rejected");
  Console.WriteLine("PASS: production Conjure Animals casting, non-damaging creation, optional triggers/saves, slot dice, per-turn limits, cumulative 30-foot movement, terrain ordering, cover origins, Strength advantage, planes and cleanup");
 }
}
class PackChecksMain {static void Main()=>TrainingGame.CheckPack();}
