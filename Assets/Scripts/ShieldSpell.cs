using System.Collections;
using UnityEngine;
public sealed partial class TrainingGame
{
    bool shieldPending;int shieldDecision;bool shieldAttackTrigger;
    bool CanOfferShield => !combat.ShieldActive&&!combat.PlayerReactionsBlocked&&combat.PlayerCanAct&&combat.PlayerReactionAvailable&&CanCastInCurrentForm&&(classes[1]>0||classes[3]>0);
    IEnumerator OfferShield(bool attackTrigger)
    {
        if(!CanOfferShield)yield break;
        shieldAttackTrigger=attackTrigger;shieldDecision=0;shieldPending=true;
        dice=attackTrigger?"攻击已命中 · 等待护盾术选择":"成为魔法飞弹目标 · 等待护盾术选择";
        while(shieldDecision==0)yield return null;
        shieldPending=false;
        if(shieldDecision==1&&combat.TryUseReaction(true)){combat.ActivateShield();history.Insert(0,"玩家消耗反应施放护盾术：AC+5，免疫魔法飞弹，至下一玩家回合开始。");}
        else history.Insert(0,"玩家放弃护盾术，保留反应。");
    }
    void DrawShieldPrompt()
    {
        if(!shieldPending)return;
        Fill(new Rect(0,0,1600,900),new Color(0,0,0,.92f));Surface(new Rect(465,245,670,380),paperTexture);
        GUI.Label(new Rect(495,270,610,50),"护盾术 · 反应施法",heading);
        GUI.Label(new Rect(495,335,610,170),(shieldAttackTrigger?"攻击已命中。选择后才显示命中骰值与总值。":"你成为魔法飞弹的目标。")+"\n是否消耗1次反应？AC提高5，并免疫魔法飞弹，至下个玩家回合开始。法术位资源暂不扣除。",label);
        if(Btn(new Rect(495,535,285,55),"施放护盾术"))shieldDecision=1;
        if(Btn(new Rect(820,535,285,55),"放弃，保留反应"))shieldDecision=2;
    }
}
