using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    const int DancingLightGroup=2;
    readonly DancingLightsEffect dancingLights=new DancingLightsEffect();
    readonly string[] danceColumns={"1","2","3","4"},danceRows={"1","1","1","1"};
    readonly Dictionary<int,string[]> danceMoveInputs=new Dictionary<int,string[]>();
    int danceAppearance,danceCount=1,danceSelected,danceMoveRevision=-1;
    Vector2 dancingScroll;
    bool DanceOnMap(DancingLightPoint p)=>map.Inside(new Vector2Int(p.X,p.Y),1);
    int DanceDistance(DancingLightPoint p)=>map.DistanceToCell(true,new Vector2Int(p.X,p.Y));
    List<DancingLightPoint> DanceDraft(bool moving)
    {
        var points=new List<DancingLightPoint>();
        if(moving){foreach(var active in dancingLights.Points){string[] inputs;if(!danceMoveInputs.TryGetValue(active.Id,out inputs))return null;int x,y;if(!int.TryParse(inputs[0],out x)||!int.TryParse(inputs[1],out y)||x<1||y<1)return null;points.Add(new DancingLightPoint(active.Id,x-1,y-1));}}
        else for(int i=0;i<(danceAppearance==3?1:danceCount);i++){int x,y;if(!int.TryParse(danceColumns[i],out x)||!int.TryParse(danceRows[i],out y)||x<1||y<1)return null;points.Add(new DancingLightPoint(i+1,x-1,y-1));}
        return points;
    }
    string DancingLightsError()
    {
        if(worldRounds<0||worldRounds>long.MaxValue-10)return "世界时间超出舞光术持续时间的可用范围。";
        if(map.IsContained(true))return "被吞咽时无法把光点放在体外；请先脱困。";
        var points=DanceDraft(false);return points==null?"光点列／行须为地图内的正整数。":DancingLightsEffect.PlacementError(points,danceAppearance,DanceOnMap,DanceDistance);
    }
    void EndDancingLights(){dancingLights.Clear();vision.ClearLightGroup(DancingLightGroup);}
    void RefreshDancingLights()
    {
        if(dancingLights.Points.Count==0){vision.ClearLightGroup(DancingLightGroup);return;}
        if(concentrationSpell!="舞光术"){EndDancingLights();return;}
        int lost=dancingLights.RefreshRange(worldRounds,DanceDistance);
        if(lost>0)history.Insert(0,"舞光术："+lost+"个光源因到期、超出120尺或失去20尺内邻点而消失；单个独立光点可继续存在。");
        if(!dancingLights.Active(worldRounds)){EndDancingLights();EndConcentration("舞光术的光点已全部消失");return;}
        var emitters=new List<double[]>();foreach(var point in dancingLights.Points)emitters.Add(new[]{point.X*5.0,point.Y*5.0});
        if(!vision.SetDimLightGroup(DancingLightGroup,emitters,10,0,dancingLights.Expires,worldRounds)){EndDancingLights();EndConcentration("舞光术与黑暗术重叠，被驱散");}
    }
    void ApplyDancingLights()
    {
        var created=DanceDraft(false);if(!dancingLights.Begin(created,danceAppearance,worldRounds,DanceOnMap,DanceDistance))return;
        concentrationSpell="舞光术";concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=dancingLights.Expires;
        RefreshDancingLights();dice=dancingLights.Active(worldRounds)?"舞光术 · 已生效":"舞光术 · 已被驱散";
        history.Insert(0,"舞光术创造"+(danceAppearance==3?"由四个光点合成的中型人形":created.Count+"个"+new[]{"光球","火把光影","灯笼光影","人形"}[danceAppearance])+"，每个光源提供10尺微光，专注至多1分钟。光影不占格、不阻挡移动，不作为生物目标。");
    }
    void SyncDanceMoveDraft()
    {
        if(danceMoveRevision==dancingLights.Revision)return;
        danceMoveInputs.Clear();foreach(var point in dancingLights.Points) danceMoveInputs[point.Id]=new[]{(point.X+1).ToString(),(point.Y+1).ToString()};
        danceMoveRevision=dancingLights.Revision;danceSelected=0;
    }
    void OpenDancingLightsControl(){RefreshDancingLights();danceMoveRevision=-1;SyncDanceMoveDraft();panel="dancingLights";}
    string DanceMoveError()
    {
        bool canAct=combat.Aftermath?combat.PlayerHp>0&&!combat.PlayerDead&&!combat.PlayerParalyzed:combat.PlayerCanAct;
        if(page!="battle"||!map.Locked||!canAct)return "玩家当前不能进行附赠动作。";
        if(rolling||spellTargeting||reactionPending||draggingToken>=0)return "请先完成当前操作。";
        if(!combat.BonusActionAvailable)return "本回合的附赠动作已使用。";
        var points=DanceDraft(true);return points==null?"光点列／行须为地图内的正整数。":dancingLights.MoveError(points,worldRounds,DanceOnMap,DanceDistance);
    }
    void MoveDancingLights()
    {
        RefreshDancingLights();if(DanceMoveError()!="")return;
        if(!dancingLights.TryMove(DanceDraft(true),worldRounds,DanceOnMap,DanceDistance,combat.TryUseAftermathBonusAction))return;
        RefreshDancingLights();history.Insert(0,"玩家消耗1个附赠动作移动舞光术的现存光点，每个至多60尺；不重新施法，不重置持续时间，无需开口。");panel="";
    }
    void DrawDancingLightsMap(Rect board)
    {
        if(!dancingLights.Active(worldRounds))return;
        foreach(var point in dancingLights.Points){var rect=CellRect(board,new Vector2Int(point.X,point.Y));float side=Mathf.Max(12,rect.width*.6f);var marker=new Rect(rect.xMax-side,rect.y,side,side);
            Fill(marker,new Color(.9f,.63f,.14f,.8f));GUI.Label(marker,dancingLights.Combined?"影":point.Id.ToString(),new GUIStyle(centered){fontSize=Mathf.Max(10,(int)(side*.7f)),normal={textColor=Color.white}});
        }
    }
}
