using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly ConjureAnimalsEffect animalPack=new ConjureAnimalsEffect();
    sealed class PackEntry {public int Actor,Count;public long Revision,Turn,Dc;public Vector2Int Origin;public string Reason;}
    readonly Queue<PackEntry> packEntries=new Queue<PackEntry>();
    int packAppearance,packDecision,packPromptActor;bool packPromptPending,packOrderPending,packFirst;
    Vector2Int PackCell=>new Vector2Int(animalPack.X,animalPack.Y);
    bool PackActor(int actor,out Vector2Int cell,out int spaces)
    {
        string name;if(!HealingHandsTarget(actor,out cell,out spaces,out name)||map.SceneOf(actor)!=animalPack.Scene)return false;
        return !((actor==0||actor==-1)&&map.IsContained(actor==0));
    }
    bool PackNear(int actor,int feet)
    {
        Vector2Int p;int n;return animalPack.Active(worldRounds)&&PackActor(actor,out p,out n)&&BattleMap.BodyDistance(PackCell,2,p,n)<=feet;
    }
    bool PackVisible(int actor)=>map.SceneOf(0)==animalPack.Scene&&ConjuredTargetVisible(true,actor);
    bool PackEndAvailable(int actor)=>PackNear(actor,10)&&PackVisible(actor)&&animalPack.Available(actor,terrainTurn,worldRounds);
    bool PackStrengthAdvantage=>PackNear(0,5);
    void EndAnimalPack(){animalPack.Clear();packEntries.Clear();packPromptPending=packOrderPending=false;}
    string ConjureAnimalsError()
    {
        Vector2Int p;if(!VisionPoint(out p)||p.x<0||p.y<0||p.x>map.Size-2||p.y>map.Size-2||!map.CanPlaceSummon(p,2))return "兽群须落在地图内未被占据的2×2空间。";
        if(map.IsContained(true))return "吞咽内部没有独立的空闲大型落位空间。";
        if(BattleMap.BodyDistance(map.Player,map.PlayerSpaces,p,2)>60)return "兽群落点超出60尺。";
        if(VisibilityAt(true,p,2)>=2)return "必须看见兽群落点。";
        return worldRounds<0||worldRounds>long.MaxValue-100?"世界时间超出范围。":"";
    }
    void DrawConjureAnimalsChoices()
    {
        packAppearance=GUILayout.SelectionGrid(packAppearance,new[]{"幽灵狼群","幽灵蛇群","幽灵鸟群"},3,button);
        visionColumn=IntegerField(visionColumn,false);visionRow=IntegerField(visionRow,false);
        GUILayout.Label("指定2×2区域的左下角列／行，60尺内须可见且未被占据。兽群无实体，不阻挡通行；5尺内的施法者力量豁免具有优势。出现时不立即造成伤害。兽群移动到生物10尺内、生物进入该范围或在其中结束回合时，可令可见目标进行敏捷豁免，失败受3d10挥砍，每升环＋1d10，每名生物每回合至多一次。专注至多10分钟。",small);
        GUILayout.Label("玩家本回合实际移动后，可控制兽群；兽群本回合累计最多移动30尺，可分多次使用，不消耗动作或附赠动作。外观不改变规则。",small);
        if(animalPack.Active(worldRounds)&&GUILayout.Button("控制现存兽群",button))OpenAnimalPackControl();
    }
    void CastConjureAnimals(Vector2Int p,int caster,int slot,int appearance)
    {
        if(!map.CanPlaceSummon(p,2)||map.IsContained(true)||VisibilityAt(true,p,2)>=2)return;
        if(!animalPack.Begin(p.x,p.y,map.SceneOf(0),slot,8L+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(caster)]),appearance,worldRounds,terrainTurn))return;
        concentrationSpell="咒唤兽群";concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=animalPack.Expires;
        history.Insert(0,"咒唤兽群：大型无实体兽群，专注至多10分钟；出现不触发伤害。每个玩家回合兽群累计移动30尺，未用额度不保留。");
    }
    void QueuePackEntry(int actor,string reason)
    {
        if(!PackEndAvailable(actor))return;
        foreach(var pending in packEntries)if(pending.Actor==actor&&pending.Turn==terrainTurn&&pending.Revision==animalPack.Revision)return;
        packEntries.Enqueue(new PackEntry{Actor=actor,Count=animalPack.Slot,Revision=animalPack.Revision,Turn=terrainTurn,Dc=animalPack.Dc,Origin=PackCell,Reason=reason});
    }
    void ObservePackMovement(int actor,Vector2Int from,Vector2Int to,int oldSpaces,int spaces,bool landing)
    {
        if(!animalPack.Active(worldRounds))return;
        if(actor==BattleMap.RescueId&&map.RescueCargoPlayer.HasValue){bool player=map.RescueCargoPlayer.Value;var cargo=player?map.Player:map.Monster;int n=player?map.PlayerSpaces:map.MonsterSpaces;ObservePackMovement(player?0:-1,new Vector2Int(cargo.x-(to.x-from.x),cargo.y-(to.y-from.y)),cargo,n,n,landing);}
        Vector2Int body;int size;if(!PackActor(actor,out body,out size))return;
        var previous=from;int n0=oldSpaces;var path=landing?new List<Vector2Int>{to}:BattleMap.MovementAnchors(from,to);if(from==to&&oldSpaces!=spaces)path.Add(to);
        foreach(var cell in path){if((cell!=previous||n0!=spaces)&&BattleMap.BodyDistance(PackCell,2,cell,spaces)<=10)QueuePackEntry(actor,"进入兽群10尺内的空间");previous=cell;n0=spaces;}
    }
    bool PackPathNeeded(Vector2Int from,Vector2Int to,int spaces)
    {
        if(!animalPack.Active(worldRounds)||animalPack.Scene!=map.Scene)return false;
        foreach(var p in BattleMap.MovementAnchors(from,to))if(BattleMap.BodyDistance(PackCell,2,p,spaces)<=10)return true;return false;
    }
    IEnumerator ChoosePackEntryOrder()
    {
        packFirst=packEntries.Count>0;if(!packFirst||PendingTerrainCount==packEntries.Count)yield break;
        packDecision=0;packOrderPending=true;try{while(packDecision==0&&packEntries.Count>0)yield return null;packFirst=packEntries.Count>0&&packDecision==1;}finally{packOrderPending=false;}
    }
    void DrawPackPrompt()
    {
        Surface(new Rect(420,225,760,430),paperTexture);GUILayout.BeginArea(new Rect(455,255,690,370));GUILayout.Label(packOrderPending?"选择结算顺序":"咒唤兽群 · 选择是否触发",heading);
        if(packOrderPending){if(GUILayout.Button("先结算兽群触发",button))packDecision=1;if(GUILayout.Button("先结算其他持续效果",button))packDecision=2;}
        else {GUILayout.Label(TransitActorName(packPromptActor)+"满足触发条件。是否令其进行敏捷豁免？同一生物每回合至多进行一次该豁免。",small);if(GUILayout.Button("令目标进行豁免",button))packDecision=1;if(GUILayout.Button("本次跳过",button))packDecision=2;}
        GUILayout.EndArea();
    }
    IEnumerator ResolvePackEntry(PackEntry entry)
    {
        if(entry.Revision!=animalPack.Revision||!animalPack.Available(entry.Actor,entry.Turn,worldRounds))yield break;
        Vector2Int cell;int n;string name;if(!HealingHandsTarget(entry.Actor,out cell,out n,out name))yield break;
        packPromptActor=entry.Actor;packDecision=0;packPromptPending=true;try{while(packDecision==0&&entry.Revision==animalPack.Revision&&animalPack.Active(worldRounds))yield return null;}finally{packPromptPending=false;}
        if(packDecision!=1||entry.Revision!=animalPack.Revision||!animalPack.Claim(entry.Actor,entry.Turn,worldRounds))yield break;
        var failed=new HashSet<int>();yield return RollMappedAbilitySave(entry.Actor,0,entry.Dc,failed,"咒唤兽群",1,entry.Origin,2);
        if(!failed.Contains(entry.Actor)){history.Insert(0,name+"通过咒唤兽群敏捷豁免，本次伤害0。");yield break;}
        int amount=0;var rolls=new int[entry.Count];for(int i=0;i<rolls.Length;i++){rolls[i]=Random.Range(1,11);amount+=rolls[i];}
        outcomeDeferDepth++;
        try{yield return PresentRoll(10,rolls,"咒唤兽群："+entry.Count+"d10 = "+amount+" 挥砍");var reduced=new RayDamage();yield return ReduceRayDamage(true,amount,"挥砍","咒唤兽群",reduced);
            int damage=ApplyMappedAreaDamage(entry.Actor,0,reduced.Amount,2,"咒唤兽群 · "+entry.Reason);if(entry.Actor==0)yield return MaintainConcentration(damage);
            ReconcileFallenRescue();RefreshBestialSpirit();NoticeCastingInterruption();
        }finally{outcomeDeferDepth--;CheckBattleOutcome();}
    }
}
