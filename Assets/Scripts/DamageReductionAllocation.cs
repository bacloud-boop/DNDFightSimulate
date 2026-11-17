using System;
public sealed class DamageReductionAllocation
{
    readonly long[] raw;
    readonly int[] assigned;
    public int Required {get;}
    public int Assigned {get;private set;}
    public bool Complete=>Assigned==Required;
    public DamageReductionAllocation(long[] values,int penalty,int minimumTotal=0)
    {
        if(values==null||values.Length!=DamageTypes.Names.Length||penalty<0||minimumTotal<0)throw new ArgumentException("Invalid damage allocation");
        raw=new long[values.Length];assigned=new int[values.Length];long total=0;
        for(int i=0;i<values.Length;i++){raw[i]=Math.Max(0,values[i]);total=raw[i]>long.MaxValue-total?long.MaxValue:total+raw[i];}
        Required=(int)Math.Min(penalty,Math.Max(0,total-minimumTotal));
    }
    public long Damage(int type)=>raw[type];
    public bool TryAssign(int[] values)
    {
        if(values==null||values.Length!=assigned.Length)return false;long total=0;
        for(int i=0;i<values.Length;i++){if(values[i]<0||values[i]>raw[i])return false;total+=values[i];}
        if(total>Required)return false;
        Array.Copy(values,assigned,values.Length);Assigned=(int)total;return true;
    }
    public bool TryCommit(out long[] result)
    {
        result=null;if(!Complete)return false;result=(long[])raw.Clone();for(int i=0;i<result.Length;i++)result[i]-=assigned[i];return true;
    }
}
