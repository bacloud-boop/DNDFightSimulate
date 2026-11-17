using System;
class SpikeGrowthChecks
{
 static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
 static void Main(){
  var f=new SpikeGrowthEffect();Check(!f.Active(0),"unstarted area inactive");f.Begin(5,5,14,10);
  Check(f.Cell(9,5,10)&&!f.Cell(9,6,10)&&f.Cell(5,5,10),"twenty-foot circular cell coverage");
  Check(f.Intersects(8,5,2,10)&&!f.Intersects(10,5,1,10),"any covered occupied cell participates in damage");
  Check(f.Travel(0,4,10)==0&&f.Travel(0,3,10)==1&&f.Travel(0,3,11)==1,"sub-five-foot distances persist across sections and turns");
  Check(f.Travel(-1,5,10)==1&&f.Travel(0,5,10)==1,"each actor has independent travelled distance");
  Check(f.Active(109)&&!f.Active(110)&&f.Travel(0,100,110)==0,"ten-minute expiry prevents queued late damage");
  f.Begin(0,0,10,111);Check(f.Travel(0,4,111)==0,"new casting clears old distance remainders");f.Clear();Check(!f.Active(111),"concentration end clears region");
  var meter=new RouteCostMeter(11,12,false);int a=meter.Next(7,false,true),first=meter.Travelled,b=meter.Next(5,false,true);
  Check(first==6&&meter.Travelled==11&&a+b==22,"segmented route conserves original physical eleven feet while difficult terrain costs twenty-two");
  Console.WriteLine("PASS: Spike Growth area, independent accumulated travel, expiry, reset and physical distance conservation");
 }
}
