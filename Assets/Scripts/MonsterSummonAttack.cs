using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    IEnumerator ResolveMonsterSpiritAttack(SummonedUnit unit,CreatureCover.Body? targetAt=null)
    {
        if(unit==null||!unit.Alive||!combat.MonsterCanAct||!MonsterCanCastAttack)yield break;
        double x=targetAt.HasValue?targetAt.Value.X:unit.X,y=targetAt.HasValue?targetAt.Value.Y:unit.Y;
        bool sees=VisibilityBetween(false,map.Monster.x,map.Monster.y,map.MonsterSpaces,x,y,unit.Spaces)<2;
        bool targetSees=SpiritSeesAt(unit,-1,x,y,map.Monster.x,map.Monster.y,map.MonsterSpaces);
        bool advantage=FaerieAdvantage(unit.Id,sees)||unit.SleepStage==2||unit.Prone&&BattleMap.BodyDistance(map.Monster,map.MonsterSpaces,new Vector2Int(unit.X,unit.Y),unit.Spaces)<=5||unit.Restrained||!targetSees,disadvantage=FearDisadvantage(false)||Entangled(false)|RayAttackDisadvantage(false,MonsterCustomAttackAbility)||unit.Prone&&BattleMap.BodyDistance(map.Monster,map.MonsterSpaces,new Vector2Int(unit.X,unit.Y),unit.Spaces)>5||MonsterArmorAttackDisadvantage||!sees||MonsterUnderwaterDisadvantage()||combat.MonsterPoisoned||combat.MonsterProne||(unit.Dodging&&targetSees&&unit.Speed>unit.Breathing.SpeedPenalty);
        if(!MonsterMeleeAttack){
            if(combat.PlayerHp>0&&!combat.PlayerParalyzed&&map.DistanceFeet<=5&&PlayerSeesMonster)disadvantage=true;
            foreach(var ally in map.Summons)if(ally.Alive&&BattleMap.BodyDistance(map.Monster,map.MonsterSpaces,new Vector2Int(ally.X,ally.Y),ally.Spaces)<=5&&SpiritSees(ally,-1,map.Monster,map.MonsterSpaces))disadvantage=true;
        }
        int first=Random.Range(1,21),second=advantage!=disadvantage?Random.Range(1,21):0,value=second==0?first:advantage?Mathf.Max(first,second):Mathf.Min(first,second);
        long bonus=(MonsterPolymorphed?BeastAttack.Find(polymorph.Form).HitBonus+combat.MonsterRevivalPenalty:MonsterCustomHitBonus)+monsterBreathing.D20Penalty;
        int bless=combat.MonsterBlessed?Random.Range(1,5):0,bane=combat.MonsterBaned?Random.Range(1,5):0;
        bonus+=bless-bane;int cover=targetAt.HasValue?map.CreatureCoverAt(-1,unit.Id,new CreatureCover.Body(map.Monster.x,map.Monster.y,map.MonsterSpaces),targetAt.Value):map.CreatureCoverBonus(-1,unit.Id),armor=unit.Armor+cover;
        BreakOrdinaryInvisibility(false,"进行攻击检定");
        yield return PresentRoll(20,second==0?new[]{first}:new[]{first,second},"木桩 → "+unit.Name+"：采用 "+value+" + "+bonus+" / 护甲 "+armor+(cover>0?"（半掩护+2）":""));
        if(bless>0)yield return PresentRoll(4,new[]{bless},"祝福术 +"+bless);
        if(bane>0)yield return PresentRoll(4,new[]{bane},"灾祸术 -"+bane);
        if(value==1||(value!=20&&(long)value+bonus<armor)){history.Insert(0,"木桩攻击"+unit.Name+"未命中，伤害0。");yield break;}
        var packet=CurrentMonsterDamage();var sides=new List<int>();var rolls=new List<int[]>();string details;
        bool critical=value==20||unit.SleepStage==2&&BattleMap.BodyDistance(map.Monster,map.MonsterSpaces,new Vector2Int(unit.X,unit.Y),unit.Spaces)<=5;
        packet.Roll(out details,critical,(s,v,c)=>{sides.Add(s);rolls.Add(v);});
        for(int i=0;i<sides.Count;i++)yield return PresentRoll(sides[i],rolls[i],details);
        bool[] resistance=new bool[DamageTypes.Names.Length];if(unit.Stoneskin)resistance[0]=resistance[1]=resistance[2]=true;if(unit.Aquatic.Submerged)resistance[DamageTypes.Index("火焰")]=true;
        for(int i=0;i<resistance.Length;i++)resistance[i]|=unit.ResistsType(i);
        bool[] immunity=new bool[DamageTypes.Names.Length];immunity[12]=SilenceUnitImmune(unit,"雷鸣");
        var sized=new RayDamage();sized.Raw=packet.LastRawTypedDamage;if(MonsterPolymorphed||combat.MonsterAttackIsWeapon)yield return ApplySizeWeaponDamage(false,sized.Raw,MonsterBaseWeaponDamageType,critical,sized);
        var reduced=new RayDamage();yield return ReduceRayPacket(false,sized.Raw,sides.Count>0||sizeChange.Applies(false)&&sizeChange.Delta>0&&(MonsterPolymorphed||combat.MonsterAttackIsWeapon),"木桩攻击召唤物",reduced);
        var resolved=DamageTypes.Resolve(reduced.Raw,resistance,immunity);int damage=DamageTypes.Total(resolved);
        int actual=unit.Damage(damage,resolvedTypes:System.Array.ConvertAll(resolved,x=>(long)x),critical:critical);history.Insert(0,"木桩对"+unit.Name+"造成 "+actual+" 伤害（结算值 "+damage+"），"+details);
        RefreshBestialSpirit();
    }
}
