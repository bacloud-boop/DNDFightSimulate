public static class ArcaneVigorRules
{
    public static bool Valid(int[] levels,int[] spent,int[] chosen,int slot)
    {
        if(slot<2||slot>9||levels==null||spent==null||chosen==null||levels.Length==0||levels.Length!=spent.Length||levels.Length!=chosen.Length)return false;
        long count=0;
        for(int i=0;i<levels.Length;i++){
            if(levels[i]<0||spent[i]<0||chosen[i]<0||spent[i]>levels[i]||chosen[i]>levels[i]-spent[i])return false;
            count+=chosen[i];
        }
        return count>=1&&count<=slot;
    }
    public static bool TrySpend(int[] levels,int[] spent,int[] chosen,int slot)
    {
        if(!Valid(levels,spent,chosen,slot))return false;
        for(int i=0;i<spent.Length;i++)spent[i]+=chosen[i];
        return true;
    }
}
