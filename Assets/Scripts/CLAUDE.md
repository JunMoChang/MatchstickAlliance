# CLAUDE.md

## 用户规则

1. **结束会话** — 用户说"结束了"时，整理本次会话的重要信息（时间、主要工作、用户提出的新规则），追加到本文件末尾的「会话记录」中。
2. **提交代码** — 用户说"帮我提交"时执行 git commit，message 用中文描述改动目的而非逐条列举。
3. **回答逻辑**— 回答不能谄媚，阿谀奉承用户；客观，严谨，有逻辑的回答。

## 会话记录

### 2026-05-01

- 重构碰撞检测系统：InstantHit 兼容 AnimationEvent、持续伤害间隔、伤害数据统一从技能配置读取
- BoxColliderReporter 传递 BoxColliderName 以支持多碰撞盒区分
- 修复数据初始化：damageIntervals 字段缺失导致 RuntimeData.Init 崩溃，在 FirstLoadSaveData/RefreshSkillsSaveData 补齐
- PlayerController.InitSelectedRoles 未解锁角色直接调用 FirstLoadSaveData
- 添加 .gitattributes 统一换行符管理
- 用户新增规则：CLAUDE.md 记录用户对我的要求而非代码规范；提交 message 用中文描述目的

### 2026-05-02

- 实现 BattleBackgroundController 视差滚动：以 SpawnPoint 为原点，玩家位移映射为背景偏移量（worldToCanvasUnit = refHeight / 2*orthoSize）
- 两张背景图定义场景总宽度，Mountain 子精灵按名称前缀（Far/Middle/Near）动态分组，每组不同视差系数
- 修复滚动范围计算：使用对称公式 `maxScrollOffset = -canvasWidth*0.5 - minX; minScrollOffset = canvasWidth*0.5 - maxX`，以初始位置 0 为中心支持左右双向视差
- 修复使用 CanvasScaler.referenceResolution 替代 Canvas rect 避免 Start() 时布局未完成导致值为 0

### 2026-05-05

- 实现敌人 AI 行为系统：EnemyContext 从简单数据容器重写为五状态状态机（Idle/Chase/Attack/HitReaction/Dead）
- EnemyData 扩展 AI 参数：attackRange、detectionRange、attackCooldown、moveSpeed
- AnimationParameters 新增敌人动画参数 Attack/Hit/Dead，Speed 复用给追击移动
- RoleContext 实现 IDamageable 使玩家可被敌人伤害
- EnemyManager 将 player Transform 和 EnemyData 传递给生成的敌人
- 攻击流程：attackRange 内触发攻击动画 → 动画 normalizedTime 0.5 命中帧造成伤害 → 动画结束后进入冷却 Idle
- 追击流程：detectionRange 内朝玩家 MovePosition 移动 → 进入 attackRange 后转攻击 → 超出 detectionRange 回 Idle
- 受击流程：TakeDamage 触发 HitReaction（打断当前动作）→ 受击动画结束回 Idle
- 死亡流程：HP<=0 触发 Dead（禁用所有 Collider 和 Rigidbody2D）→ 死亡动画结束后 Destroy

### 2026-05-06

- 实现角色穿过敌人：在 RoleContext.Start() 中设置 CapsuleCollider2D.excludeLayers = Enemy，使玩家身体碰撞体排除敌人层
- 攻击检测不受影响：HitBox 子物体触发器独立配置，InstantHit 使用显式 LayerMask 的 Overlap 查询，均不依赖身体碰撞体的层交互

### 2026-05-07

- 实现 MainMenuParallax 脚本：鼠标位置驱动视差，归一化到 [-1,1] 后乘以 maxOffset 和 parallaxFactor 计算各图层偏移
- 修复 Input.mousePosition 报错：项目已切换到新版 Input System，改用 `Mouse.current.position.ReadValue()`

### 2026-05-07

- 开始主菜单 UIToolkit → UGUI 转换：在 Main_UGUI 场景中实现，保留原有 UIToolkit 版本不动
- 架构：1 个主控制器 MainMenuUGUIController + 4 个子视图（TopBarView/NavBarView/LevelPopupView/RoleSelectPopupView），各司其职
- 点击事件统一使用 `Button.onClick.AddListener` + OnEnable/OnDisable 注销，与项目现有 PauseGame/EndedGame 模式一致
- 弹窗背景关闭方案：全屏透明 Button 作背景 + 内容面板 Image raycastTarget 自然阻挡，无需额外 StopPropagation
- 模式入口：场景中固定放置动画 Image，挂 Button 组件后拖入 modeButtons[] 数组，不需要动态卡片轮播（CardCarouselView 创建后因不适用已删除）
- 核心流程已打通：模式点击 → 关卡弹窗（章节切换+圆点指示器）→ 角色选择（最多2人+图片切换）→ 进入战斗
- 装备弹窗（EquipmentPopupView）待实现，为最复杂的弹窗

### 2026-05-08

- 重构 RoleSelectPopupView：从编辑器预配置 HeroCard 数组改为动态生成（prefab + RoleRegistry 数据），仅生成一次不销毁，后续 Show 只重置选中状态
- 实现卡片生成：根据 RoleEntry.template.unSelectedIcon/selectedIcon 设置图标，defaultStar 数量动态生成星星，cardName 从 roleName 设置
- 选中判断改用 sprite 对比（`selectedIcon != cardImages[i].sprite`）替代 HashSet，初次实现 if/else 分支逻辑颠倒已修复
- 排查模式入口按钮无响应：SpriteRenderer 无法配合 Button 组件（Button 依赖 Graphic 做射线检测），方案：SpriteRenderer 上叠加透明 Image，或改用 Collider2D + 输入检测
- SpriteRenderer 层级问题：弹窗打开时因调高 SpriteRenderer Sorting Layer 导致穿透显示，解决：弹窗全屏背景加不透明/半透明色遮盖
- 确认项目已切换新版 Input System（Mouse.current），EventSystem 需配 InputSystemUIInputModule

### 2026-05-11

- 重构物品系统：ItemType 改为 Weapon/Armor/Accessory/Material/Rune/Gold/Diamond，ItemRarity 改为 White/Green/Blue/Purple/Orange（低到高）
- 金币和钻石定义为货币类型，无稀有度，不在背包存储，由 TopBarView 显示
- PlayerData 实例化 GameProps（coins/diamonds），新增 equippedItemNames 和 firstClearedLevelIds 持久化字段
- PlayerDataManager 改为 Singleton，新增 AddGold/AddDiamond/SpendGold/SpendDiamond/IsFirstClear 方法，OnCurrencyChanged 事件驱动 UI 刷新
- TopBarView 参数改为 int 类型，OnEnable 自动绑定 PlayerDataManager 监听货币变化
- InventoryModel 新增 EquipItem/UnequipItem 方法，GetEquippedItems/GetUnequippedEquipment 查询
- ItemDataModel 新增 IsEquipped/EnhancementLevel 属性
- InventoryController 分类按钮适配 5 种背包类型，新增 EquipSelectedItem/UnequipSelectedItem/ShowBackpack/HideBackpack
- 新建 EquipmentPopupView：装备面板（未装备武器/防具/饰品，按稀有度从高到低）+ 强化面板（仅已装备），双标签切换
- RewardSystem 替换空壳：5 类掉落池随机掉落、每关固定金币、首次通关额外 10 钻石
- MainMenuUGUIController 移除硬编码货币，导航栏接入装备/背包/英雄面板
- 抽奖（gacha）功能后续再做
