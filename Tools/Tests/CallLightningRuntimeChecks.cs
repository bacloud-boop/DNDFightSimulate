using System;
using System.Collections;
using UnityEngine;
public sealed partial class TrainingGame
{
 sealed class LightningTurns {public bool Active;}
 readonly LightningTurns summonTurns=new LightningTurns();
 static void LRun(TrainingGame g,IEnumerator r){Run(g,r);}
 static TrainingGame Lightning(int slot=3,bool storm=false){var g=new TrainingGame();g.outdoorStorm=storm;g.visionColumn="8";g.visionRow="1";LRun(g,g.CastCallLightning(new Vector2Int(0,0),new Vector2Int(7,0),0,slot));return g;}
 public static void CheckLightning()
 {
  var g=Lightning();C(g.concentrationSpell=="召雷术"&&g.callLightning.Dice==3&&g.concentrationExpiresWorld==100&&g.combat.Hp==970,"initial cast creates ten-minute cloud and immediately deals three d10");
  g=Lightning(5,true);C(g.callLightning.Dice==6&&g.combat.Hp==940,"upcasting plus storm adds exactly one d10");g.outdoorStorm=false;C(g.callLightning.Dice==6,"storm bonus is captured at casting");
  int original=g.callLightning.Dice;long expiry=g.callLightning.Expires;bool bonus=g.combat.BonusActionAvailable;LRun(g,g.RepeatCallLightning());C(g.combat.Hp==880&&g.callLightning.Dice==original&&g.callLightning.Expires==expiry&&g.combat.BonusActionAvailable==bonus,"repeat consumes only a magic action, preserves spell dice, concentration and duration");C(!g.combat.MagicActionAvailable&&g.CallLightningRepeatError()!="","cannot repeat without another magic action");
  g.combat.EndRound(false);g.map.Player=new Vector2Int(25,0);C(g.CallLightningRepeatError()=="","control has no ongoing caster-to-cloud distance leash");LRun(g,g.RepeatCallLightning());C(g.combat.Hp==820,"cloud remains fixed as caster moves away");
  g=Lightning();g.combat.EndRound(false);g.visionColumn="14";C(g.CallLightningRepeatError()!="","strike outside sixty-foot cloud radius rejected without spending action");C(g.combat.MagicActionAvailable,"invalid strike does not spend action");g.visionColumn="13";C(g.CallLightningRepeatError()=="","exact sixty-foot boundary allowed");
  g.visible=false;C(g.CallLightningRepeatError()!="","caster must see strike point");g.visible=true;g.map.PlayerScene=1;C(g.CallLightningRepeatError()!="","cannot control cloud across planes");g.map.PlayerScene=0;g.map.Contained=true;C(g.CallLightningRepeatError()!="","swallowed caster cannot control external cloud");g.map.Contained=false;
  g=Lightning();g.combat.EndRound(false);g.terrainActor=0;g.visionColumn="8";g.map.Monster=new Vector2Int(7,0);g.map.Units.Add(new Unit{Id=8,X=8,Y=0});g.map.Units.Add(new Unit{Id=9,X=8,Y=1});g.damageActors.Clear();LRun(g,g.RepeatCallLightning());C(g.damageActors.Contains(-1)&&g.damageActors.Contains(8)&&!g.damageActors.Contains(9),"five-foot strike radius includes allies, excludes seven-foot diagonals");
  g=new TrainingGame();g.map.Monster=new Vector2Int(1,0);LRun(g,g.CastCallLightning(new Vector2Int(0,0),new Vector2Int(1,0),0,3));C(g.damageActors.Contains(0)&&g.damageActors.Contains(-1)&&g.combat.Hp==970,"self damage cannot prematurely skip other blast targets");
  g=new TrainingGame();g.combat.DamageResistance=(p,t)=>t==7;UnityEngine.Random.D20=20;LRun(g,g.CastCallLightning(new Vector2Int(0,0),new Vector2Int(7,0),0,3));C(g.combat.Hp==993,"successful save halves before lightning resistance");UnityEngine.Random.D20=1;
  g=Lightning();g.worldRounds=100;C(!g.callLightning.Active(100)&&g.CallLightningRepeatError()!="","expiry prevents repeats");g.callLightning.Clear();C(!g.callLightning.Active(0),"concentration cleanup clears cloud");
  g=new TrainingGame();g.cloudColumn=int.MaxValue.ToString();Vector2Int p;C(!g.CloudPoint(out p),"overflow-sized cloud coordinate rejected");
  C(!new CallLightningEffect().Begin(0,0,0,2,false,12,0)&&!new CallLightningEffect().Begin(0,0,0,3,false,12,long.MaxValue-99),"invalid slot and overflowing duration rejected");
  Console.WriteLine("PASS: production Call Lightning initial/repeated magic actions, fixed cloud, storm snapshot, scaling, radius/visibility/planes, ally and self damage, save/resistance ordering and expiry");
 }
}
class LightningChecksMain {static void Main()=>TrainingGame.CheckLightning();}
