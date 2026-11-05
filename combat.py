"""跑团数值测试：木桩基础规则。"""

from dataclasses import dataclass
import random
import time


def roll_d12() -> int:
    """展示投掷过程，最后一次随机取值是本次唯一的伤害结果。"""
    print("开始投掷十二面骰（1d12）……", flush=True)
    for _ in range(6):
        print(f"\r骰子滚动中：[ {random.randint(1, 12):2d} ]", end="", flush=True)
        time.sleep(0.15)
    result = random.randint(1, 12)
    print(f"\r十二面骰结果：[ {result:2d} ]    ")
    return result


def attack(dummy: "TrainingDummy") -> int:
    damage = roll_d12()
    dummy.take_damage(damage)
    return damage


@dataclass
class TrainingDummy:
    max_hp: int = 1000
    hp: int = 1000

    def take_damage(self, damage: int) -> int:
        if isinstance(damage, bool) or not isinstance(damage, int) or damage < 0:
            raise ValueError("伤害必须是非负整数")
        self.hp = max(0, self.hp - damage)
        print(f"本次攻击伤害：{damage}；木桩血量：{self.hp}/{self.max_hp}")
        return damage

    def end_round(self) -> None:
        self.hp = self.max_hp
        print(f"回合结束，木桩血量重置为 {self.hp}/{self.max_hp}")


def main() -> None:
    dummy = TrainingDummy()
    round_number = 1
    print("木桩已就绪：1000/1000 血量，不会主动行动。")
    print("选择操作：1 / attack / 攻击；end 结束回合；quit 退出。")
    while True:
        try:
            command = input(f"第 {round_number} 回合 > ").strip().lower()
        except (EOFError, KeyboardInterrupt):
            print("\n测试结束。")
            break
        if command == "quit":
            break
        if command == "end":
            dummy.end_round()
            round_number += 1
            continue
        if command in ("1", "attack", "攻击"):
            attack(dummy)
        else:
            print("请选择 1 / attack / 攻击、end 或 quit。")


if __name__ == "__main__":
    main()
