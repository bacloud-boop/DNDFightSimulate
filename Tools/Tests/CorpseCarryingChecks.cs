using System;
class CorpseCarryingChecks
{
 static void Check(bool v,string m){if(!v)throw new Exception(m);}
 static void Main(){
  var body=new BodyWeight();Check(body.BodyPounds==150&&body.EquipmentPounds==0,"confirmed body defaults");
  Check(!body.TryApply("149.5","0")&&!body.TryApply("150","-1")&&body.TotalPounds==150,"integer weights and atomic validation");
  var unknown=new BodyWeight(null);var load=new CorpseCarryingState();int actions=2;
  Func<bool> action=()=>{if(actions==0)return false;actions--;return true;};
  Check(!load.PickUp(2,true,unknown,10,2,0,false,action)&&actions==2,"unknown beast mass cannot consume action");
  Check(!load.PickUp(2,false,body,10,2,0,false,action)&&actions==2,"living target not a corpse");
  Check(!load.PickUp(2,true,body,10,2,1,false,action)&&actions==2,"carrier equipment reduces remaining capacity before pickup");
  Check(load.PickUp(2,true,body,10,2,0,false,action)&&actions==1&&load.Loaded,"valid pickup consumes exactly one action");
  Check(!load.PickUp(3,true,body,10,2,0,false,action)&&actions==1,"cannot pick a second corpse while loaded");
  Check(!load.Drop(()=>false)&&load.Loaded&&actions==1,"blocked drop preserves carried body");
  Check(load.Drop(()=>true)&&!load.Loaded&&actions==1,"legal drop is free");
  body.TryApply("151","10");Check(load.PickUp(2,true,body,10,2,0,true,action)&&load.WeightPounds==161&&actions==0,"corpse includes its equipment weight");
  Check(load.AllowedSpeed(10,2,0,30)==5&&load.AllowedSpeed(10,2,0,0)==0,"heavy dragging caps speed without granting movement");
  Check(load.FitsForm(10,2,200,true)&&!load.FitsForm(10,2,200,false),"melded carrier equipment excluded then restored for capacity");
  Check(!load.FitsForm(3,0,0,true)&&load.RequiresForcedDrop(3,0,0,true),"weak restored form requires drop without losing corpse state");
  Check(load.WeightPounds==161&&load.Loaded,"corpse and its equipment never meld with carrier");
  Check(load.Drop(()=>true)&&!load.PickUp(2,true,body,10,2,0,true,action)&&!load.Loaded,"no action available cannot pick up");
  Check(!CarryingRules.CanCarry(10,2,long.MaxValue,long.MaxValue)&&!CarryingRules.CanDrag(10,2,long.MaxValue),"aggregate weight overflow safe");
  Console.WriteLine("PASS: default and unknown weights, corpse equipment, atomic pickup action and free legal drop");
 }
}
