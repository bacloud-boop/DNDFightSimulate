using System;
class StoneskinChecks
{
    static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
    static void Main()
    {
        var unit=new SummonedUnit(1,"Spirit",50,12,30,2,1);
        unit.Stoneskin=true;
        Check(unit.Damage(7,0)==3&&unit.Damage(7,1)==3&&unit.Damage(7,2)==3,"all three physical types round down");
        Check(unit.Damage(7,6)==7,"force damage remains unchanged");
        Check(unit.Damage(3)==3,"already resolved mixed packets are not halved twice");
        unit.SleepStage=2;unit.Damage(1,1);Check(unit.SleepStage==2,"zero post-resistance damage does not wake a target");
        unit.Stoneskin=false;Check(unit.Damage(7,1)==7&&unit.SleepStage==0,"ending effect restores full damage and normal waking");
        Console.WriteLine("PASS: physical types, integer rounding, other types, pre-resolved packets and effect removal");
    }
}
