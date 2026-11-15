using System;
class DetectionChecks
{
    static void Check(bool value){if(!value)throw new Exception("Detection regression");}
    static void Main()
    {
        var effect=new DetectEvilGood();
        Check(!effect.Detects(0,0,true,0));Check(effect.Begin(10));
        for(int type=-1;type<=14;type++){
            bool expected=type==0||type==2||type==5||type==6||type==7||type==13;
            Check(effect.Detects(type,30,true,10)==expected);
        }
        Check(!effect.Detects(0,31,true,10));Check(!effect.Detects(0,-1,true,10));
        Check(!effect.Detects(0,0,false,10));Check(!effect.Detects(0,0,true,10,true));
        Check(effect.Active(109)&&!effect.Active(110));
        Check(!effect.Begin(long.MaxValue)&&effect.Expires==110);Check(!effect.Begin(-1));
        Check(effect.Begin(long.MaxValue-100)&&effect.Active(long.MaxValue-1)&&!effect.Active(long.MaxValue));
        effect.Clear();Check(!effect.Active(0));
        Console.WriteLine("PASS: six creature types, range, corpses, blocking, expiration, overflow, clear");
    }
}
