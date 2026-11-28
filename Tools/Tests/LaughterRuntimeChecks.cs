using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
 sealed partial class Unit {public Func<bool> ExtraIncapacitated,ExtraProne;public Action<int> ConditionDamageReceived;public void CancelDodge(){}}
 sealed partial class Rescue {public Func<bool> ExtraIncapacitated,ExtraProne;public Action<int> ConditionDamageReceived;}
 sealed partial class MapStub {public bool HasContainedCreature=>Contained||MonsterContained;}
 sealed class Fighter {public void CancelAttackAction(){}}readonly Fighter fighter=new Fighter();object actionWeapon,label=new object();
 bool IsConcentrating=>concentrationSpell!="";bool CanSeeOwnBody()=>visible;bool PlayerSeesMonster=>visible;
 void NoteRestDamage(int actor,int amount){}
 bool IsWildShaped;bool releaseRestoredHost;int restores,releases;
 int MoonbeamRootSpaces(int actor)=>1;int MoonbeamRootSize(int actor)=>2;
 IEnumerator ReleaseSwallowedTarget(string reason,int n=0,int size=-1){map.Contained=map.MonsterContained=false;releaseRestoredHost=true;releases++;yield break;}
 void FinishMoonbeamTrueForm(int actor,bool restored){IsWildShaped=false;restores++;}void ObserveForcedSharedStop(int actor){}
 void EndConcentration(string reason){EndLaughter();concentrationSpell="";}
 sealed class HellishDamage {}readonly Queue<HellishDamage> hellishDamage=new Queue<HellishDamage>();readonly Queue<int> undeadSaves=new Queue<int>(),bondConcentration=new Queue<int>();
 int bondSaves;readonly List<int> autoOrder=new List<int>();
 IEnumerator DrainResistanceRolls(){yield break;}IEnumerator DrainUndeadSaves(){undeadSaves.Clear();yield break;}
 IEnumerator DrainHellishRebukes(){hellishDamage.Clear();yield break;}
 IEnumerator DrainBondConcentration(){while(bondConcentration.Count>0){bondConcentration.Dequeue();if(IsConcentrating)bondSaves++;}yield break;}
 static TrainingGame Laugh(params int[] targets){var g=new TrainingGame();g.map.Monster=new Vector2Int(3,0);g.InitializeLaughter();Run(g,g.CastLaughter(targets,0));return g;}
 static void ResolveDamage(TrainingGame g,int first)
 {
  var stack=new Stack<IEnumerator>();stack.Push(g.DrainDamageEffects());int ticks=0;
  while(stack.Count>0){C(++ticks<1000,"damage dispatcher termination");var r=stack.Peek();if(!r.MoveNext()){stack.Pop();continue;}if(g.damageOrderPending){int index=g.damageOrderKeys.IndexOf(first);g.damageOrderChoice=index<0?0:index;}if(r.Current is IEnumerator)stack.Push((IEnumerator)r.Current);}
 }
 public static void CheckLaughter()
 {
  UnityEngine.Random.D20=1;var g=Laugh(-1);C(g.Laughing(-1)&&g.combat.MonsterProne&&!g.combat.MonsterCanAct&&g.combat.MonsterCanMove&&!g.combat.MonsterReactionAvailable,"failed save imposes prone/incapacitated but allows crawling");
  C(g.LaughterAlive(-1)&&g.laughter.Expires==10&&g.laughter.Dc==12,"duration and class spell save DC");
  UnityEngine.Random.D20=20;Run(g,g.RepeatLaughterSave(-1,g.laughter.Revision));C(!g.Laughing(-1)&&!g.combat.MonsterProne&&g.combat.MonsterReactionAvailable&&!g.IsConcentrating,"end turn success removes spell-only conditions and last target concentration");
  UnityEngine.Random.D20=1;g=Laugh(-1);g.combat.MonsterProne=true;UnityEngine.Random.D20=20;Run(g,g.RepeatLaughterSave(-1,g.laughter.Revision));C(g.combat.MonsterProne,"independent prior or later knockdown remains after spell ends");
  UnityEngine.Random.D20=1;g=Laugh(-1);g.combat.GrantMonsterTemporaryHp(5);g.combat.ApplyDamage(3);C(g.combat.Hp==1000&&g.laughterSaves.Count==1,"temporary HP absorbed positive damage queues exactly one repeat");UnityEngine.Random.Dice.Enqueue(1);UnityEngine.Random.Dice.Enqueue(20);ResolveDamage(g,0);C(!g.Laughing(-1)&&UnityEngine.Random.Dice.Count==0,"damage save rolls twice and takes higher");
  UnityEngine.Random.D20=1;g=Laugh(-1);g.combat.ApplyDamage(0);C(g.laughterSaves.Count==0,"zero damage does not trigger");g.combat.ApplyDamage(1);g.combat.ApplyDamage(1);UnityEngine.Random.D20=20;ResolveDamage(g,0);C(g.laughterSaves.Count==0&&!g.Laughing(-1),"later queued saves skip after successful escape");
  UnityEngine.Random.D20=1;g=Laugh(-1);g.weak=true;UnityEngine.Random.Dice.Enqueue(1);UnityEngine.Random.Dice.Enqueue(20);Run(g,g.RepeatLaughterSave(-1,g.laughter.Revision,true));C(g.Laughing(-1)&&UnityEngine.Random.Dice.Count==1,"damage advantage cancels existing disadvantage and uses a single die");UnityEngine.Random.Dice.Clear();
  UnityEngine.Random.D20=1;g=new TrainingGame();g.map.Monster=new Vector2Int(3,0);g.map.Units.Add(new Unit{Id=9,X=2,Y=0});g.InitializeLaughter();Run(g,g.CastLaughter(new[]{-1,9},0));UnityEngine.Random.D20=20;Run(g,g.RepeatLaughterSave(-1,g.laughter.Revision));C(g.Laughing(9)&&!g.Laughing(-1)&&g.IsConcentrating,"upcast multi-target success only removes the saving target");
  g.map.Scene=1;g.map.MonsterScene=1;C(g.Laughing(9),"attached effect does not expire from changing displayed scene");g.worldRounds=10;g.RefreshLaughter();C(!g.IsConcentrating&&g.laughter.Count==0,"minute expiry clears all targets");
  UnityEngine.Random.D20=1;g=Laugh(-1);long old=g.laughter.Revision;g.EndConcentration("recast");Run(g,g.CastLaughter(new[]{-1},0));UnityEngine.Random.D20=20;Run(g,g.RepeatLaughterSave(-1,old,true));C(g.Laughing(-1),"stale queued save cannot end replacement cast");
  UnityEngine.Random.D20=1;g=new TrainingGame();g.InitializeLaughter();g.IsWildShaped=true;g.map.MonsterContained=true;Run(g,g.CastLaughter(new[]{0},0));C(!g.IsConcentrating&&!g.IsWildShaped&&g.restores==1&&g.releases==1&&!g.map.HasContainedCreature,"self failure ends concentration and restores wild shape with joint passenger release");
  foreach(int first in new[]{0,2}){UnityEngine.Random.D20=1;g=Laugh(-1);g.combat.ApplyDamage(1);g.bondConcentration.Enqueue(1);UnityEngine.Random.D20=20;ResolveDamage(g,first);C(g.bondSaves==(first==0?0:1),"player-selected damage consequence order is mechanically meaningful");}
  UnityEngine.Random.D20=1;g=new TrainingGame();g.InitializeLaughter();g.map.Monster=new Vector2Int(7,0);g.laughterTargets.Clear();g.laughterTargets.Add(-1);C(g.LaughterError(1)!="","range validated before action consumption");g.map.Monster=new Vector2Int(3,0);g.visible=false;C(g.LaughterError(1)!="","visibility required");g.visible=true;g.laughterTargets.Add(0);C(g.LaughterError(1)!=""&&g.LaughterError(2)=="","higher slots allow one extra distinct target each");
  Console.WriteLine("PASS: production laughter initial/repeat saves, damage advantage, temp HP, zero damage, independent targets/prone, expiry/recast, self incapacity and simultaneous consequence order");
 }
}
class LaughterChecksMain {static void Main()=>TrainingGame.CheckLaughter();}
