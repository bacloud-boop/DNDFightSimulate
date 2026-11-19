using System;
using System.Collections.Generic;

public struct DancingLightPoint
{
    public readonly int Id,X,Y;
    public DancingLightPoint(int id,int x,int y){Id=id;X=x;Y=y;}
    public static int Distance(DancingLightPoint a,DancingLightPoint b){double x=(double)a.X-b.X,y=(double)a.Y-b.Y;return (int)Math.Round(Math.Sqrt(x*x+y*y)*5,MidpointRounding.ToEven);}
}
public sealed class DancingLightsEffect
{
    readonly List<DancingLightPoint> points=new List<DancingLightPoint>();
    public IReadOnlyList<DancingLightPoint> Points=>points.ToArray();
    public int Appearance {get;private set;}
    public bool Combined=>Appearance==3;
    public long Expires {get;private set;}
    public int Revision {get;private set;}
    public bool Active(long now)=>points.Count>0&&now>=0&&now<Expires;
    static bool HasNeighbor(DancingLightPoint point,IReadOnlyList<DancingLightPoint> values){foreach(var other in values)if(other.Id!=point.Id&&DancingLightPoint.Distance(point,other)<=20)return true;return false;}
    public static string PlacementError(IReadOnlyList<DancingLightPoint> values,int appearance,Func<DancingLightPoint,bool> onMap,Func<DancingLightPoint,int> distance)
    {
        if(values==null||appearance<0||appearance>3||values.Count<1||values.Count>4||appearance==3&&values.Count!=1)return "独立光点须为1～4个；合并人形填写一个位置。";
        var ids=new HashSet<int>();foreach(var point in values){
            if(point.Id<1||point.Id>4||!ids.Add(point.Id))return "光点编号无效或重复。";
            if(onMap==null||!onMap(point))return "所有光点须位于地图内。";
            if(distance==null||distance(point)>120)return "所有光点须在施法者120尺内。";
        }
        // Project rule: one remaining independent light is exempt from neighbor proximity.
        if(values.Count>1)foreach(var point in values)if(!HasNeighbor(point,values))return "有多个光点时，每个光点须距至少另一个光点不超过20尺。";
        return "";
    }
    public bool Begin(IReadOnlyList<DancingLightPoint> values,int appearance,long now,Func<DancingLightPoint,bool> onMap,Func<DancingLightPoint,int> distance)
    {
        if(now<0||now>long.MaxValue-10||PlacementError(values,appearance,onMap,distance)!="")return false;
        points.Clear();points.AddRange(values);Appearance=appearance;Expires=now+10;Revision++;return true;
    }
    public string MoveError(IReadOnlyList<DancingLightPoint> destinations,long now,Func<DancingLightPoint,bool> onMap,Func<DancingLightPoint,int> distance)
    {
        if(!Active(now))return "当前没有持续中的舞光术。";
        string error=PlacementError(destinations,Appearance,onMap,distance);if(error!="")return error;
        if(destinations.Count!=points.Count)return "只能移动现存光点，不能增加或恢复已经消失的光点。";
        foreach(var target in destinations){bool found=false;foreach(var origin in points)if(origin.Id==target.Id){found=true;if(DancingLightPoint.Distance(origin,target)>60)return "每个光点本次最多移动60尺。";break;}if(!found)return "光点已消失，请重新选择现存光点。";}
        return "";
    }
    public bool TryMove(IReadOnlyList<DancingLightPoint> destinations,long now,Func<DancingLightPoint,bool> onMap,Func<DancingLightPoint,int> distance,Func<bool> useBonus)
    {
        if(MoveError(destinations,now,onMap,distance)!=""||useBonus==null||!useBonus())return false;
        points.Clear();points.AddRange(destinations);Revision++;return true;
    }
    public int RefreshRange(long now,Func<DancingLightPoint,int> distance)
    {
        int before=points.Count;if(before==0)return 0;
        if(!Active(now)||distance==null){Clear();return before;}
        var retained=new List<DancingLightPoint>();foreach(var point in points)if(distance(point)<=120)retained.Add(point);
        if(retained.Count>1){var paired=new List<DancingLightPoint>();foreach(var point in retained)if(HasNeighbor(point,retained))paired.Add(point);retained=paired;}
        if(retained.Count!=before){points.Clear();points.AddRange(retained);Revision++;}return before-points.Count;
    }
    public void Clear(){points.Clear();Expires=0;Revision++;}
}
