using System;
public static class ChallengeRatings {
 public static readonly string[] Names=BuildNames();
 static string[] BuildNames(){var labels=new string[34];labels[0]="0";labels[1]="1/8";labels[2]="1/4";labels[3]="1/2";for(int i=4;i<labels.Length;i++)labels[i]=(i-3).ToString();return labels;}
 public static bool Valid(int index)=>index>=0&&index<34;
 public static int Eighths(int index){if(!Valid(index))throw new ArgumentOutOfRangeException(nameof(index));return index<4?new[]{0,1,2,4}[index]:(index-3)*8;}
 public static double Value(int index)=>Eighths(index)/8.0;
}
