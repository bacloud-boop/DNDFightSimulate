using System;
// A single caster concentration effect; no attack or saving throw bonuses.
public sealed class CheckSpellEffect
{
    readonly int[] skills={-1,-1},abilities={-1,-1};
    long ends=-1;
    static int Index(bool player)=>player?0:1;
    public void Guidance(bool player,int skill,long now){if(skill<0||skill>=18)throw new ArgumentOutOfRangeException(nameof(skill));Clear();skills[Index(player)]=skill;ends=now+10;}
    public void Enhance(bool player,int ability,long now){if(ability<0||ability>5||ability==2)throw new ArgumentOutOfRangeException(nameof(ability));abilities[Index(player)]=ability;ends=now+600;}
    public bool Guided(bool player,int skill,long now)=>skill>=0&&now<ends&&skills[Index(player)]==skill;
    public bool Advantage(bool player,int ability,long now)=>now<ends&&abilities[Index(player)]==ability;
    public void Clear(){skills[0]=skills[1]=abilities[0]=abilities[1]=-1;ends=-1;}
}
