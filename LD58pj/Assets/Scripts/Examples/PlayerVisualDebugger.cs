using UnityEngine;

/// <summary>
/// 玩家立绘调试器 - 监控和修复立绘消失问题
/// </summary>
public class PlayerVisualDebugger : MonoBehaviour
{
    [Header("调试设置")]
    public PlayerController playerController;
    public bool enableAutoFix = true;
    public bool showDebugInfo = true;
    
    [Header("监控阈值")]
    public float minValidScale = 0.01f;
    public float maxValidScale = 10f;
    
    private Vector3 lastValidScale = Vector3.one;
    private int fixCount = 0;
    
    void Start()
    {
        if (playerController == null)
            playerController = PlayerController.Instance;
            
        if (playerController != null)
        {
            lastValidScale = playerController.transform.localScale;
        }
    }
    
    void Update()
    {
        if (playerController == null) return;
        
        MonitorScale();
        
        if (enableAutoFix)
        {
            FixInvalidScale();
        }
        
        HandleDebugInput();
    }
    
    private void MonitorScale()
    {
        Vector3 currentScale = playerController.transform.localScale;
        
        // 检查是否有无效的缩放值
        bool hasInvalidScale = false;
        
        if (Mathf.Abs(currentScale.x) < minValidScale || Mathf.Abs(currentScale.x) > maxValidScale)
        {
            hasInvalidScale = true;
            if (showDebugInfo)
                Debug.LogWarning($"检测到无效的X缩放值: {currentScale.x}");
        }
        
        if (Mathf.Abs(currentScale.y) < minValidScale || Mathf.Abs(currentScale.y) > maxValidScale)
        {
            hasInvalidScale = true;
            if (showDebugInfo)
                Debug.LogWarning($"检测到无效的Y缩放值: {currentScale.y}");
        }
        
        if (Mathf.Abs(currentScale.z) < minValidScale || Mathf.Abs(currentScale.z) > maxValidScale)
        {
            hasInvalidScale = true;
            if (showDebugInfo)
                Debug.LogWarning($"检测到无效的Z缩放值: {currentScale.z}");
        }
        
        // 如果缩放值有效，保存为最后的有效缩放
        if (!hasInvalidScale)
        {
            lastValidScale = currentScale;
        }
    }
    
    private void FixInvalidScale()
    {
        Vector3 currentScale = playerController.transform.localScale;
        Vector3 fixedScale = currentScale;
        bool needsFix = false;
        
        // 修复X轴缩放
        if (Mathf.Abs(currentScale.x) < minValidScale)
        {
            fixedScale.x = playerController.Facing > 0 ? Mathf.Abs(lastValidScale.x) : -Mathf.Abs(lastValidScale.x);
            needsFix = true;
        }
        else if (Mathf.Abs(currentScale.x) > maxValidScale)
        {
            fixedScale.x = playerController.Facing > 0 ? 1f : -1f;
            needsFix = true;
        }
        
        // 修复Y轴缩放
        if (Mathf.Abs(currentScale.y) < minValidScale || Mathf.Abs(currentScale.y) > maxValidScale)
        {
            fixedScale.y = Mathf.Abs(lastValidScale.y);
            needsFix = true;
        }
        
        // 修复Z轴缩放
        if (Mathf.Abs(currentScale.z) < minValidScale || Mathf.Abs(currentScale.z) > maxValidScale)
        {
            fixedScale.z = Mathf.Abs(lastValidScale.z);
            needsFix = true;
        }
        
        if (needsFix)
        {
            playerController.transform.localScale = fixedScale;
            fixCount++;
            
            if (showDebugInfo)
            {
                Debug.Log($"已修复无效缩放值: {currentScale} → {fixedScale} (修复次数: {fixCount})");
            }
        }
    }
    
