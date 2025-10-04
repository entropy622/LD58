using UnityEngine;

/// <summary>
/// Inspector控制功能演示脚本
/// 展示如何通过Inspector面板控制角色能力
/// </summary>
public class InspectorControlExample : MonoBehaviour
{
    [Header("Inspector控制演示")]
    [SerializeField] private PlayerController playerController;
    
    [Header("快捷测试按钮")]
    [SerializeField] private bool testMovementToggle;
    [SerializeField] private bool testJumpToggle;
    [SerializeField] private bool testIronBlockToggle;
    [SerializeField] private bool testBalloonToggle;
    
    private bool lastMovementState;
    private bool lastJumpState;
    private bool lastIronBlockState;
    private bool lastBalloonState;
    
    void Start()
    {
        if (playerController == null)
            playerController = PlayerController.Instance;
            
        // 记录初始状态
        UpdateLastStates();
    }
    
    void Update()
    {
        // 检查测试开关是否改变
        CheckTestToggles();
        
        // 显示当前状态信息
        if (Input.GetKeyDown(KeyCode.I))
        {
            DisplayAbilityStates();
        }
    }
    
    private void CheckTestToggles()
    {
        if (playerController == null) return;
        
        // 检查移动能力开关
        if (testMovementToggle != lastMovementState)
        {
            if (testMovementToggle)
                playerController.EnableAbility<MovementAbility>();
            else
                playerController.DisableAbility<MovementAbility>();
            lastMovementState = testMovementToggle;
            Debug.Log($"移动能力: {(testMovementToggle ? "启用" : "禁用")}");
        }
        
        // 检查跳跃能力开关
        if (testJumpToggle != lastJumpState)
        {
            if (testJumpToggle)
                playerController.EnableAbility<JumpAbility>();
            else
                playerController.DisableAbility<JumpAbility>();
            lastJumpState = testJumpToggle;
            Debug.Log($"跳跃能力: {(testJumpToggle ? "启用" : "禁用")}");
        }
        
        // 检查铁块能力开关
        if (testIronBlockToggle != lastIronBlockState)
        {
            if (testIronBlockToggle)
                playerController.EnableAbility<IronBlockAbility>();
            else
                playerController.DisableAbility<IronBlockAbility>();
            lastIronBlockState = testIronBlockToggle;
            Debug.Log($"铁块能力: {(testIronBlockToggle ? "启用" : "禁用")}");
        }
        
        // 检查气球能力开关
        if (testBalloonToggle != lastBalloonState)
        {
            if (testBalloonToggle)
                playerController.EnableAbility<BalloonAbility>();
            else
                playerController.DisableAbility<BalloonAbility>();
            lastBalloonState = testBalloonToggle;
            Debug.Log($"气球能力: {(testBalloonToggle ? "启用" : "禁用")}");
        }
    }
    
    private void UpdateLastStates()
    {
        if (playerController == null) return;
        
        lastMovementState = playerController.GetAbility<MovementAbility>().isEnabled;
        lastJumpState = playerController.GetAbility<JumpAbility>().isEnabled;
        lastIronBlockState = playerController.GetAbility<IronBlockAbility>().isEnabled;
        lastBalloonState = playerController.GetAbility<BalloonAbility>().isEnabled;
        
        // 同步测试开关状态
        testMovementToggle = lastMovementState;
        testJumpToggle = lastJumpState;
        testIronBlockToggle = lastIronBlockState;
        testBalloonToggle = lastBalloonState;
    }
    
    private void DisplayAbilityStates()
    {
        if (playerController == null) return;
        
        Debug.Log("=== Inspector控制状态 ===");
        Debug.Log($"🚶 移动能力: {(playerController.GetAbility<MovementAbility>().isEnabled ? "✓" : "✗")}");
        Debug.Log($"🦘 跳跃能力: {(playerController.GetAbility<JumpAbility>().isEnabled ? "✓" : "✗")}");
        Debug.Log($"🧱 铁块能力: {(playerController.GetAbility<IronBlockAbility>().isEnabled ? "✓" : "✗")}");
        Debug.Log($"🎈 气球能力: {(playerController.GetAbility<BalloonAbility>().isEnabled ? "✓" : "✗")}");
        
        if (playerController.GetAbility<MovementAbility>().isEnabled)
        {
            var movement = playerController.GetAbility<MovementAbility>();
            Debug.Log($"  - 行走速度: {movement.walkSpeed}");
            Debug.Log($"  - 跑步速度: {movement.runSpeed}");
            Debug.Log($"  - 推动速度: {movement.pushSpeed}");
        }
        
        if (playerController.GetAbility<JumpAbility>().isEnabled)
        {
            var jump = playerController.GetAbility<JumpAbility>();
            Debug.Log($"  - 跳跃力量: {jump.jumpPower}");
            Debug.Log($"  - 土狼时间: {jump.coyoteTime}");
            Debug.Log($"  - 跳跃缓冲: {jump.jumpBufferTime}");
        }
    }
    
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 200, 400, 200));
        GUILayout.Label("Inspector控制演示", GUI.skin.label);
        GUILayout.Label("通过Inspector面板的开关控制角色能力");
        GUILayout.Label("也可以使用下面的测试按钮:");
        GUILayout.Space(10);
        
        if (playerController != null)
        {
            GUILayout.Label($"移动: {(playerController.GetAbility<MovementAbility>().isEnabled ? "✓" : "✗")}");
            GUILayout.Label($"跳跃: {(playerController.GetAbility<JumpAbility>().isEnabled ? "✓" : "✗")}");
            GUILayout.Label($"铁块: {(playerController.GetAbility<IronBlockAbility>().isEnabled ? "✓" : "✗")}");
            GUILayout.Label($"气球: {(playerController.GetAbility<BalloonAbility>().isEnabled ? "✓" : "✗")}");
        }
        
        GUILayout.Label("按 I 键显示详细状态");
        GUILayout.EndArea();
    }
}