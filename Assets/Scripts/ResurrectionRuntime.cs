using UnityEngine;
public sealed partial class TrainingGame
{
    readonly ResurrectionCasterState playerResurrectionCaster=new ResurrectionCasterState();
    readonly ResurrectionCasterState externalResurrectionCaster=new ResurrectionCasterState();
    bool ResurrectionD20Disadvantage(bool player)=>player&&playerResurrectionCaster.D20Disadvantage;
    ArmorSaveRoll RollD20Test(bool player,bool advantage=false,bool disadvantage=false,int ability=-1)
    {
        advantage|=SpeciesSaveAdvantage(player,ability)||ability==3&&BeaconHope(player?0:-1);
        disadvantage|=ResurrectionD20Disadvantage(player);
        return RollSavingThrowDice(advantage,disadvantage);
    }
    ArmorSaveRoll RollSavingThrowDice(bool advantage,bool disadvantage)
    {
        bool two=advantage!=disadvantage;
        int first=Random.Range(1,21),second=two?Random.Range(1,21):0;
        return new ArmorSaveRoll{Value=PoisonProtectionEffect.SelectDie(first,second,advantage,disadvantage),Dice=two?new[]{first,second}:new[]{first}};
    }
    void RecordResurrectionExertion(bool external,bool required)
    {
        if(!required)return;
        (external?externalResurrectionCaster:playerResurrectionCaster).ApplyExertion();
        history.Insert(0,(external?"固定外部施法者":"玩家施法者")+"因复生死亡满365天的目标而疲惫：完成长休前不能施法，所有D20检定劣势。该状态不是力竭等级。");
    }
    void DrawExternalCasterRest()
    {
        GUILayout.Label("固定外部施法者："+(externalResurrectionCaster.Taxed?"疲惫 · 禁止施法 · D20劣势":"可施法"),small);
        bool enabled=GUI.enabled;
        GUI.enabled=enabled&&combat.Aftermath&&!rolling&&!combat.NeedsDeathSave&&!rescueCarrier.NeedsDeathSave&&externalResurrectionCaster.CanLongRest(worldRounds);
        if(GUILayout.Button("外部施法者完成长休（推进8小时）",button))StartCoroutine(RestExternalCaster());
        GUI.enabled=enabled;
    }
}
