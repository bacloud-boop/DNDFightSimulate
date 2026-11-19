using System;
class DestructiveWaveChecks
{
 static void C(bool ok,string m){if(!ok)throw new Exception(m);}
 static void Main(){
  var raw=DestructiveWaveRules.Packet(15,17,false);C(raw[12]==15&&raw[11]==17&&raw[8]==0,"two distinct radiant/thunder components");
  var passed=DestructiveWaveRules.AfterSave(raw,true);C(passed[12]==7&&passed[11]==8&&raw[12]==15,"successful save halves types separately without mutating shared roll");
  var immune=new bool[13];immune[12]=true;C(DamageTypes.Total(DamageTypes.Resolve(raw,immunity:immune))==17,"silence immunity removes only thunder");
  var resistance=new bool[13];resistance[11]=true;C(DamageTypes.Total(DamageTypes.Resolve(passed,resistance,immune))==4,"save halves then type-specific resistance and immunity apply");
  var death=DestructiveWaveRules.Packet(15,17,true);C(death[8]==17&&death[11]==0&&death[12]==15,"necrotic choice replaces radiant, not thunder");
  raw[12]=0;raw[11]=10;var perTarget=DestructiveWaveRules.AfterSave(raw,false);C(perTarget[11]==10&&perTarget[12]==0,"allocated reduction remains separated before saving throws");
  Console.WriteLine("PASS: mixed damage, successful-save rounding, silence immunity, independent resistance and radiant/necrotic choice");
 }
}
