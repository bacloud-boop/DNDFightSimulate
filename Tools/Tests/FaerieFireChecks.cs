using System;
using System.Collections.Generic;
class FaerieFireChecks
{
    static void C(bool ok,string m){if(!ok)throw new Exception(m);}
    static List<double[]> Points(params double[] xy){var p=new List<double[]>();for(int i=0;i<xy.Length;i+=2)p.Add(new[]{xy[i],xy[i+1]});return p;}
    static void Main()
    {
        var f=new FaerieFireEffect();C(f.Begin(1,0)&&f.Bind(0,0)&&f.Bind(-1,0)&&f.Bind(7,0),"player, monster and summon are all legal outlined targets");
        C(f.AttackAdvantage(-1,true,0)&&!f.AttackAdvantage(-1,false,0),"outline grants advantage only to attackers who can see the target");
        C(f.Outlined(-1,9)&&!f.Outlined(-1,10),"outline duration is ten rounds independent of later map location");
        C(FaerieFireEffect.Intersects(3,3,2,2,2)&&!FaerieFireEffect.Intersects(3,3,7,3,1),"cube affects partial multi-cell intersection, not cells beyond the far edge");
        C(!f.Begin(1,long.MaxValue-9)&&!f.Begin(0,0),"invalid duration and slot cannot create an effect");f.Clear();C(!f.Outlined(0,0),"concentration termination removes all target outlines");
        var v=new VisionState{BaseLight=2};C(v.SetDimLightGroup(1,Points(0,0),10,1,10,0),"dim emitter creation");
        C(v.Light(0,0,0)==1&&v.Light(10,0,0)==1&&v.Light(11,0,0)==2,"Faerie Fire is dim even at its center and extends exactly ten feet");
        v.SetDimLightGroup(1,Points(50,0),10,1,10,0);C(v.Light(0,0,0)==2&&v.Light(50,0,0)==1,"moving emitter replaces old light without leaving a trail");
        v.SetDimLightGroup(1,Points(0,0,50,0),10,1,10,0);v.AddZone(true,0,0,2,0);
        C(!v.SetDimLightGroup(1,Points(0,0,50,0),10,1,10,0)&&v.Light(50,0,0)==2,"Darkness dispels the whole lower-level light spell, including a distant second target");
        v.ClearEffects();v.BaseLight=2;v.AddZone(true,0,0,2,0);
        C(!v.SetDimLightGroup(1,Points(0,0),10,2,10,0),"creating low-level outline inside existing Darkness is also dispelled");
        C(v.SetDimLightGroup(1,Points(0,0),10,3,10,0)&&v.HasDarkness(0),"higher-slot Faerie Fire survives but does not dispel Darkness");
        C(v.Quality(0,0,5,0,0,0)==1&&v.Quality(20,0,0,0,0,0)==2,"magical dim light illuminates its own radius but leaves an intervening dark segment blocking sight");
        v.ClearLightGroup(1);C(v.HasDarkness(0)&&v.Light(0,0,0)==2,"removing a light source preserves unrelated Darkness");
        v.ClearEffects();v.AddZone(false,0,0,3,0);v.SetDimLightGroup(1,Points(0,0),10,1,10,0);v.AddZone(true,0,0,2,0);
        C(v.SetDimLightGroup(1,Points(0,0),10,1,10,0)&&!v.HasDarkness(0),"Daylight prevents incoming low Darkness without dispelling an existing outline");
        Console.WriteLine("PASS: Faerie Fire target geometry, conditional advantage, duration, moving dim light and whole-spell Darkness interaction");
    }
}
