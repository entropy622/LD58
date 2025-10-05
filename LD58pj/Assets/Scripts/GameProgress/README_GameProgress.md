# 游戏进度系统使用说明

## 概述
实现了完整的游戏进度管理系统，包含敌人生成、水晶生成和分数计数功能。

## 主要组件

### 1. GameProgressManager (游戏进度管理器)
- **功能**: 统一管理游戏流程、敌人生成、水晶生成和分数系统
- **位置**: `GameProgress/GameProgressManager.cs`
- **类型**: MonoSingleton

#### 主要功能：
- ⏰ **限时游戏**: 默认120秒倒计时
- 👾 **敌人生成**: 自动在随机位置生成敌人，保持距离玩家安全距离
- 💎 **水晶生成**: 击败指定数量敌人后升级，生成随机能力水晶
- 📊 **分数系统**: 击败敌人+10分，升级额外奖励分数
- 📈 **等级系统**: 每击败5个敌人升级一次

### 2. Enemy (敌人组件)
- **功能**: 简单的活靶子，被玩家碰撞后消失
- **位置**: `GameProgress/Enemy.cs`
- **特点**: 静态敌人，触发器碰撞检测

#### 主要特性：
- 🎯 **碰撞击败**: 玩家碰撞敌人即可击败
- ✨ **视觉效果**: 死亡时缩小淡出动画
- 🔊 **音效支持**: 可配置死亡音效和特效
- 📡 **事件系统**: 死亡时发送`OnEnemyKilledEvent`事件

## 使用步骤

### 1. 场景设置
1. 在场景中创建空物体，添加`GameProgressManager`组件
2. 配置UI文本组件引用（分数、时间、等级、击败数量）
3. 设置敌人和水晶的预制体

### 2. 预制体准备

#### 敌人预制体：
- 创建基础游戏对象
- 添加`SpriteRenderer`显示敌人外观
- 添加`Enemy`组件（会自动添加必要的Collider2D和Rigidbody2D）
- 设置Tag为"Enemy"

#### 水晶预制体：
- 使用现有的`AbilityCrystal`预制体
- 确保有`AbilityCrystal`组件和碰撞检测

### 3. 参数配置

#### GameProgressManager参数：
```csharp
[Header("游戏设置")]
public float gameTime = 120f;           // 游戏总时间
public int enemiesToUpgrade = 5;        // 升级所需击败敌人数

[Header("敌人生成设置")]
public int maxEnemies = 10;             // 最大敌人数量
public float enemySpawnInterval = 2f;   // 生成间隔
public Vector2 spawnAreaMin/Max;        // 生成区域
public float minDistanceFromPlayer = 3f; // 与玩家最小距离

[Header("UI设置")]
public TextMeshProUGUI scoreText;       // 分数显示
public TextMeshProUGUI timeText;        // 时间显示
public TextMeshProUGUI levelText;       // 等级显示
public TextMeshProUGUI enemiesKilledText; // 击败数显示
```

## 事件系统

### 发送的事件：
- `OnEnemyKilledEvent`: 敌人被击败时发送
- `OnAbilityCollectedEvent`: 收集能力时发送（由AbilityCrystal发送）

### 监听的事件：
- `OnEnemyKilledEvent`: 更新分数和检查升级
- `OnAbilityCollectedEvent`: 从可用能力列表中移除已获取能力

## 游戏流程

1. **游戏开始**: 初始化120秒倒计时，开始生成敌人
2. **击败敌人**: 玩家碰撞敌人获得10分，击败计数+1
3. **升级条件**: 击败5个敌人后升级，等级+1
4. **水晶生成**: 升级时在随机位置生成能力水晶
5. **能力获取**: 玩家碰撞水晶获得随机未获取的能力
6. **游戏结束**: 时间结束，显示最终统计

## 调试功能

### Gizmos显示：
- **GameProgressManager**: 显示敌人生成区域（红色方框）和玩家安全距离（蓝色圆圈）
- **Enemy**: 显示碰撞检测范围（黄色方框）

### 日志输出：
- 敌人生成位置
- 敌人击败信息
- 升级和水晶生成
- 能力收集状态

## 扩展功能

### 可能的扩展：
1. **移动敌人**: 修改Enemy组件添加移动逻辑
2. **敌人类型**: 创建不同类型的敌人预制体
3. **游戏结束UI**: 在UIManager中添加结束界面
4. **难度递增**: 随等级增加敌人生成速度
5. **特殊事件**: 添加限时特殊敌人或奖励

### 接口方法：
- `RestartGame()`: 重新开始游戏
- `GetGameStats()`: 获取当前游戏统计
- `EndGame()`: 手动结束游戏

## 注意事项

1. **玩家标签**: 确保玩家对象有"Player"标签
2. **能力系统**: 需要AbilityManager和相关能力系统正常工作
3. **UI组件**: 确保UI文本组件正确赋值
4. **预制体**: 敌人和水晶预制体必须正确配置
5. **碰撞层**: 确保玩家和敌人在正确的碰撞层上

系统设计为模块化，可以根据具体需求进行调整和扩展。