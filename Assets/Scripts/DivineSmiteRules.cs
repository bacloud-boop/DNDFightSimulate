public static class DivineSmiteRules
{
    public static int Dice(int slot,int creatureType,bool critical=false)
        =>slot>=1&&slot<=9?(slot+1+(creatureType==7||creatureType==13?1:0))*(critical?2:1):0;
}
