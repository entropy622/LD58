using UnityEngine;
using UnityEditor;

/// <summary>
/// PlayerAbility的自定义属性绘制器
/// </summary>
[CustomPropertyDrawer(typeof(PlayerAbility), true)]
public class PlayerAbilityDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        
        // 获取isEnabled属性
        SerializedProperty enabledProp = property.FindPropertyRelative("_isEnabled");
        SerializedProperty nameProp = property.FindPropertyRelative("abilityName");
        
        // 计算位置
        Rect foldoutRect = new Rect(position.x, position.y, position.width - 80, EditorGUIUtility.singleLineHeight);
        Rect toggleRect = new Rect(position.width - 60, position.y, 60, EditorGUIUtility.singleLineHeight);
        
        // 绘制折叠标题
        string displayName = !string.IsNullOrEmpty(nameProp.stringValue) ? nameProp.stringValue : property.displayName;
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, displayName, true);
        
        // 绘制启用开关
        bool oldEnabled = enabledProp.boolValue;
        bool newEnabled = EditorGUI.Toggle(toggleRect, oldEnabled);
        
        if (newEnabled != oldEnabled)
        {
            enabledProp.boolValue = newEnabled;
            
            // 如果在运行时，立即应用改变
            if (Application.isPlaying)
            {
                // 通过反射调用能力的激活/停用方法
                object targetObject = fieldInfo.GetValue(property.serializedObject.targetObject);
                if (targetObject is PlayerAbility ability)
                {
                    if (newEnabled)
                        ability.OnAbilityActivated();
                    else
                        ability.OnAbilityDeactivated();
                }
            }
        }
        
        // 如果展开，绘制子属性
        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;
            
            // 绘制除了isEnabled之外的所有属性
            SerializedProperty iterator = property.Copy();
            iterator.NextVisible(true); // 跳过第一个（父属性）
            
            while (iterator.NextVisible(false))
            {
                if (iterator.name != "_isEnabled") // 跳过isEnabled，已经单独绘制
                {
                    EditorGUILayout.PropertyField(iterator, true);
                }
            }
            
            EditorGUI.indentLevel--;
        }
        
        EditorGUI.EndProperty();
    }
    
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded)
        {
            return EditorGUIUtility.singleLineHeight;
        }
        
        // 计算展开状态下的高度
        float height = EditorGUIUtility.singleLineHeight;
        
        SerializedProperty iterator = property.Copy();
        iterator.NextVisible(true);
        
        while (iterator.NextVisible(false))
        {
            if (iterator.name != "_isEnabled")
            {
                height += EditorGUI.GetPropertyHeight(iterator, true) + EditorGUIUtility.standardVerticalSpacing;
            }
        }
        
        return height;
    }
}