using System.Linq;
using UnityEngine;

public sealed partial class TrainingGame
{
    Vector2 remainsScroll;
    string remainsMessage="";
    bool RemainsLootReach(RetainedRemains body)
    {
        if(BattleMap.BodyDistance(map.Player,map.PlayerSpaces,new Vector2Int(body.X,body.Y),body.Spaces)>5)return false;
        bool underwater=map.RescueCargoId==body.Id&&!map.RescueCargoDragging?map.RescueAquatic.Submerged:WeaponFallsInWater(new Vector2Int(body.X,body.Y),body.Spaces);
        return !underwater||map.PlayerAquatic.Submerged;
    }
    void RefreshRemainsWeight(RetainedRemains body)
    {
        if(body.Owner==0)body.Weight.SetWeaponWeight(checked((int)carriedWeapons.RemainsOunces(body.Id)));
        if(rescueCarrier.Load.TargetId==body.Id)rescueCarrier.Load.RefreshWeight(body.Weight);
    }
    void DrawRetainedRemains(Rect board)
    {
        foreach(var body in map.Remains.Bodies)if(map.RemainsSpaces(body)>0){var rect=BodyRect(board,new Vector2Int(body.X,body.Y),body.Spaces);Fill(rect,new Color(.25f,.22f,.23f,.85f));GUI.Label(rect,"遗",new GUIStyle(centered){fontSize=16});}
    }
    void DrawRemainsPanel()
    {
        Surface(new Rect(350,110,480,745),paperTexture);if(Btn(new Rect(720,125,90,46),"关闭")){panel="";return;}
        GUILayout.BeginArea(new Rect(375,185,430,630));remainsScroll=GUILayout.BeginScrollView(remainsScroll);GUILayout.Label("转生后的旧躯体与装备",heading);
        GUILayout.Label("旧躯体独立保留，可由救援者肩扛或拖行；落地及拖行时按原占地提供掩护。装备回收需5尺内、水下须潜水，并遵守交互与负重。护甲、盾牌回收到背包，不在战斗中重新穿戴。",small);
        foreach(var body in map.Remains.Bodies){
            GUILayout.Space(10);GUILayout.Label(body.Name+" · 总重 "+WeightUnits.Format(body.Weight.TotalOunces),label);
            bool enabled=GUI.enabled;bool ready=page=="battle"&&!rolling&&!PlayerInBeastForm&&PlayerTurnAllowed&&combat.PlayerHp>0&&!combat.PlayerDead&&!combat.PlayerParalyzed&&!combat.PlayerFeignDeath&&RemainsLootReach(body);
            GUI.enabled=enabled&&ready&&body.NonWeaponGearOunces>0&&(!PlayerHasShield||weaponGrip.Stowed);
            if(GUILayout.Button("回收护甲、盾牌与其他装备 · "+WeightUnits.Format(body.NonWeaponGearOunces),button)){
                if(body.RecoverGear(playerWeight.Weight,extra=>CarryingRules.CanCarryOunces(EffectivePlayerAbility(0),CarryingCreatureSize(true),playerWeight.Weight.GearOunces,extra),()=>weaponGrip.TryInteract(combat.Round,worldRounds,()=>combat.TryUseObjectAction()))){RefreshRemainsWeight(body);remainsMessage="装备已放入背包；没有改变当前穿戴与AC。";}else remainsMessage="回收失败：负重或物件交互额度不足。";
            }
            foreach(var item in carriedWeapons.Items.Where(i=>i.Location==WeaponLocation.Remains&&i.RemainsId==body.Id).ToArray()){
                GUI.enabled=enabled&&ready&&weaponGrip.Stowed;
                if(GUILayout.Button("拾取 "+item.Key+" · "+WeightUnits.Format(item.WeightOunces),button)){
                    if(carriedWeapons.PickUpFromRemains(item.Key,body.Id,PlayerWeaponLoadFits,()=>weaponGrip.TryInteract(combat.Round,worldRounds,()=>combat.TryUseObjectAction()))){selectedWeapon=Weapon.Available.First(w=>w.InventoryKey==item.Key);weaponGrip.TrySetStowed(false,combat.Round,worldRounds,()=>true,true);RefreshRemainsWeight(body);SyncWeaponInventory();remainsMessage="已拾取 "+item.Key+"。";}else remainsMessage="拾取失败：负重或交互额度不足。";
                }
            }
            GUI.enabled=enabled;
        }
        GUILayout.Label(remainsMessage,small);GUILayout.EndScrollView();GUILayout.EndArea();
    }
    void DrawRescueRetainedCargo()
    {
        if(rescueCarrier.Load.Loaded)return;
        foreach(var body in map.Remains.Bodies)foreach(bool drag in new[]{false,true}){
            bool fits=drag?CarryingRules.CanDragOunces(rescueCarrier.Strength,rescueCarrier.Species.CapacitySize(rescueCarrier.Size),body.Weight.TotalOunces):CarryingRules.CanCarryOunces(rescueCarrier.Strength,rescueCarrier.Species.CapacitySize(rescueCarrier.Size),rescueCarrier.Weight.GearOunces,body.Weight.TotalOunces);
            bool enabled=GUI.enabled;GUI.enabled=enabled&&!rolling&&fits&&map.CanAttachRescueRemains(body.Id)&&rescueCarrier.TurnActive&&rescueCarrier.ActionsRemaining>0;
            if(GUILayout.Button((drag?"拖行":"肩扛")+body.Name+"（1动作）",button))if(rescueCarrier.PickUp(body.Id,true,body.Weight,drag)&&map.AttachRescueRemains(body.Id,drag)){rescueMessage="已搬运独立旧躯体，重量包含尚未回收的装备。";history.Insert(0,rescueMessage);}
            GUI.enabled=enabled;
        }
    }
}
