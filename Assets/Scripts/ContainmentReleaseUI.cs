using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class TrainingGame
{
    List<BattleMap.ReleasePlan> releasePlans;
    int releasePlanIndex;
    bool releaseConfirmed;
    string releaseReason="";int releaseRootSpaces,releaseRootSize;bool releaseRestoredHost;
    IEnumerator ReleaseSwallowedTarget(string reason,int restoredHostSpaces=0,int restoredHostSize=-1)
    {
        if(!map.HasContainedCreature)yield break;
        bool busy=rolling;rolling=true;releaseReason=reason;releaseRootSpaces=restoredHostSpaces;releaseRootSize=restoredHostSize;releaseRestoredHost=false;
        releasePlans=restoredHostSpaces>0?map.RestoredHostReleasePlans(restoredHostSpaces,restoredHostSize):map.MandatoryReleasePlans();releasePlanIndex=0;releaseConfirmed=false;
        if(releasePlans.Count==0){
            history.Insert(0,"无法放出被吞目标：没有可容纳双方完整占地的位置，内部状态保留。");
            releasePlans=null;rolling=busy;yield break;
        }
        while(!releaseConfirmed)yield return null;
        var plan=releasePlans[releasePlanIndex];bool player=map.IsContained(true);
        var host=player?map.Monster:map.Player;
        if(restoredHostSpaces>0?map.ApplyRestoredHostRelease(plan,restoredHostSpaces,restoredHostSize):map.ApplyMandatoryReleasePlan(plan)){
            releaseRestoredHost=restoredHostSpaces>0;
            history.Insert(0,reason+"："+(player?"玩家":"木桩")+"在所选合法位置被放出。"+(host!=plan.Host?"吞咽者/遗体为腾出空间作最短必要挪位。":""));
        }else history.Insert(0,"所选放出方案已失效，内部状态保留。");
        releasePlans=null;rolling=busy;
    }
    void DrawContainmentRelease()
    {
        if(releasePlans==null||releasePlans.Count==0)return;
        Fill(new Rect(0,0,1600,900),new Color(0,0,0,.92f));
        Surface(new Rect(420,125,760,650),paperTexture);
        GUI.Label(new Rect(450,150,700,48),"选择放出位置 · "+releaseReason,heading);
        var plan=releasePlans[releasePlanIndex];bool player=map.IsContained(true);
        var board=new Rect(600,220,400,400);float cell=board.width/map.Size;
        for(int i=0;i<=map.Size;i++){
            Fill(new Rect(board.x+i*cell,board.y,1,board.height),new Color(.35f,.25f,.15f,.5f));
            Fill(new Rect(board.x,board.y+i*cell,board.width,1),new Color(.35f,.25f,.15f,.5f));
        }
        int hostSpaces=releaseRootSpaces>0?releaseRootSpaces:player?map.MonsterSpaces:map.PlayerSpaces;if(!player&&map.Riding&&(releaseRootSize>=0?releaseRootSize<map.RiderMount.Size:releaseRootSpaces<map.RiderMount.Spaces))hostSpaces=map.PlayerSpaces;
        Token(BodyRect(board,plan.Host,hostSpaces),!player);
        Token(BodyRect(board,plan.Target,player?map.PlayerSpaces:map.MonsterSpaces),player);
        GUI.Label(new Rect(450,630,700,40),"方案 "+(releasePlanIndex+1)+" / "+releasePlans.Count+" · 放出格("+(plan.Target.x+1)+","+(plan.Target.y+1)+") · 已保留完整占地",centered);
        if(plan.Shared)GUI.Label(new Rect(450,665,700,24),"空间不足：必要的临时共占，回合末沿用既有倒地规则。",small);
        if(Btn(new Rect(450,690,180,50),"上一方案"))releasePlanIndex=(releasePlanIndex+releasePlans.Count-1)%releasePlans.Count;
        if(Btn(new Rect(645,690,180,50),"下一方案"))releasePlanIndex=(releasePlanIndex+1)%releasePlans.Count;
        if(Btn(new Rect(840,690,310,50),"确认放出位置"))releaseConfirmed=true;
    }
}
