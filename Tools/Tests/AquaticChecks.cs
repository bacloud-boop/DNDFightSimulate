using System;
class AquaticChecks
{
 static void Check(bool v,string m){if(!v)throw new Exception(m);}
 static void Main(){
  var a=new AquaticState();Check(!a.TrySetSubmerged(true,3,4)&&!a.Submerged,"whole footprint required to dive");
  int budget=10;Func<int,bool> spend=cost=>{if(budget<cost)return false;budget-=cost;return true;};
  Check(!a.TrySwitch(true,3,4,spend)&&budget==10,"illegal dive does not charge movement");
  Check(a.TrySwitch(true,4,4,spend)&&budget==5&&!a.TrySwitch(true,4,4,spend)&&budget==5,"switch costs five, repeated state is not charged");
  Check(a.TrySwitch(false,4,4,spend)&&budget==0&&!a.TrySwitch(true,4,4,spend)&&!a.Submerged,"surfacing also costs five and insufficient movement preserves state");
  Check(a.CanBreathe(true,false,4)&&a.CanBreathe(false,true,4),"surface permits both air and water breathing");
  Check(a.TrySetSubmerged(true,4,4)&&!a.CanBreathe(true,false,4)&&a.CanBreathe(false,true,4),"underwater requires water breathing");
  Check(!a.TrySetSubmerged(true,3,4)&&a.Submerged,"failed state change is atomic");
  a.ObservePosition(3,4);Check(!a.Submerged,"leaving full water coverage removes stale dive state");
  Check(!a.CanBreathe(false,true,0)&&a.CanBreathe(true,false,0),"land requires air breathing");
  Check(a.CanBreathe(false,false,0,false),"nonbreathing creatures exempt");
  var ocean=new BestialSpiritRules(2,5).CreateWaterUnit(1);var breath=new BreathingState();
  Check(ocean.SwimSpeed==30&&!ocean.BreathesAir&&ocean.BreathesWater,"ocean form aquatic profile");
  breath.Observe(a.CanBreathe(ocean.BreathesAir,ocean.BreathesWater,0),0,16);
  Check(!breath.CanBreathe&&breath.BreathEndsAt==40,"ocean summon on land begins holding breath");
  breath.Observe(a.CanBreathe(ocean.BreathesAir,ocean.BreathesWater,1),2,16);Check(breath.CanBreathe,"ocean summon at water surface can breathe");
  Console.WriteLine("PASS: full footprint diving, surface breathing, ocean profile and land exposure");
 }
}
