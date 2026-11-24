using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace UnityEngine {
 public struct Rect {public Rect(float a,float b,float c,float d){}}
 public static class GUI {public static void Label(Rect r,string text,object style){}}
 public static class GUILayout {public static void Label(string text,object style){}public static bool Toggle(bool value,string text,object style)=>value;}
}
public sealed partial class TrainingGame
{
 sealed class ClassStub {public int ProficiencyBonus=2;}
 sealed class BestialStub {public bool Active=false;public SummonedUnit Unit;}
 readonly BattleMap map=new BattleMap();readonly CombatState combat=new CombatState();readonly ClassStub classes=new ClassStub();readonly BestialStub bestialSpirit=new BestialStub();readonly CreatureProfile monsterProfile=new CreatureProfile();
 readonly AcidArrowResidue acidArrows=new AcidArrowResidue();readonly WeaponInventory carriedWeapons=new WeaponInventory(Weapon.Available.Select(w=>w.Name),Weapon.Available[0].Name);readonly GroundWeaponState monsterGroundWeapon=new GroundWeaponState(-1);readonly RescueCarrier rescueCarrier=new RescueCarrier();
 readonly int[] attributes={10,10,10,10,10,10};readonly List<string> history=new List<string>();readonly HashSet<int> saves=new HashSet<int>();
 string concentrationSpell="";long worldRounds,concentrationExpiresWorld;int concentrationExpiresRound,directSlotLevel=4;object small,centered,label;
 int SpellAbilityIndex(int caster)=>4;bool CanSeeOwnBody()=>true;bool PlayerSeesMonster=>true;int VisibilityAt(bool p,Vector2Int at,int n)=>0;
 SummonedUnit FindSummon(int id)=>map.Summons.FirstOrDefault(u=>u.Id==id&&u.Alive);
 bool HealingHandsTarget(int id,out Vector2Int p,out int n,out string name){name=TransitActorName(id);return map.SceneBody(id,out p,out n)&&map.OnScene(id);}
 bool AcidArrowIsolated(int a,int b)=>AcidArrowResidue.CrossesBoundary(a,b,map.IsContained(true),map.IsContained(false));
 string TransitActorName(int id)=>id.ToString();List<int> BreathActors()=>new List<int>{0,-1};
 IEnumerator RollMappedAbilitySave(int actor,int source,long dc,HashSet<int> failed,string title,int ability){if(!saves.Contains(actor))failed.Add(actor);yield break;}
 void ObserveWaterBreathing(){foreach(int id in new[]{0,-1})if(map.SceneOf(id)>0)(id==0?playerBreathing:monsterBreathing).Observe(true,worldRounds,10);}
 readonly BreathingState playerBreathing=new BreathingState(),monsterBreathing=new BreathingState();
 void SyncTimedBuffs(){}void ObserveForcedSharedStop(int actor){}
 void RefreshConcentration(){if(combat.PlayerBanished){EndBanishment();concentrationSpell="";}}
 static void Run(IEnumerator routine){var stack=new Stack<IEnumerator>();stack.Push(routine);while(stack.Count>0){var top=stack.Peek();if(!top.MoveNext()){stack.Pop();continue;}if(top.Current is IEnumerator)stack.Push((IEnumerator)top.Current);}}
 static void C(bool value,string message){if(!value)throw new Exception(message);}
 static TrainingGame New(){var g=new TrainingGame();g.map.Lock();g.monsterProfile.TryApply(7,2);bool shared;g.map.RestoreSceneBody(-1,0,new Vector2Int(1,0),out shared);return g;}
 public static void Main()
 {
  var g=New();g.saves.Add(-1);Run(g.ResolveBanishment(1,4,new List<int>{-1}));C(!g.banishment.Concentrating&&g.map.SceneOf(-1)==0,"successful save never removes a target");
  g=New();Run(g.ResolveBanishment(1,4,new List<int>{-1}));C(g.combat.MonsterBanished&&g.combat.Hp==1000&&g.map.SceneOf(-1)>0&&g.concentrationExpiresWorld==10&&g.concentrationExpiresRound==int.MaxValue,"production cast snapshots one-minute concentration and incapacitates without killing");
  g.worldRounds=9;g.EndBanishment();C(!g.combat.MonsterBanished&&!g.combat.MonsterDeparted&&g.map.SceneOf(-1)==0,"early interruption returns even a fiend");
  g=New();Run(g.ResolveBanishment(1,4,new List<int>{0}));C(!g.combat.PlayerBanished&&!g.banishment.Concentrating&&g.map.SceneOf(0)==0,"self banishment immediately ends caster concentration and returns");
  g=New();C(g.map.BeginContainment(true),"setup swallowed caster");g.playerBreathing.Observe(false,0,10,true);Run(g.ResolveBanishment(1,4,new List<int>{-1}));
  C(g.map.Scene>0&&g.map.SameScene(0,-1)&&!g.combat.PlayerBanished&&g.combat.PlayerCanAct&&g.playerBreathing.CanBreathe,"unselected swallowed passenger follows host, stays able to act and can breathe");
  var plans=g.map.MandatoryReleasePlans();C(plans.Count>0&&g.map.ApplyMandatoryReleasePlan(plans[0]),"passenger can escape into the temporary scene");
  g.worldRounds=10;g.EndBanishment();C(g.combat.MonsterDeparted&&g.combat.BattleFinished&&g.combat.Hp==1000&&g.map.SceneOf(-1)<0&&g.map.SceneOf(0)==0&&!g.map.IsContained(true),"full-minute fiend departure wins without death and returns escaped companion");
  g.ResetBanishment();C(!g.combat.MonsterDeparted&&g.map.Scene==0&&g.map.SceneOf(-1)==0,"reset restores departed participants");
  g=New();g.monsterProfile.TryApply(9,2);Run(g.ResolveBanishment(1,4,new List<int>{-1}));g.worldRounds=10;g.EndBanishment();C(!g.combat.MonsterDeparted&&g.map.SceneOf(-1)==0,"humanoid always returns after full duration");
  Console.WriteLine("PASS: production Banishment coroutine, saves, early return, self-incapacitation, swallowed passenger, operable demiplane escape, full-minute non-death victory and reset");
 }
}
