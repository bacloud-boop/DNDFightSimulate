using System;
public sealed class CombatState
{
    public int MaxHp { get; private set; } = 1000;
    public int PlayerMaxHp { get; private set; } = 10;
    public int Hp { get; private set; } = 1000;
    public int PlayerHp { get; private set; } = 10;
    public int Round { get; private set; } = 1;
    public void ApplyDamage(int damage) { if(damage < 0) throw new ArgumentOutOfRangeException(nameof(damage)); Hp = Math.Max(0, Hp - damage); }
    public void ApplyPlayerDamage(int damage) { PlayerHp = Math.Max(0, PlayerHp - Math.Max(0, damage)); }
    public void UpdatePlayerMaxHp(int constitution, int totalLevel) {
        int constitutionModifier = (int)Math.Floor((constitution-10)/2.0);
        int newMaxHp = (6 + constitutionModifier) * (totalLevel-1) + 10 + constitutionModifier;
        if(newMaxHp != PlayerMaxHp) { PlayerMaxHp = newMaxHp; PlayerHp = newMaxHp; }

    }
    public void UpdateMonsterMaxHp(int newMaxHp) {
        if(newMaxHp < 1) throw new ArgumentOutOfRangeException(nameof(newMaxHp));
        if(newMaxHp != MaxHp) { MaxHp = newMaxHp; Hp = newMaxHp; }
    }
    public void EndRound() { Round++; }
    public bool ResetIfDead() { if(Hp>0 && PlayerHp>0) return false; Hp=MaxHp; PlayerHp=PlayerMaxHp; Round=1; return true; }
}
