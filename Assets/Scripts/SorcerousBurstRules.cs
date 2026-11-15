using System;
public static class SorcerousBurstRules
{
    public static int MaximumExtra(int modifier,bool critical)=>Math.Max(0,modifier)*(critical?2:1);
    public static int NextCount(int[] lastRolls,int used,int modifier,bool critical)
    {
        if(lastRolls==null||used<0)throw new ArgumentException("Invalid burst rolls");
        int triggers=0;foreach(int value in lastRolls){if(value<1||value>8)throw new ArgumentOutOfRangeException(nameof(lastRolls));if(value==8)triggers++;}
        return Math.Min(Math.Max(0,MaximumExtra(modifier,critical)-used),triggers*(critical?2:1));
    }
}
