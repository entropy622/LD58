using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 冲刺能力 - 基础模板
/// </summary>
[System.Serializable]
public class DashAbility : PlayerAbility
{
    // 冲刺力度
    public float dashForce = 10f;

    public override string AbilityTypeId => "Dash";

    public override void Initialize(PlayerController controller)
    {
        base.Initialize(controller);
        abilityName = "冲刺";
    }

    public override void UpdateAbility()
    {
        //按下s，检测移动方向，冲刺
        if (!isEnabled) return;
        if (Input.GetKeyDown(KeyCode.S))
        {
            Vector2 dashDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;
            PerformDash(dashDirection);

    
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
        player.GetComponent<Rigidbody2D>().AddForce(direction.normalized * dashForce * 10, ForceMode2D.Impulse);

        // 执行冲刺动作的逻辑
    }


}
