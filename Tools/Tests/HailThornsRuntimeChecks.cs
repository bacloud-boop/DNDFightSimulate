using System;
using System.Collections;
using UnityEngine;
public sealed partial class TrainingGame
{
 sealed class HailTurns {public bool Active;}
 sealed class HailCaster {public bool CanCast=true;}
 sealed class SpellInterruption {public bool Cancelled;}
 readonly HailTurns summonTurns=new HailTurns();readonly HailCaster playerResurrectionCaster=new HailCaster();
 bool CanCastInCurrentForm=true,hailVoice=true,hailArmor=true,hailCountered;int spellCastSequence,voiceEvents;
 bool ArmorSpellAllowed(string name)=>hailArmor;bool SpellVoiceAllowed(bool player,string name)=>hailVoice;
 void PlayerCastWhileSubmerged(string name){voiceEvents++;}
 IEnumerator TryInterruptSpell(bool player,string name,SpellInterruption result){result.Cancelled=hailCountered;yield break;}
 static void HRun(TrainingGame g,IEnumerator routine){var stack=new System.Collections.Generic.Stack<IEnumerator>();stack.Push(routine);int ticks=0;while(stack.Count>0){if(++ticks>1000)throw new Exception("Hail coroutine stalled");var r=stack.Peek();if(!r.MoveNext()){stack.Pop();continue;}if(g.hailPromptPending)g.hailChoice=1;if(r.Current is IEnumerator)stack.Push((IEnumerator)r.Current);}}
 static HailHit Offer(TrainingGame g,int slot=1,bool before=false){g.hailSlot=slot;g.hailBefore=before;var hit=new HailHit();HRun(g,g.OfferHailThorns(-1,hit));return hit;}
 public static void CheckHail()
 {
  var g=New();var h=Offer(g);C(h.Slot==1&&!g.combat.BonusActionAvailable&&g.spellCastSequence==1&&g.voiceEvents==1,"hit-triggered cast consumes one bonus action and voices once");C(g.concentrationSpell=="咒唤兽群"&&g.animalPack.Active(0),"instant Hail does not end existing concentration");
  HRun(g,g.ProductionHailOrder(h,6));C(g.combat.Hp==984&&g.damageCounts[0]==1,"original weapon damage plus separate one-d10 area damage");
  g=New();g.map.Units.Add(new Unit{Id=8,X=8,Y=0});g.map.Units.Add(new Unit{Id=9,X=9,Y=0});h=Offer(g,9);C(h.Targets.Contains(-1)&&h.Targets.Contains(8)&&!h.Targets.Contains(9)&&!h.Targets.Contains(0),"exact five-foot target neighborhood, including allies but excluding distant caster");HRun(g,g.ResolveHailThorns(h));C(g.damageCounts[0]==9,"ninth slot rolls nine d10 without legacy five-die cap or critical doubling");
  g=New();g.map.Monster=new Vector2Int(1,0);h=Offer(g);C(h.Targets.Contains(0),"nearby caster is included");g.combat.ApplyDamage(995);HRun(g,g.ProductionHailOrder(h,20));C(g.combat.Hp==0&&g.combat.PlayerHp==0&&g.damageActors.Count==1&&g.damageActors[0]==0,"weapon-first lethal hit preserves blast center and still damages nearby caster");
  g=New();g.map.Monster=new Vector2Int(1,0);g.combat.ApplyDamage(995);h=Offer(g,1,true);HRun(g,g.ProductionHailOrder(h,20));C(g.combat.Hp==0&&g.damageActors.Contains(-1)&&g.damageActors.Contains(0),"spell-first damage reaches all targets even when caster falls, then avoids duplicate damage on dead original target");
  g=New();h=Offer(g,1,true);g.combat.AdjustTypedDamage(false,6,2);var types=new System.Collections.Generic.List<int>();g.combat.DamageApplied=(player,packet)=>{if(!player)types.Add(packet[2]>0?2:1);};HRun(g,g.ProductionHailOrder(h,6));C(types.Count==2&&types[0]==1&&types[1]==2,"spell-first processing preserves original weapon damage types across intervening area packets");
  g=New();g.hailCountered=true;h=Offer(g);C(h.Slot==0&&!g.combat.BonusActionAvailable,"counterspell spends bonus action but creates no Hail effect");HRun(g,g.ProductionHailOrder(h,6));C(g.combat.Hp==994&&g.damageCounts.Count==0,"counterspell leaves original weapon hit intact");
  g=New();h=Offer(g);UnityEngine.Random.D20=20;g.combat.DamageResistance=(p,t)=>t==1;HRun(g,g.ResolveHailThorns(h));C(g.combat.Hp==998,"successful save halves first, then piercing resistance floors again");UnityEngine.Random.D20=1;
  g=New();g.map.Contained=true;g.map.Monster=new Vector2Int(0,0);g.map.Units.Add(new Unit{Id=8,X=0,Y=0});h=Offer(g);C(h.Targets.Contains(0)&&h.Targets.Contains(-1)&&!h.Targets.Contains(8),"internal blast stays with swallowed caster and host");
  g=New();g.terrainActor=-1;C(!g.HailReady,"no bonus-action casting on another actor's turn");g.terrainActor=0;g.summonTurns.Active=true;C(!g.HailReady,"summon phase does not permit caster bonus action");g.summonTurns.Active=false;g.hailVoice=false;C(!g.HailReady,"silence blocks verbal casting");g.hailVoice=true;g.hailArmor=false;C(!g.HailReady,"untrained armor gate applies");g.hailArmor=true;g.playerResurrectionCaster.CanCast=false;C(!g.HailReady,"resurrection exhaustion prevents casting");
  Console.WriteLine("PASS: production Hail of Thorns on-hit bonus cast, selected weapon/spell order, lethal original hit, caster/allies, containment, counterspell, nine-slot dice and save/resistance ordering");
 }
}
class HailChecksMain {static void Main()=>TrainingGame.CheckHail();}
