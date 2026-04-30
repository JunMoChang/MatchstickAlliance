# CLAUDE.md

## 用户规则

1. **结束会话** — 用户说"结束了"时，整理本次会话的重要信息（时间、主要工作、用户提出的新规则），追加到本文件末尾的「会话记录」中。
2. **提交代码** — 用户说"帮我提交"时执行 git commit，message 用中文描述改动目的而非逐条列举。

## 会话记录

### 2026-05-01

- 重构碰撞检测系统：InstantHit 兼容 AnimationEvent、持续伤害间隔、伤害数据从技能配置读取
- 数据流模式：ScriptableObject(配置) → RoleSaveData(持久化) → RoleRuntimeData(运行时)
- 确立规则：数据完整性在入口保证，不在内部加 null 保护；FirstLoadSaveData 仅用于首次解锁
