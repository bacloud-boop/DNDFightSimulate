# DNDFightSimulate

项目已迁移至 Unity，后续统一使用 Unity / C# 开发。

## 打开与运行

使用 **Unity 2022.3.62f3c1** 打开仓库根目录，打开 `Assets/Scenes/Main.unity`，点击 Play。
通过 File > Build Settings 选择 Windows 平台并 Build And Run 可运行独立程序。

## 功能

- 主菜单：进入战斗、设置、退出游戏。
- 木桩 1000 血量，仅被动承伤。点击攻击展示 D12 滚动过程，最终点数为本次伤害。
- 投掷期间禁用攻击、结束回合和返回，避免重复结算。
- 结束回合恢复 1000 血量；返回菜单保留当前战斗。
- 合成背景音乐和投骰音效，独立音量滑块、音效试听，PlayerPrefs 保存音量。
- 退出游戏关闭独立程序；编辑器内停止 Play。

## 结构与验证

- `Assets/Scripts/CombatState.cs`：血量和回合规则。
- `Assets/Scripts/TrainingGame.cs`：界面、投骰动画、声音与设置。
- `Assets/Editor/ProjectSetup.cs`：创建主场景，并验证伤害、血量下限、回合重置和骰值范围。

批处理验证入口：`-batchmode -nographics -quit -projectPath <项目路径> -executeMethod ProjectSetup.CreateAndVerify -logFile <日志路径>`。
该命令会重新生成主场景，仅适用于初始场景搭建。旧 Python 和网页版本保留在 Git 历史中。
