using System;
class LaughterModelChecks
{
 static void C(bool ok,string why){if(!ok)throw new Exception(why);}
 static long[] Packet(){var p=new long[14];p[0]=2;p[5]=3;return p;}
 static void Main()
 {
  bool active=true;var combat=new CombatState();combat.ExtraIncapacitated=p=>active;combat.ExtraProne=p=>active;
  C(!combat.PlayerCanAct&&!combat.MonsterCanAct&&!combat.MagicActionAvailable&&!combat.BonusActionAvailable&&!combat.PlayerReactionAvailable&&!combat.MonsterReactionAvailable,"primary actions bonus reactions unavailable");
  C(combat.PlayerMovementReady&&combat.MonsterCanMove&&combat.PlayerProne&&combat.MonsterProne&&!combat.MonsterHelpless,"incapacity is not paralysis/unconsciousness; crawl remains legal");
  C(!combat.TryUseAction()&&!combat.TryUseDamageReaction()&&!combat.TryUseCompelledReaction(true),"resource methods enforce incapacity");
  active=false;C(combat.PlayerReactionAvailable&&combat.MonsterReactionAvailable&&combat.ActionAvailable,"unspent resources return after condition clears");active=true;combat.PlayerProne=true;active=false;C(combat.PlayerProne&&!combat.MonsterProne,"separate base prone state preserved");
  int events=0,total=0;combat.ConditionDamageReceived=(p,n)=>{events++;total+=n;};combat.GrantMonsterTemporaryHp(9);combat.ApplyDamage(5,resolvedTypes:Packet());C(events==1&&total==5&&combat.Hp==1000,"one mixed primary hit event including temp absorption");combat.ApplyDamage(0);C(events==1,"zero hit ignored");
  active=true;var u=new SummonedUnit(7,"ally",20,12,30,2,1);u.ExtraIncapacitated=()=>active;u.ExtraProne=()=>active;u.BeginTurn(1);C(u.Prone&&!u.Stand()&&!u.Attack()&&!u.TryUseBonus()&&!u.UseReaction()&&u.SpendMovement(5),"summon cannot stand/act/react but can move");active=false;C(u.ActionAvailable&&u.ReactionAvailable&&u.BonusAvailable,"summon unused resources preserved");events=0;u.ConditionDamageReceived=n=>events++;u.ChooseTemporaryHp(9,true);u.Damage(5,resolvedTypes:Packet(),prepared:true);C(events==1&&u.Hp==20,"summon damage callback counts packet not types");
  active=true;var r=new RescueCarrier();r.ExtraIncapacitated=()=>active;r.ExtraProne=()=>active;r.BeginTurn(1);C(!r.Stand()&&r.ActionsRemaining==0&&!r.TryUseBonusAction()&&!r.TryUseReaction()&&r.MovementReady,"rescuer restricted without immobilization");active=false;C(r.ActionsRemaining==1&&r.ReactionAvailable,"rescuer resources preserved");events=0;r.ConditionDamageReceived=n=>events++;r.ChooseTemporaryHp(9,true);r.TakeDamagePacket(Packet());C(events==1&&r.Hp==10,"rescuer mixed packet has one callback despite temp absorption");r.TakeDamage(0);C(events==1,"rescuer zero damage ignored");
  Console.WriteLine("PASS: actual combat, summon and rescuer models enforce incapacity, independent prone, retained movement/resources and atomic positive damage events");
 }
}
