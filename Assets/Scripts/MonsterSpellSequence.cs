using System;
using System.Collections.Generic;
public sealed class MonsterSpellEntry
{
    public string Name {get;}
    public int Slot {get;}
    public int CommandMode {get;}
    public MonsterSpellEntry(string name,int slot,int commandMode=0){Name=name;Slot=slot;CommandMode=commandMode;}
}
public sealed class MonsterSpellSequence
{
    public static readonly string[] Supported={"Vitriolic Sphere","Melf’s Acid Arrow","Dissonant Whispers","Cone of Cold","Fire Bolt","Sacred Flame","Poison Spray","Toll the Dead","Inflict Wounds","Cure Wounds","Healing Word","Magic Missile","Scorching Ray","Blight","Ice Storm","Flame Strike","Fire Storm","Conjure Barrage","Conjure Volley","Power Word Kill","Acid Splash","Fireball","Shatter","Circle of Death","Thunderclap","Word of Radiance","Burning Hands","Lightning Bolt","Color Spray","Starry Wisp","Command"};
    readonly List<MonsterSpellEntry> entries=new List<MonsterSpellEntry>();
    public IReadOnlyList<MonsterSpellEntry> Entries=>entries.AsReadOnly();
    public int Ability {get;private set;}=4;
    public int CasterLevel {get;private set;}=1;
    public bool Counterspell {get;private set;}
    public int Cursor {get;private set;}
    public MonsterSpellEntry Next=>entries.Count==0?null:entries[Cursor];
    public static int BaseLevel(string name)=>name=="Power Word Kill"?9:name=="Fire Storm"?7:name=="Circle of Death"?6:name=="Cone of Cold"||name=="Conjure Volley"||name=="Flame Strike"?5:name=="Vitriolic Sphere"||name=="Blight"||name=="Ice Storm"?4:name=="Fireball"||name=="Lightning Bolt"||name=="Conjure Barrage"?3:name=="Melf’s Acid Arrow"||name=="Scorching Ray"||name=="Shatter"?2:name=="Dissonant Whispers"||name=="Command"||name=="Burning Hands"||name=="Color Spray"||name=="Inflict Wounds"||name=="Cure Wounds"||name=="Healing Word"||name=="Magic Missile"?1:Array.IndexOf(Supported,name)>=0?0:-1;
    public bool Configure(int ability,int level,bool counterspell,IEnumerable<MonsterSpellEntry> values)
    {
        if(ability<0||ability>5||level<1||level>20||values==null)return false;
        var draft=new List<MonsterSpellEntry>();
        foreach(var entry in values){
            if(entry==null)return false;int minimum=BaseLevel(entry.Name);
            if(entry.CommandMode<0||entry.CommandMode>4)return false;
            if(minimum<0||entry.Slot<minimum||entry.Slot>9||(minimum==0&&entry.Slot!=0))return false;
            draft.Add(new MonsterSpellEntry(entry.Name,entry.Slot,entry.CommandMode));
        }
        entries.Clear();entries.AddRange(draft);Ability=ability;CasterLevel=level;Counterspell=counterspell;Cursor=0;return true;
    }
    // A failed attempt advances too; it falls back to the custom attack this turn.
    public MonsterSpellEntry Take(){var result=Next;if(result!=null)Cursor=(Cursor+1)%entries.Count;return result;}
    public void Reset(){Cursor=0;}
    public static long AttackBonus(int proficiency,int ability)=> (long)proficiency+AcidSplash.Modifier(ability);
    public static long SaveDc(int proficiency,int ability)=>8+AttackBonus(proficiency,ability);
}
