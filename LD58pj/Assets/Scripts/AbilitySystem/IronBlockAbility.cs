using UnityEngine;

/// <summary>
/// 铁块能力 - 增加重量和密度，减弱跳跃
/// </summary>
[System.Serializable]
public class IronBlockAbility : PlayerAbility
{
    [Header("铁块属性")]
    public float massMultiplier = 3f;
    public float gravityMultiplier = 1f;
    
    private float originalMass;
    private float originalGravityScale;
    
    public override string AbilityTypeId => "IronBlock";
    
    public override void Initialize(PlayerController controller)
    {
        base.Initialize(controller);
        abilityName = "铁块";
        
        // 记录原始属性
        originalMass = playerController.GetRigidbody().mass;
        originalGravityScale = playerController.GetRigidbody().gravityScale;
    }
    
    public override void OnAbilityActivated()
    {
        ModifyPhysicsProperties();
    }
    
    public override void OnAbilityDeactivated()
    {
        ResetPhysicsProperties();
    }
    
    public override void ModifyPhysicsProperties()
    {
        var rb = playerController.GetRigidbody();
        rb.mass = originalMass * massMultiplier;
        rb.gravityScale = originalGravityScale * gravityMultiplier;
        
        // 更改物理材质以增加摩擦力
        var collider = playerController.GetBoxCollider();
        if (collider.sharedMaterial != null)
        {
            var material = collider.sharedMaterial;
            // 可以在这里修改摩擦力等属性
        }
    }
    
    public override void ResetPhysicsProperties()
    {
        var rb = playerController.GetRigidbody();
        rb.mass = originalMass;
        rb.gravityScale = originalGravityScale;
    }
    
    /// <summary>
    /// 检查是否能激活重力踏板
    /// </summary>
    public bool CanActivatePressurePlate()
    {
        return isEnabled;
    }
    
    /// <summary>
    /// 检查是否能沉入水中
    /// </summary>
    public bool CanSinkInWater()
    {
        return isEnabled;
    }
}