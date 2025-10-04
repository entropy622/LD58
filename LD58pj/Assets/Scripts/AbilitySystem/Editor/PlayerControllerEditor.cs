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
    
    void OnEnable()
    {
        controller = (PlayerController)target;
        movementAbilityProp = serializedObject.FindProperty("_movementAbility");
        jumpAbilityProp = serializedObject.FindProperty("_jumpAbility");
        ironBlockAbilityProp = serializedObject.FindProperty("_ironBlockAbility");
        balloonAbilityProp = serializedObject.FindProperty("_balloonAbility");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        // 绘制默认的Inspector内容（基础组件和设置）
        SerializedProperty prop = serializedObject.GetIterator();
        prop.NextVisible(true);
        while (prop.NextVisible(false))
        {
            if (prop.name.StartsWith("_") && prop.name.Contains("Ability"))
                continue; // 跳过能力属性，由下面特殊处理
            EditorGUILayout.PropertyField(prop, true);
        }
        
        // 应用修改
        serializedObject.ApplyModifiedProperties();
    }
    
    private void DrawAbilitySection(string abilityName, SerializedProperty abilityProp, PlayerAbility ability)
    {
        EditorGUILayout.BeginVertical("box");
        
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
        
        EditorGUILayout.EndHorizontal();
        
        // 能力属性
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(abilityProp, new GUIContent("设置"), true);
        EditorGUI.indentLevel--;
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);
    }
}