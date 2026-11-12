public sealed class BeastAttack
{
    public readonly string Form,Chinese,AttackName,DamageType;
    public readonly int HitBonus,Count,Sides,Fixed,ExtraCount,ExtraSides;
    public readonly bool Agile,KnockProne;
    public BeastAttack(string form,string chinese,string attack,int hit,int count,int sides,int fixedValue,string damage,bool agile=false,bool prone=false,int extraCount=0,int extraSides=0)
    {Form=form;Chinese=chinese;AttackName=attack;HitBonus=hit;Count=count;Sides=sides;Fixed=fixedValue;DamageType=damage;Agile=agile;KnockProne=prone;ExtraCount=extraCount;ExtraSides=extraSides;}
    public static readonly BeastAttack[] Supported={
        new BeastAttack("Rat","老鼠","啮咬",2,0,0,1,"穿刺",agile:true),
        new BeastAttack("Spider","蜘蛛","啮咬",4,0,0,1,"穿刺",extraCount:1,extraSides:4),
        new BeastAttack("Wolf","狼","啮咬",4,1,6,2,"穿刺",prone:true),
        new BeastAttack("Riding Horse","乘用马","蹄击",5,1,8,3,"钝击"),
        new BeastAttack("Cat","猫","抓挠",4,0,0,1,"挥砍"),
        new BeastAttack("Weasel","鼬","啮咬",5,0,0,1,"穿刺"),
        new BeastAttack("Baboon","狒狒","啮咬",1,1,4,-1,"穿刺"),
        new BeastAttack("Deer","鹿","冲撞",2,1,4,0,"钝击",agile:true),
        new BeastAttack("Mule","骡","蹄击",4,1,4,2,"钝击"),
        new BeastAttack("Pony","矮种马","蹄击",4,1,4,2,"钝击")
    };
    public static BeastAttack Find(string form){foreach(var attack in Supported)if(attack.Form==form)return attack;return null;}
}
