using System.Collections.Generic;
public static class ChromaticOrbRules
{
    public static readonly int[] Types={3,4,5,7,9,12};
    public static int Dice(int slot,bool critical=false)=>slot>=1&&slot<=9?(slot+2)*(critical?2:1):0;
    public static bool Duplicates(IReadOnlyList<int> rolls){if(rolls==null)return false;int seen=0;foreach(int roll in rolls){if(roll<1||roll>8)return false;int bit=1<<roll;if((seen&bit)!=0)return true;seen|=bit;}return false;}
    public static bool CanLeap(int slot,int leaps,IReadOnlyList<int> rolls)=>Dice(slot)>0&&leaps>=0&&leaps<slot&&Duplicates(rolls);
}
