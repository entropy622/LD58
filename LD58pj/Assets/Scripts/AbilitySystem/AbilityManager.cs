using UnityEngine;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using QFramework;

/// <summary>
/// 能力管理器 - 管理玩家的能力装备和切换
/// </summary>
public class AbilityManager : MonoSingleton<AbilityManager>
{
    [Header("能力槽设置")]
    public int maxAbilitySlots = 2;
    public List<string> equippedAbilities = new List<string>(); // 使用字符串标识符代替AbilityType
    
    [Header("UI引用")]
    public Transform abilitySlotParent;
    public GameObject abilitySlotPrefab;
    
    // 玩家控制器引用
    private PlayerController registeredPlayerController;
    private List<Image> abilitySlotImages = new List<Image>();
    
    // 能力状态同步机制
    private bool isUpdatingFromPlayerController = false;
    
    [System.Serializable]
    public class AbilityData
    {
        public string abilityTypeId; // 使用字符串标识符
        public string name;
        public Sprite icon;
        public Color color = Color.white;
    }
    
    [Header("能力数据")]
    public List<AbilityData> abilityDataList = new List<AbilityData>();
    
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
        
        // 设置默认能力
        if (registeredPlayerController != null)
        {
            EquipAbility("Movement", 0);
            EquipAbility("Jump", 1);
        }
    }
    
    void Update()
    {
        HandleDebugInput();
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
    
    public bool EquipAbility(string abilityTypeId, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= maxAbilitySlots) return false;
        if (isUpdatingFromPlayerController) return true; // 避免循环更新
        
        // 取消装备旧能力
        if (!string.IsNullOrEmpty(equippedAbilities[slotIndex]))
        {
            UnequipAbility(slotIndex);
        }
        
        // 装备新能力
        equippedAbilities[slotIndex] = abilityTypeId;
        ActivateAbility(abilityTypeId, true);
        
        UpdateUI();
        return true;
    }
    
    public void UnequipAbility(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= maxAbilitySlots) return;
        if (isUpdatingFromPlayerController) return; // 避免循环更新
        
        string abilityTypeId = equippedAbilities[slotIndex];
        if (!string.IsNullOrEmpty(abilityTypeId))
        {
            ActivateAbility(abilityTypeId, false);
            equippedAbilities[slotIndex] = "";
        }
        
        UpdateUI();
    }
    
    public bool HasAbilityEquipped(string abilityTypeId)
    {
        return equippedAbilities.Contains(abilityTypeId);
    }
    
    public int GetAbilitySlotIndex(string abilityTypeId)
    {
        return equippedAbilities.IndexOf(abilityTypeId);
    }
    
    private void ActivateAbility(string abilityTypeId, bool activate)
    {
        if (registeredPlayerController == null || string.IsNullOrEmpty(abilityTypeId)) return;
        
        if (activate)
        {
            registeredPlayerController.EnableAbilityByTypeId(abilityTypeId);
        }
        else
        {
            registeredPlayerController.DisableAbilityByTypeId(abilityTypeId);
        }
    }
    
    private void UpdateUI()
    {
        for (int i = 0; i < abilitySlotImages.Count && i < equippedAbilities.Count; i++)
        {
            string abilityTypeId = equippedAbilities[i];
            AbilityData data = GetAbilityData(abilityTypeId);
            
            if (data != null && data.icon != null)
            {
                abilitySlotImages[i].sprite = data.icon;
                abilitySlotImages[i].color = data.color;
            }
            else
            {
                abilitySlotImages[i].sprite = null;
                abilitySlotImages[i].color = Color.gray;
            }
        }
    }
    
    private AbilityData GetAbilityData(string abilityTypeId)
    {
        return abilityDataList.Find(data => data.abilityTypeId == abilityTypeId);
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
            case "Balloon": return "";
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
        
        // 重新激活能力
        if (!string.IsNullOrEmpty(abilityA))
        {
            ActivateAbility(abilityA, false);
            ActivateAbility(abilityA, true);
        }
        if (!string.IsNullOrEmpty(abilityB))
        {
            ActivateAbility(abilityB, false);
            ActivateAbility(abilityB, true);
        }
        
        UpdateUI();
    }
    
    /// <summary>
    /// 注册PlayerController（由PlayerController调用）
    /// </summary>
    public void RegisterPlayerController(PlayerController controller)
    {
        registeredPlayerController = controller;
        Debug.Log("PlayerController已注册到AbilityManager");
    }
    
    /// <summary>
    /// 处理能力状态变化（由PlayerController调用）
    /// </summary>
    public void OnAbilityStateChanged(string abilityTypeId, bool enabled)
    {
        isUpdatingFromPlayerController = true;
        
        // 检查是否需要同步装备状态
        bool isEquipped = HasAbilityEquipped(abilityTypeId);
        
        if (enabled && !isEquipped)
        {
            // 能力被启用但未装备，尝试装备到空槽位
            PickupAbility(abilityTypeId);
        }
        else if (!enabled && isEquipped)
        {
            // 能力被禁用但已装备，取消装备
            int slotIndex = GetAbilitySlotIndex(abilityTypeId);
            if (slotIndex >= 0)
            {
                equippedAbilities[slotIndex] = "";
            }
        }
        
        // 更新UI显示
        UpdateUI();
        
        isUpdatingFromPlayerController = false;
        
        // 可以在这里添加其他响应逻辑，比如播放声音、显示提示等
        Debug.Log($"能力{abilityTypeId} {(enabled ? "已启用" : "已禁用")}");
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
    /// 检查指定能力是否正在使用（通过PlayerController检查实际状态）
    /// </summary>
    public bool IsAbilityActive(string abilityTypeId)
    {
        if (registeredPlayerController == null) return false;
        return registeredPlayerController.IsAbilityEnabledByTypeId(abilityTypeId);
    }
    
    /// <summary>
    /// 强制同步能力状态（确保AbilityManager与PlayerController一致）
    /// </summary>
    public void SyncAbilityStates()
    {
        if (registeredPlayerController == null) return;
        
        for (int i = 0; i < equippedAbilities.Count; i++)
        {
            string abilityTypeId = equippedAbilities[i];
            if (!string.IsNullOrEmpty(abilityTypeId))
            {
                bool shouldBeEnabled = true; // 装备的能力应该被启用
                bool isCurrentlyEnabled = registeredPlayerController.IsAbilityEnabledByTypeId(abilityTypeId);
                
                if (shouldBeEnabled != isCurrentlyEnabled)
                {
                    ActivateAbility(abilityTypeId, shouldBeEnabled);
                }
            }
        }
        
        UpdateUI();
    }
    
    #region Legacy Compatibility (Deprecated - Use string-based methods instead)
    
    public enum AbilityType
    {
        None,
        Movement,
        Jump,
        IronBlock,
        Balloon
    }
    
    /// <summary>
    /// 转换AbilityType到字符串标识符
    /// </summary>
    private string ConvertAbilityTypeToId(AbilityType abilityType)
    {
        switch (abilityType)
        {
            case AbilityType.Movement: return "Movement";
            case AbilityType.Jump: return "Jump";
            case AbilityType.IronBlock: return "IronBlock";
            case AbilityType.Balloon: return "Balloon";
            case AbilityType.None:
            default: return "";
        }
    }
    
    /// <summary>
    /// 转换字符串标识符到AbilityType
    /// </summary>
    private AbilityType ConvertIdToAbilityType(string abilityTypeId)
    {
        switch (abilityTypeId)
        {
            case "Movement": return AbilityType.Movement;
            case "Jump": return AbilityType.Jump;
            case "IronBlock": return AbilityType.IronBlock;
            case "Balloon": return AbilityType.Balloon;
            case "":
            default: return AbilityType.None;
        }
    }
    
    /// <summary>
    /// 装备能力（旧版本兼容）
    /// </summary>
    [System.Obsolete("Use EquipAbility(string abilityTypeId, int slotIndex) instead")]
    public bool EquipAbility(AbilityType abilityType, int slotIndex)
    {
        string abilityTypeId = ConvertAbilityTypeToId(abilityType);
        return EquipAbility(abilityTypeId, slotIndex);
    }
    
    /// <summary>
    /// 获取能力（旧版本兼容）
    /// </summary>
    [System.Obsolete("Use PickupAbility(string abilityTypeId) instead")]
    public void PickupAbility(AbilityType abilityType)
    {
        string abilityTypeId = ConvertAbilityTypeToId(abilityType);
        PickupAbility(abilityTypeId);
    }
    
    /// <summary>
    /// 检查能力是否激活（旧版本兼容）
    /// </summary>
    [System.Obsolete("Use IsAbilityActive(string abilityTypeId) instead")]
    public bool IsAbilityActive(AbilityType abilityType)
    {
        string abilityTypeId = ConvertAbilityTypeToId(abilityType);
        return IsAbilityActive(abilityTypeId);
    }
    
    #endregion
}
