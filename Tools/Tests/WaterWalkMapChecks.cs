using System;
using UnityEngine;
class WaterWalkMapChecks
{
    static Vector2Int V(int x,int y)=>new Vector2Int(x,y);
    static void C(bool ok,string text){if(!ok)throw new Exception(text);}
    static BattleMap WetColumn()
    {
        var map=new BattleMap();for(int y=0;y<6;y++)map.SetWater(V(0,y),true);C(map.Lock(),"column fixture locks");return map;
    }
    static void SurfaceAndSwitches()
    {
        var map=WetColumn();int bonus=1,calls=0;
        map.WaterWalking=id=>id==0;map.WaterWalkBonus=id=>{calls++;if(id!=0||bonus==0)return false;bonus--;return true;};
        C(map.IsWalkingOnWaterSurface(0)&&map.PlayerMovementCost(V(0,1))==5,"protected surface walker pays ordinary ground cost");
        C(map.MovePlayer(V(0,1))&&map.MovementRemaining==25&&!map.PlayerAquatic.Submerged,"ordinary water movement remains on surface");
        C(map.SwitchWaterState(true,true)&&bonus==0&&calls==1&&map.MovementRemaining==25,"diving spends bonus rather than movement");
        C(!map.IsWalkingOnWaterSurface(0)&&map.PlayerMovementCost(V(0,2))==10,"submerged protected actor still pays swimming cost without swim speed");
        C(!map.SwitchWaterState(true,false)&&map.PlayerAquatic.Submerged&&map.MovementRemaining==25,"missing bonus leaves state and movement unchanged");
        C(map.SpendPlayerMovement(25)&&map.MovementRemaining==0,"exhaust movement fixture");bonus=1;
        C(map.SwitchWaterState(true,false)&&!map.PlayerAquatic.Submerged&&map.MovementRemaining==0,"surfacing is legal with zero movement when bonus remains");
        bonus=1;C(map.SwitchWaterState(true,true)&&map.MovementRemaining==0,"diving also remains legal with zero movement");
        map.PlayerSwimmingSpeed=30;C(map.PlayerMovementCost(V(0,2))==5,"swimming speed still applies normally while submerged");
        var missing=WetColumn();missing.WaterWalking=id=>id==0;C(!missing.SwitchWaterState(true,true)&&!missing.PlayerAquatic.Submerged&&missing.MovementRemaining==30,"absent bonus callback cannot grant a free switch");
        var ordinary=WetColumn();int unexpected=0;ordinary.WaterWalkBonus=id=>{unexpected++;return true;};
        C(ordinary.SwitchWaterState(true,true)&&ordinary.MovementRemaining==25&&unexpected==0,"unprotected actor retains the original five-foot switch and ignores bonus callback");
        ordinary.SpendPlayerMovement(25);C(!ordinary.SwitchWaterState(true,false)&&ordinary.PlayerAquatic.Submerged&&unexpected==0,"unprotected zero-movement actor cannot switch for free");
    }
    static void LargeAndFalls()
    {
        var large=new BattleMap();C(large.TrySetFootprint(true,2),"large footprint fixture");
        large.SetWater(V(0,0),true);large.SetWater(V(1,0),true);large.SetWater(V(0,1),true);
        for(int y=0;y<2;y++)for(int x=2;x<4;x++)large.SetWater(V(x,y),true);
        C(large.Lock(),"large fixture locks");int spent=0;large.WaterWalking=id=>id==0;large.WaterWalkBonus=id=>{spent++;return true;};
        C(!large.SwitchWaterState(true,true)&&spent==0,"partial water footprint cannot dive or spend bonus");
        C(large.MovePlayer(V(2,0))&&!large.PlayerAquatic.Submerged&&large.SwitchWaterState(true,true)&&spent==1,"full-footprint water admits a bonus-action dive after ordinary surface movement");
        var jump=new BattleMap();jump.SetWater(V(2,0),true);jump.Lock();jump.WaterWalking=id=>id==0;int observations=0;
        jump.MovementObserved=(id,from,to,oldSize,size,landing)=>{C(jump.Player==to&&jump.PlayerAquatic.Submerged,"jump observer sees committed position and submerged state");observations++;};
        C(jump.Jump(true,V(2,0))&&jump.PlayerAquatic.Submerged&&jump.MovementRemaining==20&&observations==1,"jumping into water causes a free fall layer change after paying jump movement");
        var teleport=new BattleMap();teleport.SetWater(V(2,0),true);teleport.Lock();teleport.WaterWalking=id=>id==0;
        C(teleport.TeleportPlayer(V(2,0),30)&&!teleport.PlayerAquatic.Submerged&&teleport.MovementRemaining==30,"teleport landing notification must not be mistaken for a fall");
        var ordinaryJump=new BattleMap();ordinaryJump.SetWater(V(2,0),true);ordinaryJump.Lock();
        C(ordinaryJump.Jump(true,V(2,0))&&!ordinaryJump.PlayerAquatic.Submerged,"unprotected jumping preserves the project's original surface landing behavior");
        var monsterFall=new BattleMap();monsterFall.SetWater(V(8,9),true);monsterFall.Lock();monsterFall.WaterWalking=id=>id==-1;bool monsterObserved=false;
        monsterFall.MovementObserved=(id,from,to,oldSize,size,landing)=>{C(id==-1&&monsterFall.Monster==to&&monsterFall.MonsterAquatic.Submerged,"monster observer sees committed position and layer");monsterObserved=true;};
        C(monsterFall.ForceMoveAway(false,V(10,9),1,5)==5&&monsterObserved&&monsterFall.Monster==V(8,9),"monster falls use its own aquatic state, not the player's");
        foreach(bool pull in new[]{false,true}){
            var map=new BattleMap();map.Place(true,V(2,2));for(int x=3;x<6;x++)map.SetWater(V(x,2),true);map.Lock();map.WaterWalking=id=>id==0;
            int notified=0,bonuses=0;map.WaterWalkBonus=id=>{bonuses++;return true;};
            map.MovementObserved=(id,from,to,oldSize,size,landing)=>{C(map.Player==to,"forced movement notification sees the new position");C(map.PlayerAquatic.Submerged,"forced wet entry notification sees the new layer");notified++;};
            int distance=pull?map.ForceMoveToward(true,V(6,2),1,10):map.ForceMoveAway(true,V(1,2),1,10);
            C(distance==10&&map.Player==V(4,2)&&map.PlayerAquatic.Submerged&&notified==2&&bonuses==0&&map.MovementRemaining==30,"push and pull into water submerge without spending bonus or movement");
        }
        var partial=new BattleMap();partial.TrySetFootprint(true,2);partial.SetWater(V(2,0),true);partial.SetWater(V(3,0),true);partial.SetWater(V(2,1),true);partial.Lock();partial.WaterWalking=id=>id==0;
        C(partial.ForceMoveAway(true,V(-2,0),2,10)==10&&!partial.PlayerAquatic.Submerged,"forced entry with only partial flooded footprint stays above water");
    }
    static void AuxiliaryAndRiding()
    {
        var map=WetColumn();var unit=new SummonedUnit(1,"walker",20,12,30,2,1);C(map.AddSummon(unit,V(0,1))&&unit.BeginTurn(1),"summon fixture");
        map.WaterWalking=id=>id==1;map.WaterWalkBonus=id=>id==1&&unit.TryUseBonus();
        C(map.MoveSummon(1,V(0,2))&&unit.MovementRemaining==25&&!unit.Aquatic.Submerged,"summon uses ordinary surface movement cost");
        C(map.SwitchSummonWaterState(1,true)&&unit.Aquatic.Submerged&&unit.MovementRemaining==25&&!unit.BonusAvailable,"summon dive consumes its own bonus");
        C(!map.SwitchSummonWaterState(1,false)&&unit.MovementRemaining==25,"summon cannot repeat switch after spending bonus");
        unit.EndTurn();unit.BeginTurn(2);unit.SpendMovement(30);C(map.SwitchSummonWaterState(1,false)&&unit.MovementRemaining==0,"summon can surface with refreshed bonus and no movement");
        var rescueMap=WetColumn();var rescue=new RescueCarrier();C(rescueMap.DeployRescue(V(0,2),1,true)&&rescue.BeginTurn(1),"rescue fixture");
        rescueMap.WaterWalking=id=>id==BattleMap.RescueId;rescueMap.WaterWalkBonus=id=>id==BattleMap.RescueId&&rescue.TryUseBonusAction();
        C(rescueMap.MoveRescue(V(0,3),rescue.Move)&&rescue.MovementRemaining==25&&!rescueMap.RescueAquatic.Submerged,"rescue surface movement has no swimming surcharge");
        int movementCallback=0;C(rescueMap.SwitchRescueWaterState(true,cost=>{movementCallback++;return false;})&&rescueMap.RescueAquatic.Submerged&&rescue.MovementRemaining==25&&movementCallback==0,"rescue switch uses bonus callback rather than movement payer");
        var riding=new BattleMap();for(int y=0;y<4;y++)for(int x=1;x<3;x++)riding.SetWater(V(x,y),true);riding.Lock();var mount=new SummonedUnit(7,"mount",30,12,30,3,1);
        C(riding.AddSummon(mount,V(1,0))&&mount.BeginTurn(1)&&riding.MountSummon(7),"mounted fixture");riding.WaterWalking=id=>id==7;riding.WaterWalkBonus=id=>id==7&&mount.TryUseBonus();
        C(!riding.SwitchWaterState(true,true)&&mount.BonusAvailable,"rider cannot switch independently of mounted body");
        C(riding.SwitchSummonWaterState(7,true)&&mount.Aquatic.Submerged&&riding.PlayerAquatic.Submerged,"mount's dive synchronizes rider before further operations");
        mount.EndTurn();mount.BeginTurn(2);C(riding.SwitchSummonWaterState(7,false)&&!mount.Aquatic.Submerged&&!riding.PlayerAquatic.Submerged,"surfacing mount synchronizes rider");
        C(riding.MoveSummon(7,V(1,1))&&mount.MovementRemaining==25&&riding.Player==V(1,1)&&!riding.PlayerAquatic.Submerged,"mounted movement uses mount spell and pool while preserving shared surface state");
    }
    static void RouteCostAgreement()
    {
        foreach(bool submerged in new[]{false,true}){
            var map=WetColumn();map.WaterWalking=id=>id==0;map.WaterWalkBonus=id=>true;map.SwimmingTerrain=(p,n)=>map.WaterCells(p,n)==n*n;
            if(submerged)C(map.SwitchWaterState(true,true),"submerged route fixture");
            var from=map.Player;var to=V(0,3);int expected=map.PlayerMovementCost(to),before=map.MovementRemaining;var meter=map.RouteMeter(from,to,false);var previous=from;
            foreach(var cell in BattleMap.MovementAnchors(from,to)){int cost=map.RouteStepCost(meter,previous,cell,1,0,0);C(map.WithMovementCost(cell,cost,()=>map.MovePlayer(cell)),"segmented route commits");previous=cell;}
            C(meter.Paid==expected&&before-map.MovementRemaining==expected,"surface and underwater segmented route match direct movement quote");
        }
        var transition=new BattleMap();transition.SetWater(V(0,0),true);transition.SetWater(V(2,0),true);transition.SetWater(V(3,0),true);transition.Lock();transition.WaterWalking=id=>id==0;transition.WaterWalkBonus=id=>true;transition.SwimmingTerrain=(p,n)=>transition.WaterCells(p,n)==n*n;transition.SwitchWaterState(true,true);
        var destination=V(3,0);int quote=transition.PlayerMovementCost(destination),remaining=transition.MovementRemaining;var transitionMeter=transition.RouteMeter(transition.Player,destination,false);var last=transition.Player;
        foreach(var point in BattleMap.MovementAnchors(transition.Player,destination)){int cost=transition.RouteStepCost(transitionMeter,last,point,1,0,0);C(transition.WithMovementCost(point,cost,()=>transition.MovePlayer(point)),"land transition route commits");last=point;}
        C(quote==15&&transitionMeter.Paid==quote&&remaining-transition.MovementRemaining==quote&&!transition.PlayerAquatic.Submerged,"leaving water clears submerged layer and reentering by ordinary movement uses surface cost in quotes and execution");
    }
    static void Main(){SurfaceAndSwitches();LargeAndFalls();AuxiliaryAndRiding();RouteCostAgreement();Console.WriteLine("PASS: Water Walk surface/submerged movement, bonus-action layer switches, full footprints, jump/push/pull falls, teleport safety, auxiliary units, rider synchronization and segmented route consistency");}
}
