using UnityEngine;
public sealed partial class TrainingGame
{
    readonly SilenceEffect silence=new SilenceEffect();
    bool checkHearing;
    void InitializeSilence(){combat.DamageImmunity=(player,type)=>type==12&&SilenceWhole(player);}
    bool SilenceWhole(bool player){var cell=player?map.Player:map.Monster;return silence.WholeInside(cell.x,cell.y,player?map.PlayerSpaces:map.MonsterSpaces,worldRounds);}
    bool SilenceCentre(bool player){var cell=player?map.Player:map.Monster;return silence.CentreInside(cell.x,cell.y,player?map.PlayerSpaces:map.MonsterSpaces,worldRounds);}
    bool SpellVoiceAllowed(bool player,string name)
    {
        if(!SilenceCentre(player))return true;
        var spell=SpellInfo(name);return spell==null||System.Array.IndexOf(spell.components,"声音")<0;
    }
    bool SilenceHearMonster()=>!sensory.Deafened(true,worldRounds)&&!silence.BlocksSound(map.Player.x+(map.PlayerSpaces-1)*.5,map.Player.y+(map.PlayerSpaces-1)*.5,map.Monster.x+(map.MonsterSpaces-1)*.5,map.Monster.y+(map.MonsterSpaces-1)*.5,worldRounds);
    string MockeryTargetError(DirectSpell spell,bool self)=>spell.Name=="Vicious Mockery"&&!self&&!PlayerSeesMonster&&!SilenceHearMonster()?"恶言相加需要看见或听见目标；当前目标不可见且无法听见。":"";
    bool SilenceHearCommand(SummonedUnit unit)=>!silence.BlocksSound(map.Player.x+(map.PlayerSpaces-1)*.5,map.Player.y+(map.PlayerSpaces-1)*.5,unit.X+(unit.Spaces-1)*.5,unit.Y+(unit.Spaces-1)*.5,worldRounds);
    bool SilenceUnitImmune(SummonedUnit unit,string type)=>type=="雷鸣"&&silence.WholeInside(unit.X,unit.Y,unit.Spaces,worldRounds);
    void DrawSilenceChoices()
    {
        GUILayout.Label("固定20尺半径球状范围，选择中心列、行（1～"+map.Size+"），射程120尺。",small);
        visionColumn=IntegerField(visionColumn,false);visionRow=IntegerField(visionRow,false);
        Vector2Int cell;if(VisionPoint(out cell))GUILayout.Label("距玩家 "+map.DistanceToCell(true,cell)+"尺。单位中心在范围内时禁止声音施法；完整占地在内时失聪且免疫雷鸣。",small);
    }
    string SilenceError()
    {
        Vector2Int cell;if(!VisionPoint(out cell))return "沉默术中心必须位于地图内。";
        if(map.DistanceToCell(true,cell)>120)return "沉默术中心超出120尺射程。";
        return worldRounds<0||worldRounds>long.MaxValue-100?"世界时间超出可记录范围。":"";
    }
    void ApplySilence()
    {
        Vector2Int cell;if(!VisionPoint(out cell))return;
        EndConcentration("开始沉默术专注");silence.Begin(cell.x,cell.y,worldRounds);
        concentrationSpell="沉默术";concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=worldRounds+100;
        history.Insert(0,"沉默术生效：固定20尺范围，专注至多10分钟。按单位中心禁止声音施法；完整占地在范围内时失聪并免疫雷鸣。区域内不能产生声音，声音不能穿过区域；旧召唤指令继续有效。");
    }
    void DrawSilenceMap(Rect board)
    {
        Vector2Int center=default(Vector2Int);bool preview=panel=="spells"&&selectedSpellName=="Silence"&&VisionPoint(out center);
        if(!preview&&!silence.Active(worldRounds))return;
        if(!preview)center=new Vector2Int(silence.X,silence.Y);
        for(int y=0;y<map.Size;y++)for(int x=0;x<map.Size;x++)if((x-center.x)*(x-center.x)+(y-center.y)*(y-center.y)<=16)Fill(CellRect(board,new Vector2Int(x,y)),new Color(.42f,.64f,.75f,.28f));
    }
    void DrawSilenceStatus(float x,bool player)
    {
        string text=(sensory.Blinded(player,worldRounds)||feignDeath.Blind(player?0:-1,worldRounds))?"目盲":"";if(sensory.Deafened(player,worldRounds)||SilenceWhole(player))text+=(text==""?"":" · ")+"失聪";if(SilenceWhole(player))text+=" · 雷鸣免疫";else if(SilenceCentre(player))text+=(text==""?"":" · ")+"禁声";
        if(player&&playerResurrectionCaster.Taxed)text+=(text==""?"":" · ")+"复生疲惫";
        if(SteedFear(player?0:-1))text+=(text==""?"":" · ")+"恐慌";
        if(poisonProtection.Active(player,worldRounds))text+=(text==""?"":" · ")+"抗毒";
        if(text!="")GUI.Label(new Rect(x+20,823,230,22),text,PlaqueText(player,14,TextAnchor.MiddleCenter));
    }
}
