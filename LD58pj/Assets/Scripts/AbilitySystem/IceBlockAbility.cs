using UnityEngine;

/// <summary>
/// 冰块能力 - 摩擦力大幅降低，高速滑行，增强跳跃横向距离
/// </summary>
[System.Serializable]
public class IceBlockAbility : PlayerAbility
{
    [Header("冰块滑行设置")]
    public float frictionReduction = 0.9f; // 摩擦力减少90%
    public float slideSpeedMultiplier = 2.0f; // 滑行速度倍数
    public float jumpHorizontalBoost = 1.5f; // 跳跃横向距离增强
    public float airControlReduction = 0.3f; // 空中控制减少（更真实的滑行感）
    
    [Header("视觉效果")]
    public float slideParticleRate = 10f; // 滑行粒子效果频率
    
    private float originalDrag;
    private float originalMass;
    private PhysicsMaterial2D originalMaterial;
    private PhysicsMaterial2D iceMaterial;
    private bool isSliding = false;
    private float lastParticleTime;
    
    public override string AbilityTypeId => "IceBlock";
    
    public override void Initialize(PlayerController controller)
    {
        base.Initialize(controller);
        abilityName = "冰块";
        
        var rb = playerController.GetRigidbody();
        originalDrag = rb.drag;
        originalMass = rb.mass;
        
        var collider = playerController.GetBoxCollider();
        originalMaterial = collider.sharedMaterial;
        
        // 创建低摩擦力的物理材质
        CreateIceMaterial();
    }
    
    private void CreateIceMaterial()
    {
        iceMaterial = new PhysicsMaterial2D("IceMaterial");
        iceMaterial.friction = originalMaterial ? originalMaterial.friction * (1f - frictionReduction) : 0.05f;
        iceMaterial.bounciness = originalMaterial ? originalMaterial.bounciness : 0f;
    }
    
    public override void UpdateAbility()
    {
        UpdateSlideState();
        HandleSlideEffects();
    }
    
    public override void FixedUpdateAbility()
    {
        ApplySlidePhysics();
    }
    
    private void UpdateSlideState()
    {
        // 检测是否在滑行（地面移动且有水平输入）
        float horizontal = Input.GetAxis("Horizontal");
        bool wasSliding = isSliding;
        
        isSliding = playerController.IsGrounded && 
                   Mathf.Abs(horizontal) > 0.1f &&
                   Mathf.Abs(playerController.GetVelocity().x) > 1f;
        
        // 滑行状态变化时的处理
        if (isSliding != wasSliding)
        {
            if (isSliding)
            {
                OnStartSliding();
            }
            else
            {
                OnStopSliding();
            }
        }
    }
    
    private void OnStartSliding()
    {
        Debug.Log("[IceBlockAbility] 开始滑行");
        // 可以在这里添加滑行开始的音效和特效
    }
    
    private void OnStopSliding()
    {
        Debug.Log("[IceBlockAbility] 停止滑行");
        // 可以在这里添加滑行停止的音效和特效
    }
    
    private void ApplySlidePhysics()
    {
        if (!playerController.IsGrounded) return;
        
        float horizontal = Input.GetAxis("Horizontal");
        if (Mathf.Abs(horizontal) > 0.1f)
        {
            var rb = playerController.GetRigidbody();
            Vector2 currentVelocity = rb.velocity;
            
            // 增强水平移动速度
            float targetSpeed = horizontal * slideSpeedMultiplier * 
                               (playerController.isRun ? playerController.runSpeed : playerController.walkSpeed);
            
            // 平滑加速到目标速度
            float newXVelocity = Mathf.Lerp(currentVelocity.x, targetSpeed, Time.fixedDeltaTime * 3f);
            rb.velocity = new Vector2(newXVelocity, currentVelocity.y);
        }
    }
    
    private void HandleSlideEffects()
    {
        if (isSliding && Time.time >= lastParticleTime + 1f / slideParticleRate)
        {
            // 生成滑行粒子效果（这里只是占位，需要实际的粒子系统）
            lastParticleTime = Time.time;
            // CreateSlideParticle();
        }
    }
    
    /// <summary>
    /// 修改跳跃力以增加横向距离
    /// </summary>
    public Vector2 ModifyJumpForce(Vector2 originalJumpForce)
    {
        if (!playerController.IsGrounded) return originalJumpForce;
        
        float horizontal = Input.GetAxis("Horizontal");
        if (Mathf.Abs(horizontal) > 0.1f)
        {
            // 增强横向跳跃力
            float enhancedHorizontalForce = horizontal * originalJumpForce.y * jumpHorizontalBoost * 0.3f;
            return new Vector2(enhancedHorizontalForce, originalJumpForce.y);
        }
        
        return originalJumpForce;
    }
    
    public override void OnAbilityActivated()
    {
        ModifyPhysicsProperties();
        Debug.Log($"{abilityName} 能力已激活 - 开始冰面滑行模式");
    }
    
    public override void OnAbilityDeactivated()
    {
        isSliding = false;
        ResetPhysicsProperties();
        Debug.Log($"{abilityName} 能力已禁用");
    }
    
    public override void ModifyPhysicsProperties()
    {
        var rb = playerController.GetRigidbody();
        var collider = playerController.GetBoxCollider();
        
        // 减少阻力以实现滑行效果
        rb.drag = originalDrag * (1f - frictionReduction);
        
        // 应用低摩擦力材质
        collider.sharedMaterial = iceMaterial;
    }
    
    public override void ResetPhysicsProperties()
    {
        var rb = playerController.GetRigidbody();
        var collider = playerController.GetBoxCollider();
        
        // 恢复原始物理属性
        rb.drag = originalDrag;
        collider.sharedMaterial = originalMaterial;
    }
    
    // 公共访问器
    public bool IsSliding => isSliding;
    public float CurrentSlideSpeed => isSliding ? playerController.GetVelocity().magnitude : 0f;
}