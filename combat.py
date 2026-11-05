"""跑团数值测试：木桩基础规则。"""

from dataclasses import dataclass


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
    print("输入非负整数进行攻击；输入 end 结束回合并恢复血量；输入 quit 退出。")
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
        try:
            dummy.take_damage(int(command))
        except ValueError:
            print("请输入非负整数、end 或 quit。")


if __name__ == "__main__":
    main()
