# 模块化角色控制器使用说明

## 概述

新的模块化角色控制器将原本的PlayControl脚本重构为基于能力系统的架构。这样设计的好处包括：

1. **模块化设计**：每个能力都是独立的模块，便于开发和维护
2. **兼容旧版本**：保留了原有的公共属性访问方式
3. **灵活控制**：可以通过Inspector面板或脚本动态控制能力的开启/关闭
4. **扩展性强**：容易添加新的能力类型

## 核心组件

### 1. PlayerController（主控制器）
- 位置：`Assets/Scripts/PlayerController.cs`
- 功能：管理角色的基础行为和所有能力模块
- 使用：替换原有的PlayControl脚本

### 2. 四个基础能力模块

#### MovementAbility（移动能力）
- 控制角色的水平移动、奔跑、推箱子
- 参数：walkSpeed、runSpeed、pushSpeed

#### JumpAbility（跳跃能力）
- 控制角色的跳跃行为
- 支持土狼时间和跳跃缓冲
- 参数：jumpPower、coyoteTime、jumpBufferTime

#### IronBlockAbility（铁块能力）
- 增加角色重量和密度
- 减弱跳跃能力
- 参数：massMultiplier、gravityMultiplier、jumpReduction

#### BalloonAbility（气球能力）
- 滑翔能力，类似奥日的羽毛效果
- 长按空格键时减缓下落速度
- 不按空格键时正常下落
- 参数：glideGravityScale、glideFallSpeed

### 3. AbilityManager（能力管理器）
- 管理能力槽系统（默认2个槽位）
- 控制能力的装备和切换
- 提供UI显示支持

## 使用方法

### 1. 基础设置

1. 将`PlayerController`脚本挂载到角色对象上
2. 在Inspector面板中配置各个能力的参数
3. 确保角色有Rigidbody2D和BoxCollider2D组件

### 2. Inspector面板控制

新的PlayerController提供了直观的Inspector界面：

**能力系统控制区域：**
- 每个能力都有独立的折叠面板
- 右侧显示启用状态（✓ 已启用 / ✗ 已禁用）
- 可以直接勾选/取消勾选来启用/禁用能力
- 展开面板可以调整能力的详细参数

**实时预览：**
- 在运行时修改开关会立即生效
- 状态指示器实时显示当前能力状态

### 3. 脚本控制

```csharp
// 获取控制器实例
PlayerController player = PlayerController.Instance;

// 启用/禁用能力
player.EnableAbility<MovementAbility>();
player.DisableAbility<MovementAbility>();
player.ToggleAbility<MovementAbility>();

// 检查能力状态
bool hasMovement = player.HasAbility<MovementAbility>();
bool isEnabled = player.GetAbility<MovementAbility>().isEnabled;

// 获取能力实例并修改参数
MovementAbility movement = player.GetAbility<MovementAbility>();
movement.walkSpeed = 5f;
```

### 4. 能力槽系统

```csharp
// 获取能力管理器
AbilityManager manager = FindObjectOfType<AbilityManager>();

// 装备能力到指定槽位
manager.EquipAbility(AbilityManager.AbilityType.IronBlock, 0);

// 拾取新能力（自动装备到空槽位）
manager.PickupAbility(AbilityManager.AbilityType.Balloon);

// 交换能力槽
manager.SwapAbilities(0, 1);
```

## 快捷键（调试用）

- **F1-F4**：切换对应能力的开启/关闭
- **F5**：显示当前角色和能力状态
- **Q/R**：切换槽位中的能力（调试模式）
- **1/2**：切换对应槽位的能力状态

## 兼容性说明

新控制器保持了与旧版本的兼容性：

```csharp
// 这些属性仍然可以使用
player.jumpPower = 12f;
player.walkSpeed = 6f;
player.isGround;  // 现在是 IsGrounded 的别名
player.moveSpeed; // 委托给MovementAbility
```

## 扩展新能力

要添加新能力，只需：

1. 继承`PlayerAbility`基类
2. 实现必要的方法（UpdateAbility、OnAbilityActivated等）
3. 在PlayerController中添加该能力的实例
4. 在AbilityManager中添加对应的枚举和处理逻辑

## 注意事项

1. **能力冲突**：某些能力可能相互冲突（如铁块和气球），需要在逻辑中处理
2. **性能考虑**：能力的Update方法每帧都会调用，避免在其中进行重计算
3. **动画同步**：能力状态变化时记得更新对应的动画状态
4. **保存系统**：如需保存能力状态，请扩展相应的存档系统

## 示例场景

参考`PlayerControllerExample.cs`脚本了解完整的使用示例。