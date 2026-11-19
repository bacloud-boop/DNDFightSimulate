using System;
public static class ArmorFormulaRules
{
    public static int BestBase(ArmorEquipment equipment,int dexterity,int constitution,int wisdom,bool barbarian,bool monk,int natural=-1,bool melded=false,int mageArmor=-1)
    {
        int dex=(int)Math.Floor((dexterity-10)/2.0);
        int result=melded?Math.Max(natural,10+dex):equipment.BaseArmor(dexterity);
        if(melded||!equipment.WearingArmor){
            if(barbarian)result=Math.Max(result,10+dex+(int)Math.Floor((constitution-10)/2.0));
            if(monk&&(melded||!equipment.Shield))result=Math.Max(result,10+dex+(int)Math.Floor((wisdom-10)/2.0));
            result=Math.Max(result,mageArmor);
        }
        return result;
    }
}
