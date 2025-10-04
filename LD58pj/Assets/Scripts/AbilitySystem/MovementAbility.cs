using UnityEngine;

/// <summary>
/// 移动能力 - 控制角色的水平移动
/// </summary>
[System.Serializable]
public class MovementAbility : PlayerAbility
{
    [Header("移动设置")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float pushSpeed = 2f;
    
    private float currentSpeed;
    private bool isRunning;
    
    public override void Initialize(PlayerController controller)
    {
        base.Initialize(controller);
        abilityName = "移动";
    }
    
    public override void UpdateAbility()
    {
        if (!isEnabled) return;
        
        HandleMovementInput();
    }
    
    private void HandleMovementInput()
    {
        float horizontal = Input.GetAxis("Horizontal");
        
        // 检查是否在跑步
        isRunning = Input.GetKey(KeyCode.LeftShift) && 
                   !playerController.IsGetDown && 
                   Mathf.Abs(horizontal) > 0.1f && 
                   playerController.IsGrounded && 
                   !playerController.IsRolling;
        
        // 确定移动速度
        if (playerController.IsPushing)
        {
            currentSpeed = pushSpeed;
        }
        else if (isRunning)
        {
            currentSpeed = runSpeed;
        }
        else if (playerController.IsGetDown)
        {
            currentSpeed = playerController.GetCrouchSpeed();
        }
        else
        {
            currentSpeed = walkSpeed;
        }
        
        // 应用移动
        if (Mathf.Abs(horizontal) > 0.1f)
        {
            playerController.SetVelocity(horizontal * currentSpeed, playerController.GetVelocity().y);
            // 只在移动时更新朝向，避免与主控制器冲突
            // playerController.SetFacing(horizontal > 0 ? 1 : -1);
        }
        else
        {
            playerController.SetVelocity(0, playerController.GetVelocity().y);
        }
        
        // 更新动画状态
        UpdateAnimationState(horizontal);
    }
    
    private void UpdateAnimationState(float horizontal)
    {
        if (Mathf.Abs(horizontal) > 0.1f)
        {
            if (isRunning)
            {
                PlayerAnimatorManager.Instance.SwitchToRun();
            }
            else
            {
                PlayerAnimatorManager.Instance.SwitchToWalk();
            }
        }
        else
        {
            PlayerAnimatorManager.Instance.SwitchToIdle();
        }
    }
    
    public float GetCurrentSpeed() => currentSpeed;
    public bool IsRunning => isRunning;
}