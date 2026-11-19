using System;
class MageArmorChecks {
 static void Check(bool v){if(!v)throw new Exception("Mage Armor regression");}
 static void Main(){var effect=new MageArmorEffect();Check(effect.BaseArmor(true,10,0)==-1);Check(effect.Grant(true,10));Check(effect.BaseArmor(true,18,4809)==17&&effect.BaseArmor(true,18,4810)==-1);Check(effect.BaseArmor(false,18,10)==-1);effect.Observe(true,false,20);Check(effect.BaseArmor(true,3,20)==9);effect.Observe(true,true,21);Check(effect.BaseArmor(true,18,21)==-1);Check(effect.Grant(false,40));Check(!effect.Grant(false,long.MaxValue)&&effect.BaseArmor(false,10,41)==13);effect.Clear();Check(effect.BaseArmor(false,10,41)==-1);Console.WriteLine("PASS: independent targets, eight-hour boundary, negative dex, wearing armor, overflow and reset");}
}
