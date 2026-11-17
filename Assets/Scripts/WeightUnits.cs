using System;
public static class WeightUnits
{
 public const int OuncesPerPound=16;
 public static long FromPounds(long pounds){if(pounds<0||pounds>long.MaxValue/16)throw new ArgumentOutOfRangeException(nameof(pounds));return pounds*16;}
 public static long WholePoundsCeiling(long ounces){if(ounces<0)throw new ArgumentOutOfRangeException(nameof(ounces));return ounces/16+(ounces%16==0?0:1);}
 public static string Format(long ounces){if(ounces<0)throw new ArgumentOutOfRangeException(nameof(ounces));return ounces/16+"磅"+(ounces%16==0?"":ounces%16+"盎司");}
}
