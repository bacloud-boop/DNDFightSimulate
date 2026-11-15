using System;
using UnityEngine;
class ThornWhipChecks
{
    static Vector2Int V(int x,int y)=>new Vector2Int(x,y);
    static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
    static BattleMap Setup(int x,int y){var map=new BattleMap();Check(map.Place(false,V(x,y))&&map.Lock(),"setup");return map;}
    static void Main()
    {
        var map=Setup(5,0);int remaining=map.MonsterMovementRemaining;
        Check(map.ForceMoveToward(false,map.Player,1,10)==10&&map.Monster==V(3,0),"straight pull");
        Check(map.MonsterMovementRemaining==remaining,"forced movement is free");
        Check(map.ForceMoveToward(false,map.Player,1,10)==10&&map.Monster==V(1,0),"stops adjacent");
        Check(map.ForceMoveToward(false,map.Player,1,10)==0,"cannot overlap source");
        map=Setup(4,4);Check(map.ForceMoveToward(false,map.Player,1,10)==7&&map.Monster==V(3,3),"diagonal limit");
        Check(map.ForceMoveToward(false,map.Player,1,5)==0,"diagonal needs seven feet");
        map=Setup(5,0);Check(map.ForceMoveToward(false,map.Player,1,0)==0&&map.Monster==V(5,0),"optional no pull");
        map.Unlock();Check(map.TrySetFootprint(false,2)&&map.Place(false,V(5,0))&&map.Lock(),"large setup");
        Check(map.ForceMoveToward(false,map.Player,1,10)<=10&&!BattleMap.Overlap(map.Monster,2,map.Player,1),"large footprint safe");
        Console.WriteLine("PASS: straight, diagonal, optional pull, collision, large footprint and movement budget");
    }
}
