using UnityEngine;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using QFramework;

/// <summary>
/// 能力管理器 - 统一管理所有能力状态的中心
/// 所有能力相关的状态都在这里管理
/// </summary>
public class AbilityManager : MonoSingleton<AbilityManager>
{
    [Header("能力槽设置")]
    public int maxAbilitySlots = 2;
    public List<string> equippedAbilities = new List<string>(); // 装备的能力列表
    
    [Header("能力状态管理")]
    public List<string> activeAbilities = new List<string>(); // 当前激活的能力列表
    
    private Dictionary<string, PlayerAbility> _abilityRegistry = new Dictionary<string, PlayerAbility>();
    
    [Header("UI引用")]
    public Transform abilitySlotParent;
    public GameObject abilitySlotPrefab;
    
    // 玩家控制器引用
    private PlayerController registeredPlayerController;
    private List<Image> abilitySlotImages = new List<Image>();
    
    // Inspector变化检测
    private List<string> lastEquippedAbilities = new List<string>();
    private List<string> lastActiveAbilities = new List<string>();
    private bool hasInitializedInspectorCheck = false;

    [Header("全部能力")]
    public List<PlayerAbility> playerAbilities = new List<PlayerAbility>();
        
        
    /// <summary>
    /// 注册能力到管理字典
    /// </summary>
    private void RegisterAbility(PlayerAbility ability)
    {
        if (ability != null && !string.IsNullOrEmpty(ability.AbilityTypeId))
        {
            _abilityRegistry[ability.AbilityTypeId] = ability;
        }
    }
    
    void Start()
    {
        InitializeAbilitySlots();
        
        // 等待PlayerController初始化后再设置默认能力
        StartCoroutine(SetupDefaultAbilitiesCoroutine());
    }
    
    private System.Collections.IEnumerator SetupDefaultAbilitiesCoroutine()
    {
        // 等待一帧，确保PlayerController已初始化
        yield return null;
        
        // 如果没有注册PlayerController，尝试查找
        if (registeredPlayerController == null)
        {
            registeredPlayerController = PlayerController.Instance;
        }
        
        // 设置默认能力状态
        SetupDefaultAbilities();
    }
    
    void Update()
    {
        HandleDebugInput();
        
        // 检查Inspector面板变化并同步
        CheckAndSyncInspectorChanges();
    }
    
    private void SetupDefaultAbilities()
    {
        // 设置默认装备
        if (equippedAbilities.Count == 0 || equippedAbilities.All(a => string.IsNullOrEmpty(a)))
        {
            EquipAbility("Movement", 0);
            EquipAbility("Jump", 1);
        }
        
        // 确保装备的能力都是激活的
        SyncEquippedToActive();
    }
    
    private void InitializeAbilitySlots()
    {
        if (abilitySlotParent == null) return;
        
        // 清空现有槽位
        foreach (Transform child in abilitySlotParent)
        {
            Destroy(child.gameObject);
        }
        abilitySlotImages.Clear();
        
        // 创建新的能力槽
        for (int i = 0; i < maxAbilitySlots; i++)
        {
            GameObject slot = Instantiate(abilitySlotPrefab, abilitySlotParent);
            Image slotImage = slot.GetComponent<Image>();
            abilitySlotImages.Add(slotImage);
            
            // 确保装备列表有足够的位置
            if (equippedAbilities.Count <= i)
            {
                equippedAbilities.Add(""); // 空字符串表示无能力
            }
        }
        
        UpdateUI();
    }
    
    /// <summary>
    /// 装备能力到指定槽位
    /// </summary>
    public bool EquipAbility(string abilityTypeId, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= maxAbilitySlots) return false;
        
        // 验证能力ID有效性（空字符串表示卸载，允许通过）
        if (!string.IsNullOrEmpty(abilityTypeId) && !IsValidAbilityId(abilityTypeId))
        {
            Debug.LogWarning($"[AbilityManager] 尝试装备无效的能力ID: {abilityTypeId}");
            return false;
        }
        
        // 取消装备旧能力
        if (!string.IsNullOrEmpty(equippedAbilities[slotIndex]))
        {
            UnequipAbility(slotIndex);
        }
        
        // 装备新能力
        equippedAbilities[slotIndex] = abilityTypeId;
        
        // 激活装备的能力
        if (!string.IsNullOrEmpty(abilityTypeId))
        {
            ActivateAbility(abilityTypeId);
        }
        
