using System;
using UnityEngine;
class HeroismChecks
{
    static void C(bool ok,string message){if(!ok)throw new Exception(message);}
    static void Main()
    {
        var effect=new HeroismEffect();C(effect.Begin(new[]{0,-1,12},3,3,10,50),"third-level spell accepts three willing creatures");
        C(effect.Active(0,10)&&effect.Active(-1,19)&&!effect.Active(9,10)&&!effect.Active(0,20),"immunity is immediate, target-specific and lasts one minute");
        C(!effect.Claim(0,50,10)&&effect.Claim(-1,51,11)&&!effect.Claim(-1,51,11),"no temporary HP on casting; each later own-turn callback grants once");
        C(effect.Claim(12,52,11)&&effect.Claim(0,53,11)&&effect.Claim(-1,55,12),"summon and player turns are independent, including passive turns");
        long revision=effect.Revision;C(!effect.Begin(new[]{0,-1},5,1,11,54)&&effect.Revision==revision&&effect.Amount==3,"invalid cast is atomic and cannot replace existing buff");
        C(!effect.Begin(new[]{0},2,1,long.MaxValue,1),"duration overflow rejected");
        var spell=DirectSpell.Find("Heroism");C(spell!=null&&spell.Level==1&&spell.Range==5&&!spell.Bonus&&!spell.Attack&&!spell.Control,"Heroism uses touch, a magic action and its own concentration rules");
        var combat=new CombatState();combat.ApplyPlayerDamage(3);int hp=combat.PlayerHp;combat.ChooseTemporaryHp(true,3,true);C(combat.PlayerHp==hp&&combat.PlayerTemporaryHp==3,"temporary HP never heals actual HP");
        combat.ApplyPlayerDamage(2);C(combat.PlayerHp==hp&&combat.PlayerTemporaryHp==1,"damage depletes temporary HP before actual HP");
        combat.ChooseTemporaryHp(true,3,true);C(combat.PlayerTemporaryHp==3,"next grant refreshes rather than adds");
        combat.ChooseTemporaryHp(true,8,true);combat.ChooseTemporaryHp(true,3,false);C(combat.PlayerTemporaryHp==8,"player may keep an existing larger pool");combat.ChooseTemporaryHp(true,3,true);C(combat.PlayerTemporaryHp==3,"player may deliberately choose the smaller replacement");
        var unit=new SummonedUnit(12,"willing steed",20,12,60,3,1);unit.ChooseTemporaryHp(3,true);int before=unit.Hp;C(unit.Damage(2)==0&&unit.Hp==before&&unit.TemporaryHp==1,"summon HP-loss return value excludes absorbed damage");
        unit.ChooseTemporaryHp(5,true);unit.Stoneskin=true;unit.Damage(8,2);C(unit.TemporaryHp==1&&unit.Hp==before,"damage resistance resolves before temporary HP absorption");
        unit.SleepStage=2;unit.Damage(1,5);C(unit.SleepStage==0&&unit.Hp==before,"absorbed damage still wakes a sleeping creature");
        unit.ChooseTemporaryHp(3,true);unit.ChooseTemporaryHp(9,false);C(unit.TemporaryHp==3,"summon replacement respects explicit player choice");
        effect.Clear();C(!effect.Active(12,12)&&unit.TemporaryHp==3,"concentration loss ends immunity and future grants while existing temporary HP remains");
        unit.Dismiss();C(unit.TemporaryHp==0,"dismissal removes the summon pool");
        var rescue=new RescueCarrier();rescue.ChooseTemporaryHp(4,true);rescue.TakeDamage(3);C(rescue.Hp==10&&rescue.TemporaryHp==1,"rescuer uses the same absorption order");rescue.TakeDamage(11);C(rescue.Hp==0&&!rescue.Dead,"rescue unconscious fixture");
        rescue.ChooseTemporaryHp(5,true);rescue.TakeDamage(1);C(rescue.Hp==0&&rescue.TemporaryHp==4&&rescue.DeathFailures==1,"temporary HP cannot wake or exempt a zero-HP rescuer from damage death-save failures");
        rescue.FeignDeath=true;rescue.TakeDamage(1,damageType:0);C(rescue.DeathFailures==1,"damage reduced to zero by resistance does not add a death-save failure");
        C(effect.Begin(new[]{0},-3,1,20,70)&&effect.Amount==0&&effect.Active(0,20),"negative casting modifier grants no negative HP but preserves frightened immunity");
        effect.Clear();C(!effect.Claim(0,71,20),"reset invalidates pending grant eligibility");
        Console.WriteLine("PASS: Heroism willing-target limits, delayed own-turn grants, duration, immunity, nonstacking choices, persistent temporary HP, summon damage/resistance and unconscious rescue handling");
    }
}
