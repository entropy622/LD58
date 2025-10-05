using System.Collections.Generic;
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
    
    // 公共属性访问器
    public bool IsGrounded => isGrounded;
    public bool IsGetDown => isGetDown;
    public bool IsRolling => isRolling;
    public int Facing => facing;
    public Rigidbody2D GetRigidbody() => rb;
    public BoxCollider2D GetBoxCollider() => boxCollider;
    public float GetCrouchSpeed() => crouchSpeed;
    
    void Awake()
    {
        // 提前初始化能力系统，让AbilityManager能在Start前获得完整的注册表
        InitializeComponents();
        InitializeAbilities();
        InitializeAbilityManager();
    }
    
    void Start()
    {
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
        
        // 防止卡在tile缝隙中的优化
        PreventTileGapSticking();
        
        UpdateVelocity();
    }

    void UpdateVelocity()
    {
        PlayerAnimatorManager.Instance.ChangeVelocityY(
            abilityManager.equippedAbilities.Exists(ability => ability == "GravityFlip")
            ? -rb.velocity.y
            : rb.velocity.y);
    }
    
    private void InitializeComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        physicsMaterial = boxCollider.sharedMaterial;
        
        originalColliderSize = boxCollider.size;
        originalColliderOffset = boxCollider.offset;
        
        // 优化物理设置，提高操作手感
        OptimizePhysicsSettings();
    }
    
    /// <summary>
    /// 优化物理设置，解决常见的移动问题
    /// </summary>
    private void OptimizePhysicsSettings()
    {
        if (rb != null)
        {
            // 冻结Z轴旋转，防止角色翻转
            rb.freezeRotation = true;
            
            // 设置适当的阻力，提高停止响应
            rb.drag = 2f;
            
            // 设置重力缩放，使跳跃更自然
            rb.gravityScale = 2.5f;
            
            // 设置较小的角速度阻力
            rb.angularDrag = 5f;
        }
        
        if (boxCollider != null)
        {
            // 设置碰撞器边缘为圆角，减少卡住问题
            // 注意：这需要在Inspector中手动设置或创建PhysicsMaterial2D
            if (physicsMaterial == null)
            {
                Debug.LogWarning("建议为PlayerController的Collider添加PhysicsMaterial2D来优化物理表现");
            }
        }
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
    
    #region Movement Optimization
    /// <summary>
    /// 防止角色卡在tile缝隙中
    /// </summary>
    private void PreventTileGapSticking()
    {
        if (!isGrounded || Mathf.Abs(rb.velocity.x) < 0.1f) return;
        
        // 检测是否卡在缝隙中
        Vector2 rayOrigin = new Vector2(transform.position.x, boxCollider.bounds.min.y - 0.01f);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, 0.1f, groundLayer);
        
        if (hit.collider == null)
        {
            // 如果脚下没有地面但角色被认为在地上，可能卡在缝隙中
            // 给予微小的向上推力
            rb.AddForce(Vector2.up * 2f, ForceMode2D.Force);
        }
        
        // 检测水平移动是否被阻挡
        float horizontalInput = Input.GetAxis("Horizontal");
        if (Mathf.Abs(horizontalInput) > 0.1f && Mathf.Abs(rb.velocity.x) < 0.5f)
        {
            // 有输入但速度很小，可能卡住了
            Vector2 pushDirection = new Vector2(Mathf.Sign(horizontalInput), 0.2f);
            rb.AddForce(pushDirection * 3f, ForceMode2D.Force);
        }
    }
    #endregion
    
    #region Ground Detection
    private void UpdateGroundDetection()
    {
        // 使用更精确的地面检测，避免tile缝隙问题
        Vector3 center = boxCollider.bounds.center;
        float width = boxCollider.bounds.extents.x * 0.9f; // 稍微缩小检测范围
        float height = boxCollider.bounds.extents.y;
        float checkDistance = 0.05f; // 减小检测距离
        
        // 多点检测，包括中心点
        Vector3 leftPoint = center + Vector3.left * width + Vector3.down * height;
        Vector3 rightPoint = center + Vector3.right * width + Vector3.down * height;
        Vector3 centerPoint = center + Vector3.down * height;
        
        // 使用CapsuleCast进行更稳定的检测
        bool leftGrounded = Physics2D.Raycast(leftPoint, Vector2.down, checkDistance, groundLayer);
        bool rightGrounded = Physics2D.Raycast(rightPoint, Vector2.down, checkDistance, groundLayer);
        bool centerGrounded = Physics2D.Raycast(centerPoint, Vector2.down, checkDistance, groundLayer);
        
        isGrounded = leftGrounded || rightGrounded || centerGrounded;
        
        // 额外的圆形检测，防止角色在tile边缘卡住
        if (!isGrounded)
        {
            Vector2 circleCenter = new Vector2(center.x, center.y - height - 0.02f);
            isGrounded = Physics2D.OverlapCircle(circleCenter, 0.05f, groundLayer);
        }
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
        
        yield return new WaitForSeconds(rollDuration);
        
        isRolling = false;
        ResetColliderSize();
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
    
    /// <summary>
    /// 强制重新初始化所有能力（供Editor使用）
    /// </summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public void RefreshAllAbilities()
    {
        if (!Application.isPlaying) return;
        
        // 重新初始化所有能力
        InitializeAbilities();
        
        // 同步到AbilityManager
        if (abilityManager != null)
        {
            // 重新注册到AbilityManager
            abilityManager.RegisterPlayerController(this);
            abilityManager.SyncAbilityStates();
        }
        
        Debug.Log("[PlayerController] 已强制刷新所有能力参数");
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
    
    #region Debug Visualization
    /// <summary>
    /// 在Scene视图中显示调试信息
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (boxCollider == null) return;
        
        // 显示地面检测射线
        Vector3 center = boxCollider.bounds.center;
        float width = boxCollider.bounds.extents.x * 0.9f;
        float height = boxCollider.bounds.extents.y;
        
        Vector3 leftPoint = center + Vector3.left * width + Vector3.down * height;
        Vector3 rightPoint = center + Vector3.right * width + Vector3.down * height;
        Vector3 centerPoint = center + Vector3.down * height;
        
        // 设置颜色
        Gizmos.color = isGrounded ? Color.green : Color.red;
        
        // 绘制检测射线
        Gizmos.DrawLine(leftPoint, leftPoint + Vector3.down * 0.05f);
        Gizmos.DrawLine(rightPoint, rightPoint + Vector3.down * 0.05f);
        Gizmos.DrawLine(centerPoint, centerPoint + Vector3.down * 0.05f);
        
        // 绘制圆形检浌范围
        Vector3 circleCenter = new Vector3(center.x, center.y - height - 0.02f, center.z);
        Gizmos.DrawWireSphere(circleCenter, 0.05f);
        
        // 显示移动状态
        if (Application.isPlaying)
        {
            Gizmos.color = Color.blue;
            Vector3 velocityIndicator = transform.position + new Vector3(rb.velocity.x * 0.1f, rb.velocity.y * 0.1f, 0);
            Gizmos.DrawLine(transform.position, velocityIndicator);
        }
    }
    #endregion
}