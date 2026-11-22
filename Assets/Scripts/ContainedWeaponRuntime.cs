using System.Collections.Generic;
public sealed partial class TrainingGame
{
    int? WeaponContainer(bool player)=>map.IsContained(player)?(int?)(player?-1:0):null;
    IEnumerable<GroundWeaponState> DroppedWeapons()
    {
        foreach(var item in carriedWeapons.Items)if(item.Ground.Dropped)yield return item.Ground;
        if(monsterGroundWeapon.Dropped)yield return monsterGroundWeapon;
    }
    void SyncContainedWeapons()
    {
        foreach(var ground in DroppedWeapons())if(ground.ContainedBy.HasValue){
            bool host=ground.ContainedBy.Value==0;var p=host?map.Player:map.Monster;
            ground.FollowContainer(p.x,p.y,host?map.PlayerSpaces:map.MonsterSpaces);
        }
    }
    void ReleaseContainedWeapons(bool hostPlayer)
    {
        int host=hostPlayer?0:-1;var p=hostPlayer?map.Player:map.Monster;int n=hostPlayer?map.PlayerSpaces:map.MonsterSpaces;
        foreach(var ground in DroppedWeapons())if(ground.ContainedBy==host)ground.ReleaseContainer(p.x,p.y,n,WeaponFallsInWater(p,n));
        RefreshLightSpell();
    }
}
