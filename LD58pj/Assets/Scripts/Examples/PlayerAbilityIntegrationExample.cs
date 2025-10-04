using UnityEngine;

/// <summary>
/// PlayerController与AbilityManager集成演示脚本
/// </summary>
public class PlayerAbilityIntegrationExample : MonoBehaviour
{
    [Header("组件引用")]
    public PlayerController playerController;
    public AbilityManager abilityManager;
    
    void Start()
    {
        if (playerController == null)
            playerController = PlayerController.Instance;
            
        if (abilityManager == null)
            abilityManager = FindObjectOfType<AbilityManager>();
            
        StartCoroutine(DemonstrateIntegration());
    }
    
    void Update()
    {
        HandleTestInput();
    }
    
    private void HandleTestInput()
    {
        // 数字键1-2：切换对应槽位的能力
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            CycleAbilityInSlot(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            CycleAbilityInSlot(1);
        }
        
        // Tab键：显示当前能力状态
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            DisplayCurrentAbilityStatus();
        }
    }
    
    private System.Collections.IEnumerator DemonstrateIntegration()
    {
        yield return new WaitForSeconds(2f);
        
        Debug.Log("=== PlayerController与AbilityManager集成演示 ===");
        
        // 装备铁块能力
        abilityManager.EquipAbility(AbilityManager.AbilityType.IronBlock, 0);
        yield return new WaitForSeconds(1f);
        
        // 装备气球能力
        abilityManager.EquipAbility(AbilityManager.AbilityType.Balloon, 1);
        yield return new WaitForSeconds(1f);
        
        DisplayCurrentAbilityStatus();
        
        Debug.Log("使用1/2键切换槽位能力，Tab键查看状态");
    }
    
    private void CycleAbilityInSlot(int slotIndex)
    {
        if (abilityManager == null) return;
        
        AbilityManager.AbilityType currentAbility = abilityManager.GetAbilityInSlot(slotIndex);
        AbilityManager.AbilityType nextAbility = GetNextAbilityType(currentAbility);
        
        abilityManager.EquipAbility(nextAbility, slotIndex);
        Debug.Log($"槽位{slotIndex}：{currentAbility} → {nextAbility}");
    }
    
    private AbilityManager.AbilityType GetNextAbilityType(AbilityManager.AbilityType current)
    {
        switch (current)
        {
            case AbilityManager.AbilityType.None: return AbilityManager.AbilityType.Movement;
            case AbilityManager.AbilityType.Movement: return AbilityManager.AbilityType.Jump;
            case AbilityManager.AbilityType.Jump: return AbilityManager.AbilityType.IronBlock;
            case AbilityManager.AbilityType.IronBlock: return AbilityManager.AbilityType.Balloon;
            case AbilityManager.AbilityType.Balloon: return AbilityManager.AbilityType.None;
            default: return AbilityManager.AbilityType.Movement;
        }
    }
    
    private void DisplayCurrentAbilityStatus()
    {
        if (playerController == null || abilityManager == null) return;
        
        Debug.Log("=== 当前能力状态 ===");
        
        var equippedAbilities = abilityManager.GetEquippedAbilities();
        for (int i = 0; i < equippedAbilities.Count; i++)
        {
            AbilityManager.AbilityType abilityType = equippedAbilities[i];
            bool isActive = abilityManager.IsAbilityActive(abilityType);
            Debug.Log($"槽位{i}: {abilityType} ({(isActive ? "✓激活" : "✗未激活")})");
        }
    }
    
    // 外部API
    public void EquipAbilityToSlot(string abilityName, int slotIndex)
    {
        if (abilityManager == null) return;
        
        AbilityManager.AbilityType abilityType = ParseAbilityType(abilityName);
        if (abilityType != AbilityManager.AbilityType.None)
        {
            abilityManager.EquipAbility(abilityType, slotIndex);
        }
    }
    
    private AbilityManager.AbilityType ParseAbilityType(string abilityName)
    {
        switch (abilityName.ToLower())
        {
            case "movement": case "移动": return AbilityManager.AbilityType.Movement;
            case "jump": case "跳跃": return AbilityManager.AbilityType.Jump;
            case "ironblock": case "铁块": return AbilityManager.AbilityType.IronBlock;
            case "balloon": case "气球": return AbilityManager.AbilityType.Balloon;
            default: return AbilityManager.AbilityType.None;
        }
    }
    
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 300, 400, 200));
        GUILayout.Label("PlayerController & AbilityManager 集成");
        GUILayout.Label("1/2 - 切换槽位能力");
        GUILayout.Label("Tab - 显示状态");
        
        if (abilityManager != null)
        {
            var equipped = abilityManager.GetEquippedAbilities();
            for (int i = 0; i < equipped.Count; i++)
            {
                string status = abilityManager.IsAbilityActive(equipped[i]) ? "✓" : "✗";
                GUILayout.Label($"槽位{i}: {equipped[i]} {status}");
            }
        }
        GUILayout.EndArea();
    }
}