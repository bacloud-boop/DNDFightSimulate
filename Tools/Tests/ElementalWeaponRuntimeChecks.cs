using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UnityEngine {
    public static class Random {public static int Range(int min,int max)=>max-1;}
    public static class GUILayout {public static int SelectionGrid(int value,string[] labels,int columns,object style)=>value;public static bool Button(string text,object style)=>false;public static void Label(string text,object style){}}
}
public sealed class Weapon {public string Name;public string InventoryKey=>Name.StartsWith("长剑")?"长剑":Name;public Weapon(string name){Name=name;}}
public sealed partial class TrainingGame
{
    sealed class Profile {public int SizeIndex=2;}
    sealed class Ground {public bool Dropped;}
    sealed class EquipmentPose {public string Location="场内";public bool Touch=true;}
    readonly EquipmentObjectCatalog equipmentObjects=new EquipmentObjectCatalog();readonly TimedSpellBuffs timedBuffs=new TimedSpellBuffs();readonly CombatState combat=new CombatState();readonly Profile monsterProfile=new Profile();readonly Ground monsterGroundWeapon=new Ground();
    readonly List<string> history=new List<string>();readonly List<int> counts=new List<int>();readonly Dictionary<int,EquipmentPose> poses=new Dictionary<int,EquipmentPose>();
    object button=new object(),small=new object();long worldRounds;string concentrationSpell="";long concentrationExpiresWorld;int concentrationExpiresRound;int NaturalPlayerSize=2;bool MonsterPolymorphed;
    void SyncEquipmentObjects(){}bool EquipmentObjectPose(EquipmentObjectRecord item,out EquipmentPose pose){pose=null;return item!=null&&poses.TryGetValue(item.Id,out pose);}
    bool EquipmentInTouch(EquipmentPose pose)=>pose.Touch;
    IEnumerator PresentRoll(int sides,int[] rolls,string text){C(sides==4,"energy dice must be d4");counts.Add(rolls.Length);yield break;}
    static void C(bool ok,string text){if(!ok)throw new Exception(text);}
    static void Run(IEnumerator r){while(r.MoveNext())if(r.Current is IEnumerator)Run((IEnumerator)r.Current);}
    EquipmentObjectRecord Add(string name="短棒"){var r=equipmentObjects.Weapon(0,name,name,2,false);poses[r.Id]=new EquipmentPose();return r;}
    long[] Hit(int id,bool critical=false,int baseType=0,long baseDamage=5,bool includeMagic=true){var raw=new long[13];raw[baseType]=baseDamage;Run(AddEnchantedWeaponDamage(id,baseType,critical,raw,includeMagic));return raw;}
    public static void CheckElemental()
    {
        foreach(int slot in new[]{3,4,5,6,7,8,9})foreach(int type in new[]{3,4,5,7,12}){
            var g=new TrainingGame();var w=g.Add();g.CastElementalWeapon(w.Id,slot,type);int expected=slot>=7?3:slot>=5?2:1;
            C(g.concentrationSpell=="元素武器"&&g.concentrationExpiresWorld==600&&g.concentrationExpiresRound==int.MaxValue,"one-hour concentration");
            C(g.EnchantedWeaponAttack(w.Id)==expected&&g.WeaponObjectMagical(w.Id),"slot-scaled attack bonus and magical identity");
            var raw=g.Hit(w.Id);C(raw[type]==expected*4&&raw[0]==5&&g.counts[0]==expected,"energy dice do not add fixed damage");
            raw=g.Hit(w.Id,true);C(raw[type]==expected*8&&raw[0]==5,"critical doubles energy dice only");
            C(g.ElementalWeaponError(w.Id)!="","already magical weapon rejected before another cast");
            var other=g.Add("匕首");C(g.Hit(other.Id)[type]==0&&g.EnchantedWeaponAttack(other.Id)==0,"switching weapons never transfers enhancement");
            w.Follower=9;w.Place=EquipmentObjectPlace.Carried;C(g.Hit(w.Id)[type]==expected*4,"handoff to undead follower preserves physical identity and enhancement");
            w.Place=EquipmentObjectPlace.Remains;C(g.elementalWeapon.AttackBonus(w.Id,599)==expected,"drop or corpse storage does not end enchantment");
            g.worldRounds=600;C(g.Hit(w.Id)[type]==0&&!g.WeaponObjectMagical(w.Id),"exact expiry removes all benefits");
            g.elementalWeapon.Clear();C(g.EnchantedWeaponAttack(w.Id)==0,"concentration cleanup clears effect");
        }
        var a=new TrainingGame();var sword=a.Add("长剑");a.timedBuffs.MagicWeapon("长剑（双手）",3,0,sword.Id);
        C(a.ElementalWeaponError(sword.Id)!=""&&a.EnchantedWeaponAttack(sword.Id)==2,"Magic Weapon excludes Elemental Weapon on same object");
        C(a.PlayerWeaponObject(new Weapon("长剑（单手）"))==a.PlayerWeaponObject(new Weapon("长剑（双手）")),"both longsword grips share object identity");
        var magicHit=a.Hit(sword.Id,true);C(magicHit[0]==7&&a.counts.Count==0,"Magic Weapon fixed damage not doubled by critical");
        C(a.Hit(sword.Id,false,0,-5)[0]==-3&&a.combat.ResolveTypedPacket(false,a.Hit(sword.Id,false,0,-5))[0]==0,"fixed enhancement combines before typed zero floor");
        var fresh=a.Add("匕首");a.timedBuffs.MagicWeapon("匕首",2,0,fresh.Id);C(a.EnchantedWeaponAttack(sword.Id)==0&&a.EnchantedWeaponAttack(fresh.Id)==1,"recasting Magic Weapon removes old object benefit");
        a=new TrainingGame();var weapon=a.Add();a.poses[weapon.Id].Touch=false;C(a.ElementalWeaponError(weapon.Id)!="","unreachable, underwater or melded items rejected through touch gate");a.CastElementalWeapon(weapon.Id,3,5);C(a.elementalWeapon.ObjectId==0,"runtime revalidates target before applying");
        a.poses[weapon.Id].Touch=true;a.CastElementalWeapon(weapon.Id,7,5);var mixed=a.Hit(weapon.Id,false,5,5);a.combat.DamageResistance=(p,t)=>t==5;C(a.combat.ResolveTypedPacket(false,mixed)[5]==8,"same energy type merges before resistance rounding");
        a=new TrainingGame();a.combat.MonsterWeaponIsHeld=true;a.combat.MonsterAttackIsWeapon=true;int monsterItem=a.MonsterWeaponObject;
        C(monsterItem>0&&a.elementalWeapon.Begin(monsterItem,5,12,0)&&a.EnchantedWeaponAttack(a.MonsterWeaponObject)==2,"configured held monster weapon receives attack enhancement");
        a.MonsterPolymorphed=true;C(a.MonsterWeaponObject==0,"monster beast attacks do not use melded weapon");a.MonsterPolymorphed=false;a.monsterGroundWeapon.Dropped=true;C(a.MonsterWeaponObject==0,"dropped monster weapon not used by natural fallback attack");a.monsterGroundWeapon.Dropped=false;a.combat.MonsterAttackIsWeapon=false;C(a.MonsterWeaponObject==0,"custom spell attack excluded");a.combat.MonsterAttackIsWeapon=true;C(a.MonsterWeaponObject==monsterItem,"recovering actual weapon restores same enhancement");
        foreach(int bad in new[]{0,6,8,9,10,11})C(!new ElementalWeaponEffect().Begin(1,3,bad,0),"disallowed energy type rejected");
        C(!new ElementalWeaponEffect().Begin(0,3,5,0)&&!new ElementalWeaponEffect().Begin(1,2,5,0)&&!new ElementalWeaponEffect().Begin(1,10,5,0)&&!new ElementalWeaponEffect().Begin(1,3,5,long.MaxValue-599),"bad object, slot and overflowing duration rejected");
        Console.WriteLine("PASS: production Elemental Weapon object targeting, all energy/slot/critical dice, one-hour concentration, transfers/storage, magical exclusions, touch revalidation and typed mitigation");
    }
}
class ElementalWeaponChecksMain {static void Main()=>TrainingGame.CheckElemental();}
