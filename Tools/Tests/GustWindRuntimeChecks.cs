using System;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    sealed class BreathChoice{public int Edge,Offset=-1;public double AimX=-1,AimY;}
    sealed class Turns{public bool Active;}readonly Turns summonTurns=new Turns();
    readonly VisionState vision=new VisionState();readonly StinkingCloudEffect stinkingCloud=new StinkingCloudEffect();
    sealed partial class MapStub
    {
        public Func<int,Vector2Int,Vector2Int,int,bool> AgainstGust;public bool Riding;public Unit RiderMount;public int? RescueCargoId;public int PushSteps;public Vector2Int? Block;
        public bool IsCarried(bool p)=>false;public bool DismountRider(){Riding=false;return true;}
        public bool MovementBody(int id,out Vector2Int p,out int n){p=id==0?Player:Monster;n=id==0?PlayerSpaces:MonsterSpaces;if(id<=0)return true;var u=Units.Find(x=>x.Id==id);if(u==null)return false;p=new Vector2Int(u.X,u.Y);n=u.Spaces;return true;}
        public bool ForceWindStep(int id,Vector2Int p){if(!Inside(p,1)||Block.HasValue&&p==Block.Value)return false;PushSteps++;if(id==0)Player=p;else if(id==-1)Monster=p;else{var u=Units.Find(x=>x.Id==id);u.X=p.x;u.Y=p.y;}return true;}
    }
    bool ActorDiving(int id)=>id==0?map.PlayerAquatic.Submerged:id==-1?map.MonsterAquatic.Submerged:id==BattleMap.RescueId?map.RescueAquatic.Submerged:FindSummon(id).Aquatic.Submerged;
    bool BanishLiving(int id)=>id==0?!combat.PlayerDead:id==-1?combat.Hp>0:FindSummon(id)!=null&&FindSummon(id).Alive;
    void EndStinkingCloud(){stinkingCloud.EndCloud();startStatuses.RemoveAll(e=>e.Kind==4);}
    static TrainingGame Gust(){var g=new TrainingGame();g.InitializeGustWind();g.map.Monster=new Vector2Int(4,0);g.gustChoice.Edge=0;g.gustChoice.Offset=1;g.gustChoice.AimX=25;g.gustChoice.AimY=.5;return g;}
    void CastGust(){BreathTemplate line;PlayerGustTemplate(out line);Run(this,CastGustWind(line,gustChoice.Edge,gustChoice.Offset,0));}
    public static void CheckGust()
    {
        var g=Gust();g.CastGust();C(g.map.Monster==new Vector2Int(7,0)&&g.map.PushSteps==3,"failed creation save pushes 15 feet along wind");C(g.concentrationSpell=="造风术"&&g.gustWind.Expires==10,"one-minute concentration");C(g.GustTurnError()!="","cannot turn on cast turn");
        g=Gust();UnityEngine.Random.D20=20;g.CastGust();C(g.map.Monster.x==4,"successful creation save prevents push");UnityEngine.Random.D20=1;g.QueueGustEnd(-1);g.QueueGustEnd(-1);C(g.startStatuses.Count==1,"end-turn save notification deduplicated");Run(g,g.DrainGreaseEntries());C(g.map.Monster.x==7,"same target saves again on end turn even if creation was this turn");
        g=Gust();g.map.MonsterAquatic.Submerged=true;g.CastGust();C(g.map.Monster.x==4&&!g.GustEndAvailable(-1),"submerged creature never saves or gets pushed");C(!g.GustMover(-1),"submerged creature is excluded by layer gating");g.map.MonsterAquatic.Submerged=false;C(g.map.AgainstGust(-1,new Vector2Int(4,0),new Vector2Int(3,0),1),"surface mover approaching source pays extra");C(!g.map.AgainstGust(-1,new Vector2Int(3,0),new Vector2Int(4,0),1)&&!g.map.AgainstGust(0,new Vector2Int(0,0),new Vector2Int(1,0),1),"moving away and caster movement do not pay extra");
        g=Gust();g.map.Block=new Vector2Int(6,0);g.CastGust();C(g.map.Monster.x==5,"push stops at last legal position");
        g=Gust();g.map.Units.Add(new Unit{Id=9,X=3,Y=0});g.CastGust();C(g.FindSummon(9).X==6,"allies and summons are included");
        g=Gust();g.map.MonsterScene=1;g.CastGust();C(g.map.PushSteps==0,"other-plane creatures unaffected");
        g=Gust();g.CastGust();g.terrainTurn++;g.gustChoice.AimX=1;g.gustChoice.AimY=20;g.gustChoice.Edge=2;g.TurnGustWind();C(!g.combat.BonusActionAvailable&&g.gustWind.Expires==10&&g.map.PushSteps==3,"later-turn rotation spends only bonus action, does not push or refresh timer");
        g=Gust();g.CastGust();g.vision.Zones.Add(new VisionZone{Fog=true,X=45,Y=20,FogRadius=20,End=100});g.vision.Zones.Add(new VisionZone{Fog=true,X=45,Y=60,FogRadius=20,End=100});g.stinkingCloud.Begin(4,0,0,12,0);g.stinkingCloud.Poison(-1,g.terrainTurn,0);g.RefreshGustClouds();C(g.vision.Zones.Count==1&&!g.stinkingCloud.Active(0)&&g.stinkingCloud.Poisoned(-1,g.terrainTurn,0),"partial intersection disperses whole cloud, preserving existing poison and unrelated clouds");
        g=Gust();g.CastGust();g.map.Player=new Vector2Int(0,10);C(g.CurrentGust().Cell(3,10)&&!g.CurrentGust().Cell(3,0),"line follows caster without following old aim point");
        g=Gust();g.CastGust();long revision=g.gustWind.Revision;g.EndGustWind();Run(g,g.ResolveGustSave(-1,revision));C(g.map.PushSteps==3&&g.startStatuses.Count==0,"ending concentration invalidates queued saves");g=Gust();g.CastGust();g.worldRounds=10;C(!g.gustWind.Active(10)&&!g.GustEndAvailable(-1),"expires at one minute");
        Console.WriteLine("PASS: production Gust of Wind creation/end saves, push, diving, rotation, source movement, gas dispersion, expiry and queue dispatcher");
    }
}
class GustChecksMain{static void Main()=>TrainingGame.CheckGust();}
