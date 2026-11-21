using System;
using System.Collections.Generic;
using UnityEngine;
class FlamingSphereChecks
{
    static void C(bool ok,string text){if(!ok)throw new Exception(text);}
    static Vector2Int V(int x,int y)=>new Vector2Int(x,y);
    static void Main()
    {
        var sphere=new FlamingSphereEffect();C(sphere.Begin(V(2,3),2,14,10)&&sphere.Active(19)&&!sphere.Active(20)&&sphere.Slot==2,"two d6 and exactly one minute");
        C(!sphere.Begin(V(0,0),1,10,10)&&sphere.X==2,"invalid cast is atomic");
        C(sphere.CanMove(1,10),"bonus movement is available even on casting turn");sphere.Place(V(3,3),true,1);C(!sphere.CanMove(1,10)&&sphere.CanMove(2,11),"a collision stops further movement for this caster turn only");
        C(sphere.ClaimEnd(-1,4,10)&&!sphere.ClaimEnd(-1,4,10)&&sphere.ClaimEnd(0,4,10)&&sphere.ClaimEnd(-1,5,11),"duplicate end callback suppressed per actor, without blocking other turns or collision damage");
        C(sphere.Begin(V(0,0),9,14,11)&&sphere.Slot==9,"upcasting uses one extra d6 per level");
        List<Vector2Int> route;int feet;Func<Vector2Int,bool> inside=p=>p.x>=0&&p.y>=0&&p.x<20&&p.y<20;
        C(FlamingSphereEffect.Route(V(0,0),new[]{V(6,0)},inside,p=>false,out route,out feet)&&feet==30&&route.Count==6,"ordinary rolling costs five per cardinal grid step");
        C(!FlamingSphereEffect.Route(V(0,0),new[]{V(7,0)},inside,p=>false,out route,out feet),"31+ feet is rejected before spending a bonus action");
        C(FlamingSphereEffect.Route(V(0,0),new[]{V(4,4)},inside,p=>false,out route,out feet)&&feet==28,"diagonal rolling uses the project's seven-foot rule");
        C(FlamingSphereEffect.Route(V(0,0),new[]{V(3,0)},inside,p=>p.x==1||p.x==2,out route,out feet)&&feet==15&&route.Count==1&&route[0]==V(3,0),"two water tiles form a ten-foot gap; airborne cells aren't ground collision/ignition locations");
        C(!FlamingSphereEffect.Route(V(0,0),new[]{V(4,0)},inside,p=>p.x>0&&p.x<4,out route,out feet),"three water cells exceed jump width");
        C(!FlamingSphereEffect.Route(V(0,0),new[]{V(2,0)},inside,p=>p.x==2,out route,out feet),"water endpoint forbidden");
        C(!FlamingSphereEffect.Route(V(0,0),new[]{V(2,2)},inside,p=>p.x==1||p.x==2,out route,out feet),"invalid water landing rejected even on a diagonal");
        C(FlamingSphereEffect.Route(V(0,0),new[]{V(2,0),V(2,2)},inside,p=>false,out route,out feet)&&feet==20&&route.Count==4,"waypoints allow bends without resetting cumulative movement");
        C(!FlamingSphereEffect.Route(V(0,0),new[]{V(-1,0)},inside,p=>false,out route,out feet),"off-map routes rejected");
        var map=new BattleMap();map.Place(false,V(3,0));map.Lock();C(!map.CanPlaceSummon(V(3,0),1)&&!map.CanPlaceSummon(V(0,0),1)&&map.CanPlaceSummon(V(2,0),1),"initial placement excludes occupied footprints");
        C(BattleMap.BodyDistance(V(2,0),1,V(3,0),2)==5&&BattleMap.BodyDistance(V(2,0),1,V(3,1),2)==7,"end aura measures nearest occupied cell of multi-cell creatures");
        var fire=DirectSpell.Find("Flaming Sphere");C(fire!=null&&fire.Level==2&&fire.Range==60&&fire.DiceCount(1,2)==2&&fire.DiceCount(20,9)==9,"spell metadata and slot growth match effect");
        var actor=new CombatState();C(actor.TryUseMagicAction()&&actor.TryUseBonusAction()&&!actor.TryUseBonusAction(),"casting plus same-turn roll use different action pools");
        var vision=new VisionState();vision.BaseLight=2;C(vision.SetLightGroup(4,new[]{new[]{0.0,0.0}},20,40,2,20,10),"sphere emits magical light");C(vision.Quality(0,0,20,0,0,10)==0&&vision.Quality(0,0,40,0,0,10)==1,"bright twenty and dim another twenty");
        sphere.Clear();C(!sphere.Active(12)&&!sphere.CanMove(2,12)&&!sphere.ClaimEnd(-1,6,12),"concentration/reset removes movement and end damage");
        Console.WriteLine("PASS: sphere duration, scaling, same-turn bonus movement, collision stop, end timing, cumulative routes, water gaps, footprint geometry, action pools and lighting");
    }
}
