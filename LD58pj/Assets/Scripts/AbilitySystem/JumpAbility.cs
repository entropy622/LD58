using UnityEngine;

/// <summary>
/// 跳跃能力 - 控制角色的跳跃行为
/// </summary>
[System.Serializable]
public class JumpAbility : PlayerAbility
{
    [Header("跳跃设置")]
    public float jumpPower = 10f;
    public float coyoteTime = 0.1f; // 土狼时间
    public float jumpBufferTime = 0.1f; // 跳跃缓冲时间
    
    private float lastGroundedTime;
    private float lastJumpPressedTime;
    private bool isJumping;
    
    public override string AbilityTypeId => "Jump";
    
    public override void Initialize(PlayerController controller)
    {
        base.Initialize(controller);
        abilityName = "跳跃";
    }
    
    public override void UpdateAbility()
    {
        if (!isEnabled) return;
        
        HandleJumpInput();
        UpdateTimers();
    }
    
    private void HandleJumpInput()
    {
        // 记录跳跃输入时间
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W))
        {
            lastJumpPressedTime = Time.time;
        }
        
        // 检查是否可以跳跃
        bool canJump = (Time.time - lastGroundedTime) <= coyoteTime && 
                      (Time.time - lastJumpPressedTime) <= jumpBufferTime &&
                      !playerController.IsGetDown;
        
        if (canJump && !isJumping)
        {
            PerformJump();
        }
        
        // 检查跳跃状态
        if (isJumping && playerController.IsGrounded && playerController.GetVelocity().y <= 0.1f)
        {
            isJumping = false;
            PlayerAnimatorManager.Instance.ChangeJumpState(false);
        }
    }
    
    private void PerformJump()
    {
        // 获取修改后的跳跃力（可能被其他能力影响）
        float modifiedJumpPower = GetModifiedJumpPower();
        
        playerController.AddForce(Vector2.up * modifiedJumpPower, ForceMode2D.Impulse);
        isJumping = true;
        
        // 重置计时器，避免重复跳跃
        lastJumpPressedTime = 0f;
        lastGroundedTime = 0f;
        
        PlayerAnimatorManager.Instance.ChangeJumpState(true);
    }
    
    private void UpdateTimers()
    {
        if (playerController.IsGrounded)
        {
            lastGroundedTime = Time.time;
        }
    }
    
    /// <summary>
    /// 获取修改后的跳跃力，可能受其他能力影响
    /// </summary>
    private float GetModifiedJumpPower()
    {
        float modifiedPower = jumpPower;
        
        // 检查是否有铁块能力影响
        if (AbilityManager.Instance.activeAbilities.Contains("IronBlock"))
        {
            modifiedPower *= 0.6f; // 铁块状态下跳跃力减弱
        }
        
        return modifiedPower;
    }
    
    public bool IsJumping => isJumping;
}