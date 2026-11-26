using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
class ChromaticOrbChecksMain {public static void Main()=>TrainingGame.CheckChromaticOrb();}
public sealed partial class TrainingGame
{
 readonly Queue<int> orbDecisions=new Queue<int>();int orbPrompts;
 static void RunOrb(TrainingGame g,IEnumerator routine)
 {
  var stack=new Stack<IEnumerator>();stack.Push(routine);int limit=0;
  while(stack.Count>0){if(++limit>10000)throw new Exception("stuck production orb coroutine");var top=stack.Peek();if(!top.MoveNext()){stack.Pop();continue;}if(top.Current is IEnumerator){stack.Push((IEnumerator)top.Current);continue;}
   if(g.orbBouncePending){g.orbPrompts++;g.orbToEnvironment=g.orbDecisions.Count==0;if(!g.orbToEnvironment){g.orbBounceTarget=g.orbDecisions.Dequeue();C(g.orbBounceOptions.Contains(g.orbBounceTarget),"a selected bounce must be an actual offered legal target");}g.orbBounceReady=true;}
  }
 }
 static TrainingGame OrbGame(){UnityEngine.Random.D20=10;UnityEngine.Random.D8.Clear();UnityEngine.Random.D20Queue.Clear();return new TrainingGame();}
 static void Rolls(params int[] values){foreach(int n in values)UnityEngine.Random.D8.Enqueue(n);}
 public static void CheckChromaticOrb()
 {
  C(ChromaticOrbRules.Dice(1)==3&&ChromaticOrbRules.Dice(9,true)==22&&ChromaticOrbRules.Dice(0)==0,"slot scaling and critical bounds");
  C(!ChromaticOrbRules.Duplicates(null)&&!ChromaticOrbRules.Duplicates(new[]{1,2,3})&&ChromaticOrbRules.Duplicates(new[]{1,2,1}),"duplicate base d8 detection");C(ChromaticOrbRules.CanLeap(1,0,new[]{8,8,8})&&!ChromaticOrbRules.CanLeap(1,1,new[]{8,8,8}),"one first-slot leap, not just one total target");
  var g=OrbGame();RunOrb(g,g.ResolveChromaticOrb(1,1,-1,3));C(g.damage==24&&g.orbPrompts==1&&g.hitTargets.SequenceEqual(new[]{-1})&&!g.orbBouncePending&&g.outcomeDeferDepth==0&&g.outcomes==1,"environment exit causes no new attack/damage and closes modal/outcome scope");
  g=OrbGame();Rolls(1,2,3);RunOrb(g,g.ResolveChromaticOrb(1,1,-1,5));C(g.damage==6&&g.orbPrompts==0&&g.lastRaw[5]==6,"unique rolls do not bounce");
  g=OrbGame();g.map.SceneSummons.Add(new Unit{Id=7,X=6,Y=0});g.orbDecisions.Enqueue(7);RunOrb(g,g.ResolveChromaticOrb(1,1,-1,4));C(g.damage==48&&g.hitTargets.SequenceEqual(new[]{-1,7})&&g.orbPrompts==1,"first slot permits two hits but no third choice");C(g.map.BounceCoverCalls==1&&g.map.LastCoverSource==-1&&g.map.LastPoint==new Vector2Int(5,0),"bounce cover uses snapshotted previous target body");C(g.combat.BonusActionAvailable&&g.combat.Round==1,"bounces cost no bonus or extra turn");
  g=OrbGame();for(int i=7;i<=9;i++){g.map.SceneSummons.Add(new Unit{Id=i,X=i-1,Y=0});g.orbDecisions.Enqueue(i);}RunOrb(g,g.ResolveChromaticOrb(1,3,-1,7));C(g.hitTargets.SequenceEqual(new[]{-1,7,8,9})&&g.damage==160&&g.orbPrompts==3&&g.hitTargets.Distinct().Count()==4,"third slot permits three consecutive leaps and never retargets a visited creature");
  g=OrbGame();UnityEngine.Random.D20=20;Rolls(1,2,3,4,5,1);RunOrb(g,g.ResolveChromaticOrb(1,1,-1,12));C(g.damage==16&&g.orbPrompts==1&&g.lastRaw[12]==16,"duplicates among critical dice trigger bounce, without adding a casting modifier");
  g=OrbGame();UnityEngine.Random.D20=1;Rolls(8);RunOrb(g,g.ResolveChromaticOrb(1,1,-1,9));C(g.damage==0&&g.orbPrompts==0&&UnityEngine.Random.D8.Count==1,"miss rolls no damage and stops chain");
  g=OrbGame();g.intercept=true;RunOrb(g,g.ResolveChromaticOrb(1,1,0,3));C(g.damage==0&&g.orbPrompts==0,"mirror interception creates no damage or bounce");
  g=OrbGame();g.map.SceneSummons.Add(new Unit{Id=7,X=6,Y=0});g.orbDecisions.Enqueue(7);g.killMonster=true;RunOrb(g,g.ResolveChromaticOrb(1,1,-1,3));C(g.hitTargets.SequenceEqual(new[]{-1,7})&&g.map.LastCoverSource==-1,"killing the previous target does not delete its impact origin or abort the instantaneous chain");
  g=OrbGame();g.zeroPacket=true;RunOrb(g,g.ResolveChromaticOrb(1,1,-1,5));C(g.damage==0&&g.orbPrompts==1,"resistance/immunity or damage reduction does not erase a duplicate roll's bounce trigger");
  g=OrbGame();g.killCaster=true;RunOrb(g,g.ResolveChromaticOrb(1,1,-1,3));C(g.orbPrompts==0&&g.hitTargets.Count==1&&g.outcomeDeferDepth==0,"caster incapacitation stops later attacks and unwinds outcome scope");
  g=OrbGame();g.map.Player=new Vector2Int(19,0);g.map.Monster=new Vector2Int(1,0);RunOrb(g,g.ResolveChromaticOrb(1,1,-1,3));C(g.hitTargets.Count==1&&g.orbPrompts==0,"no legal creature within thirty feet ends chain without forcing a modal");
  g=OrbGame();var visited=new HashSet<int>{-1};C(!g.OrbTargetAllowed(-1,g.map.Monster,1,30,visited),"visited targets are rejected before attack");g.map.MonsterScene=1;C(!g.OrbTargetAllowed(-1,g.map.Player,1,90),"cross-plane target rejected");g.map.MonsterScene=0;g.map.PlayerContained=true;C(!g.OrbTargetAllowed(7,g.map.Player,1,90),"swallowing isolation and missing body rejected");
  foreach(int type in ChromaticOrbRules.Types){g=OrbGame();Rolls(1,2,3);RunOrb(g,g.ResolveChromaticOrb(1,1,-1,type));C(g.lastRaw[type]==6&&g.lastRaw.Sum()==6,"all six energy choices route into their exact typed packet");}
  Console.WriteLine("PASS: production Chromatic Orb energy choice, d8 scaling/critical duplicates, miss/mirror, bounded distinct-target leaps, previous-body cover, death origins, immunity, caster incapacitation and environment exit");
 }
}
