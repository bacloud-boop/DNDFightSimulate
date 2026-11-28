using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UnityEngine {
 public struct Rect {public Rect(float a,float b,float c,float d){}}public struct Vector2 {}
 public static class Random {public static int Range(int min,int max)=>max-1;}
 public static class GUI {public static bool enabled=true;}
 public static class GUILayout {
  public static void BeginArea(Rect r){}public static void EndArea(){}public static void Label(string s,object style){}
  public static Vector2 BeginScrollView(Vector2 p)=>p;public static void EndScrollView(){}public static bool Button(string text,object style)=>false;
  public static int SelectionGrid(int selected,string[] options,int columns,object style)=>selected;
 }
}
public static class BattleMap {
 public static int BodyDistance(Vector2Int a,int n,Vector2Int b,int m)=>Math.Max(0,Math.Abs(a.x-b.x)-Math.Max(n,m)+1)*5;
 public static List<Vector2Int> MovementAnchors(Vector2Int a,Vector2Int b)=>new List<Vector2Int>{b};
}
public static class PlayerAttributes {public static readonly string[] Names={"力量","敏捷","体质","感知","智力","魅力"};}
public static class AcidSplash {public static readonly string[] Names={"力量","敏捷","体质","感知","智力","魅力"};public static int Modifier(int score)=>(int)Math.Floor((score-10)/2.0);}
public sealed partial class TrainingGame
{
 sealed class ClassStub {public int ProficiencyBonus=2;public int this[int id]=>id==9?1:0;}
 sealed class CombatStub {public int Hp=10,Round=1,MonsterSleepStage=0;public bool PlayerDead=false;public bool PlayerCanAct=true,BonusActionAvailable=true;public bool TryUseBonusAction(){if(!PlayerCanAct||!BonusActionAvailable)return false;BonusActionAvailable=false;return true;}}
 sealed class TurnsStub {public bool Active=false;}sealed class SpellInterruption {public bool Cancelled;}
 sealed class RayDamage {public int Amount;}
 sealed class MapStub {public int Player=0,PlayerSpaces=1,PushCalls;public bool Shown=true,Contained;public Vector2Int? RescuePosition=null;public int RescueSpaces=1;public bool OnScene(int actor)=>Shown;public bool IsContained(bool player)=>Contained;public bool IsCarried(bool player)=>false;public bool Same=true;public bool SameScene(int a,int b)=>Same;public bool SceneBody(int a,out Vector2Int cell,out int n){cell=new Vector2Int(a==0?0:5,0);n=1;return true;}public int Limit=10;public int ForceMoveAway(bool player,int origin,int spaces,int feet){PushCalls++;return Math.Min(Limit,feet);}}
 sealed class GreaseFieldStub {public long Dc=10;}sealed class EnfeeblementStub {public long Dc=10;}sealed class SaveAdjustment {public int Value=0;}sealed class RollStub {public int Value=20;public int[] Dice=new[]{20};}
 readonly TurnBoundConditions turnConditions=new TurnBoundConditions();readonly FeignDeathEffect feignDeath=new FeignDeathEffect();
 readonly SensoryAfflictions sensory=new SensoryAfflictions();readonly AcidArrowResidue acidArrows=new AcidArrowResidue();readonly EnfeeblementStub enfeeblement=new EnfeeblementStub();
 bool sensoryOrderPending,sensorySavePlayer,sensoryRayOption,sensoryGreaseOption,sensorySleepOption,sensoryDaggersOption,sensorySphereOption,endAcidFirst;long sensorySaveChoice;int sleepRepeatRound=1;string dice;
 List<WrathfulSmiteEffects.Entry> sensoryWrathOptions;WrathfulSmiteEffects.Entry wrathfulSaveSelected;List<AcidArrowResidue.Entry> sensoryAcidOptions;List<SensoryAffliction> sensorySaveOptions;
 readonly List<string> endLog=new List<string>();
 GreaseFieldStub ActorGrease(int actor)=>null;bool RayWeakened(bool player)=>false;bool SphereEndAvailable(int actor)=>false;bool DaggersEndAvailable(int actor)=>false;
 IEnumerator ResolveGreaseSave(int actor,GreaseFieldStub field,string context){yield break;}IEnumerator EndFlamingSphereTurn(int actor){yield break;}IEnumerator EndDaggersTurn(int actor){yield break;}IEnumerator RepeatSleepSave(){yield break;}IEnumerator RepeatEnfeeblementSave(bool player){yield break;}
 IEnumerator ResolveAcidArrowResidue(AcidArrowResidue.Entry e){C(acidArrows.Take(e.Id),"residue claimed once");endLog.Add("acid:"+e.Id);yield break;}
 RollStub RollD20Test(bool player,int ability)=>new RollStub();int PlayerSavingThrowModifier(int ability)=>0;int MonsterSavingThrowModifier(int ability)=>0;
 IEnumerator ApplySaveAdjustments(bool player,bool automatic,SaveAdjustment result){yield break;}
 readonly VisionState vision=new VisionState{BaseLight=2};readonly FaerieFireEffect faerieFire=new FaerieFireEffect();readonly StarryWispEffect starryWisp=new StarryWispEffect();
 sealed class SenseStub {public bool Invisible(bool player,long world)=>true;}readonly SenseStub senses=new SenseStub();string concentrationSpell="";int concentrationExpiresRound,endedConcentrations;long concentrationExpiresWorld=-1;
 void EndConcentration(string reason){if(concentrationSpell!="")endedConcentrations++;concentrationSpell="";concentrationExpiresWorld=-1;EndShiningSmite();}
 static void AddFaeriePoints(List<double[]> points,Vector2Int cell,int n){for(int y=0;y<n;y++)for(int x=0;x<n;x++)points.Add(new[]{5.0*(cell.x+x),5.0*(cell.y+y)});}
 readonly MapStub map=new MapStub();bool knockedProne;void SetActorProne(int target){knockedProne=true;}void SyncTimedBuffs(){}
 readonly ClassStub classes=new ClassStub();readonly CombatStub combat=new CombatStub();readonly TurnsStub summonTurns=new TurnsStub();readonly RegenerationEffects regeneration=new RegenerationEffects();
 sealed class GreaseEntry {public int Actor=0,Field=1;}
readonly Queue<GreaseEntry> greaseEntries=new Queue<GreaseEntry>();
readonly Queue<int> packEntries=new Queue<int>();bool packFirst;IEnumerator ChoosePackEntryOrder(){packFirst=true;yield break;}IEnumerator ResolvePackEntry(int entry){yield break;}IEnumerator ResolveSleetSave(int actor,long revision){yield break;}
readonly Queue<int> spikeEntries=new Queue<int>(),webEntries=new Queue<int>(),daggerEntries=new Queue<int>(),heroismEntries=new Queue<int>(),auraStarts=new Queue<int>();
 readonly List<string> history=new List<string>();readonly int[] attributes={10,10,10,10,10,10};
 bool CanCastInCurrentForm=true,voice=true,countered,saveSuccess,alive=true,preferFire,chooseOther;int terrainActor=0,spellCastSequence,chosenCast=1,damage,saveCalls,turnHp=5,outcomeDeferDepth;long worldRounds=0,terrainTurn=1;
 object paperTexture=new object(),heading=new object(),label=new object(),small=new object(),button=new object();
 bool heroImmune,sourceVisible=true;bool HeroismImmune(int target)=>heroImmune;bool SteedFear(int target)=>false;bool ConjuredTargetVisible(bool player,int actor)=>sourceVisible;
 bool ArmorSpellAllowed(string name)=>voice;bool SpellVoiceAllowed(bool player,string name)=>voice;int SpellAbilityIndex(int caster)=>5;
 void Surface(Rect r,object style){}string TransitActorName(int actor)=>actor.ToString();void PlayerCastWhileSubmerged(string name){}
 IEnumerator TryInterruptSpell(bool player,string name,SpellInterruption result){result.Cancelled=countered;yield break;}
 IEnumerator PresentRoll(int sides,int[] values,string text){yield break;}
 int creatureType=9;int BanishType(int target)=>creatureType;
 bool BanishLiving(int target)=>alive;
 IEnumerator ReduceRayDamage(bool player,int amount,string type,string title,RayDamage result){result.Amount=amount;yield break;}
 int ApplyMappedDamagePacket(int target,int source,long[] raw){damage+=(int)raw[5];turnHp=Math.Max(0,turnHp-(int)raw[5]);return (int)raw[5];}
 IEnumerator MaintainConcentration(int amount){yield break;}
 IEnumerator RollMappedAbilitySave(int target,int source,long dc,HashSet<int> failed,string title,int ability,bool avoidsFrightened=false){if(title!="雷鸣斩"&&!title.StartsWith("激愤斩")&&!title.StartsWith("致盲斩")&&damage==0)throw new Exception("save occurred before ongoing damage");if(title.StartsWith("致盲斩"))endLog.Add("blind:"+dc);if(title.StartsWith("激愤斩")){C(avoidsFrightened,"fear saves must enable halfling Brave advantage");endLog.Add("fear:"+dc);}saveCalls++;if(!saveSuccess)failed.Add(target);yield break;}
 void RefreshBestialSpirit(){}void ReconcileFallenRescue(){}void CheckBattleOutcome(){}
 int HealRegenerationTarget(int actor,int amount){turnHp+=amount;return amount;}void ApplySteedLifeBond(int actual,int level){}
 bool rolling,greaseResolving,auraFirst=true,heroismFirst=true,terrainDaggersFirst=false;
 readonly List<string> drainLog=new List<string>();
 IEnumerator ChooseAuraVitalityOrder(){auraFirst=true;yield break;}IEnumerator ChooseHeroismStartOrder(){heroismFirst=true;yield break;}IEnumerator ChooseDaggersEntryOrder(){terrainDaggersFirst=false;yield break;}
 IEnumerator ResolveAuraVitalityHealing(int value){drainLog.Add("aura");yield break;}IEnumerator ResolveHeroismStart(int value){drainLog.Add("heroism");yield break;}
 IEnumerator ResolveDaggersEntry(int value){drainLog.Add("daggers");yield break;}IEnumerator ResolveSpikeEntry(int value){drainLog.Add("spike");yield break;}IEnumerator ResolveWebEntry(int value){drainLog.Add("web");yield break;}
 IEnumerator ResolveGreaseSave(int actor,int field,string label){drainLog.Add("grease");yield break;}
 static void C(bool v,string text){if(!v)throw new Exception(text);}
 static void Run(TrainingGame g,IEnumerator routine){var stack=new Stack<IEnumerator>();stack.Push(routine);while(stack.Count>0){var top=stack.Peek();if(!top.MoveNext()){stack.Pop();continue;}if(top.Current is IEnumerator){stack.Push((IEnumerator)top.Current);continue;}
   if(g.sensoryOrderPending){
    if(g.endAcidFirst&&g.sensoryAcidOptions.Count>0)g.sensorySaveChoice=-(g.sensoryAcidOptions[0].Id+6);
    else if(g.blindingSaveOptions.Count>0){g.blindingSaveSelected=g.blindingSaveOptions[g.blindingSaveOptions.Count-1];g.sensorySaveChoice=BlindingSmiteSaveChoice;}
    else if(g.sensoryWrathOptions.Count>0){g.wrathfulSaveSelected=g.sensoryWrathOptions[g.sensoryWrathOptions.Count-1];g.sensorySaveChoice=-6;}
    else g.sensorySaveChoice=-(g.sensoryAcidOptions[0].Id+6);
   }
   if(g.searingPromptPending)g.searingPromptChoice=g.chosenCast;
   if(g.startStatusOrderPending){int fire=g.startStatuses.FindIndex(e=>e.Kind==1);g.startStatusChoice=g.chooseOther&&g.OtherTerrainCount>0?g.startStatuses.Count:g.preferFire&&fire>=0?fire:0;}
  }}
 public static void Main()
 {
  var g=new TrainingGame();g.searingSlot=3;var initial=new SmiteHitResult();Run(g,g.OfferSearingSmite(-1,true,initial));C(initial.Fire==36&&!g.combat.BonusActionAvailable&&g.searingSmite.Pending(-1,0)[0].Dice==3&&g.spellCastSequence==1,"production on-hit offer pays bonus and doubles only initial dice");
  g=new TrainingGame();g.chosenCast=2;initial=new SmiteHitResult();Run(g,g.OfferSearingSmite(-1,false,initial));C(initial.Fire==0&&g.combat.BonusActionAvailable&&g.searingSmite.Pending(-1,0).Count==0,"declining leaves original weapon hit and bonus resource untouched");
  g=new TrainingGame();g.countered=true;initial=new SmiteHitResult();Run(g,g.OfferSearingSmite(-1,false,initial));C(initial.Fire==0&&!g.combat.BonusActionAvailable&&g.searingSmite.Pending(-1,0).Count==0,"counterspell consumes bonus but creates no fire or ongoing effect");
  g=new TrainingGame();g.terrainActor=-1;Run(g,g.OfferSearingSmite(-1,false,new SmiteHitResult()));C(g.combat.BonusActionAvailable&&!g.searingPromptPending,"cannot use on another creature's turn even for an opportunity hit");
  g.terrainActor=0;g.voice=false;Run(g,g.OfferSearingSmite(-1,false,new SmiteHitResult()));C(g.combat.BonusActionAvailable,"silence or unavailable casting blocks the offer");
  g=new TrainingGame();g.searingSmite.Add(-1,1,10,0);g.searingSmite.Add(-1,3,12,0);Run(g,g.ResolveSearingStart(-1));C(g.damage==18&&g.saveCalls==2,"one strongest damage event then separate saves for all independent casts");
  g.saveSuccess=true;Run(g,g.ResolveSearingStart(-1));C(g.searingSmite.Pending(-1,0).Count==0&&g.outcomeDeferDepth==0,"successful saves remove their own sources and outcome scope closes");
  foreach(bool fireFirst in new[]{true,false}){
   g=new TrainingGame();g.preferFire=fireFirst;g.regeneration.Begin(0,0);g.searingSmite.Add(0,1,10,0);g.startStatuses.Add(new StartStatus{Actor=0,Kind=0});g.QueueSearingStart(0);
   while(g.startStatuses.Count>0){Run(g,g.ChooseStartStatusOrder());C(g.startStatusFirst,"without terrain an actual status must be selected");Run(g,g.ResolveStartStatus());}
   C(g.turnHp==(fireFirst?1:0),"production turn-start order allows fire-before-regeneration or regeneration-before-fire");
  }
  g=new TrainingGame();g.startStatuses.Add(new StartStatus{Actor=0,Kind=0});g.greaseEntries.Enqueue(new GreaseEntry());g.chooseOther=true;Run(g,g.ChooseStartStatusOrder());C(!g.startStatusFirst&&g.startStatuses.Count==1&&g.greaseEntries.Count==1,"terrain-first choice preserves both queues for the shared dispatcher");
  g=new TrainingGame();g.chooseOther=true;g.preferFire=true;g.regeneration.Begin(0,0);g.searingSmite.Add(0,1,10,0);g.startStatuses.Add(new StartStatus{Actor=0,Kind=0});g.QueueSearingStart(0);g.greaseEntries.Enqueue(new GreaseEntry());
  Run(g,g.DrainGreaseEntries());C(g.drainLog.Count==1&&g.drainLog[0]=="grease"&&g.turnHp==1&&g.PendingTerrainCount==0&&!g.greaseResolving&&!g.rolling,"actual production terrain dispatcher integrates terrain-first then fire/regeneration without empty dequeue or stuck modal");
  Run(g,g.DrainGreaseEntries());C(g.drainLog.Count==1&&g.turnHp==1,"repeated drain cannot duplicate consumed effects");
  g=new TrainingGame();g.smiteKind=1;g.searingSlot=1;initial=new SmiteHitResult();Run(g,g.OfferSearingSmite(-1,false,initial));C(initial.Fire==0&&initial.Radiant==16&&!g.combat.BonusActionAvailable&&g.searingSmite.Pending(-1,0).Count==0,"Divine Smite shares the on-hit window but creates only radiant damage and no ongoing fire");
  g=new TrainingGame();g.smiteKind=1;g.searingSlot=3;g.creatureType=13;initial=new SmiteHitResult();Run(g,g.OfferSearingSmite(-1,true,initial));C(initial.Radiant==80&&initial.Fire==0,"third-slot Divine Smite against undead doubles all five d8 to ten on critical");
  for(int slot=1;slot<=9;slot++)foreach(int type in new[]{0,7,9,13})C(DivineSmiteRules.Dice(slot,type)==slot+1+(type==7||type==13?1:0),"Divine Smite slot scaling and exact fiend/undead bonus");
  g=new TrainingGame();g.smiteKind=2;g.searingSlot=3;initial=new SmiteHitResult();Run(g,g.OfferSearingSmite(-1,true,initial));C(initial.Thunder==48&&initial.ThunderDc==10&&initial.ThunderPush&&initial.Fire==0&&initial.Radiant==0&&!g.combat.BonusActionAvailable,"Thunderous Smite merges critical upcast thunder and snapshots save DC without creating ongoing fire");
  g.map.Limit=5;Run(g,g.ResolveThunderousSmiteHit(-1,initial));C(g.map.PushCalls==1&&g.knockedProne,"failed Strength save pushes only legal distance but knocks prone even when blocked");
  g=new TrainingGame();g.saveSuccess=true;initial=new SmiteHitResult{Thunder=12,ThunderDc=10,ThunderPush=true};Run(g,g.ResolveThunderousSmiteHit(-1,initial));C(g.map.PushCalls==0&&!g.knockedProne&&initial.Thunder==12,"successful Strength save prevents movement/prone but does not halve extra damage");
  g=new TrainingGame();g.alive=false;Run(g,g.ResolveThunderousSmiteHit(-1,initial));C(g.saveCalls==0&&g.map.PushCalls==0,"dead target does not roll or move after combined weapon damage");
  g=new TrainingGame();g.smiteKind=2;g.countered=true;initial=new SmiteHitResult();Run(g,g.OfferSearingSmite(-1,false,initial));C(!initial.ThunderPush&&initial.Thunder==0&&!g.combat.BonusActionAvailable,"countered Thunderous Smite creates neither extra damage nor a push while consuming the bonus");
  for(int slot=1;slot<=9;slot++)C(ThunderousSmiteRules.Dice(slot,true)==2*(slot+1),"Thunderous Smite exact upcast and critical dice");
  g=new TrainingGame();g.smiteKind=3;g.searingSlot=3;initial=new SmiteHitResult();Run(g,g.OfferSearingSmite(-1,true,initial));C(initial.Necrotic==36&&initial.Wrathful&&initial.WrathfulDc==10&&!g.combat.BonusActionAvailable,"Wrathful Smite critical and upcast damage uses necrotic without preapplying fear");
  Run(g,g.ResolveWrathfulSmiteHit(-1,initial));C(g.WrathfulFear(-1)&&g.WrathfulFearVisible(-1),"failed Wisdom save applies nonconcentration fear from the caster");
  C(!g.WrathfulMovementAllowed(-1,new Vector2Int(4,0))&&g.WrathfulMovementAllowed(-1,new Vector2Int(6,0)),"fear prohibits approaching while allowing movement away");
  g.sourceVisible=false;C(!g.WrathfulFearVisible(-1)&&!g.WrathfulMovementAllowed(-1,new Vector2Int(4,0)),"an unseen source removes attack disadvantage but not the approach prohibition");
  g.map.Same=false;C(g.WrathfulMovementAllowed(-1,new Vector2Int(4,0)),"a source on another plane imposes no geometric approach restriction");
  g.map.Same=true;g.heroImmune=true;C(!g.WrathfulFear(-1)&&g.WrathfulMovementAllowed(-1,new Vector2Int(4,0)),"Heroism suppresses fear penalties without deleting independent spell timers");
  g.heroImmune=false;g.wrathfulSmite.Add(-1,12,0);g.saveSuccess=true;var fears=g.wrathfulSmite.Pending(-1,0);Run(g,g.RepeatWrathfulSave(fears[0]));C(g.wrathfulSmite.Pending(-1,0).Count==1,"successful repeat save ends only its own cast");Run(g,g.RepeatWrathfulSave(fears[1]));C(!g.WrathfulFear(-1),"last successful save removes the condition");
  g=new TrainingGame();g.heroImmune=true;initial=new SmiteHitResult{Wrathful=true,WrathfulDc=12,Necrotic=6};Run(g,g.ResolveWrathfulSmiteHit(-1,initial));C(g.saveCalls==0&&!g.wrathfulSmite.Active(-1,0)&&initial.Necrotic==6,"fear immunity does not remove necrotic hit damage");
  g=new TrainingGame();g.wrathfulSmite.Add(-1,12,0);C(g.wrathfulSmite.Active(-1,9)&&!g.wrathfulSmite.Active(-1,10),"fear expires exactly at one minute");g.wrathfulSmite.Clear();C(!g.wrathfulSmite.Active(-1,0),"reset clears lingering fear");
  foreach(bool acidFirst in new[]{false,true}){
   g=new TrainingGame();g.endAcidFirst=acidFirst;g.saveSuccess=true;g.wrathfulSmite.Add(-1,10,0);g.wrathfulSmite.Add(-1,12,0);g.acidArrows.Add(0,-1,2);g.acidArrows.StartTurn(-1);Run(g,g.RepeatSensorySaves(false));
   C(g.endLog.Count==3&&g.endLog[0]==(acidFirst?"acid:1":"fear:12")&&g.endLog[2]==(acidFirst?"fear:10":"acid:1"),"actual end dispatcher allows selecting either residue or an individual fear save first");
   C(!g.sensoryOrderPending&&!g.wrathfulSmite.Active(-1,0)&&g.acidArrows.Pending(-1).Count==0,"all actual turn-end choices are consumed and modal closes");
  }
  g=new TrainingGame();g.concentrationSpell="猎人印记";g.smiteKind=4;g.searingSlot=3;initial=new SmiteHitResult();Run(g,g.OfferSearingSmite(-1,true,initial));C(initial.Radiant==36&&initial.ShiningSlot==3&&g.endedConcentrations==1&&g.concentrationSpell=="闪耀斩"&&g.concentrationExpiresWorld==10&&g.concentrationExpiresRound==int.MaxValue,"Shining Smite replaces old concentration at cast start and doubles scaled radiant dice");
  g.BeginShiningSmite(-1,initial);C(g.ShiningOutlined(-1)&&g.vision.Light(25,0,0)==0&&g.vision.Light(30,0,0)==0&&g.vision.Light(31,0,0)==2,"Shining Smite emits exactly five-foot bright light after damage without an invented dim-light ring");
  C(g.FaerieAdvantage(-1,false)&&!g.InvisibleWithFaerie(false),"actual attack/visibility adapters grant advantage even unseen and suppress invisibility benefits");
  g.map.Shown=false;g.RefreshShiningSmite();C(g.ShiningOutlined(-1)&&g.vision.Zones.Count==0,"changing the viewed plane hides illumination without deleting attached magic");g.map.Shown=true;g.RefreshShiningSmite();C(g.vision.Light(25,0,0)==0,"returning the view restores active illumination");
  g.map.Contained=true;g.RefreshShiningSmite();C(g.vision.Zones.Count==0&&g.ShiningOutlined(-1),"swallowing hides external light but retains attached reveal state");g.map.Contained=false;
  g.EndConcentration("test");C(!g.ShiningOutlined(-1)&&g.InvisibleWithFaerie(false)&&!g.FaerieAdvantage(-1,true),"ending concentration restores underlying invisibility without destroying its source");
  g=new TrainingGame();g.concentrationSpell="闪耀斩";g.BeginShiningSmite(-1,new SmiteHitResult{ShiningSlot=2});g.vision.AddZone(true,25,0,2,0);g.RefreshShiningSmite();C(!g.ShiningOutlined(-1)&&g.concentrationSpell=="","Darkness dispels an overlapping second-level Shining Smite");
  g=new TrainingGame();g.concentrationSpell="闪耀斩";g.BeginShiningSmite(-1,new SmiteHitResult{ShiningSlot=3});g.vision.AddZone(true,25,0,2,0);g.RefreshShiningSmite();C(g.ShiningOutlined(-1)&&g.vision.Light(25,0,0)==0,"third-level Shining Smite light survives and illuminates within Darkness");
  g=new TrainingGame();g.concentrationSpell="旧法术";g.smiteKind=4;g.countered=true;initial=new SmiteHitResult();Run(g,g.OfferSearingSmite(-1,false,initial));C(initial.ShiningSlot==0&&initial.Radiant==0&&g.concentrationSpell==""&&g.endedConcentrations==1&&!g.combat.BonusActionAvailable,"countered Shining Smite still loses prior concentration and spends bonus but preserves original weapon hit");
  C(ShiningSmiteEffect.Dice(1)==0&&ShiningSmiteEffect.Dice(9,true)==18,"Shining Smite minimum slot and high-slot critical bound");
  g=new TrainingGame();g.smiteKind=5;g.searingSlot=3;initial=new SmiteHitResult();Run(g,g.OfferSearingSmite(-1,true,initial));C(initial.Radiant==48&&initial.Blinding&&initial.BlindingDc==10&&!g.combat.BonusActionAvailable,"third-slot Blinding Smite rolls six d8 on critical and snapshots DC");
  g.ApplyBlindingSmiteHit(-1,initial);C(g.ActorBlinded(-1)&&g.saveCalls==0&&g.concentrationSpell=="","Blinding Smite applies blindness directly, without initial save or concentration");
  g.blindingSmite.Add(false,true,12,0);g.saveSuccess=true;Run(g,g.RepeatSensorySaves(false));C(!g.ActorBlinded(-1)&&g.saveCalls==2&&g.endLog[0]=="blind:12"&&g.endLog[1]=="blind:10","actual turn-end dispatcher can choose individual blinding casts and clears each success independently");
  g=new TrainingGame();g.ApplyBlindingSmiteHit(-1,new SmiteHitResult{Blinding=true,BlindingDc=10});g.saveSuccess=false;Run(g,g.RepeatSensorySaves(false));C(g.ActorBlinded(-1)&&g.saveCalls==1,"failed repeat save preserves blindness");g.worldRounds=10;C(!g.ActorBlinded(-1),"blindness expires at exactly one minute");
  foreach(bool acidFirst in new[]{false,true}){
   g=new TrainingGame();g.endAcidFirst=acidFirst;g.saveSuccess=true;g.blindingSmite.Add(false,true,11,0);g.wrathfulSmite.Add(-1,12,0);g.acidArrows.Add(0,-1,2);g.acidArrows.StartTurn(-1);Run(g,g.RepeatSensorySaves(false));
   C(g.endLog.Count==3&&g.endLog[0]==(acidFirst?"acid:1":"blind:11")&&!g.sensoryOrderPending,"actual mixed end ordering keeps blind/fear/residue choices independent with no ID collision");
  }
  g=new TrainingGame();g.blindingSmite.Add(false,true,10,0);g.blindingSmite.Add(false,true,12,0);g.sensory.Add(false,true,13,0);g.sensory.Add(false,false,15,0);g.feignDeath.Cast(-1,0);g.turnConditions.Add(0,-1,0,0);int cleared=g.sensory.EndCondition(false,true,0)+g.EndAuxiliaryCondition(-1,0,0);C(cleared==5&&!g.ActorBlinded(-1)&&g.sensory.Deafened(false,0)&&g.feignDeath.Active(-1,0),"actual restoration hook clears all blinding sources and suppressed old casts while preserving deafness and remaining feign death");
  g=new TrainingGame();g.smiteKind=5;g.countered=true;initial=new SmiteHitResult();Run(g,g.OfferSearingSmite(-1,false,initial));g.ApplyBlindingSmiteHit(-1,initial);C(initial.Radiant==0&&!g.ActorBlinded(-1)&&!g.combat.BonusActionAvailable,"countered Blinding Smite creates neither radiant damage nor blindness");
  g=new TrainingGame();g.smiteKind=5;g.searingSlot=9;initial=new SmiteHitResult();Run(g,g.OfferSearingSmite(-1,false,initial));C(initial.Radiant==72&&BlindingSmiteDice(2,false)==0,"upcast uses nine d8 and rejects below third slot");
  Console.WriteLine("PASS: production six on-hit smites, Shining light/reveal/concentration on-hit choice, bonus/turn/voice gates, critical dice, counterspell isolation, strongest ongoing damage, independent saves and selectable regeneration/terrain order");
 }
}
