using System;
class PermanentLightVisionChecks
{
    static void C(bool ok,string message){if(!ok)throw new Exception(message);}
    static readonly double[][] Origin={new[]{0.0,0.0}};
    static void Main()
    {
        var vision=new VisionState{BaseLight=2};
        C(vision.SetLightGroup(5,Origin,20,40,2,-1,0),"second-level permanent object light can be created");
        C(vision.Light(20,0,0)==0&&vision.Light(40,0,0)==1&&vision.Light(41,0,0)==2,"permanent source still has exactly twenty bright and twenty additional dim feet");
        C(vision.Light(0,0,long.MaxValue)==0&&vision.Zones.Count==1&&vision.Zones[0].End==-1,"permanent illumination survives the largest supported world timestamp without arithmetic overflow");
        C(vision.SetLightGroup(5,new[]{new[]{80.0,0.0}},20,40,2,-1,long.MaxValue)&&vision.Light(0,0,long.MaxValue)==2&&vision.Light(80,0,long.MaxValue)==0,"an indefinite emitter can still move at maximal time without leaving a light trail");
        vision.ClearEffects();
        C(!vision.SetLightGroup(5,Origin,20,40,2,-2,0)&&!vision.SetLightGroup(5,Origin,20,40,2,long.MinValue,0)&&vision.Zones.Count==0,"only minus-one is a legal negative expiry sentinel");
        C(!vision.SetLightGroup(0,Origin,20,40,2,-1,0)&&vision.SetLightGroup(1,Origin,20,40,2,-1,0),"positive light-group identities remain valid while zero remains reserved");
        vision.ClearEffects();vision.SetLightGroup(5,Origin,20,40,2,-1,0);
        vision.AddZone(true,55,0,2,0);
        C(vision.HasDarkness(0)&&vision.ConsumeDispelledLightGroup(5)&&!vision.ConsumeDispelledLightGroup(5),"overlap with even the outer dim ring permanently dispels a level-two light and records one notification");
        C(vision.Light(0,0,0)==2&&!vision.SetLightGroup(5,Origin,20,40,2,-1,0),"an existing overlapping Darkness also blocks recreating the low-level emitter");
        vision.ClearEffects();vision.SetLightGroup(5,Origin,20,40,3,-1,0);vision.AddZone(true,0,0,2,0);
        C(vision.HasDarkness(0)&&!vision.ConsumeDispelledLightGroup(5)&&vision.Light(0,0,0)==0&&vision.Quality(-10,0,10,0,0,0)==0,"a level-three permanent source illuminates and reveals covered Darkness without dispelling that Darkness");
        vision.HideLightGroup(5);C(vision.HasDarkness(0)&&vision.Light(0,0,0)==2&&!vision.ConsumeDispelledLightGroup(5),"covering an upcast source suppresses light rather than removing Darkness or marking the source dispelled");
        C(vision.SetLightGroup(5,Origin,20,40,3,-1,0)&&vision.HasDarkness(0)&&vision.Light(0,0,0)==0,"uncovering an upcast attachment resumes indefinite illumination within existing Darkness");
        vision.ClearEffects();vision.SetLightGroup(5,Origin,20,40,2,-1,0);vision.HideLightGroup(5);vision.AddZone(true,0,0,2,0);
        C(!vision.ConsumeDispelledLightGroup(5)&&vision.HasDarkness(0),"a completely covered low-level source has no overlapping light area when Darkness appears");
        C(!vision.SetLightGroup(5,Origin,20,40,2,-1,0),"uncovering that low-level source inside Darkness still invokes ordinary dispelling rules");
        vision.ClearEffects();vision.SetLightGroup(5,Origin,20,40,2,-1,0);vision.HideLightGroup(5);vision.AddZone(true,0,0,2,0);vision.EndDarkness();
        C(vision.SetLightGroup(5,Origin,20,40,2,-1,0)&&vision.Light(0,0,0)==0,"a covered source survives and can be uncovered after Darkness ends");
        vision.ClearEffects();vision.SetLightGroup(5,Origin,20,40,3,-1,0);vision.AddZone(true,0,0,2,0);vision.AddZone(false,0,0,3,0);
        C(!vision.HasDarkness(0)&&vision.Zones.Exists(z=>z.LightGroup==5&&z.End==-1),"Daylight retains its own Darkness dispelling and does not remove an independent permanent source");
        vision.ClearEffects();vision.SetLightGroup(5,Origin,20,40,2,-1,0);vision.SetLightGroup(6,new[]{new[]{100.0,0.0}},20,40,2,10,0);
        C(vision.Light(100,0,9)==0&&vision.Light(100,0,10)==2&&vision.Light(0,0,10)==0,"ordinary time-limited sources still expire at their boundary without affecting persistent sources");
        C(vision.SetLightGroup(6,new[]{new[]{100.0,0.0}},20,40,2,long.MaxValue,long.MaxValue-1)&&vision.Light(100,0,long.MaxValue)==2&&vision.Light(0,0,long.MaxValue)==0,"a finite maximal expiry is distinct from until-dispelled duration");
        vision.ClearLightGroup(5);C(vision.Zones.Count==0&&vision.Light(0,0,long.MaxValue)==2,"explicit source removal still works for a permanent group");
        vision.SetDimLightGroup(5,Origin,10,3,-1,long.MaxValue);C(vision.Light(0,0,long.MaxValue)==1,"the dim-light wrapper supports the same explicit indefinite duration");
        vision.ClearEffects();C(vision.Zones.Count==0&&!vision.ConsumeDispelledLightGroup(5),"full battle reset removes all perpetual and finite light state");
        Console.WriteLine("PASS: indefinite magical light, exact radii, Darkness level interactions, covered-source restoration, Daylight independence, finite expiry, maximal timestamps and reset");
    }
}
