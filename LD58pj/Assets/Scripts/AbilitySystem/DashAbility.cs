using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 冲刺能力 - 基础模板
/// </summary>
[System.Serializable]
public class DashAbility : PlayerAbility
{
    // 冲刺力度
    public float dashForce = 10f;

    // 冲刺冷却时间（秒）
    public float dashCooldown = 1f;
    // 上次冲刺时间
    private float lastDashTime = -999f;

    public override string AbilityTypeId => "Dash";

    public override void Initialize(PlayerController controller)
    {
        base.Initialize(controller);
        abilityName = "冲刺";
    }

    public override void UpdateAbility()
    {
        Debug.Log("spiriteRenderer.flipX: " + playerController.GetComponent<SpriteRenderer>().flipX);
        //按下shift，检测移动方向，冲刺
        if (!isEnabled) return;
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {

            if (Time.time - lastDashTime >= dashCooldown)
            {
                //只能左右冲刺，获取冲刺时左右朝向
                //获取player贴图的左右的朝向
                bool isFacingLeft = playerController.gameObject.transform.localScale.x < 0;
                Vector2 dashDirection = isFacingLeft ? Vector2.left : Vector2.right;

                PerformDash(dashDirection);
                lastDashTime = Time.time; // 记录冲刺时间
            }
            // else 可以加提示：冷却中
        }

        // 冲刺能力每帧逻辑
    }

    public override void FixedUpdateAbility()
    {
        // 冲刺能力物理帧逻辑
    }

    public override void OnAbilityActivated()
    {
        // 冲刺能力激活时逻辑
    }

    public override void OnAbilityDeactivated()
    {
        // 冲刺能力停用时逻辑
    }

    public void PerformDash(Vector2 direction)
    {
        if (!isEnabled) return;
        GameObject player = playerController.gameObject;
        // 在这里添加冲刺逻辑，例如应用一个瞬时的速度变化
        player.GetComponent<Rigidbody2D>().AddForce(direction.normalized * dashForce * 5, ForceMode2D.Impulse);

        // 执行冲刺动作的逻辑
    }


}
