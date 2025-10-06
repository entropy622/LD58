using UnityEngine;

/// <summary>
/// 二段跳能力 - 允许角色在空中进行第二次跳跃
/// </summary>
[System.Serializable]
public class DoubleJumpAbility : PlayerAbility
{
    [Header("二段跳设置")]
    public float firstJumpPower = 10f;
    public float secondJumpPower = 10f; // 调整为与一段跳相同
    public float coyoteTime = 0.1f; // 土狼时间
    public float jumpBufferTime = 0.1f; // 跳跃缓冲时间
    public float jumpCooldown = 0.1f; // 跳跃冷却时间，防止误触
    
    [Header("手感优化")]
    public bool enableAirControl = true; // 空中控制
    public float airControlMultiplier = 0.8f; // 空中控制倍数
    public bool enableDoubleJumpMomentum = true; // 二段跳动量保持
    
    [Header("视觉效果")]
    public bool enableDoubleJumpEffect = true;
    public Color doubleJumpEffectColor = Color.cyan;
    
    private float lastGroundedTime;
    private float lastJumpPressedTime;
    private float lastJumpTime; // 上次跳跃时间
    private bool isFirstJumping;
    private bool hasUsedDoubleJump;
    private int jumpCount; // 0: 地面, 1: 第一段跳跃, 2: 二段跳跃
    
    public override string AbilityTypeId => "DoubleJump";
    
    public override void Initialize(PlayerController controller)
    {
        base.Initialize(controller);
        abilityName = "二段跳";
    }
    
    public override void UpdateAbility()
    {
        if (!isEnabled) return;
        
        HandleJumpInput();
        UpdateTimers();
        UpdateJumpState();
    }
    
