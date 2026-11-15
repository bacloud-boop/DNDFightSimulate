using System;
using UnityEngine;

class ContainmentChecks
{
    static void Check(bool value,string message){if(!value)throw new Exception(message);}
    static Vector2Int V(int x,int y)=>new Vector2Int(x,y);
    static void Main()
    {
        var map=new BattleMap();map.Place(false,V(1,0));
        Check(!map.BeginContainment(true),"deployment cannot swallow");map.Lock();
        Check(map.BeginContainment(true)&&map.IsContained(true)&&map.OccupiedSpaces(true)==0,"player occupies no external cells while contained");
        Check(!map.BeginContainment(false),"cannot nest containment");
        Check(!map.MovePlayer(V(0,1))&&!map.TeleportPlayer(V(3,0),30)&&map.ForceMoveAway(true,V(0,0),1,10)==0,"passenger cannot independently move or be pushed");
        Check(map.MoveMonster(V(2,0))&&map.Player==map.Monster,"passenger follows host without blocking path");
        Check(map.ForceMoveAway(false,V(0,0),1,5)==5&&map.Player==V(3,0),"passenger follows forced movement");
        int budget=map.MovementRemaining;Check(!map.ReleaseContainedAt(V(3,0),5)&&!map.ReleaseContainedAt(V(7,0),5)&&map.IsContained(true),"occupied and distant release rejected atomically");
        Check(map.ReleaseContainedAt(V(3,1),5)&&!map.HasContainedCreature&&map.Player==V(3,1)&&map.MovementRemaining==budget,"chosen adjacent exit restores footprint without implicit cost");
        var reverse=new BattleMap();reverse.Place(false,V(1,0));reverse.Lock();
        Check(reverse.BeginContainment(false)&&reverse.MovePlayer(V(0,2))&&reverse.Monster==reverse.Player,"monster follows player host");
        Check(reverse.TeleportPlayer(V(4,2),30)&&reverse.Monster==V(4,2),"host teleport carries passenger");
        Check(reverse.MandatoryReleaseRange(0)==5,"mandatory exit expands to nearest legal distance when requested range has no space");
        Check(!reverse.CanMandatoryReleaseAt(V(6,2),0)&&reverse.HasContainedCreature,"fallback cannot select a farther than nearest exit");
        Check(reverse.MandatoryReleaseAt(V(5,2),0)&&reverse.MonsterSpaces==1,"monster exits into selected nearest legal cell");
        var large=new BattleMap();large.TrySetFootprint(false,2);large.Place(false,V(1,0));large.Place(true,V(0,0));large.Lock();
        Check(large.BeginContainment(true)&&large.MoveMonster(V(2,0)),"large host movement");
        Check(large.CanReleaseContainedAt(V(4,0),5)&&!large.CanReleaseContainedAt(V(3,1),5),"release uses host footprint edge rather than anchor");
        Check(!large.TransformAt(false,3,V(2,0),2),"unresolved contained shape change must be atomic");
        large.Unlock();Check(!large.HasContainedCreature&&large.CanPlace(true,large.Player)&&large.CanPlace(false,large.Monster),"deployment reset clears containment into legal separate anchors");
        var crowded=new BattleMap();crowded.TrySetFootprint(true,4);crowded.TrySetFootprint(false,4);
        crowded.Place(true,V(0,0));crowded.Place(false,V(4,0));crowded.Lock();
        Check(crowded.BeginContainment(true)&&crowded.MoveMonster(V(3,3)),"large internal passenger follows host toward map center");
        Check(crowded.MandatoryReleaseRange()==-1&&!crowded.MandatoryReleaseAt(V(0,0))&&crowded.IsContained(true),"no full footprint anywhere must not overlap, erase target or mutate state");
        int remaining=crowded.MonsterMovementRemaining;var before=crowded.Monster;
        var plans=crowded.MandatoryReleasePlans();
        Check(plans.Count>0&&crowded.Monster==before&&crowded.IsContained(true),"planning fallback is read only");
        foreach(var plan in plans){int dx=plan.Host.x-before.x,dy=plan.Host.y-before.y;Check(dx*dx+dy*dy==1,"fallback uses minimum necessary host displacement");}
        Check(!crowded.ApplyMandatoryReleasePlan(new BattleMap.ReleasePlan(V(6,6),V(0,0)))&&crowded.Monster==before,"nonminimal relocation cannot be applied");
        Check(crowded.ApplyMandatoryReleasePlan(plans[0])&&!crowded.HasContainedCreature&&!BattleMap.Overlap(crowded.Player,4,crowded.Monster,4),"selected fallback releases both complete bodies");
        Check(crowded.MonsterMovementRemaining==remaining,"mandatory displacement does not consume movement budget");
        Console.WriteLine("PASS: internal occupancy, host following, movement isolation, release geometry and atomic failures");
    }
}
