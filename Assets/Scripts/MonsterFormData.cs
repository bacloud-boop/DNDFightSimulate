using System;
public sealed class MonsterFormData {
 public int Type=9,Size=2,MaxHp=1000,Armor=10,Proficiency,AttackRange=5,Speed=30,Darkvision,Bonus;public bool Melee=true;
 public int[] Abilities={10,10,10,10,10,10},Skills=new int[18],Dice=new int[5],DamageTypes=new int[6];
 public MonsterFormData Copy(){var copy=(MonsterFormData)MemberwiseClone();copy.Abilities=(int[])Abilities.Clone();copy.Skills=(int[])Skills.Clone();copy.Dice=(int[])Dice.Clone();copy.DamageTypes=(int[])DamageTypes.Clone();return copy;}
 public bool Valid(out string error){
  error="生前类型须为非亡灵，体型与所有数字须符合怪物配置规则。";
  if(Type<0||Type>=13||Size<0||Size>5||MaxHp<1||Armor<5||Armor>30||AttackRange<0||Speed<0||Darkvision<0)return false;
  if(Abilities==null||Abilities.Length!=6||Skills==null||Skills.Length!=18||Dice==null||Dice.Length!=5||DamageTypes==null||DamageTypes.Length!=6)return false;
  foreach(int value in Abilities)if(value<3||value>20)return false;foreach(int value in Skills)if(value<0||value>2)return false;long total=0;foreach(int value in Dice){if(value<0)return false;total+=value;}if(total>10000){error="伤害骰总数不能超过10000。";return false;}foreach(int value in DamageTypes)if(value<0||value>=13)return false;error="";return true;
 }
}