    private void HandleJumpInput()
    {
        // 记录跳跃输入时间
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W))
        {
            lastJumpPressedTime = Time.time;
        }
        
        // 检查第一段跳跃
        bool canFirstJump = (Time.time - lastGroundedTime) <= coyoteTime && 
                           (Time.time - lastJumpPressedTime) <= jumpBufferTime &&
                           !playerController.IsGetDown &&
                           jumpCount == 0;
        
        // 检查二段跳跃
        bool canDoubleJump = (Time.time - lastJumpPressedTime) <= jumpBufferTime &&
                            !playerController.IsGetDown &&
                            jumpCount == 1 &&
                            !hasUsedDoubleJump &&
                            !playerController.IsGrounded &&
                            (Time.time - lastJumpTime) >= jumpCooldown; // 添加跳跃冷却
        
        if (canFirstJump)
        {
            PerformFirstJump();
        }
        else if (canDoubleJump)
        {
            PerformDoubleJump();
        }
    }
    
    private void PerformFirstJump()
    {
        // 获取修改后的跳跃力
        float modifiedJumpPower = GetModifiedJumpPower(firstJumpPower);
        
        playerController.AddForce(Vector2.up * modifiedJumpPower, ForceMode2D.Impulse);
        isFirstJumping = true;
        jumpCount = 1;
        lastJumpTime = Time.time; // 记录跳跃时间
        
        // 重置计时器，避免重复跳跃
        lastJumpPressedTime = 0f;
        lastGroundedTime = 0f;
        
        PlayerAnimatorManager.Instance.ChangeJumpState(true);
        
        Debug.Log("[DoubleJump] 第一段跳跃");
    }
    
    private void PerformDoubleJump()
    {
        // 获取修改后的二段跳跃力
        float modifiedJumpPower = GetModifiedJumpPower(secondJumpPower);
        
        // 重置垂直速度，然后应用二段跳跃力
        Vector2 currentVelocity = playerController.GetVelocity();
        // 优化：不完全重置垂直速度，而是减去一部分，使二段跳更自然
        float newVerticalVelocity = Mathf.Max(0, currentVelocity.y * 0.2f); // 保留少量向上的速度
        playerController.SetVelocity(currentVelocity.x, newVerticalVelocity);
        playerController.AddForce(Vector2.up * modifiedJumpPower, ForceMode2D.Impulse);
        
        jumpCount = 2;
        hasUsedDoubleJump = true;
        lastJumpTime = Time.time; // 记录跳跃时间
        
        // 重置计时器
        lastJumpPressedTime = 0f;
        
        // 播放二段跳效果
        if (enableDoubleJumpEffect)
        {
            PlayDoubleJumpEffect();
        }
        
        PlayerAnimatorManager.Instance.ChangeJumpState(true);
        
        Debug.Log("[DoubleJump] 二段跳跃");
    }
    
    private void UpdateTimers()
    {
        if (playerController.IsGrounded)
        {
            lastGroundedTime = Time.time;
        }
    }
    
    private void UpdateJumpState()
    {
        // 当角色落地时重置跳跃状态
        if (playerController.IsGrounded && playerController.GetVelocity().y <= 0.1f)
        {
            isFirstJumping = false;
            hasUsedDoubleJump = false;
            jumpCount = 0;
            PlayerAnimatorManager.Instance.ChangeJumpState(false);
        }
        // 添加空中下降时的重置机制，提高容错率
        else if (!playerController.IsGrounded && playerController.GetVelocity().y < -5f)
        {
            // 当角色快速下降时，重置二段跳状态，允许在特定情况下再次使用
            if (hasUsedDoubleJump && jumpCount == 2)
            {
                // 可选：在特定条件下允许重新使用二段跳（例如快速下降时靠近墙壁）
                // 这里可以添加更复杂的逻辑
            }
        }
    }
    
    /// <summary>
    /// 获取修改后的跳跃力，可能受其他能力影响
    /// </summary>
    private float GetModifiedJumpPower(float basePower)
    {
        float modifiedPower = basePower;
        
        // 检查是否有铁块能力影响
        if (AbilityManager.Instance.activeAbilities.Contains("IronBlock"))
        {
            modifiedPower *= 0.6f; // 铁块状态下跳跃力减弱
        }
        
        // 检查是否有缩小能力影响
        if (AbilityManager.Instance.activeAbilities.Contains("Shrink"))
        {
            modifiedPower *= 0.8f; // 缩小状态下跳跃力略微减弱
        }
        
        return modifiedPower;
    }
    
    /// <summary>
    /// 播放二段跳视觉效果
    /// </summary>
    private void PlayDoubleJumpEffect()
    {
        // 创建简单的粒子效果
        GameObject effectGO = new GameObject("DoubleJumpEffect");
        effectGO.transform.position = playerController.transform.position;
        
        // 添加粒子系统
        ParticleSystem particles = effectGO.AddComponent<ParticleSystem>();
        
        // 配置粒子效果
        var main = particles.main;
        main.startLifetime = 0.5f;
        main.startSpeed = 4f; // 增加速度
        main.startColor = doubleJumpEffectColor;
        main.startSize = 0.25f; // 稍大一些
        main.maxParticles = 20; // 增加粒子数量
        
        var emission = particles.emission;
        emission.enabled = true;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0.0f, 20)
        });
        
        var shape = particles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.4f; // 增加半径
        
        var velocityOverLifetime = particles.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(3f); // 增加径向速度
        
        // 添加重力影响
        var forceOverLifetime = particles.forceOverLifetime;
        forceOverLifetime.enabled = true;
        forceOverLifetime.y = new ParticleSystem.MinMaxCurve(-1f); // 向下重力
        
        // 自动销毁效果
        Object.Destroy(effectGO, 1f);
    }
    
    /// <summary>
    /// 强制重置二段跳状态（用于调试或特殊情况）
    /// </summary>
    public void ResetDoubleJumpState()
    {
        hasUsedDoubleJump = false;
        jumpCount = 0;
        isFirstJumping = false;
    }
    
    public override void FixedUpdateAbility()
    {
        if (!isEnabled) return;
        
        // 在FixedUpdate中处理物理相关的控制
        if (enableAirControl && jumpCount > 0 && !playerController.IsGrounded)
        {
            HandleAirControl();
        }
    }
    
    /// <summary>
    /// 处理空中控制
    /// </summary>
    private void HandleAirControl()
    {
        float horizontal = Input.GetAxis("Horizontal");
        if (Mathf.Abs(horizontal) > 0.1f)
        {
            Vector2 currentVelocity = playerController.GetVelocity();
            float targetVelocityX = horizontal * playerController.movementAbility.GetCurrentSpeed() * airControlMultiplier;
            
            // 平滑调整水平速度
            float newVelocityX = Mathf.Lerp(currentVelocity.x, targetVelocityX, Time.fixedDeltaTime * 5f);
            playerController.SetVelocity(newVelocityX, currentVelocity.y);
        }
    }
}