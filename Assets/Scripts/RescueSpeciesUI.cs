using UnityEngine;

public sealed partial class TrainingGame
{
    void DrawRescueStoneSense(Rect board)
    {
        if(!map.RescuePosition.HasValue||!rescueCarrier.Conscious||rescueCarrier.Species.Kind!=Species2024.Dwarf||!rescueCarrier.SpeciesAbilities.StoneActive(worldRounds)||!map.StoneContact(map.RescuePosition.Value,map.RescueSpaces))return;
        foreach(bool player in new[]{true,false}){
            var cell=player?map.Player:map.Monster;int spaces=player?map.PlayerSpaces:map.MonsterSpaces;
            if(map.IsContained(player)||map.IsCarried(player)||player&&map.Riding||BattleMap.BodyDistance(map.RescuePosition.Value,map.RescueSpaces,cell,spaces)>60||!map.SameStoneSurface(map.RescuePosition.Value,map.RescueSpaces,cell,spaces))continue;
            var rect=BodyRect(board,cell,spaces);GUI.Label(new Rect(rect.x,rect.y-18,160,18),"救援震感 · "+(player?"玩家":"木桩"),new GUIStyle(small){normal={textColor=Color.yellow}});
        }
    }
    void DrawRescueSpeciesActions()
    {
        GUILayout.Label("种族："+rescueCarrier.Species.Name+" · 等级 "+rescueCarrier.Species.Level+" · 熟练 "+rescueCarrier.ProficiencyBonus,small);
        GUILayout.Label("附赠动作 "+(rescueCarrier.BonusActionAvailable?1:0)+" / 1 · 反应 "+(rescueCarrier.ReactionAvailable?1:0)+" / 1 · 临时HP "+rescueCarrier.TemporaryHp,small);
        var species=rescueCarrier.Species;bool old=GUI.enabled;
        if(species.Kind==Species2024.Orc||species.Kind==Species2024.Dwarf){
            GUI.enabled=old&&!rolling&&rescueCarrier.Conscious&&rescueCarrier.TurnActive&&rescueCarrier.BonusActionAvailable&&rescueCarrier.SpeciesAbilities.CanUse(species.Kind,rescueCarrier.ProficiencyBonus,worldRounds);
            if(GUILayout.Button((species.Kind==Species2024.Orc?"肾上腺素激增":"石中精妙")+"（附赠动作） · 剩 "+rescueCarrier.SpeciesAbilities.Remaining(species.Kind,rescueCarrier.ProficiencyBonus),button)){
                if(rescueCarrier.UseSpeciesBonus(worldRounds))history.Insert(0,species.Kind==Species2024.Orc?"救援者附赠疾走并获得熟练加值数量的临时HP，均不与原临时HP叠加。":"救援者获得60尺震感，持续10分钟；须接触相连石面，不视为视觉。");
            }
            if(species.Kind==Species2024.Dwarf&&rescueCarrier.SpeciesAbilities.StoneActive(worldRounds))GUILayout.Label("石中精妙："+(!map.RescuePosition.HasValue||!map.StoneContact(map.RescuePosition.Value,map.RescueSpaces)?"当前未接触石面":"震感生效，不提供视觉"),small);
        }
        GUI.enabled=old&&!rolling&&rescueCarrier.Conscious&&!rescueCarrier.Load.Loaded&&!combat.NeedsDeathSave;
        if(GUILayout.Button("救援者短休（1小时，恢复短休次数）",button)){rescueCarrier.EndTurn();AdvanceAftermathTime(600);rescueCarrier.ShortRest();if(rescueCarrier.Conscious)rescueCarrier.BeginTurn(worldRounds);}
        GUI.enabled=GUI.enabled&&rescueCarrier.CanLongRest(worldRounds);
        if(GUILayout.Button("救援者长休（"+(species.LongRestRounds/600)+"小时）",button)){long start=worldRounds;rescueCarrier.EndTurn();AdvanceAftermathTime(species.LongRestRounds);if(rescueCarrier.FinishLongRest(start,worldRounds))rescueCarrier.BeginTurn(worldRounds);}
        GUI.enabled=old;
    }
}
