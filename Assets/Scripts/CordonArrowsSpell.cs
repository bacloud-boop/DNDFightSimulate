using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly CordonArrowsEffect cordon=new CordonArrowsEffect();
    readonly HashSet<int> cordonIgnored=new HashSet<int>();
    readonly List<Vector2Int> cordonPlanting=new List<Vector2Int>();string cordonCount="4";bool cordonBolts;
    sealed class CordonShot {public int Actor,Scene;public CordonArrowsEffect.Field Field;public int[] Choices;public string Reason;}
    readonly Dictionary<long,CordonShot> cordonShots=new Dictionary<long,CordonShot>();long cordonShotSequence;
    bool cordonChoicePending;int cordonArrowChoice=-1;CordonShot choosingCordon;Vector2 cordonChoiceScroll;
    List<Vector2Int> CordonLand()
    {
        var cells=new List<Vector2Int>();if(map.IsContained(true))return cells;
        for(int y=0;y<map.PlayerSpaces;y++)for(int x=0;x<map.PlayerSpaces;x++){var p=new Vector2Int(map.Player.x+x,map.Player.y+y);if(map.Inside(p,1)&&!map.IsWater(p))cells.Add(p);}return cells;
    }
    string CordonError(int slot)
    {
        if(!cordon.CanAdd(worldRounds))return "时间或箭阵编号超出范围。";
        int count;if(!int.TryParse(cordonCount,out count)||count<1||count>CordonArrowsEffect.Limit(slot))return "箭矢数量必须为1至"+CordonArrowsEffect.Limit(slot)+"的整数。";
        var cells=CordonLand();if(cells.Count==0)return "只能在自身占地内的陆地格栽箭，水域／吞咽内部不可设置。";
        if(cordonPlanting.Count!=count)return "请先为每支箭确认落位。";
        foreach(var p in cordonPlanting)if(!cells.Contains(p))return "每支箭须位于自身占地内的陆地格。";
        return "";
    }
    void DrawCordonChoices()
    {
        GUILayout.Label("非专注8小时。只在自身占地内陆地格栽箭；每环上限4＋2×（环阶－2）支。箭／弩矢沿用弹药不限，效果期间不能拔出，不阻挡棋子。",small);
        cordonCount=IntegerField(cordonCount,false);cordonBolts=GUILayout.SelectionGrid(cordonBolts?1:0,new[]{"箭","弩矢"},2,button)==1;
        int count;var cells=CordonLand();if(int.TryParse(cordonCount,out count)&&count>=1&&count<=CordonArrowsEffect.Limit(directSlotLevel)&&cells.Count>0){
            while(cordonPlanting.Count<count)cordonPlanting.Add(cells[0]);if(cordonPlanting.Count>count)cordonPlanting.RemoveRange(count,cordonPlanting.Count-count);
            var names=new string[cells.Count];for(int i=0;i<names.Length;i++)names[i]="列"+(cells[i].x+1)+"／行"+(cells[i].y+1);
            for(int i=0;i<count;i++){int selected=cells.IndexOf(cordonPlanting[i]);if(selected<0)selected=0;GUILayout.Label("第"+(i+1)+"支落位",small);cordonPlanting[i]=cells[GUILayout.SelectionGrid(selected,names,Math.Min(3,names.Length),button)];}
        }
        GUILayout.Label("施法者自动忽略。以下生物可额外忽略（施法时固定）：",small);
        foreach(int actor in BreathActors())if(actor!=0){bool next=GUILayout.Toggle(cordonIgnored.Contains(actor),TransitActorName(actor),label);if(next)cordonIgnored.Add(actor);else cordonIgnored.Remove(actor);}
        GUILayout.Label("30尺内每回合首次进入一个格位，以及回合结束时分别触发一支箭。敏捷失败2d4穿刺，成功0，均耗箭；出现时不立即攻击。重叠仅最高DC触发，同DC取最新，其他箭阵不耗箭。可选择射出哪支箭。",small);
        foreach(var field in cordon.Fields)if(field.Active(worldRounds))GUILayout.Label("箭阵 #"+field.Id+" · 剩余"+field.Remaining+"支 · DC "+field.Dc+" · 剩余"+((field.Expires-worldRounds)*6)+"秒",small);
    }
    void CastCordon(ICollection<Vector2Int> positions,int slot,int caster,HashSet<int> ignored,bool bolts)
    {
        var land=CordonLand();foreach(var p in positions)if(!land.Contains(p)){history.Insert(0,"警戒箭阵落位已失效，施法动作已消耗。");return;}
        var planted=new List<CordonArrowsEffect.Arrow>();foreach(var p in positions)planted.Add(new CordonArrowsEffect.Arrow(p.x,p.y));
        var field=cordon.Add(planted,slot,8L+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(caster)]),0,map.SceneOf(0),ignored,worldRounds,bolts);
        if(field!=null)history.Insert(0,"警戒箭阵 #"+field.Id+"：栽下"+field.Remaining+"支"+(bolts?"弩矢":"箭")+"，非专注8小时，30尺，敏捷DC "+field.Dc+"；不会因施法者失能或死亡而结束。");
    }
    bool CordonBody(int actor,out Vector2Int p,out int n)
    {
        p=Vector2Int.zero;n=0;return HitDiceActorAlive(actor)&&!((actor==0||actor==-1)&&map.IsContained(actor==0))&&map.SceneBody(actor,out p,out n);
    }
    bool CordonEndAvailable(int actor){Vector2Int p;int n;return CordonBody(actor,out p,out n)&&cordon.EndAvailable(actor,p.x,p.y,n,map.SceneOf(actor),terrainTurn,worldRounds);}
    void QueueCordonShot(int actor,CordonArrowsEffect.Field field,Vector2Int p,int n,string reason)
    {
        if(field==null)return;long id=++cordonShotSequence;cordonShots.Add(id,new CordonShot{Actor=actor,Field=field,Scene=field.Scene,Choices=CordonArrowsEffect.Available(field,p.x,p.y,n),Reason=reason});
        startStatuses.Add(new StartStatus{Actor=actor,Kind=8,Group=id});
    }
    void QueueCordonEnd(int actor){Vector2Int p;int n;if(CordonBody(actor,out p,out n))QueueCordonShot(actor,cordon.ClaimEnd(actor,p.x,p.y,n,map.SceneOf(actor),terrainTurn,worldRounds),p,n,"回合结束");}
    void ObserveCordonMovement(int actor,Vector2Int from,Vector2Int to,int oldSpaces,int spaces,bool landing)
    {
        if(actor==BattleMap.RescueId&&map.RescueCargoPlayer.HasValue){bool player=map.RescueCargoPlayer.Value;var cargo=player?map.Player:map.Monster;int size=player?map.PlayerSpaces:map.MonsterSpaces;ObserveCordonMovement(player?0:-1,new Vector2Int(cargo.x-(to.x-from.x),cargo.y-(to.y-from.y)),cargo,size,size,landing);}
        Vector2Int actual;int n;if(!CordonBody(actor,out actual,out n))return;
        var path=landing&&!map.MovementIsJump?new List<Vector2Int>{to}:BattleMap.MovementAnchors(from,to);
        if(from==to){if(spaces>oldSpaces)path.Add(to);else return;}
        foreach(var cell in path)QueueCordonShot(actor,cordon.ClaimMovement(actor,cell.x,cell.y,spaces,map.SceneOf(actor),terrainTurn,worldRounds),cell,spaces,"本回合首次进入格位");
    }
    bool CordonPathNeeded(Vector2Int from,Vector2Int to,int spaces)
    {
        foreach(var field in cordon.Fields)if(field.Active(worldRounds)&&field.Scene==map.Scene)foreach(var cell in BattleMap.MovementAnchors(from,to))if(CordonArrowsEffect.Available(field,cell.x,cell.y,spaces).Length>0)return true;return false;
    }
    IEnumerator ResolveCordonShot(long id)
    {
        CordonShot shot;if(!cordonShots.TryGetValue(id,out shot))yield break;cordonShots.Remove(id);
        var choices=new List<int>();foreach(int index in shot.Choices)if(!shot.Field.Arrows[index].Spent)choices.Add(index);if(choices.Count==0)yield break;
        int selected=choices[0];var firstArrow=shot.Field.Arrows[selected];bool differentOrigin=false;foreach(int index in choices){var a=shot.Field.Arrows[index];if(a.X!=firstArrow.X||a.Y!=firstArrow.Y)differentOrigin=true;}
        if(differentOrigin){choosingCordon=shot;cordonArrowChoice=-1;cordonChoicePending=true;try{while(cordonArrowChoice<0)yield return null;selected=cordonArrowChoice;}finally{cordonChoicePending=false;choosingCordon=null;}}
        if(!choices.Contains(selected)||!CordonArrowsEffect.Consume(shot.Field,selected))yield break;
        var arrow=shot.Field.Arrows[selected];var origin=new Vector2Int(arrow.X,arrow.Y);
        history.Insert(0,"警戒箭阵 #"+shot.Field.Id+"因"+shot.Reason+"射出一支"+(shot.Field.Bolts?"弩矢":"箭")+"，目标"+TransitActorName(shot.Actor)+"；剩余"+shot.Field.Remaining+"支。");
        // The shot was triggered before an ordering choice. It remains spent
        // even if an earlier simultaneous effect kills or removes its target.
        if(!HitDiceActorAlive(shot.Actor)||map.SceneOf(shot.Actor)!=shot.Scene)yield break;
        string saved=panel;bool busy=rolling;panel="";rolling=true;outcomeDeferDepth++;
        try{
            var failed=new HashSet<int>();yield return RollMappedAbilitySave(shot.Actor,shot.Field.Source,shot.Field.Dc,failed,"警戒箭阵",1,origin);
            if(!failed.Contains(shot.Actor)){history.Insert(0,TransitActorName(shot.Actor)+"通过敏捷豁免，警戒箭阵伤害0，箭矢仍消耗。");yield break;}
            int first=UnityEngine.Random.Range(1,5),second=UnityEngine.Random.Range(1,5),raw=first+second;dice="警戒箭阵 · 穿刺伤害";yield return PresentRoll(4,new[]{first,second},"2d4 = "+raw+"穿刺（不因升环增加）");
            var ray=new RayDamage();yield return ReduceRayDamage(true,raw,"穿刺","警戒箭阵",ray);
            int actual=ApplyMappedAreaDamage(shot.Actor,shot.Field.Source,ray.Amount,1,"警戒箭阵");if(shot.Actor==0)yield return MaintainConcentration(actual);else yield return MaintainConcentration(0);
            ReconcileFallenRescue();RefreshBestialSpirit();
        }finally{outcomeDeferDepth--;panel=saved;rolling=busy;CheckBattleOutcome();}
    }
    void DrawCordonArrowChoice()
    {
        Fill(new Rect(0,0,1600,900),new Color(0,0,0,.94f));Surface(new Rect(440,160,720,600),paperTexture);GUILayout.BeginArea(new Rect(470,190,660,540));
        GUILayout.Label("警戒箭阵 · 选择射出的箭",heading);cordonChoiceScroll=GUILayout.BeginScrollView(cordonChoiceScroll);
        foreach(int index in choosingCordon.Choices)if(!choosingCordon.Field.Arrows[index].Spent){var a=choosingCordon.Field.Arrows[index];if(GUILayout.Button("第"+(index+1)+"支 · 列"+(a.X+1)+"／行"+(a.Y+1)+" → "+TransitActorName(choosingCordon.Actor),button))cordonArrowChoice=index;}
        GUILayout.EndScrollView();GUILayout.EndArea();
    }
    void ResetCordon(){cordon.Clear();cordonShots.Clear();startStatuses.RemoveAll(e=>e.Kind==8);cordonShotSequence=0;cordonPlanting.Clear();cordonIgnored.Clear();cordonCount="4";}
    void DrawCordonMap(Rect board)
    {
        foreach(var field in cordon.Fields)if(field.Active(worldRounds)&&field.Scene==map.Scene)foreach(var arrow in field.Arrows)if(!arrow.Spent)Fill(CellRect(board,new Vector2Int(arrow.X,arrow.Y)),new Color(.95f,.65f,.12f,.35f));
        if(panel=="spells"&&selectedSpellName=="Cordon of Arrows")foreach(var p in cordonPlanting)Fill(CellRect(board,p),new Color(.95f,.8f,.25f,.55f));
    }
}
