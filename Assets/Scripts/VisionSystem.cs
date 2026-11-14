using UnityEngine;
public sealed partial class TrainingGame
{
    readonly VisionState vision=new VisionState();
    string playerVisionInput="0",monsterVisionInput="0",visionColumn="1",visionRow="1";
    int? playerSightOverride,monsterSightOverride;
    int lightDraft;bool visionWilling,checkVisual;
    int PlayerDarkvision=>IsWildShaped?Mathf.Max(vision.SpellRange(true,worldRounds),wildForm.darkvision):vision.Range(true,worldRounds);
    int PlayerBlindsight=>IsWildShaped?wildForm.blindsight:0;
    int Visibility(bool viewerPlayer,bool targetPlayer){
        if(viewerPlayer==targetPlayer)return 0;
        int? snapshot=viewerPlayer?playerSightOverride:monsterSightOverride;if(snapshot.HasValue)return snapshot.Value;
        return VisibilityAt(viewerPlayer,targetPlayer?map.Player:map.Monster,targetPlayer?map.PlayerSpaces:map.MonsterSpaces);
    }
    int VisibilityAt(bool viewerPlayer,Vector2Int target,int targetSize){
        var from=viewerPlayer?map.Player:map.Monster;
        return VisibilityBetween(viewerPlayer,from.x,from.y,viewerPlayer?map.PlayerSpaces:map.MonsterSpaces,target.x,target.y,targetSize);
    }
    int VisibilityBetween(bool viewerPlayer,double fromX,double fromY,int fromSize,double targetX,double targetY,int targetSize){
        int range=viewerPlayer?PlayerDarkvision:vision.Range(false,worldRounds),best=2,blind=viewerPlayer?PlayerBlindsight:0;
        for(int fy=0;fy<fromSize;fy++)for(int fx=0;fx<fromSize;fx++)for(int ty=0;ty<targetSize;ty++)for(int tx=0;tx<targetSize;tx++){
            best=Mathf.Min(best,vision.Quality((fromX+fx)*5,(fromY+fy)*5,(targetX+tx)*5,(targetY+ty)*5,range,worldRounds,blind));if(best==0)return 0;
        }return best;
    }
    bool OpportunitySight(bool movingPlayer,Vector2Int from,Vector2Int to,int reach,out int playerSight,out int monsterSight){
        var defender=movingPlayer?map.Monster:map.Player;int movingSize=movingPlayer?map.PlayerSpaces:map.MonsterSpaces,defenderSize=movingPlayer?map.MonsterSpaces:map.PlayerSpaces;
        double t=VisionState.ReachExit(from.x,from.y,to.x,to.y,movingSize,defender.x,defender.y,defenderSize,reach);
        double x=from.x+(to.x-from.x)*t,y=from.y+(to.y-from.y)*t;
        playerSight=movingPlayer?VisibilityBetween(true,x,y,movingSize,defender.x,defender.y,defenderSize):VisibilityBetween(true,defender.x,defender.y,defenderSize,x,y,movingSize);
        monsterSight=movingPlayer?VisibilityBetween(false,defender.x,defender.y,defenderSize,x,y,movingSize):VisibilityBetween(false,x,y,movingSize,defender.x,defender.y,defenderSize);
        return t>=0&&(movingPlayer?monsterSight:playerSight)<2;
    }
    bool PlayerSeesMonster=>Visibility(true,false)<2;
    bool MonsterSeesPlayer=>Visibility(false,true)<2;
    void DrawVisionConfiguration(bool player){
        if(player){GUILayout.Label("战场基础光照",small);lightDraft=GUILayout.SelectionGrid(lightDraft,new[]{"明亮","微光","黑暗"},3,new GUIStyle(button){fontSize=16,fixedHeight=34});}
        GUILayout.Label("黑暗视觉距离（非负整数尺，当前 "+(player?vision.PlayerRange:vision.MonsterRange)+"）",small);
        if(player)playerVisionInput=IntegerField(playerVisionInput,false);else monsterVisionInput=IntegerField(monsterVisionInput,false);
    }
    bool VisionInputValid(bool player,out int range)=>int.TryParse(player?playerVisionInput:monsterVisionInput,out range)&&range>=0;
    void DrawVisionMap(Rect board){
        for(int y=0;y<map.Size;y++)for(int x=0;x<map.Size;x++){
            if(vision.FogAt(x*5,y*5,worldRounds))Fill(CellRect(board,new Vector2Int(x,y)),new Color(.55f,.62f,.65f,.62f));
            int light=vision.Light(x*5,y*5,worldRounds);if(light>0)Fill(CellRect(board,new Vector2Int(x,y)),new Color(.025f,.025f,.07f,light==2?.63f:.26f));
        }
        if(panel=="spells"&&(selectedSpellName=="Darkness"||selectedSpellName=="Daylight"||selectedSpellName=="Fog Cloud")){
            Vector2Int cell;if(VisionPoint(out cell)){float radius=selectedSpellName=="Fog Cloud"?20*directSlotLevel:selectedSpellName=="Darkness"?15:60;for(int y=0;y<map.Size;y++)for(int x=0;x<map.Size;x++)if(VisionState.Distance(cell.x*5,cell.y*5,x*5,y*5)<=radius)Fill(CellRect(board,new Vector2Int(x,y)),new Color(.6f,.4f,.85f,.25f));}
        }
    }
    bool VisionPoint(out Vector2Int cell){int x=0,y=0;bool valid=int.TryParse(visionColumn,out x)&&int.TryParse(visionRow,out y);cell=new Vector2Int(x-1,y-1);return valid&&map.Inside(cell,1);}
    static bool IsVisionSpell(string name)=>name=="Darkvision"||name=="Darkness"||name=="Daylight"||name=="Fog Cloud";
    void DrawVisionSpellChoices(DirectSpell spell){
        if(spell.Name=="Darkvision"){if(!directTargetPlayer)visionWilling=GUILayout.Toggle(visionWilling,"确认木桩自愿接受黑暗视觉",label);return;}
        GUILayout.Label("固定地面源点（列、行，1～"+map.Size+"）；紫色区域预览。"+(spell.Name=="Fog Cloud"?"":"物件承载及遮盖暂缓。"),small);
        visionColumn=IntegerField(visionColumn,false);visionRow=IntegerField(visionRow,false);
        Vector2Int cell;GUILayout.Label(VisionPoint(out cell)?"源点距离 "+map.DistanceToCell(true,cell)+" 尺":"请输入地图内的格子",small);
    }
    string VisionSpellError(DirectSpell spell){
        if(spell.Name=="Darkvision"&&!directTargetPlayer&&combat.Hp==0)return "死亡目标不能接受此法术。";
        if(spell.Name=="Darkvision")return !directTargetPlayer&&!visionWilling?"目标须自愿，请先确认。":!directTargetPlayer&&map.DistanceFeet>5?"目标超出接触距离。":"";
        Vector2Int cell;return !VisionPoint(out cell)?"源点须位于地图内。":map.DistanceToCell(true,cell)>(spell.Name=="Fog Cloud"?120:60)?"源点超出法术射程。":"";
    }
    void ApplyVisionSpell(DirectSpell spell,bool player,int slot){
        if(spell.Name=="Fog Cloud"){
            Vector2Int cell;if(!VisionPoint(out cell))return;EndConcentration("开始新的专注法术");vision.AddFog(cell.x*5,cell.y*5,slot,worldRounds);concentrationSpell=spell.Chinese;concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=worldRounds+600;history.Insert(0,"云雾术："+(20*slot)+"尺半径重度遮蔽，专注1小时；普通视觉与黑暗视觉无法穿透，强风环境消散待环境系统接入。");
        }
        else if(spell.Name=="Darkvision"){vision.GrantDarkvision(player,worldRounds);history.Insert(0,(player?"玩家":"木桩")+"获得150尺黑暗视觉，持续8小时，不降低已有更远的视觉距离。");}
        else {Vector2Int cell;if(!VisionPoint(out cell))return;bool dark=spell.Name=="Darkness";
            if(dark)EndConcentration("开始新的专注法术");bool hadDarkness=vision.HasDarkness(worldRounds);
            vision.AddZone(dark,cell.x*5,cell.y*5,slot,worldRounds);
            if(!dark&&hadDarkness&&!vision.HasDarkness(worldRounds)&&concentrationSpell=="黑暗术")EndConcentration("被昼明术驱散");
            if(dark&&!vision.HasDarkness(worldRounds)){history.Insert(0,"黑暗术被已有昼明术立即驱散，动作已消耗。");dice="黑暗术 · 已被驱散";return;}
            if(dark){concentrationSpell="黑暗术";concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=worldRounds+100;}
            history.Insert(0,spell.Chinese+"源点("+(cell.x+1)+","+(cell.y+1)+")，"+(dark?"15尺半径魔法黑暗，专注10分钟":"60尺明亮阳光，外围再60尺微光，持续1小时")+"。");
        }dice=spell.Chinese+" · 已生效";
    }
    static bool SpellRequiresSight(string name)=>name=="Bane"||name=="Blight"||name=="Healing Word"||name=="Mass Healing Word"||name=="Hold Person"||name=="Hex"||name=="Hunter's Mark"||name=="Magic Missile"||name=="Power Word Kill"||name=="Power Word Fortify"||name=="Sacred Flame"||name=="Mind Sliver"||name=="Toll the Dead";
}
