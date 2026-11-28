using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class CreatureProfile{public static int SpacesForSize(int n)=>n<3?1:n==3?2:n==4?3:4;}
public sealed partial class TrainingGame
{
 sealed partial class MapStub{public bool MovementIsJump;public bool HasContainedCreature=>Contained||MonsterContained;public void SetPlayerSpeed(int n){}public bool RestoreTrueFormFootprint(bool player,int spaces,int size){if(player)PlayerSpaces=spaces;else MonsterSpaces=spaces;return true;}}
 sealed class Turns{public bool Active;}readonly Turns summonTurns=new Turns();
 readonly VisionState vision=new VisionState();readonly SizeChangeEffect sizeChange=new SizeChangeEffect();
 sealed class Profile{public int SizeIndex=2;}readonly Profile monsterProfile=new Profile();
 sealed class Poly{public bool Active,TargetPlayer;public void Clear(){Active=false;}}readonly Poly polymorph=new Poly();
 sealed class Wild{}Wild wildForm;Vector2Int wildNormalOffset;bool IsWildShaped=>wildForm!=null;int NaturalPlayerSize=2,NaturalPlayerSpeed=30;
 bool PlayerPolymorphed=>polymorph.Active&&polymorph.TargetPlayer;bool MonsterPolymorphed=>polymorph.Active&&!polymorph.TargetPlayer;
 sealed class Fighter{public void CancelAttackAction(){}}readonly Fighter fighter=new Fighter();object actionWeapon;
 int wildEnds,polyEnds,releases;bool releaseRestoredHost;
 void EndWildShape(string reason){wildForm=null;map.PlayerSpaces=1;wildEnds++;}
 void EndPolymorph(){bool player=polymorph.TargetPlayer;polymorph.Clear();if(player)map.PlayerSpaces=IsWildShaped?2:1;else map.MonsterSpaces=1;polyEnds++;}
 IEnumerator ReleaseSwallowedTarget(string reason,int spaces=0,int size=-1){map.Contained=map.MonsterContained=false;releaseRestoredHost=spaces>0;map.PlayerSpaces=spaces;releases++;yield break;}
 void ObserveForcedSharedStop(int actor){}
 void SyncTimedBuffs(){}
 int ApplyMappedAreaDamage(int a,int c,int d,int t,string context,bool failed)=>ApplyMappedAreaDamage(a,c,d,t,context);
 void EndConcentration(string reason){EndMoonbeam();concentrationSpell="";}
 static TrainingGame Beam(int x=3,int slot=2){var g=new TrainingGame();g.map.Monster=new Vector2Int(x,0);Run(g,g.CastMoonbeam(new Vector2Int(x,0),0,slot));return g;}
 static void Move(TrainingGame g,int x,int y=0){g.visionColumn=(x+1).ToString();g.visionRow=(y+1).ToString();Run(g,g.MoveMoonbeam(new Vector2Int(x,y)));}
 public static void CheckMoonbeam()
 {
  var g=Beam();C(g.combat.Hp==980&&g.damageCounts.Count==1&&g.damageCounts[0]==2&&g.moonbursts.Count==0,"creation Constitution save and 2d10 damage clean shared group");g.QueueMoonbeamEnd(-1);C(g.startStatuses.Count==0,"creation counts toward once-turn allowance");g.terrainTurn++;g.QueueMoonbeamEnd(-1);Run(g,g.DrainGreaseEntries());C(g.combat.Hp==960,"later global turn end triggers again");
  g=new TrainingGame();g.map.Monster=new Vector2Int(4,0);g.map.Units.Add(new Unit{Id=9,X=3,Y=0});Run(g,g.CastMoonbeam(new Vector2Int(3,0),0,3));C(g.damageActors.Count==2&&g.damageCounts.Count==1&&g.damageCounts[0]==3,"simultaneous creation targets share one upcast damage roll");
  g=new TrainingGame();g.map.Player=new Vector2Int(3,0);g.map.Monster=new Vector2Int(4,0);Run(g,g.CastMoonbeam(new Vector2Int(3,0),0,2));C(g.combat.PlayerDead&&g.combat.Hp==980&&!g.moonbeam.Active(0)&&g.moonbursts.Count==0&&g.outcomeDeferDepth==0,"caster death ends field but cannot cancel remaining targets of an already-started simultaneous burst");
  UnityEngine.Random.D20=20;g=Beam();C(g.combat.Hp==990,"successful save halves shared damage");UnityEngine.Random.D20=1;
  g=Beam(4);C(g.MoonbeamMoveError()!="","cannot move on casting turn");g.terrainTurn++;Move(g,5);C(g.combat.Hp==980&&!g.combat.MagicActionAvailable&&g.moonbeam.X==5&&g.moonbeam.Expires==10,"user option one: moving while continuously overlapping does not trigger, spends action, keeps lifetime");g.QueueMoonbeamEnd(-1);Run(g,g.DrainGreaseEntries());C(g.combat.Hp==960,"continuously covered target still triggers at own end");
  g=new TrainingGame();g.map.Monster=new Vector2Int(4,0);Run(g,g.CastMoonbeam(new Vector2Int(2,0),0,2));g.terrainTurn++;Move(g,8);C(g.combat.Hp==980&&g.moonbeam.X==8,"swept path triggers newly covered target even when final beam no longer covers it");
  g=Beam();g.terrainTurn++;g.map.Monster=new Vector2Int(6,0);g.ObserveMoonbeamMovement(-1,new Vector2Int(3,0),g.map.Monster,1,1,false);g.map.Monster=new Vector2Int(4,0);g.ObserveMoonbeamMovement(-1,new Vector2Int(6,0),g.map.Monster,1,1,false);Run(g,g.DrainGreaseEntries());C(g.combat.Hp==960,"normal or forced entry triggers once");g.QueueMoonbeamEnd(-1);C(g.startStatuses.Count==0,"no duplicate end save after entry");
  foreach(bool jump in new[]{false,true}){g=Beam();g.terrainTurn++;g.map.Monster=new Vector2Int(6,1);g.map.MovementIsJump=jump;g.ObserveMoonbeamMovement(-1,new Vector2Int(0,1),g.map.Monster,1,1,true);Run(g,g.DrainGreaseEntries());C(g.combat.Hp==(jump?960:980),"jump crosses cylinder; teleport checks landing only");}
  g=new TrainingGame();g.map.Player=new Vector2Int(3,0);g.map.Monster=new Vector2Int(9,0);g.map.PlayerSpaces=2;g.wildForm=new Wild();g.polymorph.Active=g.polymorph.TargetPlayer=true;g.combat.GrantPlayerTemporaryHp(100);Run(g,g.CastMoonbeam(new Vector2Int(3,0),0,2));C(!g.IsWildShaped&&!g.PlayerPolymorphed&&g.map.PlayerSpaces==1&&g.MoonbeamShapeBlocked(0),"failed save restores all nested forms and bars transformation");g.map.Player=new Vector2Int(7,0);g.ObserveMoonbeamMovement(0,new Vector2Int(3,0),g.map.Player,1,1,false);C(!g.MoonbeamShapeBlocked(0),"leaving cylinder ends transformation bar");
  g=new TrainingGame();g.map.Player=new Vector2Int(3,0);g.map.PlayerSpaces=2;g.map.MonsterContained=true;g.wildForm=new Wild();g.combat.GrantPlayerTemporaryHp(100);Run(g,g.CastMoonbeam(new Vector2Int(3,0),0,2));C(g.releases==1&&!g.map.HasContainedCreature&&!g.IsWildShaped&&g.map.PlayerSpaces==1,"joint true-form/release placement finishes without intermediate beast body");
  g=Beam();C(!g.MoonbeamShapeBlocked(-1),"normal form is not barred merely by failing damage save");g.vision.BaseLight=2;C(g.vision.Light(15,0,0)==1&&g.vision.Light(45,0,0)==2,"five-foot dim illumination only");g.vision.AddZone(true,15,0,2,0);g.RefreshMoonbeam();C(!g.moonbeam.Active(0),"Darkness dispels second-level dim-light spell");g=Beam(3,3);g.vision.AddZone(true,15,0,2,0);g.RefreshMoonbeam();C(g.moonbeam.Active(0)&&g.vision.HasDarkness(0),"higher-slot beam illuminates but does not dispel Darkness");
  g=Beam();g.worldRounds=10;g.RefreshMoonbeam();C(!g.moonbeam.Active(10)&&g.vision.Zones.Count==0,"one-minute expiry removes light");g=Beam();g.terrainTurn++;g.QueueMoonbeamEnd(-1);g.EndMoonbeam();Run(g,g.DrainGreaseEntries());C(g.combat.Hp==960&&g.startStatuses.Count==0&&g.moonbursts.Count==0&&g.outcomeDeferDepth==0,"ending concentration stops future triggers but preserves already-triggered damage snapshots");g=Beam();g.terrainTurn++;g.QueueMoonbeamEnd(-1);g.EndMoonbeam(true);C(g.startStatuses.Count==0&&g.moonbursts.Count==0&&g.outcomeDeferDepth==0,"battle reset cancels all queued snapshots and balances outcome guards");
  g=Beam();g.terrainTurn++;g.QueueMoonbeamEnd(-1);g.combat.ApplyDamage(980);int checks=g.outcomeChecks;Run(g,g.DrainGreaseEntries());C(g.outcomeDeferDepth==0&&g.outcomeChecks>checks,"invalidated target releases group outcome guard and checks battle end");
  g=Beam(20);g.terrainTurn++;g.visionColumn="30";g.visionRow="1";g.visible=false;C(g.MoonbeamMoveError()=="","later move has no 120-foot caster leash or sight requirement");
  Console.WriteLine("PASS: production Moonbeam creation, shared/upcast damage, CON/half, end/entry/swept triggers, user continuous-overlap rule, atomic jump/teleport, nested forms, release, light, Darkness and lifetime");
 }
}
class MoonbeamChecksMain{static void Main()=>TrainingGame.CheckMoonbeam();}
