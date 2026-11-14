using System;
class MirrorImageChecks {
 static void Check(bool ok,string m){if(!ok)throw new Exception(m);}
 static void Main(){var effect=new MirrorImageEffect();effect.Cast(0);Check(effect.Count(0)==3,"three duplicates");Check(!effect.Intercept(0,new[]{1,2,2})&&effect.Remaining==3,"failed rolls preserve images");Check(effect.Intercept(0,new[]{3,6,6})&&effect.Remaining==2,"multiple successful dice destroy only one duplicate");Check(effect.Intercept(0,new[]{1,3})&&effect.Remaining==1,"one die per remaining duplicate");Check(effect.Intercept(0,new[]{3})&&effect.Count(0)==0,"last duplicate");Check(!effect.Intercept(0,new int[0]),"no duplicate cannot intercept");effect.Cast(10);Check(effect.Count(19)==3&&effect.Count(20)==0,"one minute expiry");effect.Cast(20);Check(effect.Count(20)==3,"recast refreshes instead of stacking");effect.Clear();Check(effect.Count(20)==0,"reset");Console.WriteLine("PASS: mirror image counts, threshold, single removal, expiry, recast and reset");}
}
