using System;
using System.Collections.Generic;
class DancingLightsChecks
{
    static void C(bool value,string message){if(!value)throw new Exception(message);}
    static DancingLightPoint P(int id,int x,int y=0)=>new DancingLightPoint(id,x,y);
    static DancingLightPoint[] Points(params DancingLightPoint[] points)=>points;
    static bool OnMap(DancingLightPoint p)=>p.X>=0&&p.Y>=0&&p.X<40&&p.Y<40;
    static int Distance(DancingLightPoint p)=>DancingLightPoint.Distance(P(0,0),p);
    static void Main()
    {
        var effect=new DancingLightsEffect();
        C(effect.Begin(Points(P(1,2)),0,0,OnMap,Distance)&&effect.Active(9)&&!effect.Active(10),"one independent light is legal and lasts ten six-second rounds");
        C(!effect.Begin(Points(P(1,1)),0,long.MaxValue-9,OnMap,Distance)&&effect.Expires==10,"overflowing recast is atomic and preserves prior lights");
        C(!effect.Begin(Points(P(1,1),P(2,6)),0,0,OnMap,Distance),"multiple independent lights must each have a neighbor within twenty feet");
        C(effect.Begin(Points(P(1,1),P(2,4),P(3,20),P(4,23)),1,0,OnMap,Distance),"two separate pairs are legal without forcing a single connected chain");
        C(!effect.Begin(Points(P(1,1),P(1,2)),0,0,OnMap,Distance)&&!effect.Begin(Points(P(1,25)),0,0,OnMap,Distance),"duplicate IDs and initial out-of-range placement are rejected");
        var draft=effect.Points;var copy=(DancingLightPoint[])draft;copy[0]=P(1,30);C(effect.Points[0].X==1,"public snapshots cannot mutate live light positions");
        int used=0;long expires=effect.Expires;
        C(!effect.TryMove(Points(P(1,14),P(2,17),P(3,20),P(4,23)),1,OnMap,Distance,()=>{used++;return true;})&&used==0&&effect.Points[0].X==1,"per-light travel above sixty feet rejects before spending bonus action");
        C(!effect.TryMove(Points(P(1,2),P(2,5),P(3,20),P(4,23)),1,OnMap,Distance,()=>false)&&effect.Points[0].X==1,"failed bonus resource leaves all positions untouched");
        C(effect.TryMove(Points(P(1,13),P(2,16),P(3,20),P(4,23)),1,OnMap,Distance,()=>{used++;return true;})&&used==1&&effect.Points[0].X==13&&effect.Expires==expires,"one bonus action moves every light up to sixty feet without extending concentration");
        C(effect.RefreshRange(2,p=>p.Id==1?125:0)==1&&effect.Points.Count==3,"out-of-range point vanishes independently");
        C(effect.RefreshRange(2,p=>p.Id==2?125:0)==1&&effect.Points.Count==2,"existing neighboring pair remains after another point vanishes");
        C(effect.RefreshRange(2,p=>p.Id==3?125:0)==1&&effect.Points.Count==1&&effect.Points[0].Id==4,"last surviving point is exempt from the neighbor requirement");
        C(!effect.TryMove(Points(P(1,1)),2,OnMap,Distance,()=>{used++;return true;})&&used==1,"moving cannot recreate a vanished light or replace its identity");
        C(effect.Begin(Points(P(1,1),P(2,4),P(3,7)),2,5,OnMap,Distance),"neighbor chain fixture");
        C(effect.RefreshRange(6,p=>p.Id==2?125:0)==3&&!effect.Active(6),"removing chain midpoint leaves two isolated points, so both vanish simultaneously");
        C(effect.Begin(Points(P(1,2)),3,0,OnMap,Distance)&&effect.Combined&&!effect.Begin(Points(P(1,2),P(2,2)),3,0,OnMap,Distance),"combined Medium appearance has one emitter position representing four joined lights");
        C(effect.TryMove(Points(P(1,14)),1,OnMap,Distance,()=>true)&&effect.Combined,"combined form moves as one light source and retains appearance");
        var vision=new VisionState{BaseLight=2};
        C(vision.SetDimLightGroup(2,new[]{new[]{0.0,0.0}},10,0,10,0)&&vision.Light(0,0,0)==1&&vision.Light(10,0,0)==1&&vision.Light(11,0,0)==2,"cantrip light group is dim, extends ten feet and never adds bright light");
        vision.BaseLight=0;C(vision.Light(0,0,0)==0,"dim spell never darkens an already brightly lit area");
        vision.BaseLight=2;vision.SetDimLightGroup(2,new[]{new[]{0.0,0.0},new[]{80.0,0.0}},10,0,10,0);vision.AddZone(true,0,0,2,0);
        C(!vision.SetDimLightGroup(2,new[]{new[]{0.0,0.0},new[]{80.0,0.0}},10,0,10,0)&&vision.Light(80,0,0)==2,"overlap with Darkness dispels the entire cantrip, including distant points");
        vision.ClearEffects();vision.AddZone(true,0,0,2,0);C(!vision.SetDimLightGroup(2,new[]{new[]{0.0,0.0}},10,0,10,0),"casting directly into existing Darkness also dispels lights");
        vision.ClearEffects();vision.SetDimLightGroup(1,new[]{new[]{80.0,0.0}},10,1,10,0);vision.SetDimLightGroup(2,new[]{new[]{0.0,0.0}},10,0,10,0);vision.ClearLightGroup(2);
        C(vision.Light(80,0,0)==1&&vision.Light(0,0,0)==2,"clearing Dancing Lights leaves other spell light groups intact");
        effect.Clear();C(!effect.Active(0)&&effect.Points.Count==0,"concentration termination clears lights");
        var caster=new CombatState();caster.Aftermath=true;C(caster.TryUseAftermathBonusAction()&&!caster.TryUseAftermathBonusAction(),"postcombat movement still consumes only one bonus action until next turn");caster.EndRound();C(caster.TryUseAftermathBonusAction(),"next turn restores the postcombat bonus action");
        Console.WriteLine("PASS: singleton rule, neighbor pairs, creation and move bounds, exact bonus spending, fading, duration, combined form, cantrip illumination and whole-spell dispelling");
    }
}
