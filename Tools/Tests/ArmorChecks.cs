using System;
class ArmorChecks {
 static void Check(bool ok){if(!ok)throw new Exception("Armor rule regression");}
 static void Main(){
 Check(ArmorDefinition.All.Length==13);
 Check(ArmorDefinition.All[3].ArmorClass(20)==17);
 Check(ArmorDefinition.All[8].ArmorClass(20)==17);
 Check(ArmorDefinition.All[8].ArmorClass(3)==11);
 Check(ArmorDefinition.All[12].ArmorClass(3)==18);
 var armor=new ArmorEquipment();Check(armor.Apply(12,true,false));
 Check(armor.Weight==71&&armor.SpeedPenalty(14)==10&&armor.SpeedPenalty(15)==0);
 Check(armor.Untrained(ArmorTraining.Light)&&armor.D20Disadvantage(1,ArmorTraining.None)&&!armor.D20Disadvantage(3,ArmorTraining.None));
 Check(armor.ShieldBonus(ArmorTraining.None)==0&&armor.ShieldBonus(ArmorTraining.Shield)==2&&armor.ShieldBonus(ArmorTraining.Shield,true)==0);
 Check(!armor.Apply(99,false,false)&&armor.ArmorIndex==12);
 var classes=new PlayerClasses();Check((ArmorEquipment.ForPlayer(classes)&ArmorTraining.Heavy)!=0);
 var levels=new string[12];for(int i=0;i<12;i++)levels[i]="0";levels[0]="1";levels[1]="1";Check(classes.TryApply(levels,1));
 Check((ArmorEquipment.ForPlayer(classes)&ArmorTraining.Medium)!=0&&(ArmorEquipment.ForPlayer(classes)&ArmorTraining.Heavy)==0);
 var bare=new ArmorEquipment();
 Check(ArmorFormulaRules.BestBase(bare,18,14,20,true,true,mageArmor:17)==19);
 bare.Apply(0,true,false);Check(ArmorFormulaRules.BestBase(bare,18,14,20,true,true,mageArmor:17)==17);
 Check(ArmorFormulaRules.BestBase(bare,18,14,20,true,true,14,true,17)==19);
 bare.Apply(12,false,false);Check(ArmorFormulaRules.BestBase(bare,18,20,20,true,true,mageArmor:30)==18);
 Console.WriteLine("PASS: armor table, dex caps, negative dex, heavy armor, strength, training, shield, weight and multiclass grants");
 }
}
