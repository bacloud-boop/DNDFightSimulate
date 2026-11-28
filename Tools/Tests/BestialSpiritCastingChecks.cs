using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UnityEngine {
 public struct Rect {}public struct Color{public Color(float r,float g,float b,float a){}}
 public static class GUILayout{public static int SelectionGrid(int n,string[] options,int cols,object style)=>n;public static void Label(string text,object style){}public static string TextField(string text,object style)=>text;}
}
public static class AcidSplash {public static int Modifier(int n)=>(int)Math.Floor((n-10)/2.0);}
public sealed partial class TrainingGame
{
 readonly BattleMap map=new BattleMap();readonly CombatState combat=new CombatState();
 sealed class Classes{public int ProficiencyBonus=2;public int this[int n]=>n==2?1:0;}readonly Classes classes=new Classes();
 sealed class Caster {public bool CanCast=true;}readonly Caster playerResurrectionCaster=new Caster();
 sealed class Senses {public long Token(bool p,long world)=>0;}readonly Senses senses=new Senses();
 readonly int[] attributes={10,10,10,14,10,10};readonly List<string> history=new List<string>();
 bool rolling,spellTargeting,reactionPending,summonActing,PlayerCanSustainCast=true,CanCastInCurrentForm=true,validPool=true,visible=true;
 int draggingToken=-1,directSlotLevel=2,directCastingClass=2,concentrationExpiresRound,waterChecks,invisibilityBreaks;long worldRounds,concentrationExpiresWorld;
 string page="battle",panel="spells",selectedSpellName="Summon Beast",concentrationSpell="",dice="";object small=new object(),button=new object(),fieldStyle=new object();
 readonly Dictionary<int,HitDicePool> pools=new Dictionary<int,HitDicePool>();
 SummonedUnit steed;
 bool SummonHitDiceValid(string name)=>validPool;void ConfigureSummonHitDice(int id,string name){pools[id]=new HitDicePool();}void DrawSummonHitDiceChoices(string name){}
 bool ArmorSpellAllowed(string name)=>true;int VisibilityAt(bool p,Vector2Int cell,int n)=>visible?0:2;
 string IntegerField(string value,bool negative)=>value;void Fill(Rect r,Color c){}Rect CellRect(Rect b,Vector2Int p)=>b;
 void PlayerCastWhileSubmerged(string name){}void BreakOrdinaryInvisibility(bool p,string reason,long token){invisibilityBreaks++;}void ObserveWaterBreathing(){waterChecks++;}
 void EndConcentration(string reason){DismissBestialSpirit();concentrationSpell="";}
 static void C(bool ok,string why){if(!ok)throw new Exception(why);}
 static TrainingGame Game(){var g=new TrainingGame();g.map.Place(false,new Vector2Int(9,9));g.map.Lock();return g;}
 public static void CheckSpiritCasting()
 {
  var g=Game();var cell=new Vector2Int(2,0);C(g.BestialSpiritCastError(2,2,false,cell)=="","legal initial cast");C(g.CastBestialSpirit(2,2,false,cell),"direct cast uses one action");var old=g.bestialSpirit.Unit;
  C(old.MaxHp==30&&old.Armor==13&&old.AttacksPerAction==1&&old.Size==1&&g.map.Summons.Count==1&&!g.combat.ActionAvailable&&g.waterChecks==1&&g.invisibilityBreaks==1,"statistics, single action and creation events");C(g.pools[old.Id].Count==1&&g.concentrationExpiresWorld==600&&!old.HasVerbalOrders,"independent default HD, one hour and no initial orders");
  g.combat.EndRound();C(g.BestialSpiritCastError(2,4,true,cell)==""&&old.Alive&&g.map.Summons.Count==1,"replacement preview allows existing cell without dismissing current spirit");
  C(g.CastBestialSpirit(2,4,true,cell,false,"海豚"),"water form replacement");var unit=g.bestialSpirit.Unit;C(!old.Alive&&unit!=old&&unit.MaxHp==40&&unit.Armor==15&&unit.AttacksPerAction==2&&unit.SwimSpeed==30&&!unit.BreathesAir&&unit.BreathesWater&&unit.Name.Contains("海豚"),"water stats, appearance, replacement and breathing");
  C(BestialSpiritRules.DamageType=="穿刺"&&!UnderwaterCombatRules.Disadvantage(true,true,true,0,true),"2024 Rend is piercing, including land form underwater exemption");
  g=Game();g.combat.TryUseMagicAction();g.rolling=true;C(g.CastBestialSpirit(2,2,false,cell,true)&&g.invisibilityBreaks==0,"shared spell dispatcher already pays action and handles invisibility once");
  g=Game();g.combat.Aftermath=true;C(g.BestialSpiritCastError(2,2,false,cell)==""&&g.CastBestialSpirit(2,2,false,cell),"aftermath cast is allowed while manual control remains paused");g.SuspendSummonControlAftermath();C(g.bestialSpirit.Active,"aftermath retains summon");
  g=Game();g.visible=false;C(g.BestialSpiritCastError(2,2,false,cell)!=""&&g.combat.ActionAvailable,"invisible point rejected without payment");g.visible=true;g.validPool=false;C(!g.CastBestialSpirit(2,2,false,cell)&&g.combat.ActionAvailable,"invalid HD input rejected before action");g.validPool=true;C(!g.CastBestialSpirit(2,2,false,g.map.Player)&&g.combat.ActionAvailable,"occupied cell rejected before payment");
  g=Game();g.CastBestialSpirit(2,2,false,cell);g.worldRounds=600;g.RefreshBestialSpirit();C(!g.bestialSpirit.Active&&g.map.Summons.Count==0,"exact one-hour expiry removes unit and concentration");
  g=Game();g.CastBestialSpirit(2,2,false,cell);g.bestialSpirit.Unit.Damage(100);g.RefreshBestialSpirit();C(!g.bestialSpirit.Active&&g.concentrationSpell=="召唤野兽","0HP removes spirit without inventing a free replacement or early spell end");
  var preview=Game();var original=new BestialSpiritRules(2,4).CreateLandUnit(8);C(preview.map.AddSummon(original,cell),"preview original placement");var candidate=new BestialSpiritRules(4,4).CreateWaterUnit(9);preview.map.SetActorScene(9,7);C(preview.map.CanAddReplacingSummon(candidate,cell,8)&&original.Alive&&preview.map.Summons.Count==1&&preview.map.SceneOf(9)==7,"preview restores original list, life, candidate pose and scene");C(!preview.map.CanAddReplacingSummon(candidate,preview.map.Player,8)&&preview.map.Summons.Count==1&&original.Alive,"invalid replacement preview also restores original");
  Console.WriteLine("PASS: production Summon Beast placement, shared-dispatch action payment, replacement preview rollback, land/water statistics, appearance/HD, aftermath, range visibility, concentration expiry and piercing rules");
 }
}
class SpiritCastingMain {static void Main()=>TrainingGame.CheckSpiritCasting();}
