using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly StinkingCloudEffect stinkingCloud=new StinkingCloudEffect();
    bool ActorDiving(int actor)=>actor==0?map.PlayerAquatic.Submerged:actor==-1?map.MonsterAquatic.Submerged:actor==BattleMap.RescueId?map.RescueAquatic.Submerged:FindSummon(actor)!=null&&FindSummon(actor).Aquatic.Submerged;
    bool Nauseated(int actor)=>!Feigning(actor)&&stinkingCloud.Poisoned(actor,terrainTurn,worldRounds);
    void InitializeStinkingCloud()
    {
        combat.ExternalPoisoned=player=>Nauseated(player?0:-1);combat.ExternalPoisonClear=player=>stinkingCloud.ClearPoison(player?0:-1);combat.ActionBlock=player=>Nauseated(player?0:-1);
        rescueCarrier.ActionBlock=()=>Nauseated(BattleMap.RescueId);
    }
    void EndStinkingCloud(){stinkingCloud.EndCloud();startStatuses.RemoveAll(e=>e.Kind==4);}
    string StinkingCloudError()
    {
        Vector2Int p;if(!VisionPoint(out p)||p.x<0||p.y<0||p.x>=map.Size||p.y>=map.Size)return "云团中心须为地图内整数坐标。";
        if(map.DistanceToCell(true,p)>90)return "臭云术中心超出90尺。";
        if(map.IsContained(true))return "吞咽内部不能在外部制造臭云。";
        return worldRounds<0||worldRounds>long.MaxValue-10?"世界时间超出范围。":"";
    }
    void DrawStinkingCloudChoices()
    {
        visionColumn=IntegerField(visionColumn,false);visionRow=IntegerField(visionRow,false);
        GUILayout.Label("90尺内一点，20尺半径重度遮蔽，专注至多1分钟。陆地与水面生物在云中开始回合时体质豁免，失败中毒至本回合结束，期间不能使用动作或附赠动作；仍可移动和反应。潜水单位不接受云团的新豁免，潜水观察者不受气体遮蔽。已产生的中毒不会因离开、潜水或结束专注而提前消失。",small);
        GUILayout.Label("中毒可以被正常解除，豁免使用避免中毒的优势。普通中毒仅影响攻击与属性检定，不阻止动作；只有此法术的中毒附带动作限制。造风术与云团相交即驱散整团，已产生的中毒仍保留；室外风暴开关只用于召雷术。",small);
    }
    void CastStinkingCloud(Vector2Int p,int caster)
    {
        if(!stinkingCloud.Begin(p.x,p.y,map.SceneOf(0),8L+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(caster)]),worldRounds))return;
        concentrationSpell="臭云术";concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=stinkingCloud.Expires;
        history.Insert(0,"臭云术：20尺半径气体重度遮蔽，开始回合才作体质豁免；潜水不触发，创建与单纯进入不立即触发。专注至多1分钟。");
    }
    void QueueStinkingStart(int actor)
    {
        Vector2Int p;int n;string name;if(ActorDiving(actor)||map.SceneOf(actor)!=stinkingCloud.Scene||!HealingHandsTarget(actor,out p,out n,out name)||(actor==0||actor==-1)&&map.IsContained(actor==0))return;
        if(stinkingCloud.Intersects(p.x,p.y,n,worldRounds)&&stinkingCloud.ClaimStart(actor,terrainTurn,worldRounds))startStatuses.Add(new StartStatus{Actor=actor,Kind=4,Revision=stinkingCloud.Revision});
    }
    IEnumerator ResolveStinkingSave(int actor,long revision)
    {
        if(revision!=stinkingCloud.Revision||!stinkingCloud.Active(worldRounds)||ActorDiving(actor))yield break;
        Vector2Int p;int n;string name;if(!HealingHandsTarget(actor,out p,out n,out name))yield break;
        if(actor>0&&Undead(FindSummon(actor))!=null){history.Insert(0,name+"免疫中毒，臭云术不能限制其动作。");yield break;}
        var failed=new HashSet<int>();yield return RollMappedAbilitySave(actor,0,stinkingCloud.Dc,failed,"臭云术",2,avoidsPoisoned:true);
        if(failed.Contains(actor)&&!Feigning(actor)){stinkingCloud.Poison(actor,terrainTurn,worldRounds);history.Insert(0,name+"臭云术豁免失败，中毒至本回合结束，不能使用动作或附赠动作。");}
        else history.Insert(0,name+"未受臭云术中毒影响。");
    }
    bool StinkingBlocks(bool viewerPlayer,double ax,double ay,double bx,double by)
    {
        if(!stinkingCloud.Active(worldRounds)||stinkingCloud.Scene!=map.SceneOf(viewerPlayer?0:-1)||ActorDiving(viewerPlayer?0:-1))return false;
        double dx=bx-ax,dy=by-ay,length=dx*dx+dy*dy,t=length==0?0:System.Math.Max(0,System.Math.Min(1,((stinkingCloud.X*5-ax)*dx+(stinkingCloud.Y*5-ay)*dy)/length));double x=ax+t*dx-stinkingCloud.X*5,y=ay+t*dy-stinkingCloud.Y*5;return x*x+y*y<=400.000001;
    }
    void DrawStinkingCloudArea(Rect board)
    {
        Vector2Int p=Vector2Int.zero;bool preview=panel=="spells"&&selectedSpellName=="Stinking Cloud"&&VisionPoint(out p);
        for(int y=0;y<map.Size;y++)for(int x=0;x<map.Size;x++){if(stinkingCloud.Scene==map.Scene&&stinkingCloud.Cell(x,y,worldRounds))Fill(CellRect(board,new Vector2Int(x,y)),new Color(.65f,.7f,.15f,.27f));long dx=(long)x-p.x,dy=(long)y-p.y;if(preview&&dx*dx+dy*dy<=16)Fill(CellRect(board,new Vector2Int(x,y)),new Color(.8f,.8f,.25f,.3f));}
    }
}
