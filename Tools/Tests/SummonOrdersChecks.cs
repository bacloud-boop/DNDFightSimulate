using System;
class SummonOrdersChecks
{
 static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
 static void Main(){
  var unit=new SummonedUnit(1,"spirit",30,13,30,1,2);bool changed;
  Check(!unit.HasVerbalOrders,"new summon has no implicit command");
  Check(unit.ReceiveVerbalOrders(SummonCommand.AssistCombat,out changed)&&changed,"first broad order requires speech");
  Check(unit.BeginTurn(1)&&unit.Attack()&&unit.Attack(),"manual attacks do not change broad command");unit.EndTurn();
  Check(unit.BeginTurn(2)&&unit.HasVerbalOrders&&unit.Orders.Equals(SummonCommand.AssistCombat),"assist order persists across turns");
  Check(unit.SpendMovement(10)&&unit.ReceiveVerbalOrders(SummonCommand.AssistCombat,out changed)&&!changed,"movement and repeated command do not require new speech");
  Check(unit.ActionAvailable&&unit.MovementRemaining==20,"continuing order never refills resources");
  Check(!unit.ReceiveVerbalOrders(default(SummonCommand),out changed)&&!changed&&unit.Orders.Equals(SummonCommand.AssistCombat),"invalid command preserves current tactical order");
  Check(unit.ReceiveVerbalOrders(SummonCommand.Dodge,out changed)&&changed,"changing tactical instruction requires speech");
  Check(unit.ReceiveVerbalOrders(SummonCommand.Dodge,out changed)&&!changed,"repeating dodge instruction is silent");
  Check(unit.ReceiveVerbalOrders(SummonCommand.AssistCombat,out changed)&&changed,"resuming combat after dodge is a new command");
  Check(unit.ActionAvailable&&unit.MovementRemaining==20,"new orders consume neither summon action nor movement");
  unit.Damage(30);Check(!unit.ReceiveVerbalOrders(SummonCommand.AssistCombat,out changed)&&!changed,"vanished creature cannot receive commands");
  Console.WriteLine("PASS: persistent broad commands, silent manual execution, tactical changes and atomic invalid orders");
 }
}
