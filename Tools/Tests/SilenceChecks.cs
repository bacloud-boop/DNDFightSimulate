using System;
class SilenceChecks
{
    static void Check(bool value,string message){if(!value)throw new Exception(message);}
    static void Main()
    {
        var silence=new SilenceEffect();Check(!silence.Active(0)&&silence.Begin(0,0,10),"begin");
        Check(!silence.Active(9)&&silence.Active(10)&&silence.Active(109)&&!silence.Active(110),"ten-minute boundary");
        Check(silence.CentreInside(3,0,2,10)&&!silence.WholeInside(3,0,2,10),"centre silent but not fully deaf or immune");
        Check(!silence.CentreInside(4,0,2,10)&&silence.Contains(4,0,10),"partial entry with outside centre may speak");
        Check(silence.WholeInside(2,0,2,10)&&!silence.WholeInside(0,0,4,10),"all footprint corners required");
        Check(silence.BlocksSound(-5,0,5,0,10)&&silence.BlocksSound(-5,4,5,4,10),"crossing and tangent silence blocks sound");
        Check(!silence.BlocksSound(-5,5,5,5,10)&&!silence.BlocksSound(-5,0,5,0,110),"outside and expired sound path");
        Check(silence.CanSeeOrHear(false,-5,5,5,5,10)&&!silence.CanSeeOrHear(false,-5,0,5,0,10),"unseen targets are audible unless sound is blocked");
        Check(silence.CanSeeOrHear(true,-5,0,0,0,10),"visible deafened target remains a legal mockery target");
        Check(!silence.Begin(3,3,long.MaxValue)&&silence.X==0&&silence.Expires==110,"overflow preserves state");
        var combat=new CombatState();int x=0;long now=10;
        combat.DamageImmunity=(player,type)=>type==12&&silence.WholeInside(x,0,1,now);
        Check(combat.AdjustTypedDamage(true,20,12)==0&&combat.AdjustTypedDamage(false,20,5)==20,"only thunder immune");
        var packet=new long[13];packet[12]=20;packet[5]=8;combat.PlayerEnergyResistance=5;
        Check(combat.AdjustPlayerPacket(packet)==4,"mixed packet immunity plus fire resistance");
        x=5;Check(combat.AdjustTypedDamage(true,20,12)==20&&combat.AdjustPlayerPacket(packet)==24,"movement immediately updates immunity");
        x=0;now=110;Check(combat.AdjustTypedDamage(true,20,12)==20,"expiration immediately updates immunity");
        silence.Clear();Check(!silence.Contains(0,0,10)&&!silence.BlocksSound(-5,0,5,0,10),"concentration clear");
        Console.WriteLine("PASS: centre vs whole body, sound paths, duration, overflow, typed packets, movement and expiry");
    }
}
