using System;
using System.Collections;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly SpiritualWeaponEffect spiritualWeapon=new SpiritualWeaponEffect();
    int spiritualTarget=-1,spiritualHost=int.MaxValue;
    bool spiritualAttack=true,spiritualCreateInside;
    string spiritualColumn="1",spiritualRow="1";
    Vector2 spiritualScroll;
    bool SpiritualInput(out Vector2Int p){int x,y;bool valid=int.TryParse(spiritualColumn,out x)&int.TryParse(spiritualRow,out y);p=new Vector2Int(x-1,y-1);return valid&&map.Inside(p,1);}
    void EndSpiritualWeapon(){spiritualWeapon.Clear();spiritualHost=int.MaxValue;}
    void RefreshSpiritualWeapon()
    {
        if(spiritualHost==int.MaxValue)return;
        Vector2Int p;int n;
        if(!map.HasContainedCreature||!map.SceneBody(spiritualHost,out p,out n)||map.SceneOf(spiritualHost)!=spiritualWeapon.Scene||!BattleMap.Overlap(spiritualWeapon.Cell,1,p,n))spiritualHost=int.MaxValue;
    }
    int SpiritualCreationHost(Vector2Int cell)
    {
        if(!map.HasContainedCreature)return int.MaxValue;
        int host=map.IsContained(true)?-1:0;Vector2Int p;int n;
        return (map.IsContained(true)||spiritualCreateInside)&&map.SceneBody(host,out p,out n)&&BattleMap.Overlap(cell,1,p,n)?host:int.MaxValue;
    }
    int SpiritualHostAt(Vector2Int cell)
    {
        Vector2Int p;int n;return spiritualHost!=int.MaxValue&&map.SceneBody(spiritualHost,out p,out n)&&BattleMap.Overlap(cell,1,p,n)?spiritualHost:int.MaxValue;
    }
    bool SpiritualTargetAllowed(Vector2Int cell,int scene,int host,int target)
    {
        Vector2Int p;int n;string name;
        if(target==BattleMap.RescueId||map.SceneOf(target)!=scene||!HealingHandsTarget(target,out p,out n,out name))return false;
        if(host!=int.MaxValue){if(target!=0&&target!=-1)return false;}
        else if((target==0||target==-1)&&map.IsContained(target==0))return false;
        return BattleMap.BodyDistance(cell,1,p,n)<=5;
    }
    string SpiritualWeaponError()
    {
        Vector2Int p;if(!SpiritualInput(out p))return "请选择地图内的生成格（列／行）。";
        if(!SpiritualWeaponEffect.ValidTime(worldRounds))return "世界时间超出范围。";
        if(BattleMap.BodyDistance(map.Player,map.PlayerSpaces,p,1)>60)return "生成点超出60尺。";
        int host=SpiritualCreationHost(p);
        if(map.HasContainedCreature&&spiritualCreateInside&&!map.IsContained(true)&&host==int.MaxValue)return "体内模式的生成点须位于容纳者占地内。";
        if(map.IsContained(true)&&host==int.MaxValue)return "被吞咽时不能在体外创造灵体武器。";
        return spiritualAttack&&!SpiritualTargetAllowed(p,map.SceneOf(0),host,spiritualTarget)?"攻击目标须在武器5尺内且不隔着吞咽边界；也可取消本次攻击，仅创造武器。":"";
    }
    void DrawSpiritualTargets(Vector2Int cell,int scene,int host)
    {
        spiritualAttack=GUILayout.Toggle(spiritualAttack,"本次进行一次攻击（可不攻击）",label);
        if(!spiritualAttack)return;
        foreach(int actor in BreathActors())if(actor!=BattleMap.RescueId){bool valid=SpiritualTargetAllowed(cell,scene,host,actor);if(GUILayout.Button((spiritualTarget==actor?"✓ ":"")+TransitActorName(actor)+(valid?"":"（不在武器攻击范围内）"),button))spiritualTarget=actor;}
    }
    void DrawSpiritualWeaponChoices()
    {
        if(spiritualWeapon.Active(worldRounds)&&GUILayout.Button("操控现存灵体武器（不重新施法）",button))OpenSpiritualWeaponControl();
        GUILayout.Label("生成点列／行：60尺内，可与生物同格；灵体不阻挡移动，也不提供掩护。初始攻击可选。",small);
        spiritualColumn=IntegerField(spiritualColumn,false);spiritualRow=IntegerField(spiritualRow,false);
        if(map.HasContainedCreature&&!map.IsContained(true))spiritualCreateInside=GUILayout.Toggle(spiritualCreateInside,"在当前吞咽者体内创造（须选择其占地）",label);
        Vector2Int p;if(SpiritualInput(out p))DrawSpiritualTargets(p,map.SceneOf(0),SpiritualCreationHost(p));
        GUILayout.Label("后续自己的回合，附赠动作移动至多20尺并可攻击一次。无需再次施法、开口或空手；专注与持续时间仍照常。跨位面暂停操控，返回同一位面可继续。",small);
    }
    IEnumerator CastSpiritualWeapon(Vector2Int p,int caster,int slot,int target,bool attack,int host)
    {
        if(!spiritualWeapon.Begin(p,slot,caster,map.SceneOf(0),combat.Round,worldRounds))yield break;
        spiritualHost=host;RefreshSpiritualWeapon();concentrationSpell="灵体武器";concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=spiritualWeapon.Expires;
        history.Insert(0,"灵体武器出现在（"+(p.x+1)+","+(p.y+1)+"），专注至多1分钟，不占用生物格子。" );
        if(attack)yield return SpiritualWeaponAttack(target);
    }
    void OpenSpiritualWeaponControl(){spiritualColumn=(spiritualWeapon.Cell.x+1).ToString();spiritualRow=(spiritualWeapon.Cell.y+1).ToString();panel="spiritualWeapon";}
    string SpiritualControlError(out Vector2Int p)
    {
        RefreshSpiritualWeapon();p=spiritualWeapon.Cell;
        if(!spiritualWeapon.Active(worldRounds)||concentrationSpell!="灵体武器")return "灵体武器已结束。";
        if(map.SceneOf(0)!=spiritualWeapon.Scene)return "不在同一位面：暂停操控，专注与持续时间照常。";
        if(!spiritualWeapon.CanControl(combat.Round,worldRounds,map.SceneOf(0)))return "后续操控从施法后的下个自身回合开始。";
        if(page!="battle"||!map.Locked||rolling||reactionPending||spellTargeting||draggingToken>=0||summonTurns.Active)return "请先完成当前操作。";
        if(!PlayerTurnAllowed||!combat.PlayerCanAct||!combat.BonusActionAvailable)return "当前没有可用的玩家附赠动作。";
        if(!SpiritualInput(out p)||!spiritualWeapon.CanMove(p,map.Size))return "落点须在地图内且距武器当前位置不超过20尺。";
        int host=SpiritualHostAt(p);
        return spiritualAttack&&!SpiritualTargetAllowed(p,spiritualWeapon.Scene,host,spiritualTarget)?"所选目标须在移动后的武器5尺内；可取消攻击，仅移动。":"";
    }
    void DrawSpiritualWeaponControl()
    {
        Surface(new Rect(350,110,900,745),paperTexture);if(Btn(new Rect(1140,125,90,46),"关闭",!rolling)){panel="";return;}
        GUILayout.BeginArea(new Rect(375,190,390,620));spiritualScroll=GUILayout.BeginScrollView(spiritualScroll);
        GUILayout.Label("操控灵体武器",heading);GUILayout.Label("附赠动作：移动0～20尺，并可对武器5尺内一个生物进行一次近战法术攻击。不会触发战士额外攻击，不消耗普通移动力。",small);
        spiritualColumn=IntegerField(spiritualColumn,false);spiritualRow=IntegerField(spiritualRow,false);Vector2Int preview;if(SpiritualInput(out preview))DrawSpiritualTargets(preview,spiritualWeapon.Scene,SpiritualHostAt(preview));
        Vector2Int p;string error=SpiritualControlError(out p);GUILayout.Label(error==""?"可以移动并结算所选攻击。":error,small);
        bool enabled=GUI.enabled;GUI.enabled=enabled&&error=="";if(GUILayout.Button("确认操控（附赠动作）",button))StartCoroutine(ControlSpiritualWeapon());GUI.enabled=enabled;
        GUILayout.EndScrollView();GUILayout.EndArea();
        var board=new Rect(800,250,400,400);DrawMap(board,false);var ev=Event.current;var pointer=BoardPointer();
        if(!rolling&&ev.type==EventType.MouseDown&&ev.button==0&&board.Contains(pointer)){float step=board.width/map.Size;spiritualColumn=((int)((pointer.x-board.x)/step)+1).ToString();spiritualRow=((int)((pointer.y-board.y)/step)+1).ToString();ev.Use();}
    }
    IEnumerator ControlSpiritualWeapon()
    {
        Vector2Int p;if(SpiritualControlError(out p)!=""||!combat.TryUseBonusAction())yield break;
        bool busy=rolling;rolling=true;panel="";int target=spiritualTarget;bool attack=spiritualAttack;
        try{spiritualWeapon.Move(p);RefreshSpiritualWeapon();if(attack)yield return SpiritualWeaponAttack(target);}finally{rolling=busy;}
    }
    IEnumerator SpiritualWeaponAttack(int target)
    {
        RefreshSpiritualWeapon();if(!spiritualWeapon.Active(worldRounds)||map.SceneOf(0)!=spiritualWeapon.Scene||!SpiritualTargetAllowed(spiritualWeapon.Cell,spiritualWeapon.Scene,spiritualHost,target))yield break;
        var hit=new SpellAttackResult();yield return RollMappedSpellAttack(0,target,spiritualWeapon.Caster,"灵体武器",hit,true,spiritualWeapon.Cell);
        if(!hit.Hit){history.Insert(0,"灵体武器："+(hit.Intercepted?"镜像拦截":"未命中")+"，伤害0；武器保留。");yield break;}
        int count=SpiritualWeaponEffect.Dice(spiritualWeapon.Slot,hit.Critical),sum=0;var rolls=new int[count];for(int i=0;i<count;i++){rolls[i]=UnityEngine.Random.Range(1,9);sum+=rolls[i];}
        int modifier=AcidSplash.Modifier(attributes[SpellAbilityIndex(spiritualWeapon.Caster)]);
        yield return PresentRoll(8,rolls,"灵体武器："+count+"d8＋施法调整值 "+modifier+"，力场伤害 "+Math.Max(0,sum+modifier));
        var raw=new long[13];raw[6]=Math.Max(0,sum+modifier);
        if(target==0||target==-1){var extra=new ExtraMarkDamage();yield return RollMarkDamage(target==0,hit.Critical,extra);if(extra.Raw>0)raw[mark.DamageType]+=extra.Raw;}
        var reduced=new RayDamage();yield return ReduceRayPacket(true,raw,true,"灵体武器",reduced);
        outcomeDeferDepth++;try{int frost=target==0?AgathysRetaliation(true):0;int amount=ApplyMappedDamagePacket(target,0,reduced.Raw,hit.Critical);frost=ApplyAgathysRetaliation(frost,true);history.Insert(0,"灵体武器对"+TransitActorName(target)+"造成 "+amount+" 点结算伤害。");yield return MaintainConcentration(target==0?amount:0);if(frost>0)yield return MaintainConcentration(frost);RefreshBestialSpirit();ReconcileFallenRescue();}finally{outcomeDeferDepth--;CheckBattleOutcome();}
    }
    void DrawSpiritualWeaponMap(Rect board)
    {
        if(spiritualWeapon.Active(worldRounds)&&map.Scene==spiritualWeapon.Scene&&spiritualHost==int.MaxValue){var r=CellRect(board,spiritualWeapon.Cell);Fill(r,new Color(.7f,.7f,1,.55f));GUI.Label(r,"灵",centered);}
        Vector2Int p;if((panel=="spiritualWeapon"&&map.Scene==spiritualWeapon.Scene||panel=="spells"&&selectedSpellName=="Spiritual Weapon")&&SpiritualInput(out p))Fill(CellRect(board,p),new Color(.5f,.7f,1,.3f));
    }
}
