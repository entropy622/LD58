using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using QFramework;
using System.Collections;

/// <summary>
/// 模块化玩家控制器 - 基于能力系统的角色控制
/// 兼容旧版本的同时提供新的模块化功能
/// </summary>
public class PlayerController : MonoSingleton<PlayerController>
{
    [Header("基础组件")]
    public SpriteRenderer playerSR;
    
    [Header("基础设置")]
    public float crouchSpeed = 1f;
    public float rollPower = 1f;
    public float rollDuration = 0.5f;
    
    [Header("能力管理器")]
    [SerializeField] private AbilityManager abilityManager;
    
    [Header("能力系统")]
    [Space(10)]
    [SerializeField] private MovementAbility _movementAbility = new MovementAbility();
    [Space(5)]
    [SerializeField] private JumpAbility _jumpAbility = new JumpAbility();
    [Space(5)]
    [SerializeField] private IronBlockAbility _ironBlockAbility = new IronBlockAbility();
    [Space(5)]
    [SerializeField] private BalloonAbility _balloonAbility = new BalloonAbility();
    
    // 公开访问器供 Editor 使用
    public MovementAbility movementAbility => _movementAbility;
    public JumpAbility jumpAbility => _jumpAbility;
    public IronBlockAbility ironBlockAbility => _ironBlockAbility;
    public BalloonAbility balloonAbility => _balloonAbility;
    
    [Header("兼容旧版本")]
    [SerializeField] private bool enableLegacyCompatibility = true;
    
    // 组件引用
    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private PhysicsMaterial2D physicsMaterial;
    
    // 状态变量
    private int facing = 1;
    private bool isGrounded = true;
    private bool isGetDown = false;
    private bool isRolling = false;
    private bool isPushing = false;
    private bool canStand = true;
    private bool tryOpen = false;
    
    // 碰撞检测
    private Vector2 originalColliderSize;
    private Vector2 originalColliderOffset;
    private LayerMask groundLayer;
    
    // 推箱子相关
    private GameObject pushBox;
    private bool canPush = false;
    
    // 旧版本兼容属性（委托给能力系统）
    public float jumpPower 
    { 
        get => _jumpAbility.jumpPower; 
        set => _jumpAbility.jumpPower = value; 
    }
    public float walkSpeed 
    { 
        get => _movementAbility.walkSpeed; 
        set => _movementAbility.walkSpeed = value; 
    }
    public float runSpeed 
    { 
        get => _movementAbility.runSpeed; 
        set => _movementAbility.runSpeed = value; 
    }
    public float pushSpeed 
    { 
        get => _movementAbility.pushSpeed; 
        set => _movementAbility.pushSpeed = value; 
    }
    
    // 公共属性访问器
    public bool IsGrounded => isGrounded;
    public bool IsGetDown => isGetDown;
    public bool IsRolling => isRolling;
    public bool IsPushing => isPushing;
    public int Facing => facing;
    public Rigidbody2D GetRigidbody() => rb;
    public BoxCollider2D GetBoxCollider() => boxCollider;
    public float GetCrouchSpeed() => crouchSpeed;
    
    void Start()
    {
        InitializeComponents();
        InitializeAbilities();
        InitializeAbilityManager();
        SetupEventHandlers();
        
        groundLayer = LayerMask.GetMask("Ground");
    }
    
    void Update()
    {
        UpdateGroundDetection();
        // UpdateCrouchState();
        UpdateRollAction();
        UpdateInteraction();
        // UpdatePushState();
        
        // 更新所有启用的能力
        UpdateAbilities();
        
        // 更新朝向和动画
        UpdateFacing();
        UpdateVisuals();
    }
    
    void FixedUpdate()
    {
        // 物理更新所有启用的能力
        FixedUpdateAbilities();
    }
    
    private void InitializeComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        physicsMaterial = boxCollider.sharedMaterial;
        
        originalColliderSize = boxCollider.size;
        originalColliderOffset = boxCollider.offset;
    }
    
    private void InitializeAbilities()
    {
        _movementAbility.Initialize(this);
        _jumpAbility.Initialize(this);
        _ironBlockAbility.Initialize(this);
        _balloonAbility.Initialize(this);
    }
    
    private void InitializeAbilityManager()
    {
        // 如果没有手动指定AbilityManager，尝试获取实例
        if (abilityManager == null)
        {
            abilityManager = FindObjectOfType<AbilityManager>();
            if (abilityManager == null)
            {
                Debug.LogWarning("AbilityManager未找到，能力槽系统将不会工作");
                return;
            }
        }
        
        // 向AbilityManager注册当前玩家控制器
        abilityManager.RegisterPlayerController(this);
        
        // 初始化默认能力配置
        SetupDefaultAbilities();
    }
    
    private void SetupDefaultAbilities()
    {
        // 默认启用移动和跳跃能力，禁用特殊能力
        EnableAbility<MovementAbility>();
        EnableAbility<JumpAbility>();
        DisableAbility<IronBlockAbility>();
        DisableAbility<BalloonAbility>();
    }
    
    private void SetupEventHandlers()
    {
        TypeEventSystem.Global.Register<OnLevelResetEvent>((e) =>
        {
            ResetToInitialState();
        }).UnRegisterWhenGameObjectDestroyed(gameObject);
    }
    
    private void UpdateAbilities()
    {
        if (_movementAbility.isEnabled) _movementAbility.UpdateAbility();
        if (_jumpAbility.isEnabled) _jumpAbility.UpdateAbility();
        if (_ironBlockAbility.isEnabled) _ironBlockAbility.UpdateAbility();
        if (_balloonAbility.isEnabled) _balloonAbility.UpdateAbility();
    }
    
    private void FixedUpdateAbilities()
    {
        if (_movementAbility.isEnabled) _movementAbility.FixedUpdateAbility();
        if (_jumpAbility.isEnabled) _jumpAbility.FixedUpdateAbility();
        if (_ironBlockAbility.isEnabled) _ironBlockAbility.FixedUpdateAbility();
        if (_balloonAbility.isEnabled) _balloonAbility.FixedUpdateAbility();
    }
    
    #region Ground Detection
    private void UpdateGroundDetection()
    {
        Vector3 center = boxCollider.bounds.center;
        float width = boxCollider.bounds.extents.x;
        float checkDistance = 0.1f;
        
        Vector3 leftPoint = center + Vector3.left * width + Vector3.down * boxCollider.bounds.extents.y;
        Vector3 rightPoint = center + Vector3.right * width + Vector3.down * boxCollider.bounds.extents.y;
        
        isGrounded = Physics2D.Raycast(leftPoint, Vector2.down, checkDistance, groundLayer) ||
                    Physics2D.Raycast(rightPoint, Vector2.down, checkDistance, groundLayer);
        
        // 检查头顶是否有障碍物
        Vector3 leftTopPoint = center + Vector3.left * width + Vector3.up * boxCollider.bounds.extents.y;
        Vector3 rightTopPoint = center + Vector3.right * width + Vector3.up * boxCollider.bounds.extents.y;
        
        canStand = !Physics2D.Raycast(leftTopPoint, Vector2.up, 1f, groundLayer) &&
                  !Physics2D.Raycast(rightTopPoint, Vector2.up, 1f, groundLayer);
    }
    #endregion
    
    #region Crouch System
    private void UpdateCrouchState()
    {
        if (isPushing) return;
        
        bool wantsToCrouch = Input.GetKey(KeyCode.S);
        
        if (wantsToCrouch)
        {
            EnterCrouchState();
        }
        else if (canStand)
        {
            ExitCrouchState();
        }
    }
    
    private void EnterCrouchState()
    {
        if (!isGetDown)
        {
            isGetDown = true;
            ModifyColliderForCrouch();
            PlayerAnimatorManager.Instance.ChangeCrouchState(true);
        }
    }
    
    private void ExitCrouchState()
    {
        if (isGetDown)
        {
            isGetDown = false;
            ResetColliderSize();
            PlayerAnimatorManager.Instance.ChangeCrouchState(false);
        }
    }
    
    private void ModifyColliderForCrouch()
    {
        boxCollider.size = new Vector2(originalColliderSize.x, originalColliderSize.y / 2);
        boxCollider.offset = new Vector2(originalColliderOffset.x, 
            originalColliderOffset.y - (originalColliderSize.y - boxCollider.size.y) / 2);
    }
    
    private void ResetColliderSize()
    {
        boxCollider.size = originalColliderSize;
        boxCollider.offset = originalColliderOffset;
    }
    #endregion
    
    #region Roll System
    private void UpdateRollAction()
    {
        if (!isRolling)
        {
            if (Input.GetMouseButtonDown(0) && isGrounded && !isPushing && !isGetDown)
            {
                StartCoroutine(PerformRoll());
            }
        }
    }
    
    private IEnumerator PerformRoll()
    {
        isRolling = true;
        rb.AddForce(Vector2.right * rollPower * facing, ForceMode2D.Impulse);
        PlayerAnimatorManager.Instance.SwitchToDash();
        
        // 翻滚时修改碰撞器
        ModifyColliderForCrouch();
        
        yield return new WaitForSeconds(rollDuration);
        
        isRolling = false;
        
        if (!canStand)
        {
            isGetDown = true;
        }
        else
        {
            ResetColliderSize();
        }
    }
    #endregion
    
    #region Push System  
    private void OnCollisionStay2D(Collision2D collision)
    {
        bool tempCanPush = false;
        
        foreach (ContactPoint2D contact in collision.contacts)
        {
            Vector2 normal = contact.normal;
            
            if (contact.collider.CompareTag("Box") && isGrounded && Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f)
            {
                if (Mathf.Abs(normal.y) < 0.1f) // 水平碰撞
                {
                    tempCanPush = true;
                    pushBox = contact.collider.gameObject;
                }
            }
        }
        
        canPush = tempCanPush;
    }
    
    private void UpdatePushState()
    {
        if (pushBox != null && isGrounded && canPush)
        {
            float horizontal = Input.GetAxis("Horizontal");
            
            if (Mathf.Abs(horizontal) > 0.1f && Mathf.Sign(horizontal) == facing)
            {
                if (!isPushing)
                {
                    isPushing = true;
                    pushBox.transform.SetParent(transform);
                    pushBox.GetComponent<BoxPushed>()?.SetPushed(true);
                }
            }
            else if (Mathf.Abs(horizontal) < 0.1f)
            {
                if (isPushing)
                {
                    isPushing = false;
                    pushBox.transform.SetParent(null);
                    pushBox.GetComponent<BoxPushed>()?.SetPushed(false);
                }
            }
        }
        
        PlayerAnimatorManager.Instance.ChangePushState(isPushing);
    }
    #endregion
    
    #region Interaction System
    private void UpdateInteraction()
    {
        tryOpen = Input.GetKey(KeyCode.E);
    }
    
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Interact") && tryOpen)
        {
            ButtonOfDoor buttonOfDoor = other.GetComponent<ButtonOfDoor>();
            buttonOfDoor?.Open();
        }
    }
    #endregion
    
    private void UpdateFacing()
    {
        if (!isPushing)
        {
            float horizontal = Input.GetAxis("Horizontal");
            if (Mathf.Abs(horizontal) > 0.1f)
            {
                facing = horizontal > 0 ? 1 : -1;
            }
        }
    }
    
    private void UpdateVisuals()
    {
        Vector3 scale = transform.localScale;
        scale.x = facing * Mathf.Abs(scale.x);
        transform.localScale = scale;
    }
    
    #region Public API for Abilities
    public void SetVelocity(float x, float y)
    {
        rb.velocity = new Vector2(x, y);
    }
    
    public void AddForce(Vector2 force, ForceMode2D mode)
    {
        rb.AddForce(force, mode);
    }
    
    public Vector2 GetVelocity()
    {
        return rb.velocity;
    }
    
    public void SetFacing(int newFacing)
    {
        facing = newFacing;
    }
    #endregion
    
    #region Ability Management
    public T GetAbility<T>() where T : PlayerAbility
    {
        if (typeof(T) == typeof(MovementAbility)) return _movementAbility as T;
        if (typeof(T) == typeof(JumpAbility)) return _jumpAbility as T;
        if (typeof(T) == typeof(IronBlockAbility)) return _ironBlockAbility as T;
        if (typeof(T) == typeof(BalloonAbility)) return _balloonAbility as T;
        return null;
    }
    
    public bool HasAbility<T>() where T : PlayerAbility
    {
        return GetAbility<T>() != null;
    }
    
    public void EnableAbility<T>() where T : PlayerAbility
    {
        var ability = GetAbility<T>();
        if (ability != null && !ability.isEnabled)
        {
            ability.isEnabled = true;
            ability.OnAbilityActivated();
            
            // 通知AbilityManager能力状态变化
            NotifyAbilityStateChanged<T>(true);
        }
    }
    
    public void DisableAbility<T>() where T : PlayerAbility
    {
        var ability = GetAbility<T>();
        if (ability != null && ability.isEnabled)
        {
            ability.isEnabled = false;
            ability.OnAbilityDeactivated();
            
            // 通知AbilityManager能力状态变化
            NotifyAbilityStateChanged<T>(false);
        }
    }
    
    public void ToggleAbility<T>() where T : PlayerAbility
    {
        var ability = GetAbility<T>();
        if (ability != null)
        {
            if (ability.isEnabled)
                DisableAbility<T>();
            else
                EnableAbility<T>();
        }
    }
    
    /// <summary>
    /// 通过能力类型启用能力（供AbilityManager调用）
    /// </summary>
    public void EnableAbilityByType(AbilityManager.AbilityType abilityType)
    {
        switch (abilityType)
        {
            case AbilityManager.AbilityType.Movement:
                EnableAbility<MovementAbility>();
                break;
            case AbilityManager.AbilityType.Jump:
                EnableAbility<JumpAbility>();
                break;
            case AbilityManager.AbilityType.IronBlock:
                EnableAbility<IronBlockAbility>();
                break;
            case AbilityManager.AbilityType.Balloon:
                EnableAbility<BalloonAbility>();
                break;
        }
    }
    
    /// <summary>
    /// 通过能力类型禁用能力（供AbilityManager调用）
    /// </summary>
    public void DisableAbilityByType(AbilityManager.AbilityType abilityType)
    {
        switch (abilityType)
        {
            case AbilityManager.AbilityType.Movement:
                DisableAbility<MovementAbility>();
                break;
            case AbilityManager.AbilityType.Jump:
                DisableAbility<JumpAbility>();
                break;
            case AbilityManager.AbilityType.IronBlock:
                DisableAbility<IronBlockAbility>();
                break;
            case AbilityManager.AbilityType.Balloon:
                DisableAbility<BalloonAbility>();
                break;
        }
    }
    
    /// <summary>
    /// 检查指定类型的能力是否启用
    /// </summary>
    public bool IsAbilityEnabled(AbilityManager.AbilityType abilityType)
    {
        switch (abilityType)
        {
            case AbilityManager.AbilityType.Movement:
                return _movementAbility.isEnabled;
            case AbilityManager.AbilityType.Jump:
                return _jumpAbility.isEnabled;
            case AbilityManager.AbilityType.IronBlock:
                return _ironBlockAbility.isEnabled;
            case AbilityManager.AbilityType.Balloon:
                return _balloonAbility.isEnabled;
            default:
                return false;
        }
    }
    
    /// <summary>
    /// 通知AbilityManager能力状态变化
    /// </summary>
    private void NotifyAbilityStateChanged<T>(bool enabled) where T : PlayerAbility
    {
        if (abilityManager == null) return;
        
        AbilityManager.AbilityType abilityType = AbilityManager.AbilityType.None;
        
        if (typeof(T) == typeof(MovementAbility))
            abilityType = AbilityManager.AbilityType.Movement;
        else if (typeof(T) == typeof(JumpAbility))
            abilityType = AbilityManager.AbilityType.Jump;
        else if (typeof(T) == typeof(IronBlockAbility))
            abilityType = AbilityManager.AbilityType.IronBlock;
        else if (typeof(T) == typeof(BalloonAbility))
            abilityType = AbilityManager.AbilityType.Balloon;
        
        if (abilityType != AbilityManager.AbilityType.None)
        {
            abilityManager.OnAbilityStateChanged(abilityType, enabled);
        }
    }
    
    /// <summary>
    /// 获取AbilityManager引用
    /// </summary>
    public AbilityManager GetAbilityManager()
    {
        return abilityManager;
    }
    #endregion
    
    private void ResetToInitialState()
    {
        // 重置所有状态
        isGetDown = false;
        isRolling = false;
        isPushing = false;
        facing = 1;
        
        // 重置动画状态
        PlayerAnimatorManager.Instance.ChangeCrouchState(false);
        PlayerAnimatorManager.Instance.ChangeJumpState(false);
        PlayerAnimatorManager.Instance.ChangeLiftState(false);
        PlayerAnimatorManager.Instance.ChangePushState(false);
        PlayerAnimatorManager.Instance.Init();
        
        // 解除推箱子绑定
        if (pushBox != null)
        {
            pushBox.transform.SetParent(null);
            pushBox = null;
        }
        
        // 重置碰撞器
        ResetColliderSize();
    }
    
    public void ActionDie()
    {
        SetVelocity(0, 0);
        PlayerAnimatorManager.Instance.SwitchToDie();
    }
    
    // 兼容旧版本的属性访问
    public bool isGround => IsGrounded;
    public bool isRun => _movementAbility.IsRunning;
    public float moveSpeed => _movementAbility.GetCurrentSpeed();
}