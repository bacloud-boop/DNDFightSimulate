using System;
using System.Linq;
using System.Collections;
using UnityEngine;
public sealed partial class TrainingGame
{
 readonly WeaponInventory carriedWeapons=new WeaponInventory(Weapon.Available.Select(w=>w.Name),Weapon.Available[0].Name);
 readonly GroundWeaponState monsterGroundWeapon=new GroundWeaponState(-1);
 bool PlayerWeaponLoadFits(long ounces)=>EquipmentLoadRules.AllowedOunces(attributes[0],CarryingCreatureSize(true),((long)playerWeight.Weight.EquipmentPounds+playerWeight.Weight.ArmorPounds)*16+ounces+playerWeight.Weight.StoredGearOunces);
 bool WeaponFallsInWater(Vector2Int cell,int spaces)=>map.IsWater(new Vector2Int(cell.x+spaces/2,cell.y+spaces/2));
 void DropSleepingMonsterWeapon(){if(!MonsterPolymorphed&&combat.MonsterAttackIsWeapon&&combat.MonsterWeaponIsHeld)monsterGroundWeapon.Drop(map.Monster.x,map.Monster.y,map.MonsterSpaces,WeaponFallsInWater(map.Monster,map.MonsterSpaces),WeaponContainer(false));}
 void SyncWeaponInventory()
 {
  if(combat.BattleStarted&&!carriedWeapons.Locked)carriedWeapons.BeginBattle();
  if(combat.PlayerHp==0&&!PlayerInBeastForm&&!weaponGrip.Stowed){carriedWeapons.BeginBattle();if(carriedWeapons.Drop(selectedWeapon.Name,map.Player.x,map.Player.y,map.PlayerSpaces,WeaponFallsInWater(map.Player,map.PlayerSpaces),WeaponContainer(true))){weaponGrip.DropHeld();RefreshShillelagh();}}
  if(map.BodyPresent(-1)&&!UndeadClaimsCustomWeapon()&&combat.Hp==0&&!MonsterPolymorphed&&combat.MonsterWeaponIsHeld)monsterGroundWeapon.Drop(map.Monster.x,map.Monster.y,map.MonsterSpaces,WeaponFallsInWater(map.Monster,map.MonsterSpaces),WeaponContainer(false));
  playerWeight.Weight.SetWeaponWeight(checked((int)carriedWeapons.CarriedOunces));
  monsterWeight.Weight.SetWeaponWeight(combat.MonsterWeaponIsHeld&&!monsterGroundWeapon.Dropped?combat.MonsterWeaponOunces:0);
  if(objectLight.ObjectId>0||continualFlame.All.Length>0)RefreshLightSpell();
 }
 void DrawCarriedWeapons()
 {
  if(GUILayout.Button("物件体型与遮盖／揭开",button)&&!rolling)OpenEquipmentObjects("weapons");
  GUILayout.Label("随身武器："+WeightUnits.Format(carriedWeapons.CarriedOunces)+"；场外武器不能在战斗中取用。",small);
  if(!combat.BattleStarted)foreach(var item in carriedWeapons.Items){bool current=item.Location==WeaponLocation.Carried;bool next=GUILayout.Toggle(current,item.Key+" · "+WeightUnits.Format(item.WeightOunces),label);if(next!=current&&!carriedWeapons.TrySetCarried(item.Key,next,PlayerWeaponLoadFits))dice="拒绝配置：携带武器后超过负重上限。";}
  foreach(var item in carriedWeapons.Items)if(item.Location==WeaponLocation.Ground&&item.FollowerId==0){
   bool old=GUI.enabled;bool near=(item.Ground.ContainedBy.HasValue?item.Ground.AccessibleFrom(WeaponContainer(true)):item.Ground.WithinReach(map.Player.x,map.Player.y,map.PlayerSpaces,WeaponContainer(true)))&&(!item.Ground.Submerged||map.PlayerAquatic.Submerged);
   GUI.enabled=old&&!rolling&&near&&PlayerTurnAllowed&&combat.PlayerHp>0&&!combat.PlayerDead&&!combat.PlayerParalyzed&&!combat.PlayerFeignDeath&&!PlayerInBeastForm&&weaponGrip.Stowed;
   if(GUILayout.Button("拾取 "+item.Key+(item.Ground.ContainedBy.HasValue?"（吞咽体内，需空手）":item.Ground.Submerged?"（水下，须潜水，5尺内）":"（5尺内，需空手）"),button)){
    if(carriedWeapons.PickUp(item.Key,PlayerWeaponLoadFits,()=>weaponGrip.TryInteract(combat.Round,worldRounds,()=>combat.TryUseObjectAction()),map.PlayerAquatic.Submerged,WeaponContainer(true))){selectedWeapon=Weapon.Available.First(w=>w.InventoryKey==item.Key);weaponGrip.TrySetStowed(false,combat.Round,worldRounds,()=>true,true);SyncWeaponInventory();history.Insert(0,"拾回 "+item.Key+"。");}else dice="拾取失败：负重或交互额度不足。";
   }GUI.enabled=old;
  }
 }
 IEnumerator RecoverMonsterWeapon()
 {
  if(UndeadClaimsCustomWeapon()||!monsterGroundWeapon.AccessibleFrom(WeaponContainer(false))||!combat.MonsterCanAct||MonsterPolymorphed)yield break;
  Func<bool> fits=()=>CarryingRules.CanCarryOunces(EffectiveMonsterAbility(0),CarryingCreatureSize(false),((long)monsterWeight.Weight.EquipmentPounds+monsterWeight.Weight.ArmorPounds)*16+combat.MonsterWeaponOunces+monsterWeight.Weight.StoredGearOunces,0);
  if(!fits())yield break;
  Func<int,int,bool> near=(x,y)=>(monsterGroundWeapon.ContainedBy.HasValue||monsterGroundWeapon.WithinReach(x,y,map.MonsterSpaces,WeaponContainer(false)))&&(!monsterGroundWeapon.Submerged||map.WaterCells(new Vector2Int(x,y),map.MonsterSpaces)==map.MonsterSpaces*map.MonsterSpaces);
  if(!near(map.Monster.x,map.Monster.y)){
   Vector2Int best=map.Monster;int bestDistance=int.MaxValue,bestCost=int.MaxValue;bool found=false;
   for(int y=0;y<map.Size;y++)for(int x=0;x<map.Size;x++){
    var cell=new Vector2Int(x,y);if(!map.CanMoveMonsterTo(cell))continue;int cost=map.MonsterMovementCost(cell);
    int dive=monsterGroundWeapon.Submerged&&!map.MonsterAquatic.Submerged&&!waterWalk.Active(-1,worldRounds)?5:0;
    bool diveBonus=monsterGroundWeapon.Submerged&&!map.MonsterAquatic.Submerged&&waterWalk.Active(-1,worldRounds);
    bool reachable=near(x,y)&&cost+dive<=map.MonsterMovementRemaining&&(!diveBonus||combat.MonsterBonusActionAvailable);
    long dx=(long)x*2+map.MonsterSpaces-monsterGroundWeapon.CenterX2,dy=(long)y*2+map.MonsterSpaces-monsterGroundWeapon.CenterY2;int distance=(int)(dx*dx+dy*dy);
    if(reachable&&!found||reachable==found&&(reachable?cost<bestCost:distance<bestDistance||distance==bestDistance&&cost<bestCost)){found=reachable;best=cell;bestCost=cost;bestDistance=distance;}
   }
   long currentX=(long)map.Monster.x*2+map.MonsterSpaces-monsterGroundWeapon.CenterX2,currentY=(long)map.Monster.y*2+map.MonsterSpaces-monsterGroundWeapon.CenterY2;
   if(best!=map.Monster&&(found||bestDistance<currentX*currentX+currentY*currentY))yield return ResolveMovement(false,best,false,true);
  }
  if(!near(map.Monster.x,map.Monster.y)||!combat.MonsterCanAct)yield break;
  if(monsterGroundWeapon.Submerged&&!map.MonsterAquatic.Submerged&&!map.SwitchWaterState(false,true))yield break;
  if(monsterGroundWeapon.PickUp(-1,map.Monster.x,map.Monster.y,map.MonsterSpaces,combat.MonsterCanAct,true,()=>CarryingRules.CanCarryOunces(EffectiveMonsterAbility(0),CarryingCreatureSize(false),((long)monsterWeight.Weight.EquipmentPounds+monsterWeight.Weight.ArmorPounds)*16+combat.MonsterWeaponOunces+monsterWeight.Weight.StoredGearOunces,0),()=>true,map.MonsterAquatic.Submerged,WeaponContainer(false))){SyncWeaponInventory();history.Insert(0,"木桩使用本回合自由物件交互拾回自己的武器。");}
 }
 void DrawGroundWeapons(Rect board)
 {
  float step=board.width/map.Size;
  foreach(var item in carriedWeapons.Items)if(item.Location==WeaponLocation.Ground&&!item.Ground.ContainedBy.HasValue)GUI.Label(new Rect(board.x+(item.Ground.CenterX2*.5f-.5f)*step,board.y+(item.Ground.CenterY2*.5f-.5f)*step,step,step),item.Ground.Submerged?"⚔↓":"⚔",centered);
  if(monsterGroundWeapon.Dropped&&!monsterGroundWeapon.ContainedBy.HasValue)GUI.Label(new Rect(board.x+(monsterGroundWeapon.CenterX2*.5f-.5f)*step,board.y+(monsterGroundWeapon.CenterY2*.5f-.5f)*step,step,step),monsterGroundWeapon.Submerged?"⚔↓":"⚔",centered);
 }
 void ResetWeaponInventory(){carriedWeapons.ResetBattle();monsterGroundWeapon.Reset();SyncWeaponInventory();}
}
