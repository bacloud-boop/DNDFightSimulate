using System;
class ProduceFlameChecks
{
    static void C(bool value,string text){if(!value)throw new Exception(text);}
    static void Main()
    {
        var flame=new ProduceFlameEffect();var combat=new CombatState();
        C(combat.TryUseBonusAction()&&flame.Begin(11,0)&&!combat.BonusActionAvailable,"creation consumes bonus action");
        C(combat.TryUseMagicAction()&&!combat.TryUseMagicAction()&&flame.Active(0),"hurl consumes one magic action and retains flame");
        combat.EndRound(false);C(combat.TryUseMagicAction()&&flame.Active(1),"new turn permits another hurl without recasting");
        C(flame.Active(99)&&!flame.Active(100)&&!flame.Active(-1),"exact ten-minute duration");
        C(!flame.Begin(11,long.MaxValue-99)&&!flame.Begin(-1,0)&&flame.Caster==11,"invalid creation is atomic");
        C(flame.Begin(11,50)&&flame.Expires==150&&flame.Active(149),"recasting renews finite effect");
        int[] levels={1,4,5,10,11,16,17,20},dice={1,1,2,2,3,3,4,4};for(int i=0;i<levels.Length;i++)C(ProduceFlameEffect.DamageDice(levels[i])==dice[i],"cantrip progression");
        flame.Clear();C(!flame.Active(50),"reset and recast cancellation remove flame");
        var v=new VisionState{BaseLight=2};var origin=new[]{new[]{0d,0d}};var far=new[]{new[]{200d,0d}};
        C(v.SetLightGroup(-100,origin,20,40,2,-1,0)&&v.SetDimLightGroup(100,far,10,0,2,0)&&v.Zones.Count==2,"permanent and Starry Wisp identifiers cannot collide");
        v.AddZone(true,0,0,2,0);C(v.ConsumeDispelledLightGroup(-100)&&!v.ConsumeDispelledLightGroup(100)&&v.Light(200,0,0)==1,"Darkness notifies only affected namespace");
        v.ClearEffects();C(v.SetLightGroup(-5,far,20,40,3,-1,0)&&v.SetLightGroup(5,origin,20,40,0,100,0),"hand flame and permanent light group five are distinct");
        v.HideLightGroup(5);C(v.Light(0,0,0)==2&&v.Light(200,0,0)==0,"contained hand flame can be hidden without deleting permanent source");
        C(v.SetLightGroup(5,origin,20,40,0,100,1)&&v.Light(20,0,1)==0&&v.Light(40,0,1)==1,"released hand flame resumes exact bright and dim ranges");
        v.AddZone(true,0,0,2,1);C(v.ConsumeDispelledLightGroup(5)&&v.Light(200,0,1)==0,"Darkness ends cantrip light independently");
        Console.WriteLine("PASS: Produce Flame action resources, repeated hurls, duration/scaling/reset, disjoint light namespaces, containment hiding and Darkness isolation");
    }
}
