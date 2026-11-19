using System;
public static class SummonGeometry
{
    // Fractional anchors represent the instant an opportunity attack triggers.
    // Occupied-cell centre distances retain the map's integer-foot rounding.
    public static int Distance(CreatureCover.Body a,CreatureCover.Body b)
    {
        double best=double.MaxValue;
        for(int ay=0;ay<a.Size;ay++)for(int ax=0;ax<a.Size;ax++)for(int by=0;by<b.Size;by++)for(int bx=0;bx<b.Size;bx++){
            double x=a.X+ax-b.X-bx,y=a.Y+ay-b.Y-by;best=Math.Min(best,Math.Sqrt(x*x+y*y)*5);
        }
        return best==double.MaxValue?int.MaxValue:(int)Math.Round(best);
    }
}
