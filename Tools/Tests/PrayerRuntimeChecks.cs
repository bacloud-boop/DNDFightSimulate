using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
 sealed partial class Classes {public int HitDie(int index)=>8;}
 sealed partial class Unit {public string Name="单位";public int Hp=10,MaxHp=20;public int Heal(int n){int before=Hp;Hp=Math.Min(MaxHp,Hp+n);return Hp-before;}}
 sealed partial class Rescue {public readonly Features SpeciesAbilities=new Features();public int Heal(int n){int before=Hp;Hp=Math.Min(10,Hp+n);return Hp-before;}public void ShortRest(){}}
 sealed partial class MapStub {
  public int RiderMountId=-1;public Vector2Int? RescuePosition;public List<Unit> Summons=>Units;
  public bool SceneBody(int actor,out Vector2Int p,out int n){p=Player;n=PlayerSpaces;if(actor==0)return true;if(actor==-1){p=Monster;n=MonsterSpaces;return true;}if(actor==BattleMap.RescueId){p=RescuePosition??Vector2Int.zero;n=1;return RescuePosition.HasValue;}var u=Units.Find(x=>x.Id==actor);if(u==null)return false;p=new Vector2Int(u.X,u.Y);n=u.Spaces;return true;}
 }
 sealed class Features {public int Rests;public void ShortRest(){Rests++;}public void LongRest(){}}
 readonly Features fighter=new Features(),druid=new Features(),playerSpeciesAbilities=new Features(),monsterSpeciesAbilities=new Features(),steedRules=new Features();
 sealed class Spirit {public bool Active;public Unit Unit;}readonly Spirit bestialSpirit=new Spirit();Unit steed;
 readonly int[] hitDiceSpent=new int[PlayerClasses.Names.Length];bool shortRestReady,PlayerCanSustainCast=true,aftermathAdvanceCompleted;
 object label=new object();bool OutsideCombat=>!rolling;bool RescueTurnAllowed=true;
 int EffectivePlayerAbility(int n)=>attributes[n];int EffectiveMonsterAbility(int n)=>10;int SummonAbility(Unit u,int n)=>10;
 object Undead(Unit u)=>u!=null&&u.Id==99?u:null;
 bool SpellVoiceAllowed(bool p,string name)=>true;bool ArmorSpellAllowed(string name)=>true;void PlayerCastWhileSubmerged(string name){}
 sealed class SpellInterruption {public bool Cancelled;}bool interruptSpell;
 IEnumerator TryInterruptSpell(bool p,string name,SpellInterruption result){result.Cancelled=interruptSpell;yield break;}
 void EndConcentration(string reason){prayerChannel=null;concentrationSpell="";}void RefreshWildShape(){}void ObserveDeaths(){}
 int HealRegenerationTarget(int actor,int amount)=>actor==0?combat.HealPlayer(amount):actor==-1?combat.HealMonster(amount):actor==BattleMap.RescueId?rescueCarrier.Heal(amount):FindSummon(actor)?.Heal(amount)??0;
 int RollHealingAmount(int actor,int n,long maximum)=>n;void ApplySteedLifeBond(int n,int level){}
 IEnumerator AdvanceAftermathRounds(long n,Func<bool> valid=null){aftermathAdvanceCompleted=false;for(int i=0;i<n;i++){if(valid!=null&&!valid())yield break;EndPrayerCastingTurn();worldRounds++;combat.EndRound();yield return FinishPendingPrayer();}aftermathAdvanceCompleted=true;}
 static TrainingGame StartPrayer(int slot=2,params int[] targets){var g=new TrainingGame();g.map.Monster=new Vector2Int(3,0);g.combat.TryUseMagicAction();g.BeginPrayer(targets.Length==0?new[]{0}:targets,slot);return g;}
 static void PrayerRun(TrainingGame g,IEnumerator routine,bool healFirst=true,params int[] choices)
 {
  var selected=new Queue<int>(choices);var stack=new Stack<IEnumerator>();stack.Push(routine);int ticks=0;
  while(stack.Count>0){C(++ticks<10000,"prayer coroutine completed");var r=stack.Peek();if(!r.MoveNext()){stack.Pop();continue;}if(g.prayerChoicePending)g.prayerChoice=g.prayerChoosingOrder?(healFirst?0:1):selected.Count>0?selected.Dequeue():int.MaxValue;if(r.Current is IEnumerator)stack.Push((IEnumerator)r.Current);}
 }
 static void FinishPrayer(TrainingGame g,bool first=true,params int[] choices)
 {
  for(int i=1;i<100;i++){g.EndPrayerCastingTurn();g.worldRounds++;g.combat.EndRound();PrayerRun(g,g.ContinuePrayer());}
  g.EndPrayerCastingTurn();g.worldRounds++;PrayerRun(g,g.FinishPendingPrayer(),first,choices);
 }
 public static void CheckPrayer()
 {
  var clock=new PrayerCasting(new[]{0,-1},3,1,0);C(clock.Finish==100&&clock.Actions==1&&!clock.Ready(100),"ten-minute clock cannot complete on elapsed time alone");for(int i=2;i<=100;i++)C(clock.Continue(i,i-1),"distinct consecutive action payments");C(clock.Ready(100)&&!clock.Continue(101,100),"exactly 100 paid actions");clock.Exclude(-1);C(clock.Count==1,"range exclusions are independent");
  var pool=new HitDicePool();C(pool.Count==1&&pool.Sides==8&&!pool.Spend(),"default 1d8 requires short-rest benefit");pool.ShortRest();C(pool.Spend()&&!pool.Spend()&&pool.Remaining==0,"spend each die once");pool.LongRest();C(pool.Remaining==1&&!pool.RestReady,"long rest restores all dice without granting extra short-rest use");C(!pool.Configure(-1,8)&&!pool.Configure(2,7)&&pool.Configure(0,20),"integer pool validation and optional zero dice");C(HitDicePool.Healing(1,-4)==1,"2024 hit-die healing minimum one");
  var g=StartPrayer(3,0,-1);g.combat.ApplyPlayerDamage(7);g.combat.ApplyDamage(30);FinishPrayer(g);C(g.combat.PlayerHp==10&&g.combat.Hp==994&&g.prayerUsed.Contains(0)&&g.prayerUsed.Contains(-1)&&g.fighter.Rests==1&&g.monsterSpeciesAbilities.Rests==1&&g.shortRestReady,"shared 3d8 healing, no spell modifier, and short-rest resources");C(g.prayerChannel==null&&g.concentrationSpell=="","completed instantaneous effect leaves no concentration");g.prayerTargets.Clear();g.prayerTargets.Add(-1);C(g.PrayerError(2)!="","per-recipient long-rest lockout");g.FinishActorLongRest(-1);C(g.PrayerError(2)==""&&!g.prayerUsed.Contains(-1)&&g.prayerUsed.Contains(0),"only that target's long rest clears lockout");
  g=StartPrayer(2,-1);g.combat.ApplyDamage(100);FinishPrayer(g,true,0);C(g.combat.Hp==924&&g.monsterHitDice.Spent==1,"optional target HD spends one separate d8 and does not change maximum HP");g=StartPrayer(2,-1);g.combat.ApplyDamage(100);FinishPrayer(g,false,0);C(g.combat.Hp==924,"both simultaneous-effect orders implemented");
  g=StartPrayer(2,0);g.combat.ApplyPlayerDamage(9);FinishPrayer(g,true,2);C(g.hitDiceSpent[2]==1&&g.combat.PlayerHp==10,"player selects class hit die rather than free healing");
  g=StartPrayer(2,-1);g.map.Monster=new Vector2Int(8,0);g.ObservePrayerMovement(-1,new Vector2Int(3,0),g.map.Monster,1,false);g.map.Monster=new Vector2Int(3,0);g.RefreshPrayerTargets();C(g.prayerChannel.Count==0,"leaving range then returning never restores eligibility");
  g=StartPrayer(2,-1);g.map.Player=new Vector2Int(10,0);g.ObservePrayerMovement(0,new Vector2Int(0,0),g.map.Player,1,false);C(g.prayerChannel.Count==0,"caster's path can invalidate targets even after later return");
  g=new TrainingGame();g.map.Units.Add(new Unit{Id=7,X=0,Y=0});g.map.RiderMountId=7;g.BeginPrayer(new[]{0,7},2);g.map.Player=new Vector2Int(10,0);g.map.Units[0].X=10;g.ObservePrayerMovement(0,Vector2Int.zero,g.map.Player,1,false);g.ObservePrayerMovement(7,Vector2Int.zero,g.map.Player,1,false);C(g.prayerChannel.Count==2,"mounted caster and mount remain in range along both movement callbacks");
  g=StartPrayer(2,0);g.EndPrayerCastingTurn();g.worldRounds++;g.combat.EndRound();g.EndPrayerCastingTurn();C(g.prayerChannel==null,"skipping required Magic action fails whole casting");g=StartPrayer();g.interruptSpell=true;g.worldRounds++;g.combat.EndRound();PrayerRun(g,g.ContinuePrayer());C(g.prayerChannel==null,"counterspell cancels sustained casting");
  g=StartPrayer(2,-1);g.map.MonsterScene=1;g.RefreshPrayerTargets();C(g.prayerChannel.Count==0,"scene departure excluded");g=StartPrayer();g.PlayerCanSustainCast=false;g.EndPrayerCastingTurn();C(g.prayerChannel==null,"incapacity or illegal casting form cancels casting");
  g=StartPrayer(2,-1);g.combat.Aftermath=true;g.combat.ApplyDamage(100);PrayerRun(g,g.AutoContinuePrayer());C(g.worldRounds==100&&g.prayerChannel==null&&g.combat.Hp==916,"aftermath auto mode still pays every action and advances 100 discrete rounds");
  g=new TrainingGame();g.map.Units.Add(new Unit{Id=99});C(g.ActorHitDice(99).Count==2&&g.ActorHitDice(99).Sides==8,"official skeleton/zombie pool prefill");g.map.Units.Add(new Unit{Id=9});C(g.ActorHitDice(9).Count==1&&g.ActorHitDice(9).Sides==8,"unknown creature default 1d8");g.GrantShortRest(9);C(g.ActorHitDice(9).Spend(),"summon short-rest pool usable");g.prayerUsed.Add(9);g.FinishActorLongRest(9);C(g.ActorHitDice(9).Spent==0&&!g.prayerUsed.Contains(9),"summon long rest clears both resource limits");
  g=new TrainingGame();g.monsterHitDice.Configure(4,12);g.prayerUsed.Add(-1);g.ResetNonPlayerHitDice();C(g.monsterHitDice.Count==4&&g.monsterHitDice.Sides==12&&!g.prayerUsed.Contains(-1),"battle reset retains configured pools and clears expenditures");
  Console.WriteLine("PASS: production Prayer of Healing 100-action casting, individual range continuity, interruption/counterspell, shared upcast healing, per-target rest resources/lockouts, optional class/NPC HD, order choices, automatic aftermath and official/default pools");
 }
}
class PrayerChecksMain {static void Main()=>TrainingGame.CheckPrayer();}
