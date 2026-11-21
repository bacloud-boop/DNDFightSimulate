using System;
class CloudDaggersChecks
{
    static void C(bool ok,string text){if(!ok)throw new Exception(text);}
    static void Main()
    {
        var cloud=new CloudOfDaggersEffect();cloud.Begin(3,4,2,7,10);C(cloud.Dice==4&&cloud.Active(19)&&!cloud.Active(20),"two-slot cloud lasts one minute, rather than one hour");
        C(cloud.Intersects(3,4,1,10)&&!cloud.Intersects(2,4,1,10)&&cloud.Intersects(2,3,2,10),"five-foot cube covers one cell and each intersecting multi-cell creature once");
        C(cloud.Claim(0,40,10)&&!cloud.Claim(0,40,10)&&!cloud.Available(0,40,10),"appearance, entry, moving into and turn end share one per-creature cap");
        C(cloud.Claim(-1,40,10)&&cloud.Claim(12,40,10)&&cloud.Claim(0,41,10),"other targets and other actors' turns have independent damage opportunities");
        C(!cloud.Move(5,4,7,10,10)&&cloud.X==3,"casting turn cannot move the cloud, even with another action");
        C(!cloud.Move(10,4,8,11,35)&&cloud.X==3&&cloud.Move(9,4,8,11,30),"later-turn teleport has an exact thirty-foot limit and rejects atomically");
        C(cloud.Intersects(9,4,1,11)&&!cloud.Intersects(6,4,1,11)&&!cloud.Intersects(3,4,1,11),"teleport damages its destination, without sweeping the path or leaving an old cloud");
        C(cloud.Claim(-1,42,11)&&!cloud.Claim(-1,42,11),"teleporting repeatedly cannot evade the same-turn cap");
        long token=cloud.Revision;cloud.Begin(1,1,9,9,12);C(cloud.Dice==18&&cloud.Revision!=token&&cloud.Available(-1,42,12),"upcasting adds two d4 per level and a new casting clears only its own damage history");
        try{cloud.Begin(0,0,1,1,13);throw new Exception("invalid slot accepted");}catch(ArgumentOutOfRangeException){}
        C(cloud.X==1&&cloud.Slot==9,"invalid cast does not overwrite a live effect");
        var spell=DirectSpell.Find("Cloud of Daggers");C(spell!=null&&spell.Level==2&&spell.Range==60&&!spell.Attack&&spell.SaveAbility<0&&spell.DiceCount(1,2)==4&&spell.DiceCount(20,9)==18,"spell uses dedicated no-attack, no-save damage, scaling with slot rather than character level");
        var caster=new CombatState();C(caster.TryUseMagicAction()&&!caster.TryUseMagicAction(),"casting and moving spend magic actions");caster.AddSurgeAction();C(!caster.MagicActionAvailable&&caster.ActionAvailable,"Action Surge's restricted action cannot move a spell area");
        caster.Aftermath=true;caster.AftermathPlayerTurn=false;C(!caster.TryUseAftermathMagicAction(),"caster cannot control the cloud on rescue's turn");
        var unit=new SummonedUnit(1,"paused spirit",30,12,30,2,1);unit.BeginTurn(1);unit.EndTurn();int savedHp=unit.Hp;C(!unit.ActionAvailable&&!unit.Attack()&&!unit.Dash(),"paused spirits never acquire active commands or resources");
        unit.Stoneskin=true;C(unit.Damage(12,2)==6&&unit.Hp==savedHp-6,"slashing resistance applies to ongoing cloud damage");
        var breathing=unit.Breathing;breathing.Observe(false,0,10,true);C(breathing.EndTurn(2,1)&&breathing.EndTurn(6,2)&&breathing.EndTurn(10,3)&&breathing.SuffocationExhaustion==3,"monotonic terrain-turn identity works through active, passive and resumed turns");
        int start=99;caster.TurnStarted=actor=>start=actor;caster.EndRound(false);C(start==99,"passive effects run before the new player start callback");
        cloud.Clear();C(!cloud.Active(12)&&!cloud.Available(0,43,12),"concentration loss or battle reset removes all damage eligibility");
        Console.WriteLine("PASS: Cloud of Daggers duration, slot growth, one-cell geometry, all-trigger damage cap, later-turn teleport, magic-action budget, typed damage and paused-unit safety");
    }
}