    private void HandleDebugInput()
    {
        // H键：手动修复缩放
        if (Input.GetKeyDown(KeyCode.H))
        {
            ForceFixScale();
        }
        
        // J键：重置缩放为默认值
        if (Input.GetKeyDown(KeyCode.J))
        {
            ResetScaleToDefault();
        }
        
        // K键：显示当前状态
        if (Input.GetKeyDown(KeyCode.K))
        {
            DisplayCurrentStatus();
        }
        
        // L键：切换调试信息显示
        if (Input.GetKeyDown(KeyCode.L))
        {
            showDebugInfo = !showDebugInfo;
            Debug.Log($"调试信息显示: {(showDebugInfo ? "开启" : "关闭")}");
        }
    }
    
    public void ForceFixScale()
    {
        Vector3 currentScale = playerController.transform.localScale;
        Vector3 fixedScale = new Vector3(
            playerController.Facing > 0 ? 1f : -1f,
            1f,
            1f
        );
        
        playerController.transform.localScale = fixedScale;
        lastValidScale = fixedScale;
        
        Debug.Log($"强制修复缩放: {currentScale} → {fixedScale}");
    }
    
    public void ResetScaleToDefault()
    {
        Vector3 defaultScale = new Vector3(
            playerController.Facing > 0 ? 1f : -1f,
            1f,
            1f
        );
        
        playerController.transform.localScale = defaultScale;
        lastValidScale = defaultScale;
        
        Debug.Log($"重置缩放为默认值: {defaultScale}");
    }
    
    private void DisplayCurrentStatus()
    {
        if (playerController == null) return;
        
        Vector3 currentScale = playerController.transform.localScale;
        
        Debug.Log("=== 玩家立绘状态 ===");
        Debug.Log($"当前缩放: {currentScale}");
        Debug.Log($"最后有效缩放: {lastValidScale}");
        Debug.Log($"当前朝向: {(playerController.Facing > 0 ? "右" : "左")} ({playerController.Facing})");
        Debug.Log($"自动修复: {(enableAutoFix ? "开启" : "关闭")}");
        Debug.Log($"修复次数: {fixCount}");
        Debug.Log($"是否在地面: {playerController.IsGrounded}");
        Debug.Log($"是否在移动: {(Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f)}");
    }
    
    void OnGUI()
    {
        if (!showDebugInfo) return;
        
        GUILayout.BeginArea(new Rect(Screen.width - 250, 10, 240, 200));
        GUILayout.Label("立绘调试器", GUI.skin.label);
        
        if (playerController != null)
        {
            Vector3 scale = playerController.transform.localScale;
            GUILayout.Label($"当前缩放: {scale:F3}");
            GUILayout.Label($"朝向: {(playerController.Facing > 0 ? "右" : "左")}");
            GUILayout.Label($"修复次数: {fixCount}");
            
            // 缩放值状态指示
            Color originalColor = GUI.color;
            
            bool isValidX = Mathf.Abs(scale.x) >= minValidScale && Mathf.Abs(scale.x) <= maxValidScale;
            bool isValidY = Mathf.Abs(scale.y) >= minValidScale && Mathf.Abs(scale.y) <= maxValidScale;
            bool isValidZ = Mathf.Abs(scale.z) >= minValidScale && Mathf.Abs(scale.z) <= maxValidScale;
            
            GUI.color = isValidX ? Color.green : Color.red;
            GUILayout.Label($"X轴: {(isValidX ? "正常" : "异常")}");
            
            GUI.color = isValidY ? Color.green : Color.red;
            GUILayout.Label($"Y轴: {(isValidY ? "正常" : "异常")}");
            
            GUI.color = isValidZ ? Color.green : Color.red;
            GUILayout.Label($"Z轴: {(isValidZ ? "正常" : "异常")}");
            
            GUI.color = originalColor;
        }
        
        GUILayout.Space(10);
        GUILayout.Label("快捷键:");
        GUILayout.Label("H - 强制修复");
        GUILayout.Label("J - 重置缩放");
        GUILayout.Label("K - 显示状态");
        GUILayout.Label("L - 切换调试信息");
        
        GUILayout.EndArea();
    }
}