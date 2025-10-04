using UnityEngine;

/// <summary>
/// 示例新能力：闪现能力
/// 演示如何轻松添加新能力而不需要修改现有代码
/// </summary>
[System.Serializable]
public class DashAbility : PlayerAbility
{
    [Header("闪现设置")]
    public float dashDistance = 5f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 2f;
    public KeyCode dashKey = KeyCode.LeftControl;
    
    private float lastDashTime;
    private bool isDashing;
    private Vector2 dashDirection;
    private float dashTimer;
    
    // 实现抽象属性 - 这是添加新能力唯一需要的！
    public override string AbilityTypeId => "Dash";
    
    public override void Initialize(PlayerController controller)
    {
        base.Initialize(controller);
        abilityName = "闪现";
        lastDashTime = -dashCooldown; // 确保开始时可以闪现
    }
    
    public override void UpdateAbility()
    {
        if (!isEnabled) return;
        
        HandleDashInput();
        UpdateDashState();
    }
    
    private void HandleDashInput()
    {
        if (Input.GetKeyDown(dashKey) && CanDash())
        {
            StartDash();
        }
    }
    
    private bool CanDash()
    {
        return !isDashing && 
               Time.time >= lastDashTime + dashCooldown &&
               !playerController.IsRolling;
    }
    
    private void StartDash()
    {
        isDashing = true;
        dashTimer = 0f;
        lastDashTime = Time.time;
        
        // 确定闪现方向
        float horizontal = Input.GetAxis("Horizontal");
        if (Mathf.Abs(horizontal) < 0.1f)
        {
            horizontal = playerController.Facing; // 使用角色朝向
        }
        
        dashDirection = new Vector2(horizontal, 0).normalized;
        
        // 播放闪现动画（如果有的话）
        PlayerAnimatorManager.Instance?.SwitchToDash();
        
        Debug.Log("开始闪现！");
    }
    
    public override void FixedUpdateAbility()
    {
        if (!isEnabled || !isDashing) return;
        
        UpdateDashMovement();
    }
    
    private void UpdateDashMovement()
    {
        dashTimer += Time.fixedDeltaTime;
        
        if (dashTimer < dashDuration)
        {
            // 计算闪现速度
            float dashSpeed = dashDistance / dashDuration;
            playerController.SetVelocity(dashDirection.x * dashSpeed, 0);
        }
        else
        {
            EndDash();
        }
    }
    
    private void UpdateDashState()
    {
        if (isDashing && dashTimer >= dashDuration)
        {
            EndDash();
        }
    }
    
    private void EndDash()
    {
        isDashing = false;
        dashTimer = 0f;
        
        // 恢复正常重力
        playerController.SetVelocity(0, playerController.GetVelocity().y);
        
        Debug.Log("闪现结束");
    }
    
    public override void OnAbilityActivated()
    {
        Debug.Log($"{abilityName} 能力已激活");
    }
    
    public override void OnAbilityDeactivated()
    {
        // 如果正在闪现时被禁用，强制结束闪现
        if (isDashing)
        {
            EndDash();
        }
        
        Debug.Log($"{abilityName} 能力已禁用");
    }
    
    // 公共访问器
    public bool IsDashing => isDashing;
    public float CooldownRemaining => Mathf.Max(0, dashCooldown - (Time.time - lastDashTime));
    public bool IsOnCooldown => Time.time < lastDashTime + dashCooldown;
}