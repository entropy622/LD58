using UnityEngine;

/// <summary>
/// 气球滑翔能力测试脚本
/// 演示奥日羽毛式的滑翔效果
/// </summary>
public class BalloonGlideTest : MonoBehaviour
{
    [Header("测试设置")]
    public PlayerController playerController;
    public bool enableTestMode = true;
    
    void Start()
    {
        if (playerController == null)
            playerController = PlayerController.Instance;
            
        if (enableTestMode)
        {
            StartCoroutine(DemonstrateGlideEffect());
        }
    }
    
    void Update()
    {
        HandleTestInput();
        DisplayGlideStatus();
    }
    
    private void HandleTestInput()
    {
        // F键：切换气球能力
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (playerController != null)
            {
                playerController.ToggleAbility<BalloonAbility>();
                var balloon = playerController.GetAbility<BalloonAbility>();
                Debug.Log($"气球滑翔能力: {(balloon.isEnabled ? "已启用" : "已禁用")}");
            }
        }
        
        // G键：显示详细状态
        if (Input.GetKeyDown(KeyCode.G))
        {
            ShowDetailedStatus();
        }
    }
    
    private System.Collections.IEnumerator DemonstrateGlideEffect()
    {
        yield return new WaitForSeconds(1f);
        
        Debug.Log("=== 气球滑翔能力演示 ===");
        Debug.Log("效果类似奥日的羽毛：");
        Debug.Log("- 长按空格键：减缓下落速度（滑翔）");
        Debug.Log("- 松开空格键：正常下落");
        Debug.Log("- F键：切换气球能力开关");
        Debug.Log("- G键：显示详细状态");
        
        if (playerController != null)
        {
            // 确保气球能力已启用
            playerController.EnableAbility<BalloonAbility>();
            Debug.Log("气球滑翔能力已启用，可以开始测试！");
        }
    }
    
    private void DisplayGlideStatus()
    {
        if (playerController == null) return;
        
        var balloon = playerController.GetAbility<BalloonAbility>();
        if (balloon == null || !balloon.isEnabled) return;
        
        // 实时显示滑翔状态
        if (balloon.IsGliding)
        {
            // 可以在这里添加视觉提示，比如粒子效果或UI提示
        }
    }
    
    private void ShowDetailedStatus()
    {
        if (playerController == null) return;
        
        var balloon = playerController.GetAbility<BalloonAbility>();
        if (balloon == null)
        {
            Debug.Log("气球能力未找到");
            return;
        }
        
        Debug.Log("=== 气球滑翔能力状态 ===");
        Debug.Log($"能力启用: {balloon.isEnabled}");
        Debug.Log($"正在滑翔: {balloon.IsGliding}");
        Debug.Log($"滑翔重力倍数: {balloon.glideGravityScale}");
        Debug.Log($"滑翔下降速度: {balloon.glideFallSpeed}");
        Debug.Log($"是否在地面: {playerController.IsGrounded}");
        
        var rb = playerController.GetRigidbody();
        Debug.Log($"当前速度: {rb.velocity}");
        Debug.Log($"当前重力: {rb.gravityScale}");
    }
    
    void OnGUI()
    {
        if (!enableTestMode) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("气球滑翔能力测试", GUI.skin.label);
        GUILayout.Space(5);
        
        if (playerController != null)
        {
            var balloon = playerController.GetAbility<BalloonAbility>();
            if (balloon != null)
            {
                GUILayout.Label($"能力状态: {(balloon.isEnabled ? "✓启用" : "✗禁用")}");
                GUILayout.Label($"滑翔状态: {(balloon.IsGliding ? "✓滑翔中" : "✗正常下落")}");
                GUILayout.Label($"是否在地面: {(playerController.IsGrounded ? "是" : "否")}");
                
                var rb = playerController.GetRigidbody();
                GUILayout.Label($"下降速度: {rb.velocity.y:F2}");
            }
        }
        
        GUILayout.Space(10);
        GUILayout.Label("控制说明:");
        GUILayout.Label("长按空格 - 滑翔");
        GUILayout.Label("F键 - 切换能力");
        GUILayout.Label("G键 - 详细状态");
        
        GUILayout.EndArea();
    }
}