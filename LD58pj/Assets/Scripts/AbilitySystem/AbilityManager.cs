using UnityEngine;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// 能力管理器 - 管理玩家的能力装备和切换
/// </summary>
public class AbilityManager : MonoBehaviour
{
    [Header("能力槽设置")]
    public int maxAbilitySlots = 2;
    public List<AbilityType> equippedAbilities = new List<AbilityType>();
    
    [Header("UI引用")]
    public Transform abilitySlotParent;
    public GameObject abilitySlotPrefab;
    
    // 玩家控制器引用
    private PlayerController registeredPlayerController;
    private List<Image> abilitySlotImages = new List<Image>();
    
    public enum AbilityType
    {
        None,
        Movement,
        Jump,
        IronBlock,
        Balloon
    }
    
    [System.Serializable]
    public class AbilityData
    {
        public AbilityType type;
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
            EquipAbility(AbilityType.Movement, 0);
            EquipAbility(AbilityType.Jump, 1);
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
                equippedAbilities.Add(AbilityType.None);
            }
        }
        
        UpdateUI();
    }
    
    public bool EquipAbility(AbilityType abilityType, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= maxAbilitySlots) return false;
        
        // 取消装备旧能力
        if (equippedAbilities[slotIndex] != AbilityType.None)
        {
            UnequipAbility(slotIndex);
        }
        
        // 装备新能力
        equippedAbilities[slotIndex] = abilityType;
        ActivateAbility(abilityType, true);
        
        UpdateUI();
        return true;
    }
    
    public void UnequipAbility(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= maxAbilitySlots) return;
        
        AbilityType abilityType = equippedAbilities[slotIndex];
        if (abilityType != AbilityType.None)
        {
            ActivateAbility(abilityType, false);
            equippedAbilities[slotIndex] = AbilityType.None;
        }
        
        UpdateUI();
    }
    
    public bool HasAbilityEquipped(AbilityType abilityType)
    {
        return equippedAbilities.Contains(abilityType);
    }
    
    public int GetAbilitySlotIndex(AbilityType abilityType)
    {
        return equippedAbilities.IndexOf(abilityType);
    }
    
    private void ActivateAbility(AbilityType abilityType, bool activate)
    {
        if (registeredPlayerController == null) return;
        
        if (activate)
        {
            registeredPlayerController.EnableAbilityByType(abilityType);
        }
        else
        {
            registeredPlayerController.DisableAbilityByType(abilityType);
        }
    }
    
    private void UpdateUI()
    {
        for (int i = 0; i < abilitySlotImages.Count && i < equippedAbilities.Count; i++)
        {
            AbilityType abilityType = equippedAbilities[i];
            AbilityData data = GetAbilityData(abilityType);
            
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
    
    private AbilityData GetAbilityData(AbilityType abilityType)
    {
        return abilityDataList.Find(data => data.type == abilityType);
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
        
        AbilityType currentAbility = equippedAbilities[slotIndex];
        if (currentAbility != AbilityType.None)
        {
            UnequipAbility(slotIndex);
        }
        else
        {
            EquipAbility(AbilityType.Movement, slotIndex); // 默认装备移动
        }
    }
    
    private void CycleAbilityInSlot(int slotIndex)
    {
        if (slotIndex >= equippedAbilities.Count) return;
        
        AbilityType currentAbility = equippedAbilities[slotIndex];
        AbilityType nextAbility = GetNextAbilityType(currentAbility);
        
        EquipAbility(nextAbility, slotIndex);
    }
    
    private AbilityType GetNextAbilityType(AbilityType current)
    {
        switch (current)
        {
            case AbilityType.None: return AbilityType.Movement;
            case AbilityType.Movement: return AbilityType.Jump;
            case AbilityType.Jump: return AbilityType.IronBlock;
            case AbilityType.IronBlock: return AbilityType.Balloon;
            case AbilityType.Balloon: return AbilityType.None;
            default: return AbilityType.Movement;
        }
    }
    
    // 外部API
    public void PickupAbility(AbilityType abilityType)
    {
        // 尝试在空槽位中装备能力
        for (int i = 0; i < maxAbilitySlots; i++)
        {
            if (equippedAbilities[i] == AbilityType.None)
            {
                EquipAbility(abilityType, i);
                return;
            }
        }
        
        // 如果没有空槽位，替换第一个槽位
        EquipAbility(abilityType, 0);
    }
    
    public void SwapAbilities(int slotA, int slotB)
    {
        if (slotA < 0 || slotA >= maxAbilitySlots || 
            slotB < 0 || slotB >= maxAbilitySlots) return;
        
        AbilityType abilityA = equippedAbilities[slotA];
        AbilityType abilityB = equippedAbilities[slotB];
        
        equippedAbilities[slotA] = abilityB;
        equippedAbilities[slotB] = abilityA;
        
        // 重新激活能力
        ActivateAbility(abilityA, false);
        ActivateAbility(abilityB, false);
        ActivateAbility(abilityA, true);
        ActivateAbility(abilityB, true);
        
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
    public void OnAbilityStateChanged(AbilityType abilityType, bool enabled)
    {
        // 更新UI显示
        UpdateUI();
        
        // 可以在这里添加其他响应逻辑，比如播放声音、显示提示等
        Debug.Log($"能力{abilityType} {(enabled ? "已启用" : "已禁用")}");
    }
    
    /// <summary>
    /// 获取当前装备的能力列表
    /// </summary>
    public List<AbilityType> GetEquippedAbilities()
    {
        return new List<AbilityType>(equippedAbilities);
    }
    
    /// <summary>
    /// 获取指定槽位的能力
    /// </summary>
    public AbilityType GetAbilityInSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < equippedAbilities.Count)
            return equippedAbilities[slotIndex];
        return AbilityType.None;
    }
    
    /// <summary>
    /// 检查指定能力是否正在使用（通过PlayerController检查实际状态）
    /// </summary>
    public bool IsAbilityActive(AbilityType abilityType)
    {
        if (registeredPlayerController == null) return false;
        return registeredPlayerController.IsAbilityEnabled(abilityType);
    }
    
    /// <summary>
    /// 强制同步能力状态（确保AbilityManager与PlayerController一致）
    /// </summary>
    public void SyncAbilityStates()
    {
        if (registeredPlayerController == null) return;
        
        for (int i = 0; i < equippedAbilities.Count; i++)
        {
            AbilityType abilityType = equippedAbilities[i];
            if (abilityType != AbilityType.None)
            {
                bool shouldBeEnabled = true; // 装备的能力应该被启用
                bool isCurrentlyEnabled = registeredPlayerController.IsAbilityEnabled(abilityType);
                
                if (shouldBeEnabled != isCurrentlyEnabled)
                {
                    ActivateAbility(abilityType, shouldBeEnabled);
                }
            }
        }
        
        UpdateUI();
    }
}
