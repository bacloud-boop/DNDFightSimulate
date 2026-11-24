using System.Collections;
using UnityEngine;

public sealed partial class TrainingGame
{
    bool reincarnationPending;
    int reincarnationActor,reincarnationDecision;
    SpeciesProfile reincarnationSpecies;
    Vector2Int reincarnationAnchor;
    Vector2 reincarnationScroll;
    string reincarnationMessage="";
    CorpseClock CurrentCorpse(int actor)=>actor==0?playerCorpse:actor==-1?monsterCorpse:actor==BattleMap.RescueId?rescueCarrier.Corpse:null;
    BodyWeight CurrentBodyWeight(int actor)=>actor==0?playerWeight.Weight:actor==-1?monsterWeight.Weight:actor==BattleMap.RescueId?rescueCarrier.Weight:null;
    string ActorName(int actor)=>actor==0?"玩家":actor==-1?"木桩":"救援者";
    SpeciesProfile ActorSpecies(int actor)=>actor==0?playerSpecies:actor==-1?monsterSpecies:rescueCarrier.Species;
    bool FindReincarnationPosition(int actor,out Vector2Int anchor)
    {
        anchor=Vector2Int.zero;
        for(int y=0;y<map.Size;y++)for(int x=0;x<map.Size;x++){var cell=new Vector2Int(x,y);if(map.CanReincarnateAt(actor,cell,1)){anchor=cell;return true;}}return false;
    }
    string ReincarnationPlacementError(int actor)
    {
        var weight=CurrentBodyWeight(actor);
        if(weight==null||CurrentCorpse(actor)==null)return "旧躯体只用于搬运与回收装备，不能作为复活目标。";
        if(!weight.Known)return "请先录入原身体的体重。";
        if(actor==BattleMap.RescueId&&rescueCarrier.Load.Loaded)return "请先放下救援者搬运的对象。";
        if(combat.NeedsDeathSave||rescueCarrier.NeedsDeathSave)return "请先处理现场倒地角色的死亡豁免，再进行1小时施法。";
        Vector2Int anchor;return FindReincarnationPosition(actor,out anchor)?"":"旧遗体旁5尺内没有合法新身体位置；如遗体被搬运或吞咽，请先放下／放出。";
    }
    string ReincarnationChoiceError()
    {
        if(reincarnationSpecies==null||!reincarnationSpecies.Valid()||reincarnationSpecies.Kind==Species2024.None)return "请完成新种族的选择，熟习选项不可重复。";
        if(reincarnationActor==0){
            foreach(int id in reincarnationSpecies.Proficiencies())if(playerSkillTiers[id]>0){
                bool other=false;
                if(reincarnationSpecies.Kind==Species2024.Elf){foreach(int option in new[]{10,12,13})if(playerSkillTiers[option]==0)other=true;}
                else {int limit=id==reincarnationSpecies.ChosenSkill?18:55;for(int option=0;option<limit;option++)if(playerSkillTiers[option]==0)other=true;}
                if(other)return "新种族可选熟练与职业／背景重复，请在原列表内选择其他项目。";
            }
        }
        return map.CanReincarnateAt(reincarnationActor,reincarnationAnchor,CreatureProfile.SpacesForSize(reincarnationSpecies.BodySize))?"":"新身体位置须在旧遗体旁5尺内，且完整占地合法。";
    }
    IEnumerator CastReincarnation(int actor,bool external)
    {
        string error=actor==BattleMap.RescueId?RescueRevivalError("Reincarnate",external):RevivalError("Reincarnate",actor==0,external);
        if(error!=""||ReincarnationPlacementError(actor)!="")yield break;
        bool busy=rolling;string savedPanel=panel;panel="";rolling=true;long invisible=senses.Token(true,worldRounds);int formerLevel=ActorSpecies(actor).Level,priority=ActorSpecies(actor).BonusPriority;
        try{
            if(!external){PlayerCastWhileSubmerged("Reincarnate");EndConcentration("开始转生术的1小时施法");}
            yield return AdvanceRevivalTime("Reincarnate",external);
            error=RevivalRules.CompletionError("Reincarnate",CurrentCorpse(actor),worldRounds);
            if(!external&&(!aftermathAdvanceCompleted||!CanPlayerAftermathCast))error="施法者在施法期间失能，转生失败。";
            if(error!=""){history.Insert(0,"转生术失败："+error);yield break;}
            int result=Random.Range(1,11);dice="转生术 · 新种族 D10";
            yield return PresentRoll(10,new[]{result},"转生术："+SpeciesProfile.Names[result-1]);
            reincarnationActor=actor;reincarnationSpecies=new SpeciesProfile{Kind=ReincarnationRules.SpeciesFromDie(result),Level=formerLevel,BonusPriority=priority,ChosenSkill=result==4?12:0};
            if(!FindReincarnationPosition(actor,out reincarnationAnchor)){history.Insert(0,"转生术失败：施法完成后现场已无合法位置，施法耗时已推进。");yield break;}
            reincarnationDecision=0;reincarnationMessage="";reincarnationPending=true;
            while(reincarnationDecision==0)yield return null;reincarnationPending=false;
            if(ReincarnationChoiceError()!=""||!CompleteReincarnation(actor,reincarnationSpecies,reincarnationAnchor)){history.Insert(0,"转生术失败：完成时身体或占地条件发生变化。");yield break;}
            if(PendingTerrainCount>0)yield return DrainGreaseEntries();
            history.Insert(0,"转生术："+ActorName(actor)+"获得"+reincarnationSpecies.Name+"新身体，沿用体重、恢复至新HP上限；旧遗体和装备留在原处。已消耗的休息限定次数保留，复活只针对现身体。");dice="转生术 · 已完成";
        }finally{reincarnationPending=false;if(!external)BreakOrdinaryInvisibility(true,"施法",invisible);panel=savedPanel;rolling=busy;}
    }
    bool CompleteReincarnation(int actor,SpeciesProfile species,Vector2Int anchor)
    {
        if(CurrentCorpse(actor)==null||!CurrentCorpse(actor).Dead)return false;
        int formerSize=actor==0?EffectiveCreatureSize(true):actor==-1?EffectiveCreatureSize(false):rescueCarrier.Size;
        SyncWeaponInventory();var oldWeight=CurrentBodyWeight(actor);var armor=actor==0?playerArmor:actor==-1?monsterArmor:null;
        var old=map.ReincarnateAt(actor,anchor,CreatureProfile.SpacesForSize(species.BodySize),ActorName(actor)+"旧躯体",oldWeight,armor==null?0:armor.ArmorIndex,armor!=null&&armor.Shield);if(old==null)return false;
        old.Frozen=CurrentCorpse(actor).Frozen;old.BodySize=formerSize;equipmentObjects.Archive(actor,old.Id);
        if(actor==BattleMap.RescueId){
            if(!rescueCarrier.Reincarnate(species))return false;regeneration.RestoreBody(actor);RestoreRescueBreathingAfterRevival();
            rescueSpeciesDraft=rescueCarrier.Species.Copy();rescueSize=rescueCarrier.Size;rescueSpeed=rescueCarrier.Speed.ToString();rescueEquipment="0";rescueBody=rescueCarrier.Weight.BodyPounds.Value.ToString();rescueMaxHp=rescueCarrier.BaseMaxHp.ToString();
        }
        else {
            int mass=ReincarnationRules.NewBodyPounds(old.Weight);
            if(actor==0){carriedWeapons.BeginBattle();carriedWeapons.LeaveOnRemains(old.Id);old.Weight.SetWeaponWeight(checked((int)carriedWeapons.RemainsOunces(old.Id)));weaponGrip.DropHeld();shillelagh.Clear();playerSpecies=species.Copy();playerArmor.Apply(0,false,false);playerArmorDraft=0;playerShieldDraft=false;highElfCantripReplacement=false;}
            else {monsterSpecies=species.Copy();monsterProfile.TryApply(9,species.BodySize);monsterTypeDraft=9;monsterSizeDraft=species.BodySize;combat.UpdateMonsterBaseSpeed(species.Speed);monsterSpeedInput=species.Speed.ToString();monsterArmor.Apply(0,false,monsterArmor.Custom,combat.MonsterBaseArmorClass);monsterArmorDraft=0;monsterShieldDraft=false;}
            oldWeight.SetStoredGear(0);oldWeight.SetArmorWeight(0);oldWeight.SetWeaponWeight(0);oldWeight.TryApply(mass.ToString(),"0");
            var weightDraft=actor==0?playerWeight:monsterWeight;weightDraft.Equipment="0";weightDraft.Body=mass.ToString();ApplySpeciesHp(actor==0,false);
            combat.Revive(actor==0,ReincarnationRules.RestoredHp(actor==0?combat.PlayerMaxHp:combat.MaxHp),false);regeneration.RestoreBody(actor);CurrentCorpse(actor).Clear();RestoreBreathingAfterRevival(actor==0);
        }
        stableRecoveryAt=-1;SyncWeaponInventory();SyncTimedBuffs();ObserveDeaths();return true;
    }
    void DrawReincarnationPrompt()
    {
        Fill(new Rect(0,0,1600,900),new Color(0,0,0,.95f));Surface(new Rect(340,100,920,750),paperTexture);
        GUILayout.BeginArea(new Rect(365,130,410,680));reincarnationScroll=GUILayout.BeginScrollView(reincarnationScroll);GUILayout.Label("转生术 · "+ActorName(reincarnationActor),heading);
        GUILayout.Label("种族已由D10确定。填写该种族提供的选择，并在右侧选择旧遗体旁5尺内的绿色位置。体重沿用原值；旧体不作为复活目标。未接入的种族特性仍按其状态保留说明。",small);
        DrawSpeciesConfiguration(reincarnationSpecies,reincarnationActor==0,false,true,false);
        string error=ReincarnationChoiceError();GUILayout.Label(error==""?"当前选择与新身体位置合法。":error,small);
        bool enabled=GUI.enabled;GUI.enabled=enabled&&error=="";
        if(GUILayout.Button("完成转生（新身体恢复至HP上限）",button)){reincarnationMessage=ReincarnationChoiceError();if(reincarnationMessage=="")reincarnationDecision=1;}
        GUI.enabled=enabled;GUILayout.Label(reincarnationMessage,small);GUILayout.EndScrollView();GUILayout.EndArea();
        var board=new Rect(810,255,400,400);Surface(new Rect(805,250,410,410),paperTexture);int spaces=CreatureProfile.SpacesForSize(reincarnationSpecies.BodySize);
        for(int y=0;y<map.Size;y++)for(int x=0;x<map.Size;x++){var cell=new Vector2Int(x,y);Fill(CellRect(board,cell),map.IsWater(cell)?new Color(.1f,.4f,.7f,.4f):new Color(.4f,.25f,.1f,.15f));if(map.CanReincarnateAt(reincarnationActor,cell,spaces))Fill(CellRect(board,cell),new Color(.1f,.8f,.3f,.4f));}
        Token(BodyRect(board,map.Player,map.PlayerSpaces),true);Token(BodyRect(board,map.Monster,map.MonsterSpaces),false);DrawRetainedRemains(board);DrawRescueMarker(board);Fill(BodyRect(board,reincarnationAnchor,spaces),new Color(.85f,.75f,.2f,.5f));
        var ev=Event.current;var point=BoardPointer();if(ev.type==EventType.MouseDown&&ev.button==0&&board.Contains(point)){float step=board.width/map.Size;var selected=new Vector2Int((int)((point.x-board.x)/step),(int)((point.y-board.y)/step));if(map.CanReincarnateAt(reincarnationActor,selected,spaces))reincarnationAnchor=selected;ev.Use();}
        GUI.Label(new Rect(810,680,400,75),"绿色：合法新身体位置\n金色：当前选择；旧遗体将保留原占地",small);
    }
}
