using System;
public sealed class ArmorEquipment
{
    public int ArmorIndex {get;private set;}
    public bool Shield {get;private set;}
    public bool Custom {get;private set;}
    public int CustomAc {get;private set;}=10;
    public ArmorDefinition Armor=>ArmorDefinition.All[ArmorIndex];
    public bool WearingArmor=>!Custom&&ArmorIndex>0;
    public int Weight=> (Custom?0:Armor.Weight)+(Shield?6:0);
    public ArmorEquipment(bool custom=false){Custom=custom;}
    public bool Apply(int index,bool shield,bool custom,int customAc=10)
    {
        if(index<0||index>=ArmorDefinition.All.Length||customAc<5||customAc>30)return false;
        ArmorIndex=custom?0:index;Shield=shield;Custom=custom;CustomAc=customAc;return true;
    }
    public int BaseArmor(int dexterity)=>Custom?CustomAc:Armor.ArmorClass(dexterity);
    public int ShieldBonus(ArmorTraining training,bool melded=false)=>!melded&&Shield&&(training&ArmorTraining.Shield)!=0?2:0;
    public bool Untrained(ArmorTraining training,bool melded=false)=>!melded&&WearingArmor&&(training&Armor.Category)==0;
    public bool D20Disadvantage(int ability,ArmorTraining training,bool melded=false)=>Untrained(training,melded)&&(ability==0||ability==1);
    public int SpeedPenalty(int strength,bool melded=false)=>!melded&&WearingArmor&&Armor.Strength>strength?10:0;
    public bool StealthPenalty(bool melded=false)=>!melded&&WearingArmor&&Armor.StealthDisadvantage;
    public static ArmorTraining ForPlayer(PlayerClasses classes,bool clericProtector=false,bool druidWarden=false)
    {
        ArmorTraining result=ArmorTraining.None;
        for(int i=0;i<PlayerClasses.Names.Length;i++)if(classes[i]>0){
            switch(i){
                case 0:case 9:result|=ArmorTraining.Light|ArmorTraining.Medium|ArmorTraining.Shield;if(classes.InitialClass==i)result|=ArmorTraining.Heavy;break;
                case 2:case 6:result|=ArmorTraining.Light|ArmorTraining.Medium|ArmorTraining.Shield;break;
                case 10:result|=ArmorTraining.Shield;if(classes.InitialClass==i)result|=ArmorTraining.Light|ArmorTraining.Medium;break;
                case 11:result|=ArmorTraining.Light|ArmorTraining.Shield;break;
                case 4:case 5:case 7:result|=ArmorTraining.Light;break;
            }
        }
        if(classes[6]>0&&clericProtector)result|=ArmorTraining.Heavy;
        if(classes[11]>0&&druidWarden)result|=ArmorTraining.Medium;
        return result;
    }
}
