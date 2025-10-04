using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using QFramework;
using System.Collections;

/// <summary>
/// 模块化玩家控制器 - 基于能力系统的角色控制
/// 作为能力执行器，状态由AbilityManager统一管理
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
    
    // 能力实例字典（仅用于执行，状态由AbilityManager管理）
    public Dictionary<string, PlayerAbility> _abilityRegistry = new Dictionary<string, PlayerAbility>();
    
    [Header("能力系统")]
    [Space(10)]
    [SerializeField] private MovementAbility _movementAbility = new MovementAbility();
    [Space(5)]
    [SerializeField] private JumpAbility _jumpAbility = new JumpAbility();
    [Space(5)]
    [SerializeField] private IronBlockAbility _ironBlockAbility = new IronBlockAbility();
    [Space(5)]
    [SerializeField] private BalloonAbility _balloonAbility = new BalloonAbility();
    [Space(5)]
    [SerializeField] private GravityFlipAbility _gravityFlipAbility = new GravityFlipAbility();
    [Space(5)]
    [SerializeField] private IceBlockAbility _iceBlockAbility = new IceBlockAbility();
    [Space(5)]
    [SerializeField] private ShrinkAbility _shrinkAbility = new ShrinkAbility();
    
    // 公开访问器供 Editor 使用
    public MovementAbility movementAbility => _movementAbility;
    public JumpAbility jumpAbility => _jumpAbility;
    public IronBlockAbility ironBlockAbility => _ironBlockAbility;
    public BalloonAbility balloonAbility => _balloonAbility;
    public GravityFlipAbility gravityFlipAbility => _gravityFlipAbility;
    public IceBlockAbility iceBlockAbility => _iceBlockAbility;
    public ShrinkAbility shrinkAbility => _shrinkAbility;
    
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
        UpdateRollAction();
        UpdateInteraction();
        
        // 执行AbilityManager指定的能力
        ExecuteActiveAbilities();
        
        // 更新朝向和动画
        UpdateFacing();
        UpdateVisuals();
    }
    
    void FixedUpdate()
    {
        // 物理更新AbilityManager指定的能力
        FixedUpdateActiveAbilities();
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
        // 初始化所有能力实例
        _movementAbility.Initialize(this);
        _jumpAbility.Initialize(this);
        _ironBlockAbility.Initialize(this);
        _balloonAbility.Initialize(this);
        _gravityFlipAbility.Initialize(this);
        _iceBlockAbility.Initialize(this);
        _shrinkAbility.Initialize(this);
        
        // 注册能力到字典中
        RegisterAbility(_movementAbility);
        RegisterAbility(_jumpAbility);
        RegisterAbility(_ironBlockAbility);
        RegisterAbility(_balloonAbility);
        RegisterAbility(_gravityFlipAbility);
        RegisterAbility(_iceBlockAbility);
        RegisterAbility(_shrinkAbility);
    }
    
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
    
    private void InitializeAbilityManager()
    {
        // 如果没有手动指定AbilityManager，尝试获取实例
        if (abilityManager == null)
        {
            abilityManager = AbilityManager.Instance;
            if (abilityManager == null)
            {
                Debug.LogWarning("AbilityManager未找到，能力系统将不会工作");
                return;
            }
        }
        
        // 向AbilityManager注册当前玩家控制器
        abilityManager.RegisterPlayerController(this);
    }
    
    private void SetupEventHandlers()
    {
        TypeEventSystem.Global.Register<OnLevelResetEvent>((e) =>
        {
            ResetToInitialState();
        }).UnRegisterWhenGameObjectDestroyed(gameObject);
    }
    
    /// <summary>
    /// 执行AbilityManager中激活的能力
    /// </summary>
    private void ExecuteActiveAbilities()
    {
        if (abilityManager == null) return;

        var activeAbilities = abilityManager.GetEquippedAbilities();
        foreach (string abilityTypeId in activeAbilities)
        {
            if (_abilityRegistry.TryGetValue(abilityTypeId, out PlayerAbility ability))
            {
                ability.UpdateAbility();
            }
        }
    }
    
    /// <summary>
    /// 物理更新AbilityManager中激活的能力
    /// </summary>
    private void FixedUpdateActiveAbilities()
    {
        if (abilityManager == null) return;

        var activeAbilities = abilityManager.GetEquippedAbilities();
        foreach (string abilityTypeId in activeAbilities)
        {
            if (_abilityRegistry.TryGetValue(abilityTypeId, out PlayerAbility ability))
            {
                ability.FixedUpdateAbility();
            }
        }
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
    
    #region Ability Registry Access (For AbilityManager)
    /// <summary>
    /// 获取指定类型的能力实例（供AbilityManager使用）
    /// </summary>
    public PlayerAbility GetAbilityByTypeId(string abilityTypeId)
    {
        _abilityRegistry.TryGetValue(abilityTypeId, out PlayerAbility ability);
        return ability;
    }
    
    /// <summary>
    /// 获取所有已注册的能力实例（供AbilityManager使用）
    /// </summary>
    public Dictionary<string, PlayerAbility> GetAllAbilities()
    {
        return new Dictionary<string, PlayerAbility>(_abilityRegistry);
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
    public bool isRun => abilityManager?.IsAbilityActive("Movement") == true && _movementAbility.IsRunning;
    public float moveSpeed => _movementAbility.GetCurrentSpeed();
}