using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    const int FireShieldLightGroup=8;
    readonly FireShieldEffects fireShield=new FireShieldEffects();bool fireShieldCold;
    bool FireShieldResists(bool player,int type)=>player&&fireShield.Current(worldRounds)?.Resistance==type;
    string FireShieldError()=>FireShieldEffects.ValidTime(worldRounds)?"":"世界时间超出范围。";
    void ClearFireShield(){fireShield.Clear();vision.ClearLightGroup(FireShieldLightGroup);}
    void DrawFireShieldChoices()
    {
        fireShieldCold=GUILayout.SelectionGrid(fireShieldCold?1:0,new[]{"暖盾：抗寒冷／反伤火焰","寒盾：抗火焰／反伤寒冷"},1,button)==1;
        GUILayout.Label("非专注10分钟；5尺内生物以近战攻击检定命中你时，自动反伤2d8，不消耗反应。远程攻击或5尺外的长触及攻击不触发。发出10尺明亮与外围10尺微光。",small);
        var active=fireShield.Current(worldRounds);if(active!=null)GUILayout.Label("当前："+(active.Cold?"寒盾":"暖盾")+" · 剩余 "+(active.Expires-worldRounds)+" 回合；同名效果不叠加，等强度采用最新施法。",small);
    }
    void CastFireShield(int slot,bool cold)
    {
        if(fireShield.Add(slot,cold,worldRounds)==null)return;RefreshFireShield();
        history.Insert(0,"火焰护盾："+(cold?"寒盾，火焰抗性，反伤2d8寒冷":"暖盾，寒冷抗性，反伤2d8火焰")+"；无需专注，持续10分钟。");
    }
    void RefreshFireShield()
    {
        var active=fireShield.Current(worldRounds);if(active==null){vision.ClearLightGroup(FireShieldLightGroup);return;}
        if(vision.ConsumeDispelledLightGroup(FireShieldLightGroup)){fireShield.End(active.Id);vision.ClearLightGroup(FireShieldLightGroup);return;}
        if(!map.OnScene(0)||map.IsContained(true)){vision.HideLightGroup(FireShieldLightGroup);return;}
        Vector2Int cell=map.Player;int n=map.PlayerSpaces;if(map.IsCarried(true)&&map.RescuePosition.HasValue){cell=map.RescuePosition.Value;n=map.RescueSpaces;}
        double x=(cell.x+(n-1)*.5)*5,y=(cell.y+(n-1)*.5)*5;
        if(!vision.SetLightGroup(FireShieldLightGroup,new[]{new[]{x,y}},10,20,active.Slot,active.Expires,worldRounds)){fireShield.End(active.Id);vision.ClearLightGroup(FireShieldLightGroup);}
    }
}
