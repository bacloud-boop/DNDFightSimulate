using System;
class WeaponGripChecks {
 static void Check(bool v){if(!v)throw new Exception("Grip regression");}
 static void Main(){var grip=new WeaponGripState();int paid=0;Func<bool> spend=()=>{paid++;return true;};Check(grip.TrySetStowed(true,1,0,spend)&&paid==0);Check(grip.TrySetStowed(false,1,0,spend)&&paid==1);Check(!grip.TrySetStowed(true,1,0,()=>false)&&!grip.Stowed);grip.BeginAttack(1,0);Check(grip.TrySetStowed(true,1,0,spend)&&paid==1);grip.BeginAttack(1,0);Check(!grip.Stowed);Check(grip.TrySetStowed(true,2,1,spend)&&paid==1);grip.Reset();Check(!grip.Stowed);Console.WriteLine("PASS: free interaction, paid extra operation, failure atomicity, attack equip allowance and reset");}
}
