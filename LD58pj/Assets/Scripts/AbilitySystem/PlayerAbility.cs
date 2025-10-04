using UnityEngine;

/// <summary>
/// 玩家能力基类，所有能力都继承此类
/// </summary>
[System.Serializable]
public abstract class PlayerAbility
{
    [Header("能力基础设置")]
    public string abilityName;
    [SerializeField] private bool _isEnabled = true;
    
    /// <summary>
    /// 能力类型标识符，用于解耦能力类型管理
    /// </summary>
    public abstract string AbilityTypeId { get; }
    
    /// <summary>
    /// 能力是否启用（带有Inspector可视化支持）
    /// </summary>
    public bool isEnabled 
    { 
        get => _isEnabled; 
        set 
        { 
            if (_isEnabled != value)
            {
                _isEnabled = value;
                if (value)
                    OnAbilityActivated();
                else
                    OnAbilityDeactivated();
            }
        } 
    }
    
    protected PlayerController playerController;
    
    /// <summary>
    /// 初始化能力
    /// </summary>
    public virtual void Initialize(PlayerController controller)
    {
        playerController = controller;
    }
    
    /// <summary>
    /// 能力激活时调用
    /// </summary>
    public virtual void OnAbilityActivated() { }
    
    /// <summary>
    /// 能力停用时调用
    /// </summary>
    public virtual void OnAbilityDeactivated() { }
    
    /// <summary>
    /// 每帧更新
    /// </summary>
    public virtual void UpdateAbility() { }
    
    /// <summary>
    /// 物理更新
    /// </summary>
    public virtual void FixedUpdateAbility() { }
    
    /// <summary>
    /// 修改玩家物理属性
    /// </summary>
    public virtual void ModifyPhysicsProperties() { }
    
    /// <summary>
    /// 重置物理属性到默认值
    /// </summary>
    public virtual void ResetPhysicsProperties() { }
}