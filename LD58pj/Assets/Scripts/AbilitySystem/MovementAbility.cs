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
    
    private float currentSpeed;
    private bool isRunning;
    
    public override string AbilityTypeId => "Movement";
    
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
        if (isRunning)
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
        
        // 检查是否有缩小能力的速度增强
        if (AbilityManager.Instance.activeAbilities.Contains("Shrink"))
        {
            var shrinkAbility = playerController.GetAbilityByTypeId("Shrink") as ShrinkAbility;
            if (shrinkAbility != null)
            {
                currentSpeed = shrinkAbility.ModifyMovementSpeed(currentSpeed);
            }
        }
        
        // 优化的移动逼辑，解决撞墙停止问题
        Vector2 currentVelocity = playerController.GetVelocity();
        
        if (Mathf.Abs(horizontal) > 0.1f)
        {
            // 检测是否撞墙
            bool isHittingWall = CheckWallCollision(horizontal);
            
            if (!isHittingWall || playerController.IsGrounded)
            {
                // 如果没有撞墙或者在地上，正常移动
                float targetVelocityX = horizontal * currentSpeed;
                
                // 使用平滑过渡，提高手感
                float velocityX = Mathf.Lerp(currentVelocity.x, targetVelocityX, Time.deltaTime * 10f);
                playerController.SetVelocity(velocityX, currentVelocity.y);
            }
            else if (!playerController.IsGrounded)
            {
                // 在空中撞墙时，允许微弱的水平移动，但不影响垂直速度
                float reducedSpeed = horizontal * currentSpeed * 0.3f;
                playerController.SetVelocity(reducedSpeed, currentVelocity.y);
            }
        }
        else
        {
            // 停止移动时的优化，保持垂直速度
            float decelerationRate = playerController.IsGrounded ? 15f : 5f;
            float velocityX = Mathf.Lerp(currentVelocity.x, 0, Time.deltaTime * decelerationRate);
            
            // 当速度很小时直接设为0，避免微小抖动
            if (Mathf.Abs(velocityX) < 0.1f)
                velocityX = 0;
                
            playerController.SetVelocity(velocityX, currentVelocity.y);
        }
        
        // 更新动画状态
        UpdateAnimationState(horizontal);
    }
    
    /// <summary>
    /// 检测是否与墙壁碰撞
    /// </summary>
    private bool CheckWallCollision(float horizontalInput)
    {
        if (playerController == null) return false;
        
        BoxCollider2D collider = playerController.GetBoxCollider();
        if (collider == null) return false;
        
        float checkDistance = 0.1f;
        Vector2 rayOrigin = collider.bounds.center;
        Vector2 rayDirection = horizontalInput > 0 ? Vector2.right : Vector2.left;
        
        // 使用多条射线检测墙壁
        LayerMask wallLayer = LayerMask.GetMask("Ground");
        
        bool hitWall = false;
        for (int i = -1; i <= 1; i++)
        {
            Vector2 rayPos = rayOrigin + Vector2.up * (collider.bounds.extents.y * 0.5f * i);
            RaycastHit2D hit = Physics2D.Raycast(rayPos, rayDirection, 
                                                 collider.bounds.extents.x + checkDistance, wallLayer);
            
            if (hit.collider != null && hit.collider != collider)
            {
                hitWall = true;
                break;
            }
        }
        
        return hitWall;
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