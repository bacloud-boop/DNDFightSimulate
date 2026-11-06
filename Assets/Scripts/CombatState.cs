using System;
public sealed class CombatState
{
    public const int MaxHp = 1000;
    public int Hp { get; private set; } = MaxHp;
    public int Round { get; private set; } = 1;
    public void ApplyDamage(int damage) { if(damage < 0) throw new ArgumentOutOfRangeException(nameof(damage)); Hp = Math.Max(0, Hp - damage); }
    public void EndRound() { Hp = MaxHp; Round++; }
}
