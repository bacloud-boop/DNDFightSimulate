using System;
class RescueReleaseChecks
{
 static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
 static void Main(){
  RescueReleasePlanner.Plan p;
  Check(RescueReleasePlanner.TryFind(10,4,4,1,1,(cx,cy,bx,by)=>true,out p)&&p.CarrierX==4&&p.CarrierY==4,"keep carrier in place when possible");
  Check((p.BodyX-4)*(p.BodyX-4)+(p.BodyY-4)*(p.BodyY-4)==1,"choose nearest nonoverlapping body anchor");
  Check(RescueReleasePlanner.TryFind(10,4,4,1,1,(cx,cy,bx,by)=>cx==4&&cy==4&&bx==9&&by==9,out p)&&p.BodyX==9&&p.BodyY==9,"mandatory release may use distant legal footprint");
  Check(RescueReleasePlanner.TryFind(10,4,4,1,1,(cx,cy,bx,by)=>cx!=4&&cy==4,out p)&&p.CarrierY==4&&Math.Abs(p.CarrierX-4)==1,"minimum carrier displacement when original carrier anchor cannot accommodate release");
  Check(!RescueReleasePlanner.TryFind(4,0,0,4,1,(cx,cy,bx,by)=>true,out p),"never overlap two complete footprints even if external predicate allows it");
  Check(!RescueReleasePlanner.TryFind(10,0,0,1,1,(cx,cy,bx,by)=>false,out p),"no legal layout fails without a fabricated position");
  Check(!RescueReleasePlanner.TryFind(2,0,0,3,1,(cx,cy,bx,by)=>true,out p),"oversized footprint rejected");
  Console.WriteLine("PASS: nearest forced drop, distant fallback, minimal relocation, complete footprints and impossible layouts");
 }
}
