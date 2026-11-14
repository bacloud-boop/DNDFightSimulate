public sealed class SenseEffects {
 long sequence;readonly long[] normalEnd={-1,-1},normalToken=new long[2];
 long blurEnd=-1;readonly long[] invisibleEnd={-1,-1},trueEnd={-1,-1};long seeEnd=-1;
 public void Blur(long now){blurEnd=now+10;}
 public bool Blurred(long now)=>now<blurEnd;
 public void GreaterInvisible(bool player,long now){invisibleEnd[player?0:1]=now+10;}
 public bool Invisible(bool player,long now)=>now<invisibleEnd[player?0:1]||NormalInvisible(player,now);
 public bool NormalInvisible(bool player,long now)=>now<normalEnd[player?0:1];
 public void NormalInvisibility(bool player,long now){int i=player?0:1;normalEnd[i]=now+600;normalToken[i]=++sequence;}
 public long Token(bool player,long now)=>NormalInvisible(player,now)?normalToken[player?0:1]:0;
 public bool BreakNormal(bool player,long now,long token){int i=player?0:1;if(token==0||!NormalInvisible(player,now)||normalToken[i]!=token)return false;normalEnd[i]=-1;return true;}
 public bool AnyNormal(long now)=>NormalInvisible(true,now)||NormalInvisible(false,now);
 public void SeeInvisible(long now){seeEnd=now+600;}
 public bool SeesInvisible(bool player,long now)=>player&&now<seeEnd;
 public bool RevealsInvisible(bool player,double distance,long now,int blindsight)=>SeesInvisible(player,now)||(TrueSight(player,now)&&System.Math.Round(distance)<=120)||(blindsight>0&&System.Math.Round(distance)<=blindsight);
 public void TrueSeeing(bool player,long now){trueEnd[player?0:1]=now+600;}
 public bool TrueSight(bool player,long now)=>now<trueEnd[player?0:1];
 public void EndConcentration(){blurEnd=invisibleEnd[0]=invisibleEnd[1]=normalEnd[0]=normalEnd[1]=-1;}
 public void Clear(){EndConcentration();seeEnd=trueEnd[0]=trueEnd[1]=-1;}
}
