using System;
class MonsterSpellCatalogChecks
{
    static void Check(bool value,string message){if(!value)throw new Exception(message);}
    static void Main()
    {
        foreach(string name in MonsterSpellSequence.Supported){var spell=DirectSpell.Find(name);Check(spell!=null&&spell.Level==MonsterSpellSequence.BaseLevel(name),name+" metadata and slot");}
        var bolt=DirectSpell.Find("Fire Bolt");Check(bolt.DiceCount(1,0)==1&&bolt.DiceCount(5,0)==2&&bolt.DiceCount(11,0)==3&&bolt.DiceCount(17,0)==4,"caster-level scaling");
        var heal=DirectSpell.Find("Cure Wounds");Check(heal.DiceCount(1,1)==2&&heal.DiceCount(1,9)==18,"healing upcast");
        Check(DirectSpell.Find("Magic Missile").DiceCount(1,1)==3&&DirectSpell.Find("Magic Missile").DiceCount(1,9)==11,"missiles");
        Check(DirectSpell.Find("Scorching Ray").DiceCount(1,2)==3&&DirectSpell.Find("Scorching Ray").DiceCount(1,9)==10,"rays");
        var blight=DirectSpell.Find("Blight");Check(blight.HalfSave&&blight.DiceCount(1,4)==8&&blight.DiceCount(1,9)==13,"Blight");
        var kill=DirectSpell.Find("Power Word Kill");Check(kill.DiceCount(1,9)==12&&kill.Sides==12,"Power Word Kill fallback");
        Console.WriteLine("PASS: all monster spells match catalog, cantrip growth, upcast dice, missiles, rays and high-level damage");
    }
}
