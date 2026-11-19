using System;
class CarryingChecks
{
 static void Check(bool v,string m){if(!v)throw new Exception(m);}
 static void Main(){
  Check(CarryingRules.CanCarry(10,2,0,150)&&!CarryingRules.CanCarry(10,2,1,150),"equipment counts against carry limit");
  Check(CarryingRules.CanDrag(10,2,300)&&!CarryingRules.CanDrag(10,2,301),"drag limit twice carrying capacity");
  Check(CarryingRules.DragSpeed(10,2,150,30)==30&&CarryingRules.DragSpeed(10,2,151,30)==5,"dragging over carrying capacity caps speed at five");
  Check(CarryingRules.DragSpeed(10,2,151,0)==0&&CarryingRules.DragSpeed(10,2,151,3)==3,"dragging cannot increase an already lower speed");
  Check(CarryingRules.CanCarry(3,0,0,22)&&!CarryingRules.CanCarry(3,0,0,23),"tiny odd strength exact half pound capacity without float rounding");
  Check(CarryingRules.CapacityHalfPounds(10,3)==600&&CarryingRules.CapacityHalfPounds(10,4)==1200&&CarryingRules.CapacityHalfPounds(10,5)==2400,"2024 size multipliers");
  Check(!CarryingRules.CanCarry(10,2,int.MaxValue,int.MaxValue)&&!CarryingRules.CanDrag(10,2,int.MaxValue),"large integer weights cannot overflow into valid capacity");
  Console.WriteLine("PASS: carrying weight, size factors, exact tiny capacity and drag speed cap");
 }
}
