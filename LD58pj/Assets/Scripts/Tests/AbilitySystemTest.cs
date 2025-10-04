using UnityEngine;

/// <summary>
/// 能力系统测试脚本 - 用于验证解耦和状态同步功能
/// </summary>
public class AbilitySystemTest : MonoBehaviour
{
    [Header("测试设置")]
    public bool enableDebugLogs = true;
    
    private PlayerController playerController;
    private AbilityManager abilityManager;
    
    void Start()
    {
        // 延迟初始化以确保其他组件已准备就绪
        Invoke(nameof(InitializeTest), 0.5f);
    }
    
    private void InitializeTest()
    {
        playerController = PlayerController.Instance;
        abilityManager = AbilityManager.Instance;
        
        if (playerController == null || abilityManager == null)
        {
            Debug.LogError("测试失败：找不到PlayerController或AbilityManager实例");
            return;
        }
        
        LogInfo("=== 开始能力系统测试 ===");
        
        // 测试1：字符串标识符解耦测试
        TestAbilityDecoupling();
        
        // 测试2：状态同步测试
        TestStateSynchronization();
        
        // 测试3：动态能力添加测试
        TestDynamicAbilityAddition();
        
        LogInfo("=== 能力系统测试完成 ===");
    }
    
    void Update()
    {
        // 测试快捷键
        if (Input.GetKeyDown(KeyCode.T))
        {
            TestManualSync();
        }
        
        if (Input.GetKeyDown(KeyCode.Y))
        {
            TestNewAbilityCreation();
        }
    }
    
    /// <summary>
    /// 测试能力类型解耦功能
    /// </summary>
    private void TestAbilityDecoupling()
    {
        LogInfo("--- 测试1：能力类型解耦 ---");
        
        // 通过字符串标识符操作能力
        var allAbilities = playerController.GetAllAbilities();
        LogInfo($"已注册的能力数量：{allAbilities.Count}");
        
        foreach (var kvp in allAbilities)
        {
            LogInfo($"能力ID：{kvp.Key}，名称：{kvp.Value.abilityName}，状态：{(kvp.Value.isEnabled ? "启用" : "禁用")}");
        }
        
        // 测试通过字符串启用/禁用能力
        LogInfo("测试通过字符串ID控制能力...");
        playerController.DisableAbilityByTypeId("IronBlock");
        playerController.EnableAbilityByTypeId("Balloon");
        
        LogInfo($"铁块能力状态：{(playerController.IsAbilityEnabledByTypeId("IronBlock") ? "启用" : "禁用")}");
        LogInfo($"气球能力状态：{(playerController.IsAbilityEnabledByTypeId("Balloon") ? "启用" : "禁用")}");
    }
    
    /// <summary>
    /// 测试状态同步功能
    /// </summary>
    private void TestStateSynchronization()
    {
        LogInfo("--- 测试2：状态同步 ---");
        
        // 在PlayerController中改变能力状态
        LogInfo("在PlayerController中禁用跳跃能力...");
        playerController.DisableAbilityByTypeId("Jump");
        
        // 检查AbilityManager是否同步
        bool isJumpEquipped = abilityManager.HasAbilityEquipped("Jump");
        LogInfo($"AbilityManager中跳跃能力装备状态：{(isJumpEquipped ? "已装备" : "未装备")}");
        
        // 在AbilityManager中改变装备状态
        LogInfo("在AbilityManager中装备铁块能力...");
        abilityManager.EquipAbility("IronBlock", 0);
        
        // 检查PlayerController是否同步
        bool isIronBlockEnabled = playerController.IsAbilityEnabledByTypeId("IronBlock");
        LogInfo($"PlayerController中铁块能力状态：{(isIronBlockEnabled ? "启用" : "禁用")}");
    }
    
    /// <summary>
    /// 测试动态能力添加
    /// </summary>
    private void TestDynamicAbilityAddition()
    {
        LogInfo("--- 测试3：动态能力扩展性 ---");
        
        // 模拟添加新能力的过程（不需要修改枚举）
        LogInfo("模拟添加新能力'TestAbility'...");
        
        // 检查系统是否能处理未知的能力ID
        bool hasTestAbility = playerController.HasAbilityByTypeId("TestAbility");
        LogInfo($"系统是否有TestAbility：{hasTestAbility}");
        
        // 尝试启用不存在的能力
        playerController.EnableAbilityByTypeId("TestAbility");
        LogInfo("尝试启用不存在的能力完成（应该安全失败）");
    }
    
    /// <summary>
    /// 手动测试状态强制同步
    /// </summary>
    private void TestManualSync()
    {
        LogInfo("--- 手动状态同步测试 ---");
        abilityManager.SyncAbilityStates();
        LogInfo("强制同步完成");
    }
    
    /// <summary>
    /// 测试新能力创建的便利性
    /// </summary>
    private void TestNewAbilityCreation()
    {
        LogInfo("--- 新能力创建便利性测试 ---");
        LogInfo("要添加新能力，只需要：");
        LogInfo("1. 创建继承PlayerAbility的新类");
        LogInfo("2. 实现AbilityTypeId属性返回唯一字符串");
        LogInfo("3. 在PlayerController中注册该能力");
        LogInfo("4. 在AbilityManager的数据列表中添加配置");
        LogInfo("无需修改任何枚举或现有代码！");
    }
    
    private void LogInfo(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[AbilitySystemTest] {message}");
        }
    }
}