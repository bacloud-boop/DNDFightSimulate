using System;
class ArcaneVigorChecks
{
    static void Check(bool ok){if(!ok)throw new Exception("Arcane Vigor regression");}
    static void Main()
    {
        int[] levels={3,2},spent={1,1},chosen={1,1};
        Check(ArcaneVigorRules.TrySpend(levels,spent,chosen,2)&&spent[0]==2&&spent[1]==2);
        Check(!ArcaneVigorRules.TrySpend(levels,spent,chosen,2)&&spent[0]==2&&spent[1]==2);
        Check(!ArcaneVigorRules.Valid(levels,spent,new[]{0,0},2));
        Check(ArcaneVigorRules.Valid(new[]{9},new[]{0},new[]{9},9));
        Check(!ArcaneVigorRules.Valid(new[]{9},new[]{0},new[]{3},2));
        Check(!ArcaneVigorRules.Valid(levels,new[]{-1,0},chosen,2));
        Check(!ArcaneVigorRules.Valid(levels,spent,new[]{-1,1},2));
        Check(!ArcaneVigorRules.Valid(levels,spent,chosen,1));
        Check(!ArcaneVigorRules.Valid(levels,spent,new[]{1},2));
        Check(!ArcaneVigorRules.Valid(null,spent,chosen,2));
        Console.WriteLine("PASS: shared multiclass dice, atomic spending, slot limits, empty and malformed selections");
    }
}
