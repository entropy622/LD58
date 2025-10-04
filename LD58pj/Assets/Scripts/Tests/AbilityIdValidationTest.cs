using UnityEngine;

/// <summary>
/// 能力ID验证测试脚本 - 测试AbilityManager中的ID限制功能
/// </summary>
public class AbilityIdValidationTest : MonoBehaviour
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
        
        LogInfo("=== 能力ID验证测试 ===");
        LogInfo("测试目标：验证AbilityManager只接受_abilityRegistry中的有效能力ID");
        LogInfo("");
        
        // 显示可用的能力ID
        ShowAvailableAbilities();
        
        // 执行各种验证测试
        TestValidAbilityIds();
        TestInvalidAbilityIds();
        TestSafeMethods();
        
        LogInfo("=== 测试完成 ===");
        LogInfo("按键说明：");
        LogInfo("T键 - 显示当前有效能力ID");
        LogInfo("Y键 - 测试无效ID处理");
        LogInfo("U键 - 测试安全方法");
    }
    
    void Update()
    {
        // 测试快捷键
        if (Input.GetKeyDown(KeyCode.T))
        {
            ShowAvailableAbilities();
        }
        
        if (Input.GetKeyDown(KeyCode.Y))
        {
            TestInvalidAbilityIds();
        }
        
        if (Input.GetKeyDown(KeyCode.U))
        {
            TestSafeMethods();
        }
    }
    
    /// <summary>
    /// 显示所有可用的能力ID
    /// </summary>
    private void ShowAvailableAbilities()
    {
        LogInfo("--- 可用能力ID列表 ---");
        
        var validIds = abilityManager.GetValidAbilityIds();
        LogInfo($"总共 {validIds.Count} 个有效能力ID:");
        
        for (int i = 0; i < validIds.Count; i++)
        {
            LogInfo($"  {i + 1}. {validIds[i]}");
        }
        
        if (validIds.Count == 0)
        {
            LogInfo("  没有找到有效的能力ID！请确保PlayerController已初始化。");
        }
    }
    
    /// <summary>
    /// 测试有效的能力ID
    /// </summary>
    private void TestValidAbilityIds()
    {
        LogInfo("--- 有效ID测试 ---");
        
        var validIds = abilityManager.GetValidAbilityIds();
        if (validIds.Count > 0)
        {
            string testId = validIds[0]; // 使用第一个有效ID进行测试
            
            LogInfo($"测试有效ID: {testId}");
            
            // 测试IsValidAbilityId方法
            bool isValid = abilityManager.IsValidAbilityId(testId);
            LogInfo($"IsValidAbilityId('{testId}'): {isValid}");
            
            // 测试激活有效能力
            bool activateResult = abilityManager.SafeActivateAbility(testId);
            LogInfo($"SafeActivateAbility('{testId}'): {activateResult}");
            
            // 测试装备有效能力
            bool equipResult = abilityManager.SafeEquipAbility(testId, 0);
            LogInfo($"SafeEquipAbility('{testId}', 0): {equipResult}");
        }
        else
        {
            LogInfo("没有有效的能力ID可供测试");
        }
    }
    
    /// <summary>
    /// 测试无效的能力ID
    /// </summary>
    private void TestInvalidAbilityIds()
    {
        LogInfo("--- 无效ID测试 ---");
        
        string[] invalidIds = { "InvalidAbility", "NotExists", "FakeSkill", "" };
        
        foreach (string invalidId in invalidIds)
        {
            LogInfo($"测试无效ID: '{invalidId}'");
            
            // 测试IsValidAbilityId方法
            bool isValid = abilityManager.IsValidAbilityId(invalidId);
            LogInfo($"  IsValidAbilityId: {isValid}");
            
            // 测试尝试激活无效能力
            bool activateResult = abilityManager.SafeActivateAbility(invalidId);
            LogInfo($"  SafeActivateAbility: {activateResult}");
            
            // 测试尝试装备无效能力
            bool equipResult = abilityManager.SafeEquipAbility(invalidId, 0);
            LogInfo($"  SafeEquipAbility: {equipResult}");
            
            LogInfo(""); // 空行分隔
        }
    }
    
    /// <summary>
    /// 测试安全方法
    /// </summary>
    private void TestSafeMethods()
    {
        LogInfo("--- 安全方法测试 ---");
        
        var validIds = abilityManager.GetValidAbilityIds();
        
        if (validIds.Count >= 2)
        {
            string validId = validIds[0];
            string anotherValidId = validIds[1];
            
            LogInfo("测试安全激活方法：");
            LogInfo($"  SafeActivateAbility('{validId}'): {abilityManager.SafeActivateAbility(validId)}");
            LogInfo($"  SafeActivateAbility('InvalidId'): {abilityManager.SafeActivateAbility("InvalidId")}");
            
            LogInfo("测试安全装备方法：");
            LogInfo($"  SafeEquipAbility('{anotherValidId}', 1): {abilityManager.SafeEquipAbility(anotherValidId, 1)}");
            LogInfo($"  SafeEquipAbility('FakeAbility', 1): {abilityManager.SafeEquipAbility("FakeAbility", 1)}");
            
            // 显示当前状态
            LogInfo("当前激活的能力：");
            var activeAbilities = abilityManager.GetActiveAbilities();
            foreach (string ability in activeAbilities)
            {
                LogInfo($"  - {ability}");
            }
            
            LogInfo("当前装备的能力：");
            var equippedAbilities = abilityManager.GetEquippedAbilities();
            for (int i = 0; i < equippedAbilities.Count; i++)
            {
                string ability = equippedAbilities[i];
                LogInfo($"  槽位{i}: {(string.IsNullOrEmpty(ability) ? "空" : ability)}");
            }
        }
        else
        {
            LogInfo("没有足够的有效能力ID进行安全方法测试");
        }
    }
    
    /// <summary>
    /// 测试Inspector面板配置验证
    /// </summary>
    public void TestInspectorValidation()
    {
        LogInfo("--- Inspector配置验证测试 ---");
        LogInfo("请在Inspector面板中尝试：");
        LogInfo("1. 在装备能力列表中设置无效的能力ID");
        LogInfo("2. 在激活能力列表中添加不存在的能力");
        LogInfo("3. 观察Editor是否正确显示警告和验证信息");
        LogInfo("4. 使用'清除无效能力ID'按钮验证清理功能");
    }
    
    private void LogInfo(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[AbilityIdValidationTest] {message}");
        }
    }
}