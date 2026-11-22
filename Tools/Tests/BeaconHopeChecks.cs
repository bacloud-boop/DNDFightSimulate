using System;
using System.Linq;
class BeaconHopeChecks
{
    static void C(bool ok,string text){if(!ok)throw new Exception(text);}
    static void Main()
    {
        var beacon=new BeaconHopeEffect();
        C(BeaconHopeEffect.Duration==10&&BeaconHopeEffect.RadiusFeet==30,"one-minute initial thirty-foot selection");
        C(!beacon.Active(0,0),"uncreated beacon grants no benefits");
        C(beacon.Begin(new[]{0,-1,1,int.MinValue},0)&&beacon.Expires==10&&beacon.Revision==1,"all supported creature identities can be selected together");
        C(beacon.Active(0,0)&&beacon.Active(-1,9)&&beacon.Active(1,9)&&beacon.Active(int.MinValue,9)&&!beacon.Active(2,1),"only selected creatures receive benefits");
        C(!beacon.Active(0,-1)&&!beacon.Active(0,10)&&!beacon.Active(0,long.MaxValue),"exact exclusive expiry and invalid time queries");
        int sharedRoll=5;
        C(BeaconHopeEffect.ResolveHealing(beacon.Active(0,1),sharedRoll,12)==12&&BeaconHopeEffect.ResolveHealing(beacon.Active(2,1),sharedRoll,12)==sharedRoll,"shared group dice can be maximized per target without modifying another target's roll");
        C(BeaconHopeEffect.ResolveHealing(true,2,7)==7&&BeaconHopeEffect.ResolveHealing(true,-3,-1)==0&&BeaconHopeEffect.ResolveHealing(false,-3,7)==0,"negative modifiers are part of the maximum and healing floors at zero");
        C(BeaconHopeEffect.ResolveHealing(true,1,(long)int.MaxValue+1)==int.MaxValue&&BeaconHopeEffect.ResolveHealing(true,1,long.MaxValue)==int.MaxValue&&BeaconHopeEffect.ResolveHealing(true,1,long.MinValue)==0,"long maxima clamp safely without integer overflow");
        C(BeaconHopeEffect.ResolveHealing(false,3,long.MaxValue)==3,"unprotected target keeps its roll even when another target's maximum is large");
        C(BeaconHopeEffect.ResolveHealing(true,4)==4&&BeaconHopeEffect.ResolveHealing(false,4)==4&&BeaconHopeEffect.ResolveHealing(true,-4)==0,"fixed healing and Life Transference amounts stay fixed when no dice maximum is supplied");
        long revision=beacon.Revision;
        C(!beacon.Begin(new[]{2,2},5)&&!beacon.Begin(null,5)&&!beacon.Begin(new[]{2},-1)&&!beacon.Begin(new[]{2},long.MaxValue-9),"duplicate, null, negative and overflowing casts reject atomically");
        C(beacon.Revision==revision&&beacon.Expires==10&&beacon.Active(0,9)&&!beacon.Active(2,9),"invalid recasts preserve old targets and their expiry");
        C(beacon.Begin(Enumerable.Range(1,100),5)&&beacon.Revision==revision+1&&!beacon.Active(0,5)&&beacon.Active(100,14)&&!beacon.Active(1,4)&&!beacon.Active(1,15),"arbitrary target count is allowed and a new cast replaces the previous list and timer");
        C(beacon.Begin(new int[0],10)&&!beacon.Active(1,10)&&beacon.Expires==20,"an optional empty selection still replaces the previous casting");
        C(beacon.Begin(new[]{0},long.MaxValue-10)&&beacon.Active(0,long.MaxValue-1)&&!beacon.Active(0,long.MaxValue),"last full minute fits without world-time overflow");
        revision=beacon.Revision;beacon.Clear();
        C(beacon.Revision==revision+1&&beacon.Expires==0&&!beacon.Active(0,long.MaxValue-1),"concentration ending clears every target and invalidates the casting revision");
        C(beacon.Begin(new[]{-1},0)&&beacon.Active(-1,0)&&!beacon.Active(-1,10),"new battle can restart at time zero");
        Console.WriteLine("PASS: Beacon of Hope target isolation, unbounded optional selection, recast replacement, duration, atomic validation, per-target maximum healing, fixed transfers, integer boundaries and reset");
    }
}
