using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
 sealed partial class MapStub {
  public bool MovementIsJump;public Vector2Int? RescuePosition;
  public bool IsCarried(bool p)=>false;
  public bool SceneBody(int actor,out Vector2Int p,out int n){p=actor==0?Player:Monster;n=actor==0?PlayerSpaces:MonsterSpaces;if(actor==0||actor==-1)return true;if(actor==BattleMap.RescueId){p=RescuePosition??Vector2Int.zero;n=1;return RescuePosition.HasValue;}var u=Units.Find(x=>x.Id==actor);if(u==null)return false;p=new Vector2Int(u.X,u.Y);n=u.Spaces;return true;}
 }
 object label=new object();bool HitDiceActorAlive(int actor)=>actor==0?!combat.PlayerDead:actor==-1?combat.Hp>0:actor==BattleMap.RescueId?!rescueCarrier.Dead:FindSummon(actor)?.Alive==true;
 int directSlotLevel=2;
 readonly List<int> cordonShownSides=new List<int>();readonly List<int[]> cordonShownDice=new List<int[]>();
 static TrainingGame ArrowGame(int count=4,int slot=2)
 {
  var g=new TrainingGame();g.map.Monster=new Vector2Int(3,0);var cells=new List<Vector2Int>();for(int i=0;i<count;i++)cells.Add(Vector2Int.zero);g.CastCordon(cells,slot,0,new HashSet<int>(),false);return g;
 }
 static void CordonRun(TrainingGame g,IEnumerator routine,int choice=0)
 {
  int ticks=0;var stack=new Stack<IEnumerator>();stack.Push(routine);while(stack.Count>0){C(++ticks<10000,"cordon coroutine terminates");var current=stack.Peek();if(!current.MoveNext()){stack.Pop();continue;}if(g.cordonChoicePending)g.cordonArrowChoice=choice;if(current.Current is IEnumerator)stack.Push((IEnumerator)current.Current);}
 }
 public static void CheckCordon()
 {
  UnityEngine.Random.D20=1;var g=ArrowGame();var f=g.cordon.Fields[0];C(f.Remaining==4&&f.Expires==4800&&g.startStatuses.Count==0&&g.combat.Hp==1000,"casting plants arrows but does not make an entry attack");
  g.map.Monster=new Vector2Int(4,0);g.ObserveCordonMovement(-1,new Vector2Int(3,0),g.map.Monster,1,1,false);C(g.startStatuses.Count==1,"movement into a new space already inside range triggers");CordonRun(g,g.DrainGreaseEntries());C(g.combat.Hp==992&&f.Remaining==3&&g.cordonShownSides.Contains(4),"DEX failure deals exactly 2d4 piercing and spends one arrow");
  g.map.Monster=new Vector2Int(5,0);g.ObserveCordonMovement(-1,new Vector2Int(4,0),g.map.Monster,1,1,false);C(g.startStatuses.Count==0,"further movement on same global turn does not repeat");g.QueueCordonEnd(-1);CordonRun(g,g.DrainGreaseEntries());C(g.combat.Hp==984&&f.Remaining==2,"end-turn trigger is separate from first movement");g.QueueCordonEnd(-1);C(g.startStatuses.Count==0,"duplicate end callbacks cannot consume more arrows");
  g.terrainTurn++;g.map.Monster=new Vector2Int(4,0);g.ObserveCordonMovement(-1,new Vector2Int(5,0),g.map.Monster,1,1,false);CordonRun(g,g.DrainGreaseEntries());C(f.Remaining==1,"movement allowance refreshes on each creature/global turn");g.QueueCordonEnd(-1);CordonRun(g,g.DrainGreaseEntries());C(f.Remaining==0&&!g.CordonEndAvailable(-1),"last arrow ends field without canceling its own shot");
  UnityEngine.Random.D20=20;g=ArrowGame();g.QueueCordonEnd(-1);CordonRun(g,g.DrainGreaseEntries());C(g.combat.Hp==1000&&g.cordon.Fields[0].Remaining==3&&!g.cordonShownSides.Contains(4),"successful save still spends arrow and deals zero without a damage roll");UnityEngine.Random.D20=1;
  g=ArrowGame();var older=g.cordon.Fields[0];var planted=new[]{new CordonArrowsEffect.Arrow(0,0)};var newer=g.cordon.Add(planted,2,20,0,0,new int[0],0);g.QueueCordonEnd(-1);CordonRun(g,g.DrainGreaseEntries());C(newer.Remaining==0&&older.Remaining==4,"highest DC alone fires and suppressed arrows remain");g.terrainTurn++;g.QueueCordonEnd(-1);CordonRun(g,g.DrainGreaseEntries());C(older.Remaining==3,"older still-active cast can fire after stronger is exhausted on later turn");
  var effect=new CordonArrowsEffect();var a=effect.Add(planted,2,10,0,0,new int[0],0);var b=effect.Add(planted,2,10,0,0,new int[0],0);C(effect.ClaimMovement(-1,3,0,1,0,1,0)==b,"same DC takes latest cast");C(a.Remaining==1&&b.Remaining==1&&!ReferenceEquals(a.Arrows[0],b.Arrows[0]),"claim does not spend and casts own independent projectiles");CordonArrowsEffect.Consume(b,0);C(effect.ClaimMovement(-1,4,0,1,0,1,0)==null,"suppressed cast retains first-entry history for the turn");
  g=ArrowGame();f=g.cordon.Fields[0];f.Ignored.Add(-1);g.QueueCordonEnd(-1);g.QueueCordonEnd(0);C(g.startStatuses.Count==0&&f.Remaining==4,"caster and nominated creatures ignored");
  g=ArrowGame(18,9);g.QueueCordonEnd(-1);CordonRun(g,g.DrainGreaseEntries());C(g.combat.Hp==992&&g.cordon.Fields[0].Remaining==17,"upcast increases capacity to18 but damage remains2d4");
  g=new TrainingGame();g.map.Monster=new Vector2Int(3,0);g.map.PlayerSpaces=2;g.CastCordon(new[]{new Vector2Int(0,0),new Vector2Int(1,0)},2,0,new HashSet<int>(),true);g.QueueCordonEnd(-1);CordonRun(g,g.DrainGreaseEntries(),1);C(g.cordon.Fields[0].Arrows[1].Spent&&!g.cordon.Fields[0].Arrows[0].Spent&&g.map.LastCover==new Vector2Int(1,0),"player chooses actual firing origin; DEX half cover uses that arrow");
  g=ArrowGame();g.concentrationSpell="other spell";g.combat.KillPlayer();g.QueueCordonEnd(-1);CordonRun(g,g.DrainGreaseEntries());C(g.combat.Hp==992&&g.cordon.Fields[0].Remaining==3,"field survives caster death and never depends on concentration");
  g=ArrowGame();g.worldRounds=4800;g.cordon.Refresh(g.worldRounds);g.QueueCordonEnd(-1);C(g.startStatuses.Count==0&&g.cordon.Fields.Count==0,"exact eight-hour expiry");g=ArrowGame();f=g.cordon.Fields[0];g.worldRounds=4799;g.QueueCordonEnd(-1);g.worldRounds=4800;g.cordon.Refresh(g.worldRounds);CordonRun(g,g.DrainGreaseEntries());C(g.combat.Hp==992&&f.Remaining==3,"already-triggered shot survives field expiry");
  g=ArrowGame();f=g.cordon.Fields[0];g.QueueCordonEnd(-1);g.combat.ApplyDamage(1000);CordonRun(g,g.DrainGreaseEntries());C(f.Remaining==3&&g.damageActors.Count==0,"committed arrow remains consumed when prior simultaneous effect kills target");
  foreach(bool jump in new[]{false,true}){g=ArrowGame();g.map.MovementIsJump=jump;g.map.Monster=new Vector2Int(7,0);g.ObserveCordonMovement(-1,new Vector2Int(-7,0),g.map.Monster,1,1,true);C(g.startStatuses.Count==(jump?1:0),"jump visits reachable airborne spaces; teleport evaluates landing only");}
  g=ArrowGame();g.map.MonsterScene=1;g.QueueCordonEnd(-1);C(g.startStatuses.Count==0,"arrows cannot target another scene");g=ArrowGame();g.map.MonsterAquatic.Submerged=true;g.QueueCordonEnd(-1);CordonRun(g,g.DrainGreaseEntries());C(g.combat.Hp==992,"spell save projectiles can affect submerged targets");
  g=new TrainingGame();g.cordonPlanting.Add(Vector2Int.zero);g.map.Water.Add(Vector2Int.zero);C(g.CordonError(2)!="","water planting forbidden by user rule");g.map.Water.Clear();C(g.CordonError(2)!="","must validate every arrow placement");g.cordonCount="1.5";C(g.CordonError(2)!="","decimal arrow counts rejected");g.cordonCount="1";C(g.CordonError(2)=="","one arrow is legal");g.cordonPlanting[0]=new Vector2Int(1,0);C(g.CordonError(2)!="","planting outside caster footprint forbidden");
  g=ArrowGame();g.QueueCordonEnd(-1);g.ResetCordon();C(g.startStatuses.Count==0&&g.cordonShots.Count==0&&g.cordon.Fields.Count==0,"battle reset cancels all fields and committed shots");
  C(CordonArrowsEffect.InRange(new CordonArrowsEffect.Arrow(8,0),1,0,2)&&!CordonArrowsEffect.InRange(new CordonArrowsEffect.Arrow(8,0),1,0,1),"multi-cell distance uses nearest occupied cell");
  Console.WriteLine("PASS: production Cordon planting, first-space movement and separate end triggers, DEX/cover, 2d4/upcast capacity, consumption, highest-DC/latest priority, independent casts, exclusions, death/expiry snapshots, jump/teleport, scenes/water and reset");
 }
}
class CordonChecksMain{static void Main()=>TrainingGame.CheckCordon();}
