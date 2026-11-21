using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    const int SphereLightGroup=4;
    readonly FlamingSphereEffect flamingSphere=new FlamingSphereEffect();
    readonly List<Vector2Int> sphereWaypoints=new List<Vector2Int>();
    string sphereColumn="1",sphereRow="1";
    Vector2Int SphereCell=>new Vector2Int(flamingSphere.X,flamingSphere.Y);
    void EndFlamingSphere(){flamingSphere.Clear();sphereWaypoints.Clear();vision.ClearLightGroup(SphereLightGroup);}
    void RefreshFlamingSphere()
    {
        if(!flamingSphere.Active(worldRounds)){vision.ClearLightGroup(SphereLightGroup);return;}
        if(vision.ConsumeDispelledLightGroup(SphereLightGroup)||!vision.SetLightGroup(SphereLightGroup,new[]{new[]{flamingSphere.X*5.0,flamingSphere.Y*5.0}},20,40,flamingSphere.Slot,flamingSphere.Expires,worldRounds)){
            EndFlamingSphere();if(concentrationSpell=="炽焰法球")EndConcentration("法球照明与黑暗术重叠，被驱散");
        }
    }
    string FlamingSphereError()
    {
        Vector2Int p;if(!VisionPoint(out p)||map.IsWater(p)||!map.CanPlaceSummon(p,1))return "法球须在地图内未被占据的陆地格生成。";
        if(map.DistanceToCell(true,p)>60)return "法球初始落点超出60尺。";
        if(map.IsContained(true))return "吞咽内部没有可放置法球的战场地面。";
        return worldRounds<0||worldRounds>long.MaxValue-10?"世界时间超出范围。":"";
    }
    void DrawFlamingSphereChoices(){GUILayout.Label("选择60尺内空闲陆地格：列／行。生成当下不造成伤害。附赠动作沿路线滚动30尺，首次碰撞后停下；水域按沟壑，可跨越至多10尺，不能在水域停留。",small);visionColumn=IntegerField(visionColumn,false);visionRow=IntegerField(visionRow,false);}
    void CastFlamingSphere(Vector2Int p,int caster,int slot)
    {
        if(!flamingSphere.Begin(p,slot,8L+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(caster)]),worldRounds))return;
        concentrationSpell="炽焰法球";concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=flamingSphere.Expires;
        RefreshFlamingSphere();if(flamingSphere.Active(worldRounds))IgniteWebArea((x,y)=>x==p.x&&y==p.y);
        history.Insert(0,"炽焰法球生成：占据一个5尺格的火焰区域，不阻挡生物移动；20尺明亮、外围再20尺微光。生成及生物经过时不立即伤害，碰撞及回合末分别结算。");
    }
    bool SphereBody(int actor,out Vector2Int p,out int spaces)
    {
        string name;return HealingHandsTarget(actor,out p,out spaces,out name)&&!((actor==0||actor==-1)&&map.IsContained(actor==0));
    }
    bool SphereEndAvailable(int actor)
    {
        Vector2Int p;int n;return flamingSphere.EndAvailable(actor,terrainTurn,worldRounds)&&SphereBody(actor,out p,out n)&&BattleMap.BodyDistance(SphereCell,1,p,n)<=5;
    }
    List<int> SphereCollisions(Vector2Int p)
    {
        var targets=new List<int>();foreach(int actor in BreathActors()){Vector2Int cell;int n;if(SphereBody(actor,out cell,out n)&&BattleMap.Overlap(p,1,cell,n))targets.Add(actor);}return targets;
    }
    IEnumerator EndFlamingSphereTurn(int actor)
    {
        if(SphereEndAvailable(actor)&&flamingSphere.ClaimEnd(actor,terrainTurn,worldRounds))yield return SphereDamage(new[]{actor},"回合末");
    }
    IEnumerator SphereDamage(IEnumerable<int> selected,string reason)
    {
        if(!flamingSphere.Active(worldRounds))yield break;int count=flamingSphere.Slot;long dc=flamingSphere.Dc;var origin=SphereCell;
        var targets=new List<int>();foreach(int actor in selected){Vector2Int p;int n;if(SphereBody(actor,out p,out n))targets.Add(actor);}
        if(targets.Count==0)yield break;var failed=new HashSet<int>();string saved=panel;panel="";outcomeDeferDepth++;
        try{
            foreach(int target in targets)yield return RollMappedAbilitySave(target,0,dc,failed,"炽焰法球 · "+reason,1,origin);
            int raw=0;var rolls=new int[count];for(int i=0;i<count;i++){rolls[i]=UnityEngine.Random.Range(1,7);raw+=rolls[i];}
            yield return PresentRoll(6,rolls,"炽焰法球："+count+"d6 = "+raw+" 火焰；豁免成功减半");
            var reduction=new RayDamage();yield return ReduceRayDamage(true,raw,"火焰","炽焰法球",reduction);
            bool oldFire=webAreaFire;webAreaFire=true;int self=0;
            try{foreach(int target in targets){int damage=ApplyMappedAreaDamage(target,0,failed.Contains(target)?reduction.Amount:reduction.Amount/2,5,"炽焰法球 · "+reason,failed.Contains(target));if(target==0)self=damage;}}
            finally{webAreaFire=oldFire;}
            if(self>0)yield return MaintainConcentration(self);ReconcileFallenRescue();RefreshBestialSpirit();NoticeCastingInterruption();
        }finally{outcomeDeferDepth--;panel=saved;CheckBattleOutcome();}
    }
    void OpenFlamingSphereControl(){sphereWaypoints.Clear();sphereColumn=(flamingSphere.X+1).ToString();sphereRow=(flamingSphere.Y+1).ToString();panel="flamingSphere";}
    bool SphereInput(out Vector2Int p){int x,y;bool valid=int.TryParse(sphereColumn,out x)&int.TryParse(sphereRow,out y);p=new Vector2Int(x-1,y-1);return valid&&map.Inside(p,1);}
    string SphereMoveError(out List<Vector2Int> landings,out int feet)
    {
        landings=null;feet=0;if(!flamingSphere.CanMove(combat.Round,worldRounds))return "法球已结束或本回合碰撞后不能再移动。";
        if(page!="battle"||!map.Locked||rolling||reactionPending||spellTargeting||draggingToken>=0)return "请先完成当前操作。";
        if(!PlayerTurnAllowed||combat.PlayerHp<=0||combat.PlayerDead||combat.PlayerParalyzed||combat.PlayerFeignDeath||!combat.BonusActionAvailable||summonTurns.Active)return "当前不能使用玩家附赠动作。";
        return FlamingSphereEffect.Route(SphereCell,sphereWaypoints,p=>map.Inside(p,1),map.IsWater,out landings,out feet)?"":"请添加总长不超过30尺的陆地路径点；每段水域至多跨越10尺。";
    }
    void DrawFlamingSphereControl()
    {
        Surface(new Rect(350,110,900,745),paperTexture);if(Btn(new Rect(1140,125,90,46),"关闭",!rolling)){panel="";return;}
        GUILayout.BeginArea(new Rect(375,190,390,620));GUILayout.Label("滚动炽焰法球",heading);GUILayout.Label("本次附赠动作沿路径滚动至多30尺；首次撞入生物占格时造成豁免伤害并停止。可以在施法当回合使用。无需再施法或开口，持续时间不刷新。",small);
        sphereColumn=IntegerField(sphereColumn,false);sphereRow=IntegerField(sphereRow,false);Vector2Int p;
        if(GUILayout.Button("添加路径点",button)&&SphereInput(out p)&&sphereWaypoints.Count<6&&(sphereWaypoints.Count==0?p!=SphereCell:p!=sphereWaypoints[sphereWaypoints.Count-1]))sphereWaypoints.Add(p);
        if(GUILayout.Button("清空路线",button))sphereWaypoints.Clear();
        for(int i=0;i<sphereWaypoints.Count;i++)GUILayout.Label((i+1)+". ("+(sphereWaypoints[i].x+1)+","+(sphereWaypoints[i].y+1)+")",small);
        List<Vector2Int> landings;int feet;string error=SphereMoveError(out landings,out feet);GUILayout.Label(error==""?"路线总长 "+feet+" 尺；碰撞后剩余路程作废。":error,small);
        bool old=GUI.enabled;GUI.enabled=old&&error=="";if(GUILayout.Button("滚动法球（附赠动作）",button))StartCoroutine(MoveFlamingSphere());GUI.enabled=old;GUILayout.EndArea();
        var board=new Rect(800,250,400,400);DrawMap(board,false);var ev=Event.current;var pointer=BoardPointer();if(!rolling&&ev.type==EventType.MouseDown&&ev.button==0&&board.Contains(pointer)){float step=board.width/map.Size;sphereColumn=((int)((pointer.x-board.x)/step)+1).ToString();sphereRow=((int)((pointer.y-board.y)/step)+1).ToString();ev.Use();}foreach(var point in sphereWaypoints)if(map.Inside(point,1))Fill(CellRect(board,point),new Color(1,.6f,.1f,.5f));
    }
    IEnumerator MoveFlamingSphere()
    {
        List<Vector2Int> route;int feet;if(SphereMoveError(out route,out feet)!=""||!combat.TryUseAftermathBonusAction())yield break;
        bool busy=rolling;rolling=true;panel="";
        try{foreach(var p in route){
            if(!flamingSphere.Active(worldRounds))break;var hits=SphereCollisions(p);flamingSphere.Place(p,hits.Count>0,combat.Round);RefreshFlamingSphere();if(!flamingSphere.Active(worldRounds))break;
            IgniteWebArea((x,y)=>x==p.x&&y==p.y);yield return new WaitForSecondsRealtime(fastAnimation?.04f:.12f);
            if(hits.Count>0){yield return SphereDamage(hits,"滚动碰撞");break;}
        }}finally{rolling=busy;}
    }
    void DrawFlamingSphereMap(Rect board)
    {
        if(!flamingSphere.Active(worldRounds))return;var r=CellRect(board,SphereCell);Fill(r,new Color(1,.22f,.02f,.8f));GUI.Label(r,"火",centered);
    }
}
