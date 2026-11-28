using System;
using UnityEngine;
public sealed partial class TrainingGame
{
 sealed class EmptyTerrain {public bool Cell(int x,int y,long now,int scene=0)=>false;public bool FullFootprint(int x,int y,int n,long now)=>false;}
 readonly EmptyTerrain entangle=new EmptyTerrain(),grease=new EmptyTerrain(),spikeGrowth=new EmptyTerrain(),web=new EmptyTerrain(),iceStorm=new EmptyTerrain();
 void SyncTimedBuffs(){}void EndConcentration(string reason){concentrationSpell="";EndSleetStorm();}
 static TrainingGame Sleet(){var g=new TrainingGame();g.map.Player=new Vector2Int(0,10);g.CastSleetStorm(new Vector2Int(3,0),0);return g;}
 public static void CheckSleet()
 {
  var g=Sleet();C(g.sleetStorm.Active(0)&&g.concentrationSpell=="雪雨暴"&&g.concentrationExpiresWorld==10&&g.startStatuses.Count==0,"one-minute concentration, no save merely on creation");
  C(g.FullDifficultTerrain(3,0,2),"complete footprint on affected land is difficult");g.map.Water.Add(new Vector2Int(4,0));C(!g.FullDifficultTerrain(3,0,2),"mixed land/water footprint is not wholly difficult");C(!g.SleetLandCell(4,0),"water has no ground movement surcharge");
  g.map.Water.Add(g.map.Monster);g.QueueSleetStart(-1);Run(g,g.DrainGreaseEntries());C(g.combat.MonsterProne,"water occupants still make save and fall prone on failure");C(g.concentrationSpell=="雪雨暴","target failing does not end another actor's concentration");g.combat.MonsterProne=false;g.QueueSleetStart(-1);C(g.startStatuses.Count==0,"duplicate own-turn starts do not repeat save");
  g.ObserveSleetMovement(-1,new Vector2Int(9,0),g.map.Monster,1,1,false);C(g.startStatuses.Count==1,"first reentry after a start-of-turn save is a distinct trigger");Run(g,g.DrainGreaseEntries());g.ObserveSleetMovement(-1,new Vector2Int(9,0),g.map.Monster,1,1,false);C(g.startStatuses.Count==0,"later entries in same turn do not repeat save");
  g.terrainTurn++;g.QueueSleetStart(-1);UnityEngine.Random.D20=20;Run(g,g.DrainGreaseEntries());g.combat.MonsterProne=false;C(g.startStatuses.Count==0,"successful save completes without persistent queued work");UnityEngine.Random.D20=1;
  g=Sleet();g.map.Player=new Vector2Int(3,0);g.QueueSleetStart(0);g.QueueSleetStart(-1);Run(g,g.DrainGreaseEntries());C(g.combat.PlayerProne&&g.concentrationSpell==""&&!g.sleetStorm.Active(0)&&g.startStatuses.Count==0,"caster failing immediately drops concentration and clears remaining field triggers");
  g=Sleet();C(g.SleetVision(0,0,40,0,0)==2,"line crossing storm is heavily obscured");C(g.SleetVision(0,30,40,30,0)==0,"line outside storm stays clear");C(g.SleetVision(0,0,5,0,10)==0,"blindsight inside its range bypasses obscurement");C(g.SleetVision(0,0,40,0,10)==2,"blindsight outside its range cannot bypass storm");
  g.map.PlayerScene=1;C(g.SleetVision(0,0,40,0,0)==0,"other planes do not share heavy obscurement");g.map.PlayerScene=0;g.map.Contained=true;g.QueueSleetStart(0);C(g.startStatuses.Count==0,"swallowed target isolated from external storm");
  g=Sleet();C(g.SleetPathNeeded(new Vector2Int(0,0),new Vector2Int(9,0),1),"movement crossing area requests stepped traversal");g.worldRounds=10;C(!g.sleetStorm.Active(10)&&!g.FullDifficultTerrain(3,0,1)&&g.SleetVision(0,0,40,0,0)==0,"expiry removes saves, difficult ground and sight blocking");
  g=Sleet();g.QueueSleetStart(-1);long old=g.sleetStorm.Revision;g.CastSleetStorm(new Vector2Int(4,0),0);Run(g,g.ResolveSleetSave(-1,old));C(!g.combat.MonsterProne,"stale entries cannot affect a replacement spell");g.EndSleetStorm();C(g.startStatuses.Count==0,"reset removes queued spell saves");
  C(!new SleetStormEffect().Begin(0,0,0,10,long.MaxValue-9),"overflowing duration rejected");
  Console.WriteLine("PASS: production Sleet Storm land-only full-footprint terrain, water saves, distinct start/entry triggers, prone and concentration loss, exact sight blocking/blindsight, planes and cleanup");
 }
}
class SleetChecksMain {static void Main()=>TrainingGame.CheckSleet();}
