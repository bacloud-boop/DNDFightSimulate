using System.Collections;
using UnityEngine;

public sealed partial class TrainingGame
{
    bool temporaryHpPending,temporaryHpPlayer;
    int temporaryHpDecision,temporaryHpOld,temporaryHpNew;
    string temporaryHpSource="";
    IEnumerator OfferTemporaryHp(bool player,int amount,string source)
    {
        int old=player?combat.PlayerTemporaryHp:combat.MonsterTemporaryHp;
        if(old>0){
            temporaryHpPlayer=player;temporaryHpOld=old;temporaryHpNew=amount;temporaryHpSource=source;
            temporaryHpDecision=0;temporaryHpPending=true;
            while(temporaryHpDecision==0)yield return null;
            temporaryHpPending=false;
        }else temporaryHpDecision=2;
        bool takeNew=temporaryHpDecision==2;
        combat.ChooseTemporaryHp(player,amount,takeNew);
        history.Insert(0,source+"："+(player?"玩家":"木桩")+(takeNew?"采用新临时生命 "+amount:"保留已有临时生命 "+old)+"，两者不叠加。");
    }
    void DrawTemporaryHpChoice()
    {
        if(!temporaryHpPending)return;
        Fill(new Rect(0,0,1600,900),new Color(0,0,0,.92f));Surface(new Rect(465,245,670,380),paperTexture);
        GUI.Label(new Rect(495,270,610,50),temporaryHpSource+" · 临时生命选择",heading);
        GUI.Label(new Rect(495,335,610,150),(temporaryHpPlayer?"玩家":"木桩")+"已有 "+temporaryHpOld+" 点临时生命，本次获得 "+temporaryHpNew+" 点。\n选择保留旧值或采用新值；不会相加，也不会恢复实际生命。",label);
        if(Btn(new Rect(495,535,285,55),"保留旧值 · "+temporaryHpOld))temporaryHpDecision=1;
        if(Btn(new Rect(820,535,285,55),"采用新值 · "+temporaryHpNew))temporaryHpDecision=2;
    }
}
