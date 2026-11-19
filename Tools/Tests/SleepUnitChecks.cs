using System;
class SleepUnitChecks
{
    static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
    static void Main()
    {
        var unit=new SummonedUnit(1,"Spirit",30,14,30,2,2);
        unit.BeginTurn(1);unit.SleepStage=1;
        Check(!unit.ActionAvailable&&!unit.Attack()&&!unit.Dodge()&&!unit.UseReaction(),"incapacitation blocks actions and reactions");
        Check(unit.SpendMovement(5)&&unit.MovementRemaining==25,"incapacitation still permits movement");
        unit.SleepStage=2;unit.Prone=true;
        Check(unit.EffectiveSpeed==0&&!unit.SpendMovement(1)&&!unit.Stand(),"unconscious creature cannot move or stand");
        unit.Damage(0);Check(unit.SleepStage==2,"zero damage does not wake target");
        unit.Damage(1);Check(unit.SleepStage==0&&unit.Prone&&unit.ActionAvailable,"damage wakes only affected creature and leaves it prone");
        Check(unit.Attack()&&unit.AttacksRemaining==1,"normal attack resources remain available after waking");
        unit.SleepStage=1;Check(!unit.Attack()&&unit.AttacksRemaining==1,"extra attacks cannot bypass incapacitation");
        var other=new SummonedUnit(2,"Other",10,10,30,2,1);other.SleepStage=2;unit.Damage(1);
        Check(other.SleepStage==2,"waking one target leaves another asleep");
        Console.WriteLine("PASS: incapacitated movement, blocked actions/reactions, unconscious speed, damage waking and multi-target independence");
    }
}
