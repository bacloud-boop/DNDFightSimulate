using System;
public sealed class CorpseClock {
 public bool Dead {get;private set;}public int Type {get;private set;}public long DeathAt {get;private set;}
 long age,last,preservedUntil=-1;
 public void Observe(bool dead,int type,long now){if(dead&&!Dead){Dead=true;Type=type;DeathAt=last=now;age=0;preservedUntil=-1;}else if(!dead&&Dead)Clear();}
 public long Age(long now){if(Dead&&now>last){age+=Math.Max(0,now-Math.Max(last,preservedUntil));last=now;}return age;}
 public void Preserve(long now){Age(now);preservedUntil=now+144000;}
 public bool Preserved(long now)=>Dead&&now<preservedUntil;
 public long ProjectedAge(long now,long duration){long current=Age(now);return current+Math.Max(0,now+duration-Math.Max(now,preservedUntil));}
 public void Clear(){Dead=false;age=0;last=DeathAt=0;preservedUntil=-1;}
}
public static class RevivalRules {
 public static bool Supported(string name)=>name=="Revivify"||name=="Raise Dead"||name=="Gentle Repose"||name=="True Resurrection";
 public static long Duration(string name)=>name=="Raise Dead"||name=="True Resurrection"?600:1;
 public static long Limit(string name)=>name=="Revivify"?10:name=="True Resurrection"?200L*365*14400:144000;
 static string Eligibility(string name,CorpseClock corpse){if(!Supported(name))return "该复活法术尚未接入。";if(!corpse.Dead)return "目标必须已经死亡；倒地但未死亡不能作为复活目标。";return name=="Raise Dead"&&corpse.Type==13?"死者复活不能复活死亡时为亡灵的目标。":"";}
 public static string Error(string name,CorpseClock corpse,long now){string error=Eligibility(name,corpse);if(error!=""||name=="Gentle Repose")return error;return corpse.ProjectedAge(now,Duration(name))>Limit(name)?"计入施法耗时后超过复活时限。":"";}
 public static string CompletionError(string name,CorpseClock corpse,long now){string error=Eligibility(name,corpse);if(error!=""||name=="Gentle Repose")return error;return corpse.Age(now)>Limit(name)?"已超过复活时限。":"";}
}
