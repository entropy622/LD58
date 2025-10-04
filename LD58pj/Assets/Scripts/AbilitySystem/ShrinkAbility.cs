using UnityEngine;

/// <summary>
/// 缩小能力 - 角色缩小到指定比例
/// </summary>
[System.Serializable]
public class ShrinkAbility : PlayerAbility
{
    [Header("缩小设置")]
    [Range(0.1f, 1f)]
    public float shrinkScale = 0.5f; // 缩小到50%
    public float shrinkDuration = 0.3f; // 缩小动画时长
    public bool maintainMass = false; // 是否保持质量（影响物理表现）
    
    [Header("物理属性调整")]
    public float massMultiplier = 1f; // 质量倍数
    public float jumpPowerMultiplier = 0.6f; // 跳跃力倍数（较小体型跳跃力稍弱）
    public float speedMultiplier = 1.2f; // 移动速度倍数（较小体型更灵活）
    
    private Vector3 originalScale;
    private Vector2 originalColliderSize;
    private Vector2 originalColliderOffset;
    private float originalMass;
    private bool isShrunken = false;
    private bool isTransitioning = false;
    
    // 缩放动画相关
    private float transitionTimer = 0f;
    private Vector3 targetScale;
    private Vector2 targetColliderSize;
    
    public override string AbilityTypeId => "Shrink";
    
    public override void Initialize(PlayerController controller)
    {
        base.Initialize(controller);
        abilityName = "缩小";
        
        // 记录原始属性
        originalScale = playerController.transform.localScale;
        
        var collider = playerController.GetBoxCollider();
        originalColliderSize = collider.size;
        originalColliderOffset = collider.offset;
        
        var rb = playerController.GetRigidbody();
        originalMass = rb.mass;
    }
    
    public override void UpdateAbility()
    {
        HandleShrinkTransition();
    }
    
    private void HandleShrinkTransition()
    {
        if (!isTransitioning) return;
        
        transitionTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(transitionTimer / shrinkDuration);
        
        // 使用平滑缓动
        float easedProgress = Mathf.SmoothStep(0f, 1f, progress);
        
        // 插值缩放
        Vector3 currentScale = Vector3.Lerp(originalScale, targetScale, easedProgress);
        // 根据记忆，确保scale.x不为0以防止立绘消失
        if (Mathf.Abs(currentScale.x) < 0.001f) currentScale.x = 0.001f * Mathf.Sign(originalScale.x);
        if (Mathf.Abs(currentScale.y) < 0.001f) currentScale.y = 0.001f;
        
        playerController.transform.localScale = currentScale;
        
        // 插值碰撞器大小
        var collider = playerController.GetBoxCollider();
        collider.size = Vector2.Lerp(originalColliderSize, targetColliderSize, easedProgress);
        
        // 完成过渡
        if (progress >= 1f)
        {
            isTransitioning = false;
            ApplyPhysicsChanges();
            
            Debug.Log($"[ShrinkAbility] 缩放完成 - 当前比例: {(isShrunken ? shrinkScale : 1f):F2}");
        }
    }
    
    private void StartShrinkTransition(bool shrink)
    {
        isShrunken = shrink;
        isTransitioning = true;
        transitionTimer = 0f;
        
        // 设置目标值
        if (shrink)
        {
            targetScale = new Vector3(
                originalScale.x * shrinkScale * Mathf.Sign(playerController.transform.localScale.x), // 保持朝向
                originalScale.y * shrinkScale,
                originalScale.z
            );
            targetColliderSize = originalColliderSize * shrinkScale;
        }
        else
        {
            targetScale = new Vector3(
                originalScale.x * Mathf.Sign(playerController.transform.localScale.x), // 保持朝向
                originalScale.y,
                originalScale.z
            );
            targetColliderSize = originalColliderSize;
        }
        
        Debug.Log($"[ShrinkAbility] 开始{(shrink ? "缩小" : "恢复")}到比例 {(shrink ? shrinkScale : 1f):F2}");
    }
    
    private void ApplyPhysicsChanges()
    {
        var rb = playerController.GetRigidbody();
        
        if (isShrunken)
        {
            // 缩小状态的物理属性
            if (!maintainMass)
            {
                rb.mass = originalMass * massMultiplier;
            }
        }
        else
        {
            // 恢复原始物理属性
            rb.mass = originalMass;
        }
    }
    
    /// <summary>
    /// 修改移动速度（为MovementAbility调用）
    /// </summary>
    public float ModifyMovementSpeed(float originalSpeed)
    {
        return isShrunken ? originalSpeed * speedMultiplier : originalSpeed;
    }
    
    /// <summary>
    /// 修改跳跃力（为JumpAbility调用）
    /// </summary>
    public float ModifyJumpPower(float originalJumpPower)
    {
        return isShrunken ? originalJumpPower * jumpPowerMultiplier : originalJumpPower;
    }
    
    public override void OnAbilityActivated()
    {
        if (!isShrunken && !isTransitioning)
        {
            StartShrinkTransition(true);
        }
        Debug.Log($"{abilityName} 能力已激活");
    }
    
    public override void OnAbilityDeactivated()
    {
        if (isShrunken && !isTransitioning)
        {
            StartShrinkTransition(false);
        }
        Debug.Log($"{abilityName} 能力已禁用");
    }
    
    public override void ModifyPhysicsProperties()
    {
        // 物理属性在过渡完成时应用
        if (!isTransitioning)
        {
            ApplyPhysicsChanges();
        }
    }
    
    public override void ResetPhysicsProperties()
    {
        // 恢复原始属性
        var rb = playerController.GetRigidbody();
        rb.mass = originalMass;
        
        var collider = playerController.GetBoxCollider();
        collider.size = originalColliderSize;
        collider.offset = originalColliderOffset;
        
        // 恢复原始缩放，确保不为0
        Vector3 resetScale = originalScale;
        if (Mathf.Abs(resetScale.x) < 0.001f) resetScale.x = 0.001f;
        if (Mathf.Abs(resetScale.y) < 0.001f) resetScale.y = 0.001f;
        
        playerController.transform.localScale = resetScale;
        
        isShrunken = false;
        isTransitioning = false;
    }
    
    // 公共访问器
    public bool IsShrunken => isShrunken;
    public bool IsTransitioning => isTransitioning;
    public float CurrentScale => isShrunken ? shrinkScale : 1f;
    public float TransitionProgress => isTransitioning ? transitionTimer / shrinkDuration : (isShrunken ? 1f : 0f);
}