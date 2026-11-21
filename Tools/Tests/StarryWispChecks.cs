using System;
class StarryWispChecks
{
    static void C(bool ok,string message){if(!ok)throw new Exception(message);}
    static void Main()
    {
        var spell=DirectSpell.Find("Starry Wisp");C(spell!=null&&spell.Attack&&!spell.Melee&&spell.Level==0&&spell.Range==60&&spell.DamageType=="光耀"&&spell.Sides==8,"ranged radiant cantrip definition");
        foreach(int level in new[]{1,4,5,10,11,16,17,20})C(spell.DiceCount(level,0)==(level>=17?4:level>=11?3:level>=5?2:1),"total-level cantrip scaling");
        var mark=new StarryWispEffect();C(mark.Bind(0,-1,10),"successful hit fixture");int group=mark.Active(10)[0].Group;
        mark.EndTurn(-1);C(mark.Suppresses(-1,10),"target ending its turn does not end the caster's mark");
        mark.EndTurn(0);C(mark.Suppresses(-1,11),"mark survives the casting turn's end");
        mark.EndTurn(0);C(!mark.Suppresses(-1,11),"expires at the end of the caster's next turn");
        C(mark.Bind(0,0,20)&&mark.Bind(-1,0,20),"independent casters can hit one creature");
        mark.EndTurn(0);mark.EndTurn(0);C(mark.Suppresses(0,20)&&mark.Active(20).Count==1,"one caster's expiry does not erase another caster's mark");
        mark.Clear();C(mark.Bind(0,-1,30),"replacement fixture");mark.EndTurn(0);C(mark.Bind(0,-1,31)&&mark.Active(31).Count==1,"another hit refreshes one caster's effect without multiplying light zones");
        mark.EndTurn(0);C(mark.Suppresses(-1,31),"refresh survives that casting turn's end");mark.EndTurn(0);C(!mark.Suppresses(-1,31),"refreshed mark expires on the new next-turn boundary");
        mark.Clear();mark.Bind(0,-1,40);C(!mark.Bind(0,-1,long.MaxValue)&&mark.Suppresses(-1,40),"overflow rejects atomically");C(!mark.Suppresses(-1,42)&&mark.Active(42).Count==0,"world-time waiting clears stale turn effects");
        mark.Bind(0,-1,50);var entry=mark.Active(50)[0];var vision=new VisionState{BaseLight=2};
        C(vision.SetDimLightGroup(entry.Group,new[]{new[]{0.0,0.0}},10,0,entry.EndWorld,50),"light originates on the hit target");
        C(vision.Light(0,0,50)==1&&vision.Light(10,0,50)==1&&vision.Light(11,0,50)==2,"ten-foot dim radius, without bright light");
        C(vision.SetDimLightGroup(entry.Group,new[]{new[]{40.0,0.0}},10,0,entry.EndWorld,50)&&vision.Light(0,0,50)==2&&vision.Light(40,0,50)==1,"moving a marked creature moves its light without a trail");
        var senses=new SenseEffects();senses.NormalInvisibility(false,50);C(senses.Invisible(false,50)&&mark.Suppresses(-1,50),"suppression retains the underlying invisibility source");
        mark.EndTurn(0);mark.EndTurn(0);C(senses.Invisible(false,51)&&!mark.Suppresses(-1,51),"invisibility benefits return after the mark, without recasting");
        mark.Clear();mark.Bind(0,-1,60);entry=mark.Active(60)[0];vision.ClearEffects();vision.SetDimLightGroup(entry.Group,new[]{new[]{0.0,0.0}},10,0,entry.EndWorld,60);vision.AddZone(true,24,0,2,60);
        C(vision.ConsumeDispelledLightGroup(entry.Group),"overlap of the outer dim radius lets Darkness dispel the low-level light spell");mark.Remove(entry.Group);C(!mark.Suppresses(-1,60),"dispel ends the entire lingering hit effect");
        mark.Bind(0,12,61);C(mark.Suppresses(12,61),"summon actors use the same status model");mark.Clear();C(mark.Active(61).Count==0,"battle reset removes all lights and marks");
        C(Array.IndexOf(MonsterSpellSequence.Supported,"Starry Wisp")>=0&&MonsterSpellSequence.BaseLevel("Starry Wisp")==0,"monster cycling supports cantrip configuration");
        Console.WriteLine("PASS: Starry Wisp scaling, caster turn expiry, refresh, independent sources, dim light movement, invisibility suppression, Darkness dispel and monster configuration");
    }
}
