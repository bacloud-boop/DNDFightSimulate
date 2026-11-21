using System;
class WaterWalkChecks
{
    static void C(bool ok,string text){if(!ok)throw new Exception(text);}
    static void Main()
    {
        var waterWalk=new WaterWalkEffect();
        C(waterWalk.Begin(new[]{0,-1,1,int.MinValue},10),"one casting accepts player, monster, summon and rescue identities");
        C(!waterWalk.Active(0,9)&&waterWalk.Active(0,10)&&waterWalk.Active(0,609)&&!waterWalk.Active(0,610),"a target is active from casting through one hour, with an exclusive expiry boundary");
        C(!waterWalk.Active(2,10)&&!waterWalk.Active(0,-1),"unselected creatures and invalid earlier time do not gain the effect");
        C(waterWalk.Begin(new[]{0,2},110),"a later casting can select overlapping and new targets");
        C(waterWalk.Active(-1,609)&&!waterWalk.Active(-1,610),"unselected old targets keep their original expiry");
        C(waterWalk.Active(0,709)&&waterWalk.Active(2,709)&&!waterWalk.Active(0,710),"reselected and new targets each receive a fresh hour rather than stacked duration");
        C(waterWalk.Begin(new[]{3,4,5,6,7,8,9,10,11,12},200),"a single casting supports ten distinct creatures");
        C(waterWalk.Active(-1,300)&&waterWalk.Active(3,300),"later batches do not replace earlier independent targets");
        C(!waterWalk.Begin(new[]{3,4,5,6,7,8,9,10,11,12,13},300),"eleven distinct targets are rejected");
        C(waterWalk.Active(3,799)&&!waterWalk.Active(3,800)&&!waterWalk.Active(13,300),"rejected oversized draft neither refreshes existing timers nor partially grants new ones");
        C(!waterWalk.Begin(new int[0],300)&&!waterWalk.Begin(null,300)&&!waterWalk.Begin(new[]{3},-1),"empty, missing and negative-time drafts reject atomically");
        C(!waterWalk.Begin(new[]{3},long.MaxValue-599)&&!waterWalk.Begin(new[]{3},long.MaxValue),"duration overflow is rejected before updating target state");
        C(!waterWalk.Active(3,800),"rejected overflow cannot overwrite a previous target's expiry");
        C(waterWalk.Begin(new[]{20,20,20},400)&&waterWalk.Active(20,999)&&!waterWalk.Active(20,1000),"duplicate target identity cannot stack duration or count as multiple creatures");
        C(waterWalk.Begin(new[]{21},long.MaxValue-600)&&waterWalk.Active(21,long.MaxValue-1)&&!waterWalk.Active(21,long.MaxValue),"largest representable complete duration remains valid");
        waterWalk.Clear();C(!waterWalk.Active(0,300)&&!waterWalk.Active(3,300)&&!waterWalk.Active(20,500)&&!waterWalk.Active(21,long.MaxValue-1),"battle reset removes every casting's targets");
        C(waterWalk.Begin(new[]{-1},0)&&waterWalk.Active(-1,599)&&!waterWalk.Active(-1,600),"clear allows a new battle starting at zero without stale timers");
        Console.WriteLine("PASS: Water Walk target identities, one-hour duration, independent castings, refresh without stacking, ten-target limit, atomic rejection, overflow boundaries and reset");
    }
}
