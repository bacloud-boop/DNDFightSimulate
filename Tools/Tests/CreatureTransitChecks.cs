using System;
using UnityEngine;
class CreatureTransitChecks
{
    static Vector2Int V(int x,int y)=>new Vector2Int(x,y);
    static void C(bool ok,string message){if(!ok)throw new Exception(message);}
    static BattleMap Map(int playerSize=1,int monsterSize=2,bool nimble=true)
    {
        var map=new BattleMap();map.Place(false,V(2,0));map.Lock();
        map.CreatureSizeIndex=id=>id==0?playerSize:monsterSize;map.CreatureNimble=id=>id==0&&nimble;
        return map;
    }
    static void Main()
    {
        C(CreatureTransitRules.CanPass(1,2,false,false,true)&&!CreatureTransitRules.CanPass(1,2,false,false,false),"nimbleness uses size categories, including Small versus Medium in one-cell footprints");
        C(CreatureTransitRules.CanPass(2,2,true,false,false)&&CreatureTransitRules.CanPass(2,2,false,true,false)&&CreatureTransitRules.CanPass(2,0,false,false,false)&&CreatureTransitRules.CanPass(4,2,false,false,false),"allies, incapacitated, Tiny and two-size differences permit passage");
        C(!CreatureTransitRules.SharedTurnProne(0,5)&&!CreatureTransitRules.SharedTurnProne(3,2)&&CreatureTransitRules.SharedTurnProne(2,2)&&CreatureTransitRules.SharedTurnProne(1,2),"only Tiny or larger shared occupants are exempt from turn-end Prone");
        var m=Map();C(m.CanMoveTo(V(4,0))&&!m.CanMoveTo(V(2,0)),"transit allows route but never voluntarily occupied final destination");
        C(m.PlayerMovementCost(V(4,0))==25&&m.CreaturePathNeedsSteps(0,m.Player,V(4,0),1),"hostile non-Tiny space adds difficult terrain and requires section resolution");
        var meter=m.RouteMeter(m.Player,V(4,0),false);var previous=m.Player;
        foreach(var cell in BattleMap.MovementAnchors(previous,V(4,0))){
            int cost=m.RouteStepCost(meter,previous,cell,1,0,0);previous=cell;
            C(m.WithCreatureTransitStep(0,()=>m.WithMovementCost(cell,cost,()=>m.MovePlayer(cell))),"committed route section can enter and exit permitted occupied space");
            if(cell==V(2,0))break;
        }
        C(m.Player==m.Monster&&m.HasSharedPosition(0)&&m.MovementSpent==15&&m.MovementRemaining==15,"forced interruption retains shared trigger cell and actual paid movement");
        C(m.SharedTurnNeedsProne(0)&&!m.SharedTurnNeedsProne(-1),"smaller stopped halfling is Prone at turn end, larger opponent is exempt");
        C(!m.CanMoveTo(m.Monster)&&m.MovePlayer(V(3,0))&&!m.HasSharedPosition(0),"transit scope is restored and forced shared actor can leave");
        C(!Map(1,2,false).CanMoveTo(V(4,0)),"ordinary Small creature cannot pass Medium enemy");
        var incapacitated=Map(2,2,false);incapacitated.CreatureIncapacitated=id=>id==-1;C(incapacitated.CanMoveTo(V(4,0)),"incapacitated living enemy permits passage");
        incapacitated.CreatureLiving=id=>id!=-1;C(!incapacitated.CanMoveTo(V(4,0)),"grounded corpse is not a living incapacitated creature");
        var diagonal=Map();diagonal.Place(false,V(2,2));diagonal.Unlock();diagonal.Place(false,V(2,2));diagonal.Lock();
        var target=V(3,3);int expected=diagonal.PlayerMovementCost(target),paid=0;meter=diagonal.RouteMeter(diagonal.Player,target,false);previous=diagonal.Player;
        foreach(var cell in BattleMap.MovementAnchors(previous,target)){int cost=diagonal.RouteStepCost(meter,previous,cell,1,0,0);paid+=cost;previous=cell;}
        C(paid==expected,"rounded diagonal route and full-route previews conserve identical paid distance");
        var rough=Map();rough.DifficultTerrain=(x,y,n)=>x==2&&y==0;C(rough.PlayerMovementCost(V(4,0))==25,"spell and creature difficult terrain do not stack");
        C(Map(2,0,false).PlayerMovementCost(V(4,0))==20&&Map(2,0,false).CanMoveTo(V(4,0)),"Tiny enemies permit passage without difficult terrain");
        var mixed=new BattleMap();mixed.TrySetFootprint(true,2);mixed.Place(false,V(2,1));mixed.Lock();mixed.CreatureSizeIndex=id=>id==0?3:2;
        C(mixed.PlayerMovementCost(V(1,0))==5,"partial hostile footprint is not fully difficult terrain");
        mixed.DifficultTerrain=(x,y,n)=>x!=2||y!=1;C(mixed.PlayerMovementCost(V(1,0))==10,"spell and creature rough cells can jointly cover a complete footprint");
        var budget=Map();budget.SetPlayerSpeed(20);C(!budget.CanMoveTo(V(4,0))&&budget.Player==V(0,0)&&budget.MovementSpent==0,"insufficient full-route budget rejects before entering creature space");
        var larger=Map(3,1,false);larger.TrySetRecoverySize(true,1);C(!larger.CanMoveTo(V(4,0)),"transit cannot invalidate guaranteed original-form recovery");
        var exit=Map();C(exit.WithCreatureTransitStep(0,()=>exit.MovePlayer(V(2,0))),"forced shared fixture");C(exit.MoveMonster(V(3,0)),"non-nimble counterpart can leave existing forced overlap");
        var old=Map();old.Remains.Add(0,1,0,1,"body",new BodyWeight(),0,false);C(!old.CanMoveTo(V(4,0)),"retained remains block passage");
        var ally=new BattleMap();ally.Lock();var summon=new SummonedUnit(1,"ally",10,10,30,2,1);C(ally.AddSummon(summon,V(2,0)),"ally fixture");ally.CreatureSizeIndex=id=>2;
        C(ally.CanMoveTo(V(4,0))&&ally.PlayerMovementCost(V(4,0))==20&&!ally.CanMoveTo(V(2,0)),"allied space allows passage without surcharge or occupied endpoint");
        summon.BeginTurn(1);int ended=0;ally.ActorTurnEnded=id=>{if(id==1)ended++;};summon.EndTurn();summon.EndTurn();C(ended==1,"summon turn end notifies once for an active turn");
        var rescue=new BattleMap();rescue.Place(false,V(2,2));rescue.Lock();rescue.CreatureSizeIndex=id=>id==BattleMap.RescueId?1:2;rescue.CreatureNimble=id=>id==BattleMap.RescueId;
        C(rescue.DeployRescue(V(0,2),1,true)&&rescue.CanMoveRescue(V(4,2))&&!rescue.CanMoveRescue(V(2,2))&&rescue.RescueMovementCost(V(4,2))==25,"rescue halfling passage shares occupancy and cost rules");
        C(rescue.WithCreatureTransitStep(BattleMap.RescueId,()=>rescue.MoveRescue(V(2,2),cost=>true))&&rescue.SharedTurnNeedsProne(BattleMap.RescueId),"rescue forced stop retains shared position");
        var dragging=new BattleMap();dragging.Place(false,V(2,2));dragging.Lock();dragging.CreatureSizeIndex=id=>id==BattleMap.RescueId?1:2;dragging.CreatureNimble=id=>id==BattleMap.RescueId;
        C(dragging.DeployRescue(V(0,2),1,true),"dragging fixture");var dragBody=dragging.Remains.Add(0,0,1,1,"body",new BodyWeight(),0,false);
        C(dragging.AttachRescueRemains(dragBody.Id,true),"attach grounded old corpse");
        C(dragging.CanMoveRescue(V(4,2))&&!dragging.CanMoveRescue(V(4,3)),"dragged body does not inherit carrier passage through living opponents");
        var carrier=new RescueCarrier();int rescueEnds=0;carrier.TurnEnded=()=>rescueEnds++;carrier.BeginTurn(1);carrier.Reset();C(rescueEnds==0,"reset never dispatches an obsolete end-turn overlap penalty");carrier.BeginTurn(1);carrier.EndTurn();carrier.EndTurn();C(rescueEnds==1,"rescue turn end notifies once");
        var legacy=new BattleMap();legacy.Place(false,V(2,0));legacy.Lock();C(!legacy.CanMoveTo(V(4,0)),"unbound standalone map retains previous hostile blocking");
        Console.WriteLine("PASS: halfling transit, standard permissions, hostile terrain costs, forced sharing, turn-end size exceptions, resource isolation and rescue/summon regressions");
    }
}
