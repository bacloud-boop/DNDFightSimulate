using System;
class SensoryAfflictionChecks
{
    static void Check(bool value,string message){if(!value)throw new Exception(message);}
    static void Main()
    {
        var effects=new SensoryAfflictions();
        var old=effects.Add(false,true,18,0);var low=effects.Add(false,false,12,1);
        Check(effects.Blinded(false,1)&&effects.Current(false,1).Id==old.Id,"higher DC suppresses newer mode");
        var latest=effects.Add(false,false,18,2);
        Check(effects.Deafened(false,2)&&effects.Current(false,2).Id==latest.Id,"equal DC newest wins");
        Check(effects.Pending(false,2).Count==3,"suppressed effects still repeat saves");
        var snapshot=effects.Pending(false,2);Check(effects.End(old.Id)&&snapshot.Count==3&&effects.Pending(false,2).Count==2,"independent saves and stable pending snapshot");
        Check(effects.End(latest.Id)&&effects.Current(false,2).Id==low.Id&&effects.Deafened(false,2),"old unsaved effect resumes");
        Check(effects.Current(false,11)==null,"independent expiry");
        var timed=new SensoryAfflictions();timed.Add(false,true,20,0);var suppressed=timed.Add(false,false,12,5);
        Check(timed.Blinded(false,9)&&timed.Current(false,10).Id==suppressed.Id&&timed.Deafened(false,10),"expired stronger effect restores remaining weaker duration");
        var player=effects.Add(true,true,20,20);var monster=effects.Add(false,false,15,20);
        Check(!effects.Blinded(true,19)&&effects.Blinded(true,20)&&effects.Deafened(false,20),"actor isolation and start time");
        Check(effects.Blinded(true,29)&&!effects.Blinded(true,30),"one-minute boundary");
        Check(effects.Add(false,true,20,long.MaxValue)==null&&!effects.CanAdd(-1,1)&&!effects.CanAdd(0,0),"overflow and invalid time");
        Check(SensoryAfflictions.BlocksSight(true,0,0)&&SensoryAfflictions.BlocksSight(true,10,15),"blindness blocks normal sight");
        Check(!SensoryAfflictions.BlocksSight(true,10,10)&&!SensoryAfflictions.BlocksSight(false,0,100),"blindsight exception and normal vision");
        effects.Clear();Check(effects.Pending(true,20).Count==0&&effects.Current(false,20)==null,"battle reset");
        Console.WriteLine("PASS: DC precedence, newest tie, independent saves, snapshots, resume, actors, expiry and blindsight");
    }
}
