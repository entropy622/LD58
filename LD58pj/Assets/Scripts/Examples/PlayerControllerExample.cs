
using UnityEngine;

/// <summary>
/// 示例脚本：展示如何使用新的模块化角色控制器
/// </summary>
public class PlayerControllerExample : MonoBehaviour
{
    [Header("能力控制示例")]
    [SerializeField] private bool enableMovement = true;
    [SerializeField] private bool enableJump = true;
    [SerializeField] private bool enableIronBlock = false;
    [SerializeField] private bool enableBalloon = false;
    
    private PlayerController playerController;
    private AbilityManager abilityManager;
    
    void Start()
    {
        playerController = PlayerController.Instance;
        abilityManager = FindObjectOfType<AbilityManager>();
        ApplyAbilitySettings();
    }
    
    void Update()
    {
        ApplyAbilitySettings();
        HandleAbilityToggleKeys();
        DisplayCurrentStatus();
    }
    
    private void ApplyAbilitySettings()
    {
        if (playerController == null) return;
        
        if (enableMovement && !playerController.GetAbility<MovementAbility>().isEnabled)
            playerController.EnableAbility<MovementAbility>();
        else if (!enableMovement && playerController.GetAbility<MovementAbility>().isEnabled)
            playerController.DisableAbility<MovementAbility>();
            
        if (enableJump && !playerController.GetAbility<JumpAbility>().isEnabled)
            playerController.EnableAbility<JumpAbility>();
        else if (!enableJump && playerController.GetAbility<JumpAbility>().isEnabled)
            playerController.DisableAbility<JumpAbility>();
            
        if (enableIronBlock && !playerController.GetAbility<IronBlockAbility>().isEnabled)
            playerController.EnableAbility<IronBlockAbility>();
        else if (!enableIronBlock && playerController.GetAbility<IronBlockAbility>().isEnabled)
            playerController.DisableAbility<IronBlockAbility>();
            
        if (enableBalloon && !playerController.GetAbility<BalloonAbility>().isEnabled)
            playerController.EnableAbility<BalloonAbility>();
        else if (!enableBalloon && playerController.GetAbility<BalloonAbility>().isEnabled)
            playerController.DisableAbility<BalloonAbility>();
    }
    
    private void HandleAbilityToggleKeys()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            enableMovement = !enableMovement;
            Debug.Log($"移动能力: {(enableMovement ? "开启" : "关闭")}");
        }
        
        if (Input.GetKeyDown(KeyCode.F2))
        {
            enableJump = !enableJump;
            Debug.Log($"跳跃能力: {(enableJump ? "开启" : "关闭")}");
        }
        
        if (Input.GetKeyDown(KeyCode.F3))
        {
            enableIronBlock = !enableIronBlock;
            Debug.Log($"铁块能力: {(enableIronBlock ? "开启" : "关闭")}");
        }
        
        if (Input.GetKeyDown(KeyCode.F4))
        {
            enableBalloon = !enableBalloon;
            Debug.Log($"气球能力: {(enableBalloon ? "开启" : "关闭")}");
        }
    }
    
    private void DisplayCurrentStatus()
    {
        if (Input.GetKeyDown(KeyCode.F5) && playerController != null)
        {
            Debug.Log("=== 角色状态 ===");
            Debug.Log($"是否在地面: {playerController.IsGrounded}");
            Debug.Log($"当前速度: {playerController.GetVelocity()}");
            Debug.Log($"移动能力: {playerController.GetAbility<MovementAbility>().isEnabled}");
            Debug.Log($"跳跃能力: {playerController.GetAbility<JumpAbility>().isEnabled}");
            Debug.Log($"铁块能力: {playerController.GetAbility<IronBlockAbility>().isEnabled}");
            Debug.Log($"气球能力: {playerController.GetAbility<BalloonAbility>().isEnabled}");
        }
    }
    
    // 外部API
    public void SetAbilityEnabled(string abilityName, bool enabled)
    {
        if (playerController == null) return;
        
        switch (abilityName.ToLower())
        {
            case "movement":
                if (enabled) playerController.EnableAbility<MovementAbility>();
                else playerController.DisableAbility<MovementAbility>();
                break;
            case "jump":
                if (enabled) playerController.EnableAbility<JumpAbility>();
                else playerController.DisableAbility<JumpAbility>();
                break;
            case "ironblock":
                if (enabled) playerController.EnableAbility<IronBlockAbility>();
                else playerController.DisableAbility<IronBlockAbility>();
                break;
            case "balloon":
                if (enabled) playerController.EnableAbility<BalloonAbility>();
                else playerController.DisableAbility<BalloonAbility>();
                break;
        }
    }
    
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 150));
        GUILayout.Label("角色能力控制演示");
        GUILayout.Label($"移动: {(enableMovement ? "✓" : "✗")} (F1切换)");
        GUILayout.Label($"跳跃: {(enableJump ? "✓" : "✗")} (F2切换)");
        GUILayout.Label($"铁块: {(enableIronBlock ? "✓" : "✗")} (F3切换)");
        GUILayout.Label($"气球: {(enableBalloon ? "✓" : "✗")} (F4切换)");
        GUILayout.Label("F5 - 显示详细状态");
        GUILayout.EndArea();
    }
}