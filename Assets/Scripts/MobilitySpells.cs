using UnityEngine;
public sealed partial class TrainingGame
{
    bool expeditiousRetreat;
    string teleportColumn="1",teleportRow="1";
    bool TeleportDestination(out Vector2Int cell){int x=0,y=0;bool valid=int.TryParse(teleportColumn,out x)&&int.TryParse(teleportRow,out y);cell=new Vector2Int(x-1,y-1);return valid&&map.Inside(cell,map.PlayerSpaces);}
    string MistyStepError(){Vector2Int cell;if(!TeleportDestination(out cell)||!map.CanTeleportPlayer(cell,30))return "目的地须在30尺内、地图内且占地不与木桩重叠。";return VisibilityAt(true,cell,map.PlayerSpaces)==2?"迷踪步需要看见目的地。":"";}
    void DrawMobilityChoices(DirectSpell spell){
        if(spell.Name=="Expeditious Retreat"){GUILayout.Label("仅作用于玩家自身，施法立即疾走一次。",small);return;}
        GUILayout.Label("传送目的地占地左上格：列、行（1～"+map.Size+"）。绿色占地预览。",small);
        teleportColumn=IntegerField(teleportColumn,false);teleportRow=IntegerField(teleportRow,false);
        Vector2Int cell;if(TeleportDestination(out cell))GUILayout.Label("传送距离 "+BattleMap.FeetBetween(map.Player,cell)+" 尺；不消耗移动力。",small);
    }
    void ApplyMobilitySpell(DirectSpell spell){
        if(spell.Name=="Expeditious Retreat"){
            EndConcentration("开始新的专注法术");concentrationSpell=spell.Chinese;concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=worldRounds+100;expeditiousRetreat=true;map.Dash();
            history.Insert(0,"脚底抹油：立即疾走，普通移动剩余 "+map.MovementRemaining+" 尺；专注10分钟内可使用附赠动作疾走。");
        }else{Vector2Int cell;if(!TeleportDestination(out cell)||!map.TeleportPlayer(cell,30))return;history.Insert(0,"迷踪步：传送到("+(cell.x+1)+","+(cell.y+1)+")，不消耗移动力、不引发借机攻击。");}
        dice=spell.Chinese+" · 已结算";
    }
    void TakeDash(bool bonus){
        if(page!="battle"||!map.Locked||rolling||spellTargeting||panel!=""||draggingToken>=0||!combat.PlayerCanAct||bonus&&!expeditiousRetreat)return;
        if(!(bonus?combat.TryUseBonusAction():combat.TryUseAction()))return;
        map.Dash();history.Insert(0,"玩家消耗"+(bonus?"附赠动作":"动作")+"疾走，普通移动剩余 "+map.MovementRemaining+" 尺；本回合疾走 "+map.DashesThisTurn+" 次。");
    }
    void DrawDashActions(){
        bool free=page=="battle"&&map.Locked&&!rolling&&!spellTargeting&&panel==""&&draggingToken<0;
        if(Btn(new Rect(380,345,205,48),"疾走（动作）",free&&combat.ActionAvailable))TakeDash(false);
        if(Btn(new Rect(380,402,205,48),"疾走（附赠动作）",free&&expeditiousRetreat&&combat.BonusActionAvailable&&combat.PlayerCanAct))TakeDash(true);
    }
    void DrawTeleportPreview(Rect board){if(panel!="spells"||selectedSpellName!="Misty Step")return;Vector2Int cell;if(TeleportDestination(out cell))Fill(BodyRect(board,cell,map.PlayerSpaces),MistyStepError()==""?new Color(.1f,.8f,.4f,.4f):new Color(.8f,.1f,.1f,.4f));}
}
