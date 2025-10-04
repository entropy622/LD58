using UnityEngine;

/// <summary>
/// 重力反转能力 - 反转重力方向
/// </summary>
[System.Serializable]
public class GravityFlipAbility : PlayerAbility
{
    [Header("重力反转设置")]
    public KeyCode flipKey = KeyCode.G;
    public float flipCooldown = 0.5f; // 防止频繁切换
    
    private float lastFlipTime;
    private bool isGravityFlipped = false;
    private float originalGravityScale;
    
    public override string AbilityTypeId => "GravityFlip";
    
    public override void Initialize(PlayerController controller)
    {
        base.Initialize(controller);
        abilityName = "重力反转";
        
        // 记录原始重力
        originalGravityScale = playerController.GetRigidbody().gravityScale;
        lastFlipTime = -flipCooldown; // 确保开始时可以使用
    }
    
    public override void UpdateAbility()
    {
        HandleFlipInput();
    }
    
    private void HandleFlipInput()
    {
        if (Input.GetKeyDown(flipKey) && CanFlip())
        {
            FlipGravity();
        }
    }
    
    private bool CanFlip()
    {
        return Time.time >= lastFlipTime + flipCooldown;
    }
    
    private void FlipGravity()
    {
        isGravityFlipped = !isGravityFlipped;
        lastFlipTime = Time.time;
        
        var rb = playerController.GetRigidbody();
        rb.gravityScale = isGravityFlipped ? -Mathf.Abs(originalGravityScale) : Mathf.Abs(originalGravityScale);
        
        Debug.Log($"[GravityFlipAbility] 重力{(isGravityFlipped ? "反转" : "恢复")}");
        
        // 更新角色朝向（可选，让角色"翻转"更有视觉效果）
        if (isGravityFlipped)
        {
            // 可以在这里添加视觉效果，比如角色翻转动画
            PlayerAnimatorManager.Instance?.ChangeJumpState(true); // 临时使用跳跃动画表示反转状态
        }
        else
        {
            PlayerAnimatorManager.Instance?.ChangeJumpState(false);
        }
    }
    
    public override void OnAbilityActivated()
    {
        Debug.Log($"{abilityName} 能力已激活 - 按 {flipKey} 键反转重力");
    }
    
    public override void OnAbilityDeactivated()
    {
        // 禁用时恢复正常重力
        if (isGravityFlipped)
        {
            FlipGravity(); // 切换回正常状态
        }
        
        Debug.Log($"{abilityName} 能力已禁用");
    }
    
    public override void ModifyPhysicsProperties()
    {
        // 重力反转主要影响gravityScale，已在FlipGravity中处理
    }
    
    public override void ResetPhysicsProperties()
    {
        // 恢复原始重力
        var rb = playerController.GetRigidbody();
        rb.gravityScale = originalGravityScale;
        isGravityFlipped = false;
    }
    
    // 公共访问器
    public bool IsGravityFlipped => isGravityFlipped;
    public float CooldownRemaining => Mathf.Max(0, flipCooldown - (Time.time - lastFlipTime));
}