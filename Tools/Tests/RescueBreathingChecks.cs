using System;
class RescueBreathingChecks
{
    static void C(bool ok,string message){if(!ok)throw new Exception(message);}
    static void Main()
    {
        var rescue=new RescueCarrier();rescue.Configure(10,2,30,150,0);rescue.BeginTurn(1);
        rescue.Breathing.Observe(false,0,rescue.Ability(2));C(rescue.Breathing.BreathEndsAt==10,"constitution ten holds breath for one minute");
        C(!rescue.Breathing.EndTurn(1,9)&&rescue.Breathing.EndTurn(2,10),"suffocation starts only after hold expires");
        C(!rescue.Breathing.EndTurn(2,11)&&rescue.Breathing.SuffocationExhaustion==1,"duplicate same-turn callbacks cannot double exhaustion");
        C(rescue.EffectiveSpeed==25&&rescue.DexteritySaveModifier==-2,"exhaustion reduces rescue movement and dexterity saves");
        rescue.TakeDamage(10);C(rescue.ResolveDeathSave(12,1)&&rescue.DeathSuccesses==1,"death save includes suffocation D20 penalty");
        rescue.ResolveDeathSave(11,2);C(rescue.DeathFailures==1,"lower death-save roll fails under exhaustion");rescue.ResolveDeathSave(20,3);C(rescue.Hp==1,"natural twenty still restores one HP");
        rescue.Breathing.Observe(true,11,rescue.Ability(2));C(rescue.EffectiveSpeed==30&&rescue.DexteritySaveModifier==0,"breathing normally removes nonfatal suffocation penalties");
        rescue.Reset();rescue.BeginTurn(2);int damageEvents=0;rescue.DamageApplied=(amount,type)=>damageEvents++;rescue.ChooseTemporaryHp(20,true);
        rescue.Breathing.Observe(false,0,10,true);for(int i=1;i<=6;i++)rescue.Breathing.EndTurn(i,0);
        C(rescue.ApplyFatalSuffocation()&&rescue.Dead&&rescue.Hp==0&&rescue.TemporaryHp==0&&!rescue.TurnActive&&!rescue.ReactionAvailable,"level six directly kills, ignoring temporary HP and ending resources");
        C(damageEvents==0&&!rescue.ApplyFatalSuffocation(),"suffocation death is not damage or repeated death");rescue.ObserveLife(10);
        C(rescue.Revive(1,true)&&!rescue.ReconcileRevivalBreathing(false)&&rescue.Dead&&rescue.Breathing.SuffocationExhaustion==6,"revival underwater preserves fatal exhaustion and immediately kills again");
        rescue.ObserveLife(12);C(rescue.Corpse.Age(12)==0,"immediate re-death starts a fresh corpse clock");
        C(rescue.Revive(1,false)&&rescue.ReconcileRevivalBreathing(true)&&rescue.Conscious&&rescue.Breathing.CanBreathe&&rescue.Breathing.D20Penalty==0,"moving to breathable environment before revival clears lethal suffocation");
        C(!rescue.TurnActive&&rescue.BeginTurn(3),"revival does not grant a turn before the next own turn");rescue.Reset();C(rescue.Breathing.CanBreathe&&!rescue.Breathing.Fatal&&rescue.Breathing.SuffocationExhaustion==0,"reset removes all rescue breathing state");
        Console.WriteLine("PASS: rescue breath hold, exact turn identity, exhaustion movement/saves, direct fatality, damage isolation and breathable versus submerged revival");
    }
}
