using System;
using System.Linq;
using UnityEngine;
class AnimatedUndeadChecks
{
    static void C(bool v,string m){if(!v)throw new Exception(m);}
    static void Main()
    {
        var skeleton=new AnimatedUndeadRules(true);var zombie=new AnimatedUndeadRules(false);
        C(skeleton.Armor==14&&skeleton.MaxHp==13&&skeleton.Ability(1)==16&&zombie.Armor==8&&zombie.MaxHp==15&&zombie.Save(3)==0,"2024 profiles and zombie Wisdom proficiency, not legacy statistics");
        var s=skeleton.Create(1);int events=0;long[] packet=null;s.DamageApplied=p=>{events++;packet=p;};s.Damage(9,9);C(s.Hp==13&&events==0,"poison immunity causes neither damage nor a transfer event");
        s.Stoneskin=true;s.Damage(7,0);C(s.Hp==7&&events==1&&packet[0]==6,"resistance rounds before bludgeoning vulnerability and transfers resolved types");
        var mixed=skeleton.Create(2);var raw=new long[13];raw[0]=3;raw[9]=100;raw[11]=1;mixed.Damage(104,resolvedTypes:raw);C(mixed.Hp==6&&raw[0]==3&&raw[9]==100,"typed intrinsic mitigation is atomic and never mutates the caller packet");
        var overflow=skeleton.Create(3);raw[0]=long.MaxValue;overflow.Damage(int.MaxValue,resolvedTypes:raw);C(!overflow.Alive,"vulnerability cannot overflow into healing");
        var z=zombie.Create(4);int saves=0;z.LethalGuard=(amount,types,critical)=>{if(!zombie.FortitudeEligible(amount,types,critical))return false;saves++;return true;};
        z.Damage(15,0);C(z.Hp==1&&saves==1,"successful Fortitude retains one HP before ending turn or dismissing a unit");
        z.Damage(1,11);C(!z.Alive&&saves==1,"radiant damage bypasses Fortitude");
        var criticalZombie=zombie.Create(20);criticalZombie.LethalGuard=(amount,types,critical)=>zombie.FortitudeEligible(amount,types,critical);criticalZombie.Damage(15,0,critical:true);C(!criticalZombie.Alive,"a real critical damage event bypasses the live Fortitude callback");
        var protectedZombie=zombie.Create(21);protectedZombie.ChooseTemporaryHp(10,true);int lastDamage=0;protectedZombie.LethalGuard=(amount,types,critical)=>{lastDamage=amount;return true;};protectedZombie.Damage(24,0);C(protectedZombie.Hp==1&&lastDamage==0,"temporary HP prevents an unnecessary Fortitude save");protectedZombie.Damage(1,0);C(protectedZombie.Hp==1&&lastDamage==1,"Fortitude uses the current damage event after temporary HP is gone");
        z=zombie.Create(5);z.LethalGuard=(amount,types,critical)=>{saves++;return true;};
        // The actual runtime guard checks this predicate before rolling.
        C(!zombie.FortitudeEligible(20,new long[13],true)&&!skeleton.FortitudeEligible(20,new long[13],false)&&AnimatedUndeadRules.FortitudeDc(int.MaxValue)==2147483652L,"critical hits and skeletons cannot use Fortitude; DC is overflow safe");
        var control=new AnimateDeadControl();control.Register(new[]{5},3,0);z.ControllerAllows=()=>control.Controlled(5,14400);z.BeginTurn(1);bool changed;z.ReceiveVerbalOrders(SummonCommand.AssistCombat,out changed);
        C(z.Alive&&!z.ActionAvailable&&z.MovementRemaining==0&&!z.Attack()&&!z.Dash()&&!z.UseReaction(),"expired neutral undead retains HP but cannot move, attack, Dash, or react");
        control.Reassert(new[]{5},3,14400,id=>true);C(z.ActionAvailable&&z.MovementRemaining==20,"reassertion re-enables the same existing unit without healing or respawning it");
        var map=new BattleMap();map.Lock();var unit=zombie.Create(10);
        C(map.CanAnimateBody(unit,-1)&&map.BodyPresent(-1)&&map.Summons.Count==0,"placement preview does not consume or spawn anything");
        C(map.AnimateBody(unit,-1)&&!map.BodyPresent(-1)&&map.OccupiedSpaces(false)==0&&map.Summons.Count==1,"corpse is atomically replaced by one independent unit at the same position");
        C(!map.AnimateBody(zombie.Create(11),-1)&&map.Summons.Count==1,"consumed corpse cannot be animated twice");
        C(!map.CanRestoreBodyAt(-1,map.Monster,1)&&!map.BodyPresent(-1),"the old position remains occupied by the independent undead, without duplicating the old body");
        C(map.CanRestoreBodyAt(-1,new Vector2Int(8,8),1)&&!map.BodyPresent(-1),"replacement-body preview preserves consumed state");
        C(map.RestoreBodyAt(-1,new Vector2Int(8,8),1)&&map.BodyPresent(-1)&&unit.Alive&&unit.X==9&&unit.Y==9,"new body appears in another legal space and does not destroy or move the undead");
        var old=map.Remains.Add(0,2,2,1,"old body",new BodyWeight(),0,false);C(map.AnimateBody(skeleton.Create(12),old.Id)&&map.RemainsSpaces(old)==0&&!map.CanAttachRescueRemains(old.Id),"consumed archived body no longer blocks, covers or offers a second corpse for carrying");
        var inventory=new WeaponInventory(Weapon.Available.Select(w=>w.Name),"长剑（双手）");inventory.BeginBattle();var item=inventory.Find("长剑（单手）");var ground=item.Ground;
        inventory.GiveToFollower(item.Key,10);C(!inventory.Carried(item.Key)&&inventory.CarriedOunces==0&&ReferenceEquals(ground,item.Ground),"handed sword variants remain one physical item with one weight");
        C(inventory.FollowerDrop(item.Key,10,4,4,true)&&item.Ground.Submerged&&!inventory.PickUp(item.Key,x=>true,()=>true,true),"dropped follower weapon sinks and cannot also be retrieved through the original inventory owner");
        C(!inventory.FollowerPickUp(item.Key,10,x=>true,()=>true,false)&&inventory.FollowerPickUp(item.Key,10,x=>true,()=>true,true),"follower must dive before retrieving its sunken weapon");
        inventory.FollowerRemains(10,-99);C(item.Location==WeaponLocation.Remains&&item.RemainsId==-99&&item.FollowerId==0,"stored gear transfers to the new independent corpse exactly once");
        inventory.ResetBattle();C(inventory.Carried(item.Key)&&item.FollowerId==0&&inventory.CarriedOunces==48,"reset restores preparation inventory without duplicating the transferred weapon");
        map.Unlock();C(map.BodyPresent(old.Id)&&map.Summons.Count==0,"map reset clears consumed-body flags and live followers");
        Console.WriteLine("PASS: 2024 undead profiles, intrinsic typed mitigation, Fortitude, neutral action gates, consumed/replacement bodies, shared physical equipment, underwater pickup and reset");
    }
}
