# 火柴人联盟

2D 火柴人横版动作游戏（Unity 6），包含单机章节闯关与基于 Photon Fusion 的实时 PvP 联机对战。

## 技术栈

- **Unity 6000.3.7f1**（Unity 6），URP 渲染管线
- **C#** 核心玩法逻辑
- **Photon Fusion** — PvP 联机框架
- **Addressables** — 资源管理
- **Aseprite** — 角色动画源文件

## 游戏内容

- **单机闯关（PvE）**：章节制关卡（Chapter 1~3），包含敌人配置、掉落与奖励系统
- **角色系统**：策略模式（`IRoleStrategy`）驱动角色行为，当前角色为猴子（Warrior）；动画由 Animator 状态机 + 碰撞盒配置驱动
- **PvP 联机对战**：基于 Fusion 的房间对战，包含大厅、回合管理、对战管理、HUD 与结算界面
- **编辑器工具**：Aseprite 动画重绑定工具、运行期调参保存工具

## 目录结构

```
Assets/
├── Characters/            # 角色动画源文件（Aseprite）、预制体
├── Scenes/                # Main（主菜单）、Stage（闯关）、PVPBattleScene（联机对战）
├── Scripts/
│   └── GamePlay/
│       ├── Role/          # 角色数据（ScriptableObject）与角色策略
│       ├── PvP/           # Fusion 联机对战逻辑
│       ├── UI/            # 主菜单、暂停、结算、选人界面
│       └── GameModel/     # 关卡配置、动画参数等
├── Photon/                # Photon Fusion SDK（不含 AppId 配置）
└── UI/                    # UI 素材
```

## 运行方法

1. 使用 **Unity 6000.3.7f1** 打开项目，等待包解析完成（Photon Fusion 包需重启 Unity 完成导入）
2. 打开 `Assets/Scenes/Main.unity` 运行单机内容
3. **PvP 联机需要配置自己的 Photon AppId**（代码已随仓库公开，但 AppId 是开发者私有的）：
   - 前往 [Photon 控制台](https://dashboard.photonengine.com/) 创建一个 Fusion 应用
   - 在 Unity 菜单 **Fusion → Fusion Hub** 中填入你的 AppId
   - 注意：`Assets/Photon/Fusion/Resources/PhotonAppSettings.asset` 已在 `.gitignore` 中排除，请勿提交真实 AppId
4. 联机对战：一端创建房间，另一端加入房间（支持同机多开测试）

## 说明

- 仓库未包含任何 .md 格式的个人开发笔记（会话记录、方案研究等），这些文件仅存在于本地
