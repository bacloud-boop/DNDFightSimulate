using System;
class GreaseChecks
{
 static void Check(bool value,string message){if(!value)throw new Exception(message);}
 static void Main(){
  var fields=new GreaseFields();var high=fields.Add(2,2,5,18,0);
  Check(high.Cell(2,2)&&high.Cell(2,3)&&!high.Cell(3,2)&&!high.Cell(3,3),"land mask excludes water cells");
  Check(high.Intersects(1,2,2)&&!high.Intersects(3,2,1),"partial body occupancy triggers but water does not");
  var low=fields.Add(2,2,15,10,1);Check(fields.Current(2,2,1,1)==high&&fields.Current(2,2,1,10)==low,"highest DC, independent expiry");
  var newest=fields.Add(2,2,15,10,2);Check(fields.Current(2,2,1,2)==high&&fields.Current(2,2,1,10)==newest,"same DC uses newest while older effects retain duration");
  Check(fields.Any(11)&&!fields.Any(12),"nonconcentration minute expires");fields.Clear();Check(!fields.Any(0),"reset clears all regions");
  fields.Add(0,0,0,15,0);Check(!fields.Any(0),"all-water casting has no oil");
  var meter=new RouteCostMeter(11,12,false);Check(meter.Next(5,false,false)==5&&meter.Next(7,false,false)==6&&meter.Paid==11,"split diagonal conserves original 11 feet rather than 12");
  meter=new RouteCostMeter(11,12,true);Check(meter.Next(5,false,true)==15&&meter.Next(7,true,false)==18&&meter.Paid==33,"crawl, water and difficult terrain add, including stopped partial route");
  for(int a=1;a<20;a++)for(int b=1;b<20;b++){
   int weight=5*a+7*b,distance=weight-1;meter=new RouteCostMeter(distance,weight,false);int paid=0;
   for(int i=0;i<a;i++){int cost=meter.Next(5,false,true);Check(cost>=0,"prefix never refunds earlier movement");paid+=cost;}
   for(int i=0;i<b;i++)paid+=meter.Next(7,false,true);
   Check(paid==2*distance&&meter.Paid==paid,"all rough route conserves rounded full cost");
  }
  var rescue=new RescueCarrier();Check(rescue.Dexterity==10&&rescue.DexteritySaveModifier==0,"rescue defaults, no proficiency");
  Check(rescue.Configure(10,2,30,150,0,3)&&rescue.DexteritySaveModifier==-4,"negative odd Dexterity modifier floors");
  Check(!rescue.Configure(20,2,30,150,0,21)&&rescue.Strength==10&&rescue.Dexterity==3,"out-of-range edit rejected atomically");
  Check(rescue.Configure(10,2,30,150,0,20)&&rescue.DexteritySaveModifier==5,"Dexterity maximum");
  Check(!rescue.Configure(3,2,30,150,46,10)&&rescue.Dexterity==20,"rescue overloaded configuration is rejected");
  rescue.BeginTurn(1);rescue.Prone=true;Check(rescue.Stand()&&!rescue.Prone&&rescue.MovementRemaining==15&&rescue.ActionsRemaining==1,"rescue stands using its own movement, not its action");
  Console.WriteLine("PASS: land-only fields, overlap priority, timed expiry, conserved route costs, additive penalties, rescue Dexterity and stand resources");
 }
}
