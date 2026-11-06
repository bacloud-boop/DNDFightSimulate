using System;
public sealed class CombatState
{
    public const int MaxHp = 1000;
    public int PlayerMaxHp { get; private set; } = 10;
    public int Hp { get; private set; } = MaxHp;
    public int PlayerHp { get; private set; } = 10;
    public int Round { get; private set; } = 1;
    public void ApplyDamage(int damage) { if(damage < 0) throw new ArgumentOutOfRangeException(nameof(damage)); Hp = Math.Max(0, Hp - damage); }
    public void ApplyPlayerDamage(int damage) { PlayerHp = Math.Max(0, PlayerHp - Math.Max(0, damage)); }
    public void UpdatePlayerMaxHp(int constitution, int totalLevel) {
        PlayerMaxHp = (6 + (int)Math.Floor((constitution-10)/2.0)) * (totalLevel-1) + 10;
        PlayerHp = Math.Min(PlayerHp, PlayerMaxHp);
    }
    public void EndRound() { Round++; }
    public bool ResetIfDead() { if(Hp>0 && PlayerHp>0) return false; Hp=MaxHp; PlayerHp=PlayerMaxHp; Round=1; return true; }
}
