using UnityEngine;
public sealed partial class TrainingGame
{
    bool spiritWater;string spiritColumn="2",spiritRow="2",spiritAppearance="";
    bool SpiritCell(out Vector2Int p){int x,y;bool valid=int.TryParse(spiritColumn,out x)&int.TryParse(spiritRow,out y);p=new Vector2Int(x-1,y-1);return valid&&map.Inside(p,1);}
    string SummonBeastError(int caster,int slot){Vector2Int p;if(!SpiritCell(out p))return "落位列、行必须是地图内的整数。";return BestialSpiritCastError(caster,slot,spiritWater,p);}
    void DrawSummonBeastChoices()
    {
        spiritWater=GUILayout.SelectionGrid(spiritWater?1:0,new[]{"大地","海洋"},2,button)==1;
        GUILayout.Label("小型野兽灵魄 · 大地与海洋均有集群战术；天空形态和立体攀爬暂缓。海洋形态有游泳速度，只能水下呼吸；沿用水面同时允许呼吸的项目规则。",small);
        GUILayout.Label("动物外观（仅影响名称）",small);spiritAppearance=GUILayout.TextField(spiritAppearance,fieldStyle);
        GUILayout.Label("落位列／行（90尺内可见空格）",small);spiritColumn=IntegerField(spiritColumn,false);spiritRow=IntegerField(spiritRow,false);
        DrawSummonHitDiceChoices("Summon Beast");int slot=System.Math.Max(2,System.Math.Min(9,directSlotLevel));var rules=new BestialSpiritRules(slot,0);
        GUILayout.Label("HP "+rules.MaxHp+" · AC "+rules.Armor+" · 速度30尺 · 黑暗视觉60尺\n每动作"+rules.Attacks+"次撕裂，各1d8＋"+rules.FixedDamage+"穿刺；命中采用施法者法术命中加值。暴击仅翻倍伤害骰。",small);
        GUILayout.Label("专注至多1小时；0HP或法术结束即消失。自己的回合结束后逐次手动操控；未收到指令时闪避并避险。更改笼统口头指令免费，旧指令跨回合保留，屏息时开口会呛水。战后保留灵魄和计时，但不开放移动／攻击。",small);
    }
    void DrawSummonBeastPreview(Rect board)
    {
        Vector2Int p;if(panel!="spells"||selectedSpellName!="Summon Beast"||!SpiritCell(out p))return;
        bool valid=BestialSpiritCastError(directCastingClass,directSlotLevel,spiritWater,p)=="";
        Fill(CellRect(board,p),valid?new Color(.2f,.8f,.3f,.4f):new Color(.9f,.2f,.2f,.4f));
    }
}
