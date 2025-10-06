using UnityEngine;

/// <summary>
/// 弹力球能力 - 提高移速并在撞墙时反弹
/// </summary>
[System.Serializable]
public class BouncyBallAbility : PlayerAbility
{
    [Header("弹力球设置")]
    public float speedMultiplier = 1.5f; // 移速倍数
    public float bounceForce = 12f; // 反弹力
    public float minimumBounceVelocity = 2f; // 最小反弹速度
    public bool enableWallBounce = true; // 是否启用墙壁反弹
    public bool enableGroundBounce = true; // 是否启用地面反弹
    
    [Header("视觉效果")]
    public bool enableBounceEffect = true;
    public Color bounceEffectColor = Color.yellow;
    public float effectDuration = 0.3f;
    
    [Header("动画设置")]
    public bool enableBallAnimation = true;
    
    private bool isBouncing = false;
    private float bounceTimer = 0f;
    private SpriteRenderer playerSpriteRenderer;
    private Color originalColor;
    
    public override string AbilityTypeId => "BouncyBall";
    
    public override void Initialize(PlayerController controller)
    {
        base.Initialize(controller);
        abilityName = "弹力球";
        
        // 获取玩家SpriteRenderer
        playerSpriteRenderer = controller.GetComponent<SpriteRenderer>();
        if (playerSpriteRenderer != null)
        {
            originalColor = playerSpriteRenderer.color;
        }
    }
    
    public override void UpdateAbility()
    {
        if (!isEnabled) return;
        
        UpdateBounceTimer();
        UpdateBallAnimation();
    }
    
    public override void FixedUpdateAbility()
    {
        if (!isEnabled) return;
        
        // 在FixedUpdate中处理物理相关的反弹逻辑
        if (enableWallBounce || enableGroundBounce)
        {
            CheckForBounce();
        }
    }
    
    /// <summary>
    /// 修改移动速度
    /// </summary>
    public float ModifyMovementSpeed(float baseSpeed)
    {
        if (!isEnabled) return baseSpeed;
        return baseSpeed * speedMultiplier;
    }
    
    /// <summary>
    /// 检测碰撞并执行反弹
    /// </summary>
    private void CheckForBounce()
    {
        if (isBouncing) return;
        
        Vector2 velocity = playerController.GetVelocity();
        bool bounced = false;
        
        // 检测墙壁碰撞反弹
        if (enableWallBounce)
        {
            bounced = CheckWallBounce(ref velocity) || bounced;
        }
        
        // 检测地面反弹
        if (enableGroundBounce && !playerController.IsGrounded)
        {
            bounced = CheckGroundBounce(ref velocity) || bounced;
        }
        
        if (bounced)
        {
            // 应用反弹后的速度
            playerController.SetVelocity(velocity.x, velocity.y);
            
            // 触发反弹效果
            OnBounce();
        }
    }
    
    /// <summary>
    /// 检测墙壁反弹
    /// </summary>
    private bool CheckWallBounce(ref Vector2 velocity)
    {
        BoxCollider2D collider = playerController.GetBoxCollider();
        if (collider == null) return false;
        
        // 检查右侧碰撞
        if (velocity.x > minimumBounceVelocity)
        {
            Vector2 rightPoint = new Vector2(collider.bounds.max.x, collider.bounds.center.y);
            if (Physics2D.Raycast(rightPoint, Vector2.right, 0.1f, LayerMask.GetMask("Ground")))
            {
                velocity.x = -bounceForce;
                return true;
            }
        }
        // 检查左侧碰撞
        else if (velocity.x < -minimumBounceVelocity)
        {
            Vector2 leftPoint = new Vector2(collider.bounds.min.x, collider.bounds.center.y);
            if (Physics2D.Raycast(leftPoint, Vector2.left, 0.1f, LayerMask.GetMask("Ground")))
            {
                velocity.x = bounceForce;
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// 检测地面反弹（类似弹球效果）
    /// </summary>
    private bool CheckGroundBounce(ref Vector2 velocity)
    {
        BoxCollider2D collider = playerController.GetBoxCollider();
        if (collider == null) return false;
        
        // 检查底部碰撞并具有向下的速度
        if (velocity.y < -minimumBounceVelocity)
        {
            Vector2 bottomPoint = new Vector2(collider.bounds.center.x, collider.bounds.min.y);
            if (Physics2D.Raycast(bottomPoint, Vector2.down, 0.1f, LayerMask.GetMask("Ground")))
            {
                velocity.y = bounceForce * 0.7f; // 较小的垂直反弹力
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// 反弹时的处理
    /// </summary>
    private void OnBounce()
    {
        isBouncing = true;
        bounceTimer = effectDuration;
        
        // 播放反弹效果
        if (enableBounceEffect)
        {
            PlayBounceEffect();
        }
        
        Debug.Log("[BouncyBall] 角色反弹!");
    }
    
    /// <summary>
    /// 更新反弹计时器
    /// </summary>
    private void UpdateBounceTimer()
    {
        if (isBouncing)
        {
            bounceTimer -= Time.deltaTime;
            if (bounceTimer <= 0)
            {
                isBouncing = false;
                ResetVisualEffects();
            }
        }
    }
    
    /// <summary>
    /// 播放反弹视觉效果
    /// </summary>
    private void PlayBounceEffect()
    {
        if (playerSpriteRenderer == null) return;
        
        // 改变颜色
        playerSpriteRenderer.color = bounceEffectColor;
        
        // 创建粒子效果
        GameObject effectGO = new GameObject("BounceEffect");
        effectGO.transform.position = playerController.transform.position;
        
        ParticleSystem particles = effectGO.AddComponent<ParticleSystem>();
        
        var main = particles.main;
        main.startLifetime = 0.4f;
        main.startSpeed = 4f;
        main.startColor = bounceEffectColor;
        main.startSize = 0.15f;
        main.maxParticles = 20;
        
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
        shape.radius = 0.2f;
        
        var velocityOverLifetime = particles.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(3f);
        
        Object.Destroy(effectGO, 1f);
    }
    
    /// <summary>
    /// 重置视觉效果
    /// </summary>
    private void ResetVisualEffects()
    {
        if (playerSpriteRenderer != null)
        {
            playerSpriteRenderer.color = originalColor;
        }
    }
    
    /// <summary>
    /// 更新球体动画效果
    /// </summary>
    private void UpdateBallAnimation()
    {
        if (!enableBallAnimation || playerSpriteRenderer == null) return;
        
        // 简单的弹跳缩放动画
        if (isBouncing)
        {
            float scale = 1.2f - (bounceTimer / effectDuration) * 0.2f;
            playerController.transform.localScale = new Vector3(scale, 2f - scale, 1f);
        }
        else
        {
            // 恢复正常缩放
            playerController.transform.localScale = Vector3.Lerp(
                playerController.transform.localScale, 
                new Vector3(1f, 1f, 1f), 
                Time.deltaTime * 5f
            );
        }
    }
    
    /// <summary>
    /// 获取反弹力
    /// </summary>
    public float GetBounceForce() => bounceForce;
    
    /// <summary>
    /// 检查是否正在反弹
    /// </summary>
    public bool IsBouncing => isBouncing;
}