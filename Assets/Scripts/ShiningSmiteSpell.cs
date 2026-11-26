using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    const int ShiningLightGroup=6;
    readonly ShiningSmiteEffect shiningSmite=new ShiningSmiteEffect();
    bool ShiningOutlined(int target)=>shiningSmite.Outlined(target,worldRounds);
    void EndShiningSmite(){shiningSmite.Clear();vision.ClearLightGroup(ShiningLightGroup);}
    void BeginShiningSmite(int target,SmiteHitResult hit)
    {
        if(hit.ShiningSlot<2||concentrationSpell!="闪耀斩")return;
        if(!shiningSmite.Begin(target,hit.ShiningSlot,worldRounds))return;
        RefreshShiningSmite();history.Insert(0,"闪耀斩：目标发出5尺明亮光照；攻击目标具有优势，不能受益于隐形。专注至多1分钟；光照驱散不影响已经结算的命中伤害。");
    }
    void RefreshShiningSmite()
    {
        if(!shiningSmite.Active(worldRounds)){vision.ClearLightGroup(ShiningLightGroup);return;}
        if(vision.ConsumeDispelledLightGroup(ShiningLightGroup)){EndShiningSmite();if(concentrationSpell=="闪耀斩")EndConcentration("闪耀斩被黑暗术驱散");return;}
        Vector2Int cell;int spaces;int target=shiningSmite.Target;
        if(!map.OnScene(target)||!map.SceneBody(target,out cell,out spaces)){vision.HideLightGroup(ShiningLightGroup);return;}
        if(target==0||target==-1){bool player=target==0;if(map.IsContained(player)){vision.HideLightGroup(ShiningLightGroup);return;}if(map.IsCarried(player)){cell=map.RescuePosition.Value;spaces=map.RescueSpaces;}else if(player)spaces=map.PlayerSpaces;}
        var points=new List<double[]>();AddFaeriePoints(points,cell,spaces);
        if(!vision.SetLightGroup(ShiningLightGroup,points,5,5,shiningSmite.Slot,shiningSmite.Expires,worldRounds)){
            EndShiningSmite();if(concentrationSpell=="闪耀斩")EndConcentration("闪耀斩光照与黑暗术重叠，低环法术被驱散");
        }
    }
}
