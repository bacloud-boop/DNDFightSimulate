using System;
using System.Reflection;
class ContinualFlameChecks
{
    static void C(bool ok,string text){if(!ok)throw new Exception(text);}
    static void Main()
    {
        var flame=new ContinualFlameEffect();ContinualFlameEffect.Source first,second,strong,newest,draft;
        C(flame.All.Length==0&&flame.Objects.Length==0&&flame.Effective(1)==null,"uncast model contains no attached flames");
        C(!flame.Cast(0,2,out draft)&&draft==null&&!flame.Cast(-1,2,out draft)&&!flame.Cast(1,1,out draft)&&!flame.Cast(1,10,out draft),"invalid object identities and slots reject without creating a source");
        C(flame.Cast(1,2,out first)&&first.ObjectId==1&&first.Slot==2&&first.Group==5,"first valid source uses the first unreserved light group");
        C(flame.Cast(int.MaxValue,9,out second)&&second.Group==6&&flame.Effective(1)==first&&flame.Effective(int.MaxValue)==second,"multiple objects remain independent and use distinct monotonic groups");
        var allSnapshot=flame.All;var objectsSnapshot=flame.Objects;allSnapshot[0]=null;objectsSnapshot[0]=99;
        C(flame.All[0]==first&&flame.Objects[0]==1,"enumeration arrays are independent snapshots and cannot mutate the registry");
        C(flame.Cast(1,7,out strong)&&strong.Group==7&&flame.Effective(1)==strong&&flame.All.Length==3&&flame.Objects.Length==2,"higher slot suppresses an old source without removing it or adding another object");
        C(allSnapshot.Length==2&&objectsSnapshot.Length==2,"older snapshots remain stable after a new casting");
        C(flame.Cast(1,7,out newest)&&newest.Group==8&&flame.Effective(1)==newest,"same-slot sources select the most recent casting");
        C(flame.Cast(1,2,out draft)&&draft.Group==9&&flame.Effective(1)==newest,"a newer weaker source does not displace a stronger one");
        C(flame.Remove(newest.Group)&&flame.Effective(1)==strong&&flame.All.Length==4,"removing the current same-slot winner restores its older peer");
        C(flame.Remove(strong.Group)&&flame.Effective(1)==draft&&flame.Effective(int.MaxValue)==second,"removing a stronger casting restores the latest weak source without touching other objects");
        C(flame.Remove(draft.Group)&&flame.Effective(1)==first,"all original sources remain available until explicitly removed");
        C(!flame.Remove(0)&&!flame.Remove(-1)&&!flame.Remove(999)&&flame.All.Length==2,"missing source removals are harmless and atomic");
        C(flame.Cast(2,2,out draft)&&draft.Group==10,"removed groups are not reused by later castings");
        C(flame.Remove(first.Group)&&flame.Effective(1)==null&&flame.Objects.Length==2,"removing the final source removes only that object from the effective object list");
        // Exercise allocator exhaustion directly rather than constructing over
        // two billion permanent spell sources merely to reach the boundary.
        typeof(ContinualFlameEffect).GetField("nextGroup",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(flame,(long)int.MaxValue-1);
        C(flame.Cast(3,3,out first)&&first.Group==int.MaxValue-1&&flame.Cast(3,4,out strong)&&strong.Group==int.MaxValue,"last two positive integer groups can be allocated without overflow");
        int count=flame.All.Length;
        C(!flame.Cast(4,2,out draft)&&draft==null&&flame.All.Length==count&&flame.Effective(3)==strong&&flame.Effective(4)==null,"group exhaustion refuses atomically instead of wrapping or reusing a group");
        C(flame.Remove(strong.Group)&&!flame.Cast(4,2,out draft)&&flame.Effective(3)==first,"removing the final group does not reopen an exhausted allocator");
        flame.Clear();
        C(flame.All.Length==0&&flame.Objects.Length==0&&flame.Effective(3)==null&&flame.Effective(int.MaxValue)==null,"clear removes every permanent and suppressed source");
        C(flame.Cast(1,2,out first)&&first.Group==5,"after runtime clears Vision groups a new battle may restart allocation at five");
        Console.WriteLine("PASS: Continual Flame permanent objects, strongest/latest suppression, source restoration, independent snapshots, atomic validation, monotonic light groups, overflow exhaustion and reset");
    }
}
