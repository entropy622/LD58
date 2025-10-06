using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerController))]
public class PlayerControllerEditor : Editor
{
    private PlayerController controller;
    private SerializedProperty movementAbilityProp;
    private SerializedProperty jumpAbilityProp;
    private SerializedProperty ironBlockAbilityProp;
    private SerializedProperty balloonAbilityProp;
    private SerializedProperty gravityFlipAbilityProp;
    private SerializedProperty iceBlockAbilityProp;

    private SerializedProperty dashAbilityProp;
    // private SerializedProperty shrinkAbilityProp;
    
    // 缓存上一次的参数值，用于检测变化
    private bool hasChanged = false;

    void OnEnable()
    {
        controller = (PlayerController)target;
        movementAbilityProp = serializedObject.FindProperty("_movementAbility");
        jumpAbilityProp = serializedObject.FindProperty("_jumpAbility");
        ironBlockAbilityProp = serializedObject.FindProperty("_ironBlockAbility");
        balloonAbilityProp = serializedObject.FindProperty("_balloonAbility");
        gravityFlipAbilityProp = serializedObject.FindProperty("_gravityFlipAbility");
        iceBlockAbilityProp = serializedObject.FindProperty("_iceBlockAbility");
        dashAbilityProp = serializedObject.FindProperty("_dashAbility");
        // shrinkAbilityProp = serializedObject.FindProperty("_shrinkAbility");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        // 记录更新前的GUI状态
        EditorGUI.BeginChangeCheck();
        
        // 绘制默认的Inspector内容（基础组件和设置）
        SerializedProperty prop = serializedObject.GetIterator();
        prop.NextVisible(true);
        while (prop.NextVisible(false))
        {
            if (prop.name.StartsWith("_") && prop.name.Contains("Ability"))
                continue; // 跳过能力属性，由下面特殊处理
            EditorGUILayout.PropertyField(prop, true);
        }
        
        // 添加分隔线
        EditorGUILayout.Space(15);
        EditorGUILayout.LabelField("能力系统控制", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);
        
        // 能力开关控制
        DrawAbilitySection("😶 移动能力", movementAbilityProp, controller.movementAbility);
        DrawAbilitySection("🦘 跳跃能力", jumpAbilityProp, controller.jumpAbility);
        DrawAbilitySection("🧱 铁块能力", ironBlockAbilityProp, controller.ironBlockAbility);
        DrawAbilitySection("🎈 气球能力", balloonAbilityProp, controller.balloonAbility);
        DrawAbilitySection("👻 翻转能力", gravityFlipAbilityProp, controller.gravityFlipAbility);
        DrawAbilitySection("氷 冰块能力", iceBlockAbilityProp, controller.iceBlockAbility);
        DrawAbilitySection("Dash 闪现能力", dashAbilityProp, controller.dashAbility);
        // DrawAbilitySection("縮 小能力", shrinkAbilityProp, controller.shrinkAbility);

        // 检测是否有变化
        if (EditorGUI.EndChangeCheck())
        {
            hasChanged = true;
        }
        
        // 应用修改
        if (serializedObject.ApplyModifiedProperties() || hasChanged)
        {
            // 在运行时即时更新能力参数
            if (Application.isPlaying)
            {
                RefreshAbilitiesInRuntime();
            }
            
            // 标记对象为脏状态，确保保存
            EditorUtility.SetDirty(controller);
            hasChanged = false;
        }
    }
    
    /// <summary>
    /// 在运行时刷新能力参数
    /// </summary>
    private void RefreshAbilitiesInRuntime()
    {
        if (controller == null) return;
        
        // 强制重新初始化所有能力
        controller.movementAbility?.Initialize(controller);
        controller.jumpAbility?.Initialize(controller);
        controller.ironBlockAbility?.Initialize(controller);
        controller.balloonAbility?.Initialize(controller);
        controller.gravityFlipAbility?.Initialize(controller);
        controller.iceBlockAbility?.Initialize(controller);
        controller.dashAbility?.Initialize(controller);
        // controller.shrinkAbility?.Initialize(controller);
        
        // 同步AbilityManager的状态
        var abilityManager = controller.GetAbilityManager();
        if (abilityManager != null)
        {
            abilityManager.SyncAbilityStates();
        }
        
        Debug.Log("[PlayerControllerEditor] 已刷新运行时能力参数");
    }
    
    private void DrawAbilitySection(string abilityName, SerializedProperty abilityProp, PlayerAbility ability)
    {
        EditorGUILayout.BeginVertical("box");
        
        // 检测参数变化
        EditorGUI.BeginChangeCheck();
        
        // 标题和开关
        EditorGUILayout.BeginHorizontal();
        
        EditorGUILayout.LabelField(abilityName, EditorStyles.boldLabel, GUILayout.Width(120));
        
        // 状态指示器
        string statusText = ability.isEnabled ? "✓ 已启用" : "✗ 已禁用";
        Color statusColor = ability.isEnabled ? Color.green : Color.red;
        
        GUIStyle statusStyle = new GUIStyle(EditorStyles.label);
        statusStyle.normal.textColor = statusColor;
        statusStyle.fontStyle = FontStyle.Bold;
        
        EditorGUILayout.LabelField(statusText, statusStyle, GUILayout.Width(80));
        
        // 在运行时显示刷新按钮
        if (Application.isPlaying)
        {
            if (GUILayout.Button("🔄", GUILayout.Width(25)))
            {
                // 立即刷新这个能力
                ability.Initialize(controller);
                Debug.Log($"[PlayerControllerEditor] 已刷新 {abilityName}");
            }
        }
        
        EditorGUILayout.EndHorizontal();
        
        // 能力属性
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(abilityProp, new GUIContent("设置"), true);
        EditorGUI.indentLevel--;
        
        // 检测这个能力的参数是否变化
        if (EditorGUI.EndChangeCheck())
        {
            // 在运行时即时应用变化
            if (Application.isPlaying)
            {
                ability.Initialize(controller);
                
                // 同步到AbilityManager
                var abilityManager = controller.GetAbilityManager();
                if (abilityManager != null)
                {
                    abilityManager.SyncAbilityStates();
                }
            }
            
            // 标记为有变化
            hasChanged = true;
        }
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);
    }
}