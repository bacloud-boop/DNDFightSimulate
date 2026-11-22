using System;
using UnityEngine;
class FreedomMovementMapChecks
{
    static Vector2Int V(int x,int y)=>new Vector2Int(x,y);
    static void C(bool value,string message){if(!value)throw new Exception(message);}
    static void Main()
    {
        var rough=new BattleMap();rough.DifficultTerrain=(x,y,n)=>true;rough.Lock();
        C(rough.PlayerMovementCost(V(1,0))==10,"ordinary difficult terrain baseline");
        rough.MovementFreedom=id=>id==0;
        C(rough.PlayerMovementCost(V(1,0))==5&&rough.MonsterMovementCost(V(8,9))==10,"freedom ignores rough terrain only for the affected actor");
        rough.PlayerCrawling=true;C(rough.PlayerMovementCost(V(1,0))==10,"crawling still doubles movement while difficult terrain is ignored");
        rough.PlayerCrawling=false;rough.SetPlayerSpeed(0);C(!rough.CanMoveTo(V(1,0)),"freedom never grants movement beyond the current speed budget");

        var wet=new BattleMap();wet.SetWater(V(0,1),true);wet.SetWater(V(0,2),true);wet.DifficultTerrain=(x,y,n)=>true;wet.Lock();wet.SwimmingTerrain=(p,n)=>wet.WaterCells(p,n)==n*n;
        C(wet.PlayerMovementCost(V(0,1))==15,"swimming and rough terrain each impose their existing surcharge");
        bool enabled=true;wet.MovementFreedom=id=>id==0&&enabled;
        C(wet.PlayerMovementCost(V(0,2))==10&&wet.CommandStepCost(0,V(0,0),V(0,1))==5,"whole-route and hypothetical command costs use the same freedom rules");
        C(wet.Player==V(0,0)&&wet.MovementRemaining==30,"command quote does not move or spend the actor's resources");
        var meter=wet.RouteMeter(V(0,0),V(0,2),false);int first=wet.RouteStepCost(meter,V(0,0),V(0,1),1,0,0),second=wet.RouteStepCost(meter,V(0,1),V(0,2),1,0,0);
        C(first==5&&second==5,"segmented terrain movement ignores both swimming and rough surcharges");
        var crawlMeter=wet.RouteMeter(V(0,0),V(0,1),true);C(wet.RouteStepCost(crawlMeter,V(0,0),V(0,1),1,0,0)==10,"segmented crawling still costs extra");
        C(wet.MovePlayer(V(0,1))&&wet.PlayerAquatic.TrySetSubmerged(true,1,1)&&wet.PlayerMovementCost(V(0,2))==5,"freedom provides effective swimming speed even when submerged");
        wet.WaterWalking=id=>id==0;enabled=false;
        C(wet.PlayerMovementCost(V(0,2))==15,"expiring freedom restores underwater surcharges despite Water Walk");
        wet.PlayerAquatic.TrySetSubmerged(false,1,1);C(wet.PlayerMovementCost(V(0,2))==10,"surface Water Walk still removes only swimming, not difficult terrain");
        enabled=true;C(wet.PlayerMovementCost(V(0,2))==5,"Water Walk and Freedom coexist without stacking reductions below normal distance");
        C(wet.WaterMovementCost(V(0,1),V(-1,1),1,0,false,0)==int.MaxValue,"freedom doesn't legalize off-map destinations");
        wet.RestrainedActor=player=>true;C(!wet.CanMoveTo(V(0,2)),"map costs alone don't silently clear containment or restraint state");

        var enemy=new BattleMap();enemy.Place(false,V(2,0));enemy.Lock();enemy.CreatureSizeIndex=id=>id==0?1:2;enemy.CreatureNimble=id=>id==0;
        C(enemy.PlayerMovementCost(V(4,0))==25&&enemy.CanMoveTo(V(4,0)),"nimble passage through a hostile footprint normally adds difficult terrain cost");
        enemy.MovementFreedom=id=>id==0;C(enemy.PlayerMovementCost(V(4,0))==20&&enemy.CanMoveTo(V(4,0))&&!enemy.CanMoveTo(V(2,0)),"freedom removes hostile-space surcharge without changing crossing or endpoint permissions");

        var summonMap=new BattleMap();summonMap.SetWater(V(1,1),true);summonMap.SetWater(V(1,2),true);summonMap.SetWater(V(1,3),true);summonMap.DifficultTerrain=(x,y,n)=>true;summonMap.Lock();
        var unit=new SummonedUnit(1,"free swimmer",20,12,20,2,1);C(summonMap.AddSummon(unit,V(1,1))&&unit.BeginTurn(1),"summon fixture");summonMap.MovementFreedom=id=>id==1;
        C(summonMap.CanMoveSummon(1,V(1,3))&&summonMap.MoveSummon(1,V(1,3))&&unit.MovementRemaining==10,"summon walk/drag movement uses its own unchanged budget and normal swimming distance");

        var rescueMap=new BattleMap();for(int y=1;y<=3;y++)rescueMap.SetWater(V(0,y),true);rescueMap.DifficultTerrain=(x,y,n)=>true;rescueMap.Lock();C(rescueMap.DeployRescue(V(0,1),1,true)&&rescueMap.AttachRescueCorpse(true,false),"carrying fixture");rescueMap.MovementFreedom=id=>id==BattleMap.RescueId;
        int paid=0;C(rescueMap.RescueMovementCost(V(0,3))==10&&rescueMap.MoveRescue(V(0,3),cost=>{paid=cost;return true;})&&paid==10&&rescueMap.Player==rescueMap.RescuePosition.Value,"rescue cargo movement preserves geometry and pays only normal distance");
        C(!rescueMap.CanDropRescueCorpse(rescueMap.RescuePosition.Value),"freedom never changes corpse drop geometry");

        var swallowed=new BattleMap();swallowed.Place(false,V(1,0));swallowed.Lock();C(swallowed.BeginContainment(true),"swallow fixture");swallowed.MovementFreedom=id=>id==0;
        C(swallowed.IsContained(true)&&!swallowed.CanMoveTo(V(2,0))&&swallowed.IsContained(true),"freedom does not escape swallowed containment or spend movement to leave it automatically");

        var diving=new BattleMap();diving.SetWater(V(1,0),true);diving.Lock();diving.MovementFreedom=id=>id==0;diving.MovePlayer(V(1,0));int before=diving.MovementRemaining;
        C(diving.SwitchWaterState(true,true)&&diving.MovementRemaining==before-5,"Freedom alone doesn't alter the project's ordinary water-layer switching cost");
        Console.WriteLine("PASS: movement freedom removes water/terrain surcharges across normal, command, segmented, hostile-transit, summon and rescue routes while retaining crawling, budgets, bounds, states and Water Walk");
    }
}
