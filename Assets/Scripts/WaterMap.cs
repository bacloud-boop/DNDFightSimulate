using UnityEngine;

public sealed partial class BattleMap
{
    readonly bool[,,] water=new bool[3,20,20];
    public readonly AquaticState PlayerAquatic=new AquaticState(),MonsterAquatic=new AquaticState(),RescueAquatic=new AquaticState();
    public System.Func<int,bool> WaterWalking,WaterWalkBonus;
    bool HasWaterWalking(int actor)=>WaterWalking!=null&&WaterWalking(actor);
    AquaticState ActorAquatic(int actor)
    {
        if(actor==0)return PlayerAquatic;if(actor==-1)return MonsterAquatic;if(actor==RescueId)return RescueAquatic;
        foreach(var unit in summons)if(unit.Id==actor&&unit.Alive)return unit.Aquatic;return null;
    }
    public bool IsWalkingOnWaterSurface(int actor){var state=ActorAquatic(actor);return HasWaterWalking(actor)&&state!=null&&!state.Submerged;}
    bool ChangeWaterState(int actor,AquaticState state,bool submerged,Vector2Int cell,int spaces,System.Func<int,bool> spendMovement)
    {
        int count=WaterCells(cell,spaces),area=spaces*spaces;
        if(state==null||state.Submerged==submerged||submerged&&!AquaticState.FullyInWater(count,area))return false;
        bool changed;
        if(HasWaterWalking(actor)){
            if(WaterWalkBonus==null||!WaterWalkBonus(actor))return false;
            changed=state.TrySetSubmerged(submerged,count,area);
        }else changed=state.TrySwitch(submerged,count,area,spendMovement);
        if(changed)SyncWaterPositions();return changed;
    }
    public bool SwitchSummonWaterState(int id,bool submerged)
    {
        if(!Locked)return false;foreach(var unit in summons)if(unit.Id==id&&unit.Alive)
            return ChangeWaterState(id,unit.Aquatic,submerged,new Vector2Int(unit.X,unit.Y),unit.Spaces,unit.SpendMovement);
        return false;
    }
    public bool SwitchRescueWaterState(bool submerged,System.Func<int,bool> spendMovement)
        =>Locked&&RescuePosition.HasValue&&ChangeWaterState(RescueId,RescueAquatic,submerged,RescuePosition.Value,RescueSpaces,spendMovement);
    // A long ordinary move may leave the water and re-enter it before the final
    // position notification. Water walkers who crossed dry ground emerge there.
    void SyncWaterWalkRoute(int actor,Vector2Int from,Vector2Int to,int spaces)
    {
        var state=ActorAquatic(actor);if(!HasWaterWalking(actor)||state==null||!state.Submerged)return;
        if(!AquaticState.FullyInWater(WaterCells(from,spaces),spaces*spaces)){state.TrySetSubmerged(false,0,1);return;}
        foreach(var cell in MovementAnchors(from,to))if(!AquaticState.FullyInWater(WaterCells(cell,spaces),spaces*spaces)){state.TrySetSubmerged(false,0,1);return;}
    }
    // Jump and push/pull call this explicitly. Teleports and form changes also use
    // landing notifications, so the generic observer's bool cannot identify falls.
    void ApplyWaterWalkFall(int actor,Vector2Int from,Vector2Int to,int spaces,bool jumping)
    {
        if(!HasWaterWalking(actor)||!AquaticState.FullyInWater(WaterCells(to,spaces),spaces*spaces))return;
        if(!jumping&&AquaticState.FullyInWater(WaterCells(from,spaces),spaces*spaces))return;
        var state=ActorAquatic(actor);if(state==null)return;
        state.TrySetSubmerged(true,WaterCells(to,spaces),spaces*spaces);SyncWaterPositions();
    }
    public bool SwitchWaterState(bool player,bool submerged)
    {
        if(!Locked||IsContained(player)||player&&Riding)return false;
        var cell=player?Player:Monster;int spaces=player?PlayerSpaces:MonsterSpaces;
        return ChangeWaterState(player?0:-1,player?PlayerAquatic:MonsterAquatic,submerged,cell,spaces,cost=>{
            if(player)return SpendPlayerMovement(cost);
            if(MonsterMovementRemaining<cost)return false;
            SpendMonsterMovement(cost);return true;
        });
    }
    void SyncWaterPositions()
    {
        if(RescuePosition.HasValue)RescueAquatic.ObservePosition(WaterCells(RescuePosition.Value,RescueSpaces),RescueSpaces*RescueSpaces);
        if(IsCarried(true))PlayerAquatic.TrySetSubmerged(RescueAquatic.Submerged,WaterCells(RescuePosition.Value,RescueSpaces),RescueSpaces*RescueSpaces);else PlayerAquatic.ObservePosition(WaterCells(Player,PlayerSpaces),PlayerSpaces*PlayerSpaces);
        if(IsCarried(false))MonsterAquatic.TrySetSubmerged(RescueAquatic.Submerged,WaterCells(RescuePosition.Value,RescueSpaces),RescueSpaces*RescueSpaces);else MonsterAquatic.ObservePosition(WaterCells(Monster,MonsterSpaces),MonsterSpaces*MonsterSpaces);
        foreach(var unit in summons)unit.Aquatic.ObservePosition(WaterCells(new Vector2Int(unit.X,unit.Y),unit.Spaces),unit.Spaces*unit.Spaces);SyncRiderWater();
    }
    public bool IsWater(Vector2Int cell)=>Inside(cell,1)&&water[Selected,cell.x,cell.y];
    public bool SetWater(Vector2Int cell,bool value){if(Locked||!Inside(cell,1))return false;water[Selected,cell.x,cell.y]=value;return true;}
    public bool ClearWater(){if(Locked)return false;for(int y=0;y<Size;y++)for(int x=0;x<Size;x++)water[Selected,x,y]=false;return true;}
    public int WaterCells(Vector2Int anchor,int spaces)
    {
        if(spaces<1||spaces>4||!Inside(anchor,spaces))return -1;
        int count=0;for(int y=anchor.y;y<anchor.y+spaces;y++)for(int x=anchor.x;x<anchor.x+spaces;x++)if(water[Selected,x,y])count++;
        return count;
    }
}