        UpdateUI();
        Debug.Log($"[AbilityManager] 装备能力 {abilityTypeId} 到槽位 {slotIndex}");
        return true;
    }
    
    /// <summary>
    /// 从指定槽位卸载能力
    /// </summary>
    public void UnequipAbility(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= maxAbilitySlots) return;
        
        string abilityTypeId = equippedAbilities[slotIndex];
        if (!string.IsNullOrEmpty(abilityTypeId))
        {
            equippedAbilities[slotIndex] = "";
            
            // 如果能力没有在其他槽位装备，则去激活
            if (!IsAbilityEquipped(abilityTypeId))
            {
                DeactivateAbility(abilityTypeId);
            }
            
            Debug.Log($"[AbilityManager] 卸载能力 {abilityTypeId} 从槽位 {slotIndex}");
        }
        
        UpdateUI();
    }
    
    /// <summary>
    /// 激活指定能力
    /// </summary>
    public void ActivateAbility(string abilityTypeId)
    {
        if (string.IsNullOrEmpty(abilityTypeId)) return;
        
        // 验证能力ID有效性
        if (!IsValidAbilityId(abilityTypeId))
        {
            Debug.LogWarning($"[AbilityManager] 尝试激活无效的能力ID: {abilityTypeId}");
            return;
        }
        
        if (!activeAbilities.Contains(abilityTypeId))
        {
            activeAbilities.Add(abilityTypeId);
            
            // 调用能力的激活回调
            if (registeredPlayerController != null)
            {
                var ability = registeredPlayerController.GetAbilityByTypeId(abilityTypeId);
                ability?.OnAbilityActivated();
            }
            
            Debug.Log($"[AbilityManager] 激活能力: {abilityTypeId}");
        }
    }
    
    /// <summary>
    /// 去激活指定能力
    /// </summary>
    public void DeactivateAbility(string abilityTypeId)
    {
        if (string.IsNullOrEmpty(abilityTypeId)) return;
        
        if (activeAbilities.Remove(abilityTypeId))
        {
            // 调用能力的去激活回调
            if (registeredPlayerController != null)
            {
                var ability = registeredPlayerController.GetAbilityByTypeId(abilityTypeId);
                ability?.OnAbilityDeactivated();
            }
            
            Debug.Log($"[AbilityManager] 去激活能力: {abilityTypeId}");
        }
    }
    
    /// <summary>
    /// 切换能力激活状态
    /// </summary>
    public void ToggleAbility(string abilityTypeId)
    {
        if (IsAbilityActive(abilityTypeId))
        {
            DeactivateAbility(abilityTypeId);
        }
        else
        {
            ActivateAbility(abilityTypeId);
        }
    }
    
    /// <summary>
    /// 检查能力是否激活
    /// </summary>
    public bool IsAbilityActive(string abilityTypeId)
    {
        return activeAbilities.Contains(abilityTypeId);
    }
    
    /// <summary>
    /// 检查能力是否装备
    /// </summary>
    public bool IsAbilityEquipped(string abilityTypeId)
    {
        return equippedAbilities.Contains(abilityTypeId);
    }
    
    /// <summary>
    /// 获取当前激活的能力列表
    /// </summary>
    public List<string> GetActiveAbilities()
    {
        return new List<string>(activeAbilities);
    }
    
    /// <summary>
    /// 获取当前装备的能力列表
    /// </summary>
    public List<string> GetEquippedAbilities()
    {
        return new List<string>(equippedAbilities);
    }
    
    /// <summary>
    /// 获取指定槽位的能力
    /// </summary>
    public string GetAbilityInSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < equippedAbilities.Count)
            return equippedAbilities[slotIndex];
        return "";
    }
    
    /// <summary>
    /// 获取能力的槽位索引
    /// </summary>
    public int GetAbilitySlotIndex(string abilityTypeId)
    {
        return equippedAbilities.IndexOf(abilityTypeId);
    }
    
    /// <summary>
    /// 同步装备状态到激活状态
    /// </summary>
    private void SyncEquippedToActive()
    {
        foreach (string abilityTypeId in equippedAbilities)
        {
            if (!string.IsNullOrEmpty(abilityTypeId))
            {
                ActivateAbility(abilityTypeId);
            }
        }
    }
    
    /// <summary>
    /// 检查Inspector面板变化并同步状态
    /// </summary>
    private void CheckAndSyncInspectorChanges()
    {
        // 初始化检查列表
        if (!hasInitializedInspectorCheck)
        {
            lastEquippedAbilities = new List<string>(equippedAbilities);
            lastActiveAbilities = new List<string>(activeAbilities);
            hasInitializedInspectorCheck = true;
            return;
        }
        
        // 检查装备列表变化
        bool equippedChanged = HasListChanged(lastEquippedAbilities, equippedAbilities);
        // 检查激活列表变化
        bool activeChanged = HasListChanged(lastActiveAbilities, activeAbilities);
        
        // 如果有变化，进行同步
        if (equippedChanged || activeChanged)
        {
            Debug.Log("[AbilityManager] 检测到Inspector面板变化，正在同步...");
            
            // 更新检查列表
            lastEquippedAbilities = new List<string>(equippedAbilities);
            lastActiveAbilities = new List<string>(activeAbilities);
            
            // 触发激活状态变化的回调
            if (activeChanged && registeredPlayerController != null)
            {
                SyncAbilityCallbacks();
            }
            
            UpdateUI();
        }
    }
    
    /// <summary>
    /// 检查列表是否有变化
    /// </summary>
    private bool HasListChanged(List<string> oldList, List<string> newList)
    {
        if (oldList.Count != newList.Count) return true;
        
        for (int i = 0; i < newList.Count; i++)
        {
            if (i >= oldList.Count || oldList[i] != newList[i])
            {
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// 同步能力激活状态的回调
    /// </summary>
    private void SyncAbilityCallbacks()
    {
        if (registeredPlayerController == null) return;
        
        var allAbilities = registeredPlayerController.GetAllAbilities();
        
        foreach (var kvp in allAbilities)
        {
            string abilityTypeId = kvp.Key;
            PlayerAbility ability = kvp.Value;
            
            bool shouldBeActive = IsAbilityActive(abilityTypeId);
            
            // 根据激活状态调用相应回调
            if (shouldBeActive)
            {
                ability.OnAbilityActivated();
            }
            else
            {
                ability.OnAbilityDeactivated();
            }
        }
    }
    
    private void UpdateUI()
    {
        for (int i = 0; i < abilitySlotImages.Count && i < equippedAbilities.Count; i++)
        {
            string abilityTypeId = equippedAbilities[i];
            PlayerAbility data = GetAbilityData(abilityTypeId);
            
            if (data != null && data.icon != null)
            {
                abilitySlotImages[i].sprite = data.icon;
                abilitySlotImages[i].color = IsAbilityActive(abilityTypeId) ? data.color : Color.gray;
            }
            else
            {
                abilitySlotImages[i].sprite = null;
                abilitySlotImages[i].color = Color.gray;
            }
        }
    }
    
    private PlayerAbility GetAbilityData(string abilityTypeId)
    {
        return playerAbilities.Find(data => data.AbilityTypeId == abilityTypeId);
    }
    
    // 调试用的快捷键
    private void HandleDebugInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ToggleAbilityInSlot(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ToggleAbilityInSlot(1);
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            CycleAbilityInSlot(0);
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            CycleAbilityInSlot(1);
        }
    }
    
    private void ToggleAbilityInSlot(int slotIndex)
    {
        if (slotIndex >= equippedAbilities.Count) return;
        
        string currentAbility = equippedAbilities[slotIndex];
        if (!string.IsNullOrEmpty(currentAbility))
        {
            UnequipAbility(slotIndex);
        }
        else
        {
            EquipAbility("Movement", slotIndex); // 默认装备移动
        }
    }
    
    private void CycleAbilityInSlot(int slotIndex)
    {
        if (slotIndex >= equippedAbilities.Count) return;
        
        string currentAbility = equippedAbilities[slotIndex];
        string nextAbility = GetNextAbilityTypeId(currentAbility);
        
        EquipAbility(nextAbility, slotIndex);
    }
    
    private string GetNextAbilityTypeId(string current)
    {
        switch (current)
        {
            case "": return "Movement";
            case "Movement": return "Jump";
            case "Jump": return "IronBlock";
            case "IronBlock": return "Balloon";
            case "Balloon": return "GravityFlip";
            case "GravityFlip": return "IceBlock";
            case "IceBlock": return "Shrink";
            case "Shrink": return "";
            default: return "Movement";
        }
    }
    
    // 外部API
    public void PickupAbility(string abilityTypeId)
    {
        // 尝试在空槽位中装备能力
        for (int i = 0; i < maxAbilitySlots; i++)
        {
            if (string.IsNullOrEmpty(equippedAbilities[i]))
            {
                EquipAbility(abilityTypeId, i);
                return;
            }
        }
        
        // 如果没有空槽位，替换第一个槽位
        EquipAbility(abilityTypeId, 0);
    }
    
    public void SwapAbilities(int slotA, int slotB)
    {
        if (slotA < 0 || slotA >= maxAbilitySlots || 
            slotB < 0 || slotB >= maxAbilitySlots) return;
        
        string abilityA = equippedAbilities[slotA];
        string abilityB = equippedAbilities[slotB];
        
        equippedAbilities[slotA] = abilityB;
        equippedAbilities[slotB] = abilityA;
        UpdateUI();
    }
    
    /// <summary>
    /// 注册PlayerController（由PlayerController调用）
    /// </summary>
    public void RegisterPlayerController(PlayerController controller)
    {
        registeredPlayerController = controller;
        
        // 获取所有已注册的能力并更新本地注册表
        if (registeredPlayerController != null)
        {
            var playerAbilities = registeredPlayerController.GetAllAbilities();
            _abilityRegistry.Clear();
            foreach (var kvp in playerAbilities)
            {
                _abilityRegistry[kvp.Key] = kvp.Value;
            }
            
            // 验证并清理无效的能力ID
            ValidateAndCleanAbilityIds();
        }
        
        Debug.Log($"[AbilityManager] PlayerController已注册，可用能力: {string.Join(", ", _abilityRegistry.Keys)}");
    }
    
    /// <summary>
    /// 强制同步能力状态
    /// </summary>
    public void SyncAbilityStates()
    {
        SyncAbilityCallbacks();
        UpdateUI();
        Debug.Log("[AbilityManager] 强制同步完成");
    }
    
    /// <summary>
    /// 验证并清理无效的能力ID
    /// </summary>
    private void ValidateAndCleanAbilityIds()
    {
        List<string> validAbilityIds = new List<string>(_abilityRegistry.Keys);
        
        // 清理装备能力中的无效ID
        for (int i = 0; i < equippedAbilities.Count; i++)
        {
            string abilityId = equippedAbilities[i];
            if (!string.IsNullOrEmpty(abilityId) && !validAbilityIds.Contains(abilityId))
            {
                Debug.LogWarning($"[AbilityManager] 装备列表中发现无效能力ID: {abilityId}，已清除");
                equippedAbilities[i] = "";
            }
        }
        
        // 清理激活能力中的无效ID
        int originalCount = activeAbilities.Count;
        activeAbilities.RemoveAll(abilityId => 
        {
            bool isInvalid = !string.IsNullOrEmpty(abilityId) && !validAbilityIds.Contains(abilityId);
            if (isInvalid)
            {
                Debug.LogWarning($"[AbilityManager] 激活列表中发现无效能力ID: {abilityId}，已清除");
            }
            return isInvalid;
        });
        
        if (activeAbilities.Count != originalCount)
        {
            Debug.Log($"[AbilityManager] 已清理 {originalCount - activeAbilities.Count} 个无效的激活能力ID");
        }
    }
    
    /// <summary>
    /// 检查能力ID是否有效
    /// </summary>
    public bool IsValidAbilityId(string abilityId)
    {
        return !string.IsNullOrEmpty(abilityId) && _abilityRegistry.ContainsKey(abilityId);
    }
    
    /// <summary>
    /// 获取所有有效的能力ID列表
    /// </summary>
    public List<string> GetValidAbilityIds()
    {
        return new List<string>(_abilityRegistry.Keys);
    }
    
    /// <summary>
    /// 安全地激活能力（会验证ID有效性）
    /// </summary>
    public bool SafeActivateAbility(string abilityTypeId)
    {
        if (!IsValidAbilityId(abilityTypeId))
        {
            Debug.LogError($"[AbilityManager] 无效的能力ID: {abilityTypeId}，无法激活");
            return false;
        }
        
        ActivateAbility(abilityTypeId);
        return true;
    }
    
    /// <summary>
    /// 安全地装备能力（会验证ID有效性）
    /// </summary>
    public bool SafeEquipAbility(string abilityTypeId, int slotIndex)
    {
        if (!string.IsNullOrEmpty(abilityTypeId) && !IsValidAbilityId(abilityTypeId))
        {
            Debug.LogError($"[AbilityManager] 无效的能力ID: {abilityTypeId}，无法装备");
            return false;
        }
        
        return EquipAbility(abilityTypeId, slotIndex);
    }
}