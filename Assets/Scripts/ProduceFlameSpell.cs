using System.Collections;
using UnityEngine;
public sealed partial class TrainingGame
{
    const int ProduceLightGroup=5;
    readonly ProduceFlameEffect produceFlame=new ProduceFlameEffect();
    int produceTarget=-1;
    void EndProduceFlame(){produceFlame.Clear();vision.ClearLightGroup(ProduceLightGroup);}
    string ProduceFlameError()=>worldRounds<0||worldRounds>long.MaxValue-ProduceFlameEffect.Duration?"世界时间超出范围。":"";
    void CastProduceFlame(int caster)
    {
        if(!produceFlame.Begin(caster,worldRounds))return;
        RefreshProduceFlame();dice=produceFlame.Active(worldRounds)?"燃火术 · 掌中火焰已生成":"燃火术 · 被黑暗术驱散";
        history.Insert(0,dice+"；投射火焰消耗魔法动作，不消耗附赠动作，不会结束仍有效的火焰。");
    }
    void RefreshProduceFlame()
    {
        if(!produceFlame.Active(worldRounds)){EndProduceFlame();return;}
        if(vision.ConsumeDispelledLightGroup(ProduceLightGroup)){EndProduceFlame();history.Insert(0,"燃火术被黑暗术驱散。");return;}
        if(map.IsContained(true)){vision.HideLightGroup(ProduceLightGroup);return;}
        double x=(map.Player.x+(map.PlayerSpaces-1)*.5)*5,y=(map.Player.y+(map.PlayerSpaces-1)*.5)*5;
        if(!vision.SetLightGroup(ProduceLightGroup,new[]{new[]{x,y}},20,40,0,produceFlame.Expires,worldRounds)){EndProduceFlame();history.Insert(0,"燃火术与黑暗术重叠，被驱散。");}
    }
    string ProduceHurlError()
    {
        if(!produceFlame.Active(worldRounds))return "请先以附赠动作施放燃火术。";
        if(rolling||spellTargeting||reactionPending||draggingToken>=0||summonTurns.Active)return "请等待当前操作结束。";
        if(!combat.MagicActionAvailable)return "投射需要战斗中的一个魔法动作。";
        Vector2Int p;int n;string name;if(!SummonTarget(produceTarget,out p,out n,out name))return "请选择存活的目标。";
        if(produceTarget==-1&&map.IsContained(false))return "目标被吞咽，体外无法攻击。";
        if(map.IsContained(true)&&produceTarget!=0&&produceTarget!=-1)return "吞咽内部无法攻击体外目标。";
        return BattleMap.BodyDistance(map.Player,map.PlayerSpaces,p,n)>60?"目标超出60尺。":"";
    }
    void DrawProduceFlameChoices()
    {
        GUILayout.Label("附赠动作生成掌中火焰，非专注10分钟；20尺明亮、外围20尺微光。持有时无热、不引燃物件。生成后可反复用魔法动作投射，60尺远程法术攻击，1d8火焰，按总等级5／11／17变为2／3／4d8。投射不会结束火焰，也不是再次施法。",small);
        if(!produceFlame.Active(worldRounds))return;
        GUILayout.Label("当前火焰剩余 "+((produceFlame.Expires-worldRounds)*6)+" 秒",small);
        foreach(int actor in new[]{0,-1})if(GUILayout.Button((produceTarget==actor?"✓ ":"")+TransitActorName(actor),button))produceTarget=actor;
        foreach(var unit in map.SceneSummons)if(unit.Alive&&GUILayout.Button((produceTarget==unit.Id?"✓ ":"")+unit.Name,button))produceTarget=unit.Id;
        string error=ProduceHurlError();GUILayout.Label(error==""?"消耗1个魔法动作，保留火焰。":error,small);
        bool previous=GUI.enabled;GUI.enabled=previous&&error=="";
        if(GUILayout.Button("投射火焰 · 魔法动作",button)){panel="";StartCoroutine(HurlProduceFlame());}GUI.enabled=previous;
    }
    IEnumerator HurlProduceFlame()
    {
        if(ProduceHurlError()!=""||!combat.TryUseMagicAction())yield break;
        int caster=produceFlame.Caster,target=produceTarget;rolling=true;long token=senses.Token(true,worldRounds);
        try{yield return ResolvePlayerStarryWisp(caster,target,"燃火术",false,"火焰");}
        finally{BreakOrdinaryInvisibility(true,"进行攻击",token);rolling=false;}
    }
}
