using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly WebEffect web=new WebEffect();
    sealed class WebEntry {public int Actor;public long Revision;public bool Save,Fire;}
    readonly Queue<WebEntry> webEntries=new Queue<WebEntry>();
    long terrainTurn;int terrainActor;bool webOrderPending;int webOrderChoice;
    bool webAreaFire;
    ArmorSaveRoll RollRescueSave(int ability,bool advantage=false,bool disadvantage=false)
    {
        advantage|=ability==3&&BeaconHope(BattleMap.RescueId);
        disadvantage|=ability==1&&rescueCarrier.Restrained;
        return RollSavingThrowDice(advantage,disadvantage);
    }
    void InitializeWeb()
    {
        combat.TurnStarted=StartWebTurn;map.ActorTurnStarted=StartWebTurn;map.SummonAdded=unit=>{unit.FireExposure=()=>IgniteWebActor(unit.Id);HookBondSummon(unit);};
        combat.FireExposure=player=>IgniteWebActor(player?0:-1);rescueCarrier.FireExposure=()=>IgniteWebActor(BattleMap.RescueId);
        rescueCarrier.TurnStarted=()=>StartWebTurn(BattleMap.RescueId);
        vision.LightObscuration=(x,y,now)=>web.Cell((int)Math.Floor((x+2.5)/5),(int)Math.Floor((y+2.5)/5),now);
    }
    bool WebBody(int actor,out Vector2Int cell,out int spaces)
    {
        cell=Vector2Int.zero;spaces=0;string name;if(map.SceneOf(actor)!=0||!HealingHandsTarget(actor,out cell,out spaces,out name))return false;
        return !((actor==0||actor==-1)&&(map.IsContained(actor==0)||map.IsCarried(actor==0)));
    }
    bool InWeb(int actor){Vector2Int p;int n;return WebBody(actor,out p,out n)&&web.Intersects(p.x,p.y,n,worldRounds);}
    void SyncWeb()
    {
        web.Refresh(worldRounds);
        foreach(int actor in BreathActors())if(web.Restrained(actor,worldRounds)&&!InWeb(actor))web.Release(actor);
        rescueCarrier.Restrained=!MovementFreedom(BattleMap.RescueId)&&web.Restrained(BattleMap.RescueId,worldRounds);
        combat.MonsterRestrained=Entangled(false);foreach(var unit in map.Summons)unit.Restrained=MagicalRestraint(unit.Id);
    }
    void StartWebTurn(int actor)=>StartTerrainTurn(actor,false);
    void StartTerrainTurn(int actor,bool independent)
    {
        map.ViewActorScene(actor);acidArrows.StartTurn(actor);if(actor==0){StartFingerCasterTurn();heatMetal.CasterTurnStarts();}
        bool shared=!independent&&actor>0&&IsSteed(FindSummon(actor))&&ControlledSteed;
        if(!shared){terrainActor=actor;terrainTurn++;}web.StartTurn(actor);SyncWeb();
        QueueWebEntry(actor,true);QueueHeroismStart(actor);QueueAuraVitalityStart(actor);SyncTimedBuffs();
    }
    void QueueWebEntry(int actor,bool starting)
    {
        Vector2Int cell;int n;if(!WebBody(actor,out cell,out n))return;QueueWebAt(actor,cell,n,starting);
    }
    void QueueWebAt(int actor,Vector2Int cell,int n,bool starting)
    {
        if(!web.Intersects(cell.x,cell.y,n,worldRounds))return;
        bool save=web.TrySave(actor,terrainTurn,worldRounds),fire=starting&&web.BurningBody(cell.x,cell.y,n,worldRounds);
        if(save||fire)webEntries.Enqueue(new WebEntry{Actor=actor,Revision=web.Revision,Save=save,Fire=fire});
    }
    void ObserveWebMovement(int actor,Vector2Int from,Vector2Int to,int oldSpaces,int spaces,bool landing)
    {
        if(!web.Active(worldRounds))return;
        Vector2Int actual;int n;if(!WebBody(actor,out actual,out n))return;
        var previous=from;int previousSpaces=oldSpaces;var path=landing?new List<Vector2Int>{to}:BattleMap.MovementAnchors(from,to);
        if(from==to&&oldSpaces!=spaces)path.Add(to);
        foreach(var cell in path){if(!web.Intersects(previous.x,previous.y,previousSpaces,worldRounds)&&web.Intersects(cell.x,cell.y,spaces,worldRounds))QueueWebAt(actor,cell,spaces,false);previous=cell;previousSpaces=spaces;}
        SyncWeb();
    }
    bool WebPathNeeded(Vector2Int from,Vector2Int to,int spaces)
    {
        if(!web.Active(worldRounds))return false;foreach(var cell in BattleMap.MovementAnchors(from,to))if(web.Intersects(cell.x,cell.y,spaces,worldRounds))return true;return false;
    }
    int WebLand(Vector2Int anchor){int mask=0;for(int y=0;y<4;y++)for(int x=0;x<4;x++){var p=anchor+new Vector2Int(x,y);if(map.Inside(p,1)&&!map.IsWater(p))mask|=1<<(y*4+x);}return mask;}
    string WebError(){Vector2Int p;if(!VisionPoint(out p))return "蛛网区域左上格须位于地图内。";if(map.DistanceToCell(true,p)>60)return "区域起点超出60尺。";return worldRounds<0||worldRounds>long.MaxValue-600?"世界时间超出范围。":"";}
    void DrawWebChoices(){GUILayout.Label("20尺方形（4×4格），输入左上格列／行。仅陆地铺网；水域不生成。困难地形、轻度遮蔽，专注至多1小时。首次进入每个回合及自己回合开始时敏捷豁免，失败束缚；动作力量（运动）脱困。",small);visionColumn=IntegerField(visionColumn,false);visionRow=IntegerField(visionRow,false);}
    void ApplyWeb(Vector2Int p,int caster)
    {
        web.Begin(p.x,p.y,WebLand(p),8L+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(caster)]),worldRounds);
        concentrationSpell="蛛网术";concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=worldRounds+600;SyncTimedBuffs();
        history.Insert(0,"蛛网术：陆地4×4格铺网，出现时不立即豁免；进入每个回合首次及自己回合开始时进行敏捷豁免。离开蛛网或脱困可结束对应束缚，困难地形仍保留。");
    }
    void EndWeb(){web.Clear();webEntries.Clear();rescueCarrier.Restrained=false;}
    void IgniteWebActor(int actor)
    {
        if(webAreaFire)return;Vector2Int p;int n;if(WebBody(actor,out p,out n))IgniteWebArea((x,y)=>x>=p.x&&x<p.x+n&&y>=p.y&&y<p.y+n);
    }
    void IgniteWebArea(Func<int,int,bool> exposed)
    {
        if(!web.Active(worldRounds))return;int count=0;for(int y=0;y<4;y++)for(int x=0;x<4;x++)if(exposed(web.X+x,web.Y+y)&&web.Ignite(web.X+x,web.Y+y,terrainActor,worldRounds))count++;
        if(count>0)history.Insert(0,"蛛网遇火："+count+"格燃烧，1回合后烧毁；生物在火中开始回合受到2d4火焰伤害。");
    }
    void DrawWebArea(Rect board)
    {
        Vector2Int p=Vector2Int.zero;bool preview=panel=="spells"&&selectedSpellName=="Web"&&VisionPoint(out p);
        for(int y=0;y<map.Size;y++)for(int x=0;x<map.Size;x++){
            bool live=web.Cell(x,y,worldRounds),draft=preview&&x>=p.x&&x<p.x+4&&y>=p.y&&y<p.y+4&&!map.IsWater(new Vector2Int(x,y));
            if(live||draft)Fill(CellRect(board,new Vector2Int(x,y)),web.Burning(x,y,worldRounds)?new Color(.95f,.27f,.03f,.6f):new Color(.8f,.85f,.8f,draft?.4f:.3f));
        }
    }
    void DrawWebOrder()
    {
        if(!webOrderPending)return;Fill(new Rect(0,0,1600,900),new Color(0,0,0,.92f));Surface(new Rect(465,260,670,330),paperTexture);
        GUI.Label(new Rect(495,285,610,60),"回合开始 · 蛛网与燃烧同时触发",heading);
        if(GUI.Button(new Rect(495,380,610,60),"先进行敏捷豁免"))webOrderChoice=1;
        if(GUI.Button(new Rect(495,465,610,60),"先结算2d4火焰伤害"))webOrderChoice=2;
    }
    IEnumerator ResolveWebEntry(WebEntry entry)
    {
        Vector2Int body;int n;if(entry.Revision!=web.Revision||!web.Active(worldRounds)||!WebBody(entry.Actor,out body,out n))yield break;
        int order=1;string saved=panel;panel="";
        try{
            if(entry.Save&&entry.Fire){webOrderChoice=0;webOrderPending=true;while(webOrderChoice==0)yield return null;webOrderPending=false;order=webOrderChoice;}
            for(int step=0;step<2;step++){
                if(entry.Revision!=web.Revision||!web.Active(worldRounds)||!WebBody(entry.Actor,out body,out n))yield break;
                bool damage=order==1?step==1:step==0;
                if(damage&&entry.Fire&&InWeb(entry.Actor)){int a=UnityEngine.Random.Range(1,5),b=UnityEngine.Random.Range(1,5);yield return PresentRoll(4,new[]{a,b},"蛛网燃烧 · "+TransitActorName(entry.Actor)+"："+(a+b)+" 火焰");
                    // Existing flames do not ignite additional occupied cells.
                    var reduced=new RayDamage();yield return ReduceRayDamage(true,a+b,"火焰","蛛网燃烧",reduced);
                    webAreaFire=true;int amount;try{amount=ApplyMappedAreaDamage(entry.Actor,0,reduced.Amount,5,"蛛网燃烧",creatureSource:false);}finally{webAreaFire=false;}
                    if(entry.Actor==0)yield return MaintainConcentration(amount);ReconcileFallenRescue();RefreshBestialSpirit();CheckBattleOutcome();
                }else if(!damage&&entry.Save&&MovementFreedom(entry.Actor)){history.Insert(0,TransitActorName(entry.Actor)+"受到行动自如保护，不会被蛛网束缚。");}
                else if(!damage&&entry.Save){var failed=new HashSet<int>();yield return RollMappedAbilitySave(entry.Actor,entry.Actor,web.Dc,failed,"蛛网术",1);if(failed.Contains(entry.Actor)){if(InWeb(entry.Actor))web.Bind(entry.Actor,worldRounds);SyncTimedBuffs();history.Insert(0,TransitActorName(entry.Actor)+"蛛网敏捷豁免失败；"+(InWeb(entry.Actor)?"在网内束缚，主动移动停在触发位置。":"已被强制移出，不保留束缚。"));}else history.Insert(0,TransitActorName(entry.Actor)+"通过蛛网敏捷豁免；先前仍有效的束缚不会因此自动解除。");}
            }
        }finally{webOrderPending=false;panel=saved;}
    }
}
