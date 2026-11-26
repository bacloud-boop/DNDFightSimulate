using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    sealed class HitRetaliation {public int Frost,FireType=-1;public readonly List<int> PlayerDamage=new List<int>();}
    bool retaliationOrderPending,retaliationOrderReady,retaliationFireFirst;
    HitRetaliation CaptureHitRetaliation(int attacker,bool melee,int? distance=null)
    {
        var result=new HitRetaliation{Frost=AgathysRetaliation(melee)};
        var shield=fireShield.Current(worldRounds);Vector2Int p;int n;
        if(melee&&shield!=null&&map.SameScene(0,attacker)&&map.SceneBody(attacker,out p,out n)&&(distance??BattleMap.BodyDistance(map.Player,map.PlayerSpaces,p,n))<=5)result.FireType=shield.DamageType;
        return result;
    }
    void DrawRetaliationOrder()
    {
        Surface(new Rect(440,235,720,400),paperTexture);GUILayout.BeginArea(new Rect(470,265,660,340));GUILayout.Label("命中反伤 · 选择结算先后",heading);
        GUILayout.Label("火焰护盾与黯冰狱铠同时触发。先后顺序可能影响存活、临时生命和其他效果。",small);
        if(GUILayout.Button("先火焰护盾 · 2d8",button)){retaliationFireFirst=true;retaliationOrderReady=true;}
        if(GUILayout.Button("先黯冰狱铠 · 固定寒冷",button)){retaliationFireFirst=false;retaliationOrderReady=true;}
        GUILayout.EndArea();
    }
    IEnumerator ResolveHitRetaliation(int attacker,HitRetaliation result)
    {
        bool fireFirst=result.FireType>=0;
        if(fireFirst&&result.Frost>0){retaliationOrderPending=true;retaliationOrderReady=false;try{while(!retaliationOrderReady)yield return null;}finally{retaliationOrderPending=false;}fireFirst=retaliationFireFirst;}
        outcomeDeferDepth++;
        try{for(int i=0;i<2;i++){
            bool fire=i==0?fireFirst:!fireFirst;if(fire?result.FireType<0:result.Frost<=0)continue;
            if(!BanishLiving(attacker))continue;
            var raw=new long[13];int type=fire?result.FireType:4;
            if(fire){var rolls=new[]{Random.Range(1,9),Random.Range(1,9)};int sum=rolls[0]+rolls[1];yield return PresentRoll(8,rolls,"火焰护盾反伤：2d8 = "+sum+" "+DamageTypes.Names[type]+"；不因原攻击暴击翻倍");raw[type]=sum;
                var reduced=new RayDamage();yield return ReduceRayPacket(true,raw,true,"火焰护盾反伤",reduced);raw=reduced.Raw;
            }else raw[4]=result.Frost;
            int amount=ApplyMappedDamagePacket(attacker,0,raw);if(attacker==0)result.PlayerDamage.Add(amount);
            history.Insert(0,(fire?"火焰护盾":"黯冰狱铠")+"对"+TransitActorName(attacker)+"反伤 "+amount+"，不消耗反应。");
            yield return DrainUndeadSaves();
        }}finally{outcomeDeferDepth--;}
    }
    IEnumerator MaintainRetaliationConcentration(HitRetaliation result){foreach(int amount in result.PlayerDamage)if(amount>0)yield return MaintainConcentration(amount);}
}
