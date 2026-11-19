using System;
class SteedRulesChecks
{
    static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
    static void Main()
    {
        var steed=new OtherworldlySteedRules(2,SteedKind.Celestial);
        Check(steed.Armor==12&&steed.MaxHp==25&&steed.Speed==60,"second-level statistics");
        Check(steed.CreateUnit(1).Spaces==2&&OtherworldlySteedRules.SharedSpaces(2,true,1)==2,"rider and steed share full two-by-two footprint");
        Check(!OtherworldlySteedRules.CanAttack(true,false)&&OtherworldlySteedRules.CanAttack(true,true),"controlled versus independent actions");
        Check(!steed.TryUseSpecial(()=>false)&&steed.SpecialAvailable,"failed special spends no use");
        Check(steed.TryUseSpecial(()=>true)&&!steed.TryUseSpecial(()=>true),"one special per long rest");
        steed.LongRest();Check(steed.SpecialAvailable,"long rest restores special");
        Check(steed.Ability(3)==12&&steed.Ability(4)==6,"simulator wisdom-before-intelligence order");
        Console.WriteLine("PASS: steed statistics, shared footprint, control and special resource atomicity");
    }
}
