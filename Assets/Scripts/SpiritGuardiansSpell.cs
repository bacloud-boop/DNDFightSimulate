using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    int playerAlignment=Alignment2024.Default,alignmentDraft=Alignment2024.Default,guardianAppearance;
    readonly SpiritGuardiansEffect guardians=new SpiritGuardiansEffect();readonly HashSet<int> guardianExclusions=new HashSet<int>();
    void InitializeGuardians(){combat.MonsterSpeedTransform=speed=>GuardianSpeed(-1,speed);map.MonsterTurnSpeed=()=>combat.MonsterSpeed;rescueCarrier.SpeedTransform=speed=>GuardianSpeed(BattleMap.RescueId,speed);}
    void EndGuardians(){guardians.Clear();startStatuses.RemoveAll(e=>e.Kind==3);}
    bool GuardianBody(int actor,out Vector2Int p,out int n)
    {
        string name;if(!guardians.Affects(actor,worldRounds)||combat.PlayerHp<=0||combat.PlayerDead||combat.PlayerParalyzed||combat.PlayerFeignDeath||combat.PlayerBanished||!map.SameScene(0,actor)||!HealingHandsTarget(actor,out p,out n,out name)){p=Vector2Int.zero;n=0;return false;}
        return map.IsContained(true)?actor==-1:!((actor==0||actor==-1)&&map.IsContained(actor==0));
    }
    bool GuardianAffected(int actor){Vector2Int p;int n;return GuardianBody(actor,out p,out n)&&BattleMap.BodyDistance(map.Player,map.PlayerSpaces,p,n)<=15;}
    int GuardianSpeed(int actor,int speed)=>GuardianAffected(actor)&&!MovementFreedom(actor)?speed/2:speed;
    bool GuardianEndAvailable(int actor)=>GuardianAffected(actor)&&guardians.Available(actor,terrainTurn,worldRounds);
    string GuardianError()=>worldRounds<0||worldRounds>long.MaxValue-100?"世界时间超出范围。":"";
    void DrawGuardianChoices()
    {
        GUILayout.Label("当前阵营："+Alignment2024.Names[playerAlignment]+" · "+DamageTypes.Names[Alignment2024.GuardianDamageType(playerAlignment)]+"伤害。指定免受影响的生物：",small);
        if(playerAlignment<6)guardianAppearance=GUILayout.SelectionGrid(guardianAppearance,new[]{"天使灵体","妖精灵体"},2,button);else GUILayout.Label("邪恶阵营：邪魔灵体",small);
        foreach(int actor in BreathActors())if(actor!=0){bool old=guardianExclusions.Contains(actor),value=GUILayout.Toggle(old,TransitActorName(actor)+"免受影响",label);if(value)guardianExclusions.Add(actor);else guardianExclusions.Remove(actor);}
        GUILayout.Label("15尺光环随自己移动，未排除的其他生物速度减半。创建时、光环移入目标、目标进入或在其中结束回合时，感知豁免，每环1d8伤害，成功半伤；每个生物每回合至多一次。专注至多10分钟，施法者自身不受影响。",small);
    }
    void CastGuardians(int caster,int slot,HashSet<int> exclusions,int appearance=0)
    {
        if(!guardians.Begin(slot,playerAlignment,8L+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(caster)]),exclusions,worldRounds,appearance))return;
        concentrationSpell="灵体卫士";concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=guardians.Expires;
        foreach(int actor in BreathActors())if(GuardianAffected(actor))QueueGuardian(actor,map.Player,map.PlayerSpaces);
        SyncTimedBuffs();history.Insert(0,"灵体卫士："+slot+"d8 "+DamageTypes.Names[guardians.Type]+"，15尺光环；创建立即触发，计入本回合每个目标一次上限。");
    }
    void QueueGuardian(int actor,Vector2Int origin,int spaces)
    {
        Vector2Int p;int n;if(GuardianBody(actor,out p,out n)&&guardians.Claim(actor,terrainTurn,worldRounds))startStatuses.Add(new StartStatus{Actor=actor,Kind=3,Revision=guardians.Revision,Origin=origin,Spaces=spaces});
    }
    void QueueGuardianEnd(int actor){if(GuardianEndAvailable(actor))QueueGuardian(actor,map.Player,map.PlayerSpaces);}
    void ObserveGuardianMovement(int actor,Vector2Int from,Vector2Int to,int oldSpaces,int spaces,bool landing)
    {
        if(!guardians.Active(worldRounds))return;
        var path=landing?new List<Vector2Int>{to}:BattleMap.MovementAnchors(from,to);if(from==to&&oldSpaces!=spaces)path.Add(to);var previous=from;int previousSize=oldSpaces;
        foreach(var cell in path){
            if(actor==0){foreach(int target in BreathActors()){Vector2Int p;int n;if(GuardianBody(target,out p,out n)&&BattleMap.BodyDistance(previous,previousSize,p,n)>15&&BattleMap.BodyDistance(cell,spaces,p,n)<=15)QueueGuardian(target,cell,spaces);}}
            else {Vector2Int p;int n;if(GuardianBody(actor,out p,out n)&&BattleMap.BodyDistance(map.Player,map.PlayerSpaces,previous,previousSize)>15&&BattleMap.BodyDistance(map.Player,map.PlayerSpaces,cell,spaces)<=15)QueueGuardian(actor,map.Player,map.PlayerSpaces);}
            previous=cell;previousSize=spaces;
        }
    }
    bool GuardianPathNeeded(Vector2Int from,Vector2Int to,int spaces)
    {
        if(!guardians.Active(worldRounds))return false;
        // All moves while an emanation exists are stepped: either the mover or
        // its carried caster can cross a speed boundary along the route.
        return from!=to;
    }
    IEnumerator ResolveGuardianSave(int actor,long revision,Vector2Int origin,int spaces)
    {
        if(revision!=guardians.Revision||!guardians.Active(worldRounds))yield break;
        Vector2Int p;int n;string name;if(!HealingHandsTarget(actor,out p,out n,out name))yield break;
        int count=guardians.Slot,type=guardians.Type;long dc=guardians.Dc;int sum=0;var rolls=new int[count];for(int i=0;i<count;i++){rolls[i]=Random.Range(1,9);sum+=rolls[i];}
        outcomeDeferDepth++;try{yield return PresentRoll(8,rolls,"灵体卫士："+count+"d8 = "+sum+" "+DamageTypes.Names[type]);
            var reduced=new RayDamage();yield return ReduceRayDamage(true,sum,DamageTypes.Names[type],"灵体卫士",reduced);var failed=new HashSet<int>();yield return RollMappedAbilitySave(actor,0,dc,failed,"灵体卫士",3,origin,spaces);
            ApplyMappedAreaDamage(actor,0,failed.Contains(actor)?reduced.Amount:reduced.Amount/2,type,"灵体卫士");yield return MaintainConcentration(0);RefreshBestialSpirit();ReconcileFallenRescue();
        }finally{outcomeDeferDepth--;CheckBattleOutcome();}
    }
    void DrawGuardianArea(Rect board)
    {
        if(!guardians.Active(worldRounds)||!map.OnScene(0)||map.IsContained(true))return;
        for(int y=0;y<map.Size;y++)for(int x=0;x<map.Size;x++)if(BattleMap.BodyDistance(map.Player,map.PlayerSpaces,new Vector2Int(x,y),1)<=15)Fill(CellRect(board,new Vector2Int(x,y)),guardians.Type==8?new Color(.55f,.25f,.7f,.18f):new Color(1f,.8f,.3f,.18f));
        GUI.Label(BodyRect(board,map.Player,map.PlayerSpaces),new[]{"天使卫士","妖精卫士","邪魔卫士"}[guardians.Appearance],small);
    }
}
