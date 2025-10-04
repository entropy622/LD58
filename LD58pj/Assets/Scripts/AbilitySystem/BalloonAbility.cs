using UnityEngine;

/// <summary>
/// 气球能力 - 滑翔能力，类似奥日的羽毛效果
/// 长按空格键时减缓下落速度，不按时正常下落
/// </summary>
[System.Serializable]
public class BalloonAbility : PlayerAbility
{
    [Header("滑翔属性")]
    public float glideGravityScale = 0.3f; // 滑翔时的重力倍数
    public float glideFallSpeed = 1f; // 滑翔时的最大下降速度
    
    [Header("控制设置")]
    public KeyCode glideKey = KeyCode.Space; // 滑翔键
    
    private float originalGravityScale;
    private bool isGliding; // 是否正在滑翔
    
    public override string AbilityTypeId => "Balloon";
    
    public override void Initialize(PlayerController controller)
    {
        base.Initialize(controller);
        abilityName = "气球";
        
        // 记录原始属性
        originalGravityScale = playerController.GetRigidbody().gravityScale;
    }
    
    public override void UpdateAbility()
    {
        if (!isEnabled) return;
        
        HandleGlideInput();
        ApplyGlideEffect();
    }
    
    public override void OnAbilityActivated()
    {
        // 气球能力激活时不修改物理属性，只在滑翔时才修改
    }
    
    public override void OnAbilityDeactivated()
    {
        ResetPhysicsProperties();
        isGliding = false;
    }
    
    /// <summary>
    /// 处理滑翔输入
    /// </summary>
    private void HandleGlideInput()
    {
        // 检查滑翔输入（长按空格键）
        bool wantsToGlide = (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W)) 
                            && !playerController.IsGrounded;
        if (wantsToGlide && !isGliding)
        {
            StartGlide();
        }
        else if (!wantsToGlide && isGliding)
        {
            StopGlide();
        }
    }
    
    /// <summary>
    /// 开始滑翔
    /// </summary>
    private void StartGlide()
    {
        isGliding = true;
        ModifyPhysicsProperties();
    }
    
    /// <summary>
    /// 停止滑翔
    /// </summary>
    private void StopGlide()
    {
        isGliding = false;
        ResetPhysicsProperties();
    }
    
    /// <summary>
    /// 应用滑翔效果
    /// </summary>
    private void ApplyGlideEffect()
    {
        if (!isGliding) return;
        
        var rb = playerController.GetRigidbody();
        
        // 限制下降速度
        if (rb.velocity.y < -glideFallSpeed)
        {
            rb.velocity = new Vector2(rb.velocity.x, -glideFallSpeed);
        }
    }
    
    public override void ModifyPhysicsProperties()
    {
        var rb = playerController.GetRigidbody();
        // 只在滑翔时修改重力
        rb.gravityScale = originalGravityScale * glideGravityScale;
    }
    
    public override void ResetPhysicsProperties()
    {
        var rb = playerController.GetRigidbody();
        rb.gravityScale = originalGravityScale;
    }
    
    public bool IsGliding => isGliding;
}