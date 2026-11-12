using System.Collections;
using UnityEngine;
public sealed partial class TrainingGame
{
    bool thunderwaveTargeting,thunderwaveIncludeSelf;
    int thunderDirection,thunderAlignment=1,thunderCaster,thunderSlot;
    void BeginThunderwave(int caster,int slot){thunderCaster=caster;thunderSlot=slot;thunderwaveTargeting=true;spellTargeting=true;thunderwaveIncludeSelf=false;}
    Vector2Int ThunderAnchor()
    {
        var p=map.Player;int n=map.PlayerSpaces;
        return thunderDirection==0?new Vector2Int(p.x+n,p.y-thunderAlignment):thunderDirection==1?new Vector2Int(p.x-3,p.y-thunderAlignment):thunderDirection==2?new Vector2Int(p.x-thunderAlignment,p.y+n):new Vector2Int(p.x-thunderAlignment,p.y-3);
    }
    void DrawThunderwaveTargeting(Rect board)
    {
        var anchor=ThunderAnchor();for(int y=0;y<map.Size;y++)for(int x=0;x<map.Size;x++)if(x>=anchor.x&&x<anchor.x+3&&y>=anchor.y&&y<anchor.y+3)Fill(CellRect(board,new Vector2Int(x,y)),new Color(.7f,.6f,.1f,.4f));
        if(thunderwaveIncludeSelf)Fill(BodyRect(board,map.Player,map.PlayerSpaces),new Color(.9f,.4f,.1f,.4f));
        bool monster=combat.Hp>0&&BattleMap.Overlap(anchor,3,map.Monster,map.MonsterSpaces);
        Surface(new Rect(1010,110,225,580),paperTexture);GUILayout.BeginArea(new Rect(1025,125,195,545));
        GUILayout.Label("雷鸣波",heading);thunderDirection=GUILayout.SelectionGrid(thunderDirection,new[]{"向右","向左","向下","向上"},2,new GUIStyle(button){fontSize=16,fixedHeight=36});
        GUILayout.Label("沿源点所在面调整区域",small);thunderAlignment=GUILayout.SelectionGrid(thunderAlignment,new[]{"前端","居中","后端"},1,new GUIStyle(button){fontSize=16,fixedHeight=32});
        thunderwaveIncludeSelf=GUILayout.Toggle(thunderwaveIncludeSelf,"将源点自身纳入范围",label);
        GUILayout.Label("15尺立方（3×3格）\n目标："+(monster?"木桩":"无木桩")+(thunderwaveIncludeSelf?"、玩家":"")+"\n体质豁免；失败推离10尺。",small);
        bool previous=GUI.enabled;GUI.enabled=!rolling&&combat.MagicActionAvailable&&CanCastInCurrentForm;
        if(GUILayout.Button("确认施法",button)){var origin=map.Player;int originSpaces=map.PlayerSpaces;int dc=8+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(thunderCaster)]);if(combat.TryUseMagicAction()){spellTargeting=false;thunderwaveTargeting=false;StartCoroutine(ResolveThunderwave(monster,thunderwaveIncludeSelf,dc,thunderSlot,origin,originSpaces));}}
        GUI.enabled=previous;if(GUILayout.Button("取消",button)){spellTargeting=false;thunderwaveTargeting=false;}
        GUILayout.EndArea();
    }
    IEnumerator ResolveThunderwave(bool monster,bool player,int dc,int slot,Vector2Int origin,int originSpaces)
    {
        rolling=true;bool[] targets={monster,player},failed=new bool[2];
        if(!monster&&!player){history.Insert(0,"雷鸣波：范围无生物，已消耗动作。声响可传至300尺，当前无听觉警戒系统。");rolling=false;yield break;}
        for(int i=0;i<2;i++)if(targets[i]){
            int value=Random.Range(1,21),modifier=i==0?AcidSplash.Modifier(monsterAttributes[2]):PlayerSavingThrowModifier(2);dice="雷鸣波 · "+(i==0?"木桩":"玩家")+"体质豁免";
            yield return PresentRoll(20,new[]{value},value+" + "+modifier+" / DC "+dc);failed[i]=value+modifier<dc;
        }
        int[] rolls=new int[slot+1];int amount=0;for(int i=0;i<rolls.Length;i++){rolls[i]=Random.Range(1,9);amount+=rolls[i];}
        dice="雷鸣波 · 共享雷鸣伤害骰";yield return PresentRoll(8,rolls,"合计 "+amount+"，成功豁免者减半向下取整");
        for(int i=0;i<2;i++)if(targets[i]){
            int damage=failed[i]?amount:amount/2;
            if(i==0)combat.ApplyDamage(damage);else combat.ApplyPlayerDamage(damage);
            int pushed=i==0&&failed[i]?map.ForceMoveAway(false,origin,originSpaces,10):0;
            history.Insert(0,"雷鸣波对"+(i==0?"木桩":"玩家")+"："+(failed[i]?"豁免失败":"豁免成功")+"，"+damage+"雷鸣伤害，推离"+pushed+"尺。强制位移不消耗移动力、不触发借机、不追加碰撞伤害。");
        }
        if(player)yield return MaintainConcentration(failed[1]?amount:amount/2);
        dice="雷鸣波 · 结算完成";CheckBattleOutcome();rolling=false;
    }
}
