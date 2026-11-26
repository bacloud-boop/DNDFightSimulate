using UnityEngine;
public sealed class SpiritualWeaponEffect
{
    public Vector2Int Cell {get;private set;}
    public int Slot {get;private set;}
    public int Caster {get;private set;}
    public int Scene {get;private set;}
    public int CastRound {get;private set;}
    public long Expires {get;private set;}=-1;
    public static bool ValidTime(long now)=>now>=0&&now<=long.MaxValue-10;
    public static int Dice(int slot,bool critical=false)=>slot>=2&&slot<=9?(slot-1)*(critical?2:1):0;
    public bool Active(long now)=>now>=0&&now>=Expires-10&&now<Expires;
    public bool Begin(Vector2Int cell,int slot,int caster,int scene,int round,long now)
    {
        if(Dice(slot)==0||scene<0||round<1||!ValidTime(now))return false;
        Cell=cell;Slot=slot;Caster=caster;Scene=scene;CastRound=round;Expires=now+10;return true;
    }
    public bool CanControl(int round,long now,int scene)=>Active(now)&&round>CastRound&&scene==Scene;
    public bool CanMove(Vector2Int to,int size)=>size>0&&to.x>=0&&to.y>=0&&to.x<size&&to.y<size&&BattleMap.FeetBetween(Cell,to)<=20;
    public void Move(Vector2Int to){Cell=to;}
    public void Clear(){Expires=-1;Slot=0;}
}
