using System.Collections;
using UnityEngine;
public sealed partial class TrainingGame
{
    void DrawPlayerWebEscape()
    {
        if(MovementFreedom(0)||!web.Restrained(0,worldRounds))return;bool old=GUI.enabled;
        GUI.enabled=old&&!rolling&&!reactionPending&&!spellTargeting&&(combat.Aftermath?combat.PlayerHp>0&&!combat.PlayerDead&&!combat.PlayerFeignDeath&&!combat.PlayerParalyzed&&combat.ActionsRemaining>0:combat.ActionAvailable);
        if(GUILayout.Button("挣脱蛛网 · 力量（运动）DC "+web.Dc+"（1动作）",button)){panel="";StartCoroutine(RollSkillCheck((int)web.Dc,true));}
        GUI.enabled=old;GUILayout.Label("只解除蛛网造成的束缚；不需要先完成背景／技能配置，也不修改手动检定DC。",small);
    }
    void DrawRescueWebEscape()
    {
        if(MovementFreedom(BattleMap.RescueId)||!web.Restrained(BattleMap.RescueId,worldRounds))return;bool old=GUI.enabled;
        GUI.enabled=old&&!rolling&&rescueCarrier.Conscious&&rescueCarrier.TurnActive&&rescueCarrier.ActionsRemaining>0;
        if(GUILayout.Button("挣脱蛛网（救援者动作） · DC "+web.Dc,button))StartCoroutine(EscapeRescueWeb());GUI.enabled=old;
    }
    IEnumerator EscapeRescueWeb()
    {
        if(MovementFreedom(BattleMap.RescueId)||!web.Restrained(BattleMap.RescueId,worldRounds)||!rescueCarrier.UseAction())yield break;
        bool busy=rolling;string saved=panel;rolling=true;panel="";
        try{
            long modifier=AcidSplash.Modifier(rescueCarrier.Strength)+(long)rescueCarrier.SkillTier(0)*rescueCarrier.ProficiencyBonus+rescueCarrier.RevivalPenalty;
            int raw=Random.Range(1,21);yield return PresentRoll(20,new[]{raw},"救援者挣脱蛛网 · 力量（运动）："+raw+" + "+modifier+" / DC "+web.Dc);
            if(raw+modifier>=web.Dc){web.Release(BattleMap.RescueId);SyncTimedBuffs();history.Insert(0,"救援者挣脱蛛网，消耗1动作；已用移动力不返还。");}else history.Insert(0,"救援者未挣脱蛛网，消耗1动作。");
        }finally{rolling=busy;panel=saved;}
    }
}
