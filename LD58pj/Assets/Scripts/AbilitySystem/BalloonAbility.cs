using UnityEngine;

/// <summary>
/// 气球能力 - 降低密度，缓慢下落或漂浮
/// </summary>
[System.Serializable]
public class BalloonAbility : PlayerAbility
{
    [Header("气球属性")]
    public float massReduction = 0.3f; // 质量减少到30%
    public float gravityReduction = 0.2f; // 重力减少到20%
    public float floatForce = 2f; // 漂浮力
    public float slowFallMultiplier = 0.3f; // 慢速下落倍数
    
    [Header("控制设置")]
    public KeyCode floatKey = KeyCode.Space; // 漂浮键
    
    private float originalMass;
    private float originalGravityScale;
    private bool isFloating;
    
    public override void Initialize(PlayerController controller)
    {
        base.Initialize(controller);
        abilityName = "气球";
        
        // 记录原始属性
        originalMass = playerController.GetRigidbody().mass;
        originalGravityScale = playerController.GetRigidbody().gravityScale;
    }
    
    public override void UpdateAbility()
    {
        if (!isEnabled) return;
        
        HandleFloatInput();
        ApplySlowFall();
    }
    
    public override void OnAbilityActivated()
    {
        ModifyPhysicsProperties();
    }
    
    public override void OnAbilityDeactivated()
    {
        ResetPhysicsProperties();
        isFloating = false;
    }
    
    private void HandleFloatInput()
    {
        // 检查漂浮输入（长按空格键）
        if (Input.GetKey(floatKey) && !playerController.IsGrounded)
        {
            isFloating = true;
            ApplyFloatForce();
        }
        else
        {
            isFloating = false;
        }
    }
    
    private void ApplyFloatForce()
    {
        var rb = playerController.GetRigidbody();
        
        // 应用向上的漂浮力
        rb.AddForce(Vector2.up * floatForce, ForceMode2D.Force);
        
        // 限制上升速度
        if (rb.velocity.y > 3f)
        {
            rb.velocity = new Vector2(rb.velocity.x, 3f);
        }
    }
    
    private void ApplySlowFall()
    {
        var rb = playerController.GetRigidbody();
        
        // 如果正在下落且速度过快，应用慢速下落
        if (rb.velocity.y < -2f && !playerController.IsGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * slowFallMultiplier);
        }
    }
    
    public override void ModifyPhysicsProperties()
    {
        var rb = playerController.GetRigidbody();
        rb.mass = originalMass * massReduction;
        rb.gravityScale = originalGravityScale * gravityReduction;
    }
    
    public override void ResetPhysicsProperties()
    {
        var rb = playerController.GetRigidbody();
        rb.mass = originalMass;
        rb.gravityScale = originalGravityScale;
    }
    
    public bool IsFloating => isFloating;
}