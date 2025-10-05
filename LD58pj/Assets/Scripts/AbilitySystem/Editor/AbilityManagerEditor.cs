using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// AbilityManager的自定义Inspector，提供能力ID的下拉选择功能
/// </summary>
[CustomEditor(typeof(AbilityManager))]
public class AbilityManagerEditor : Editor
{
    private AbilityManager targetManager;
    private List<string> availableAbilityIds = new List<string>();
    private bool showEquippedAbilities = true;
    private bool showActiveAbilities = true;
    private bool showIconConfigs = true;
    
    private void OnEnable()
    {
        targetManager = (AbilityManager)target;
        RefreshAvailableAbilities();
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        // 绘制默认属性（除了能力列表）
        DrawDefaultPropertiesExcludingAbilityLists();
        
        // 刷新可用能力按钮
        EditorGUILayout.Space();
        if (GUILayout.Button("刷新可用能力列表"))
        {
            RefreshAvailableAbilities();
        }
        
        if (availableAbilityIds.Count == 0)
        {
            EditorGUILayout.HelpBox("未找到已注册的能力。请确保PlayerController已在场景中并已初始化。", MessageType.Warning);
        }
        else
        {
            EditorGUILayout.LabelField($"可用能力数量: {availableAbilityIds.Count}");
            EditorGUILayout.LabelField($"可用能力: {string.Join(", ", availableAbilityIds)}");
        }
        
        EditorGUILayout.Space();
        
        // 绘制装备能力列表
        DrawEquippedAbilitiesList();
        
        EditorGUILayout.Space();
        
        // 绘制激活能力列表
        DrawActiveAbilitiesList();
        
        EditorGUILayout.Space();
        
        // 绘制图标配置列表
        DrawIconConfigsList();
        
        // 验证能力ID
        ValidateAbilityIds();
        
        serializedObject.ApplyModifiedProperties();
    }
    
    private void DrawDefaultPropertiesExcludingAbilityLists()
    {
        SerializedProperty prop = serializedObject.GetIterator();
        bool enterChildren = true;
        
        while (prop.NextVisible(enterChildren))
        {
            enterChildren = false;
            
            // 跳过能力列表，我们自定义绘制
            if (prop.name == "equippedAbilities" || prop.name == "activeAbilities")
                continue;
                
            // 跳过脚本引用
            if (prop.name == "m_Script")
                continue;
                
            EditorGUILayout.PropertyField(prop, true);
        }
    }
    
    private void DrawEquippedAbilitiesList()
    {
        showEquippedAbilities = EditorGUILayout.Foldout(showEquippedAbilities, "装备的能力列表", true);
        
        if (showEquippedAbilities)
        {
            EditorGUI.indentLevel++;
            
            // 确保列表大小匹配槽位数量
            while (targetManager.equippedAbilities.Count < targetManager.maxAbilitySlots)
            {
                targetManager.equippedAbilities.Add("");
            }
            while (targetManager.equippedAbilities.Count > targetManager.maxAbilitySlots)
            {
                targetManager.equippedAbilities.RemoveAt(targetManager.equippedAbilities.Count - 1);
            }
            
            for (int i = 0; i < targetManager.equippedAbilities.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"槽位 {i}:", GUILayout.Width(60));
                
                string currentAbility = targetManager.equippedAbilities[i];
                int selectedIndex = GetAbilityIndex(currentAbility);
                
                List<string> options = new List<string> { "无" };
                options.AddRange(availableAbilityIds);
                
                int newIndex = EditorGUILayout.Popup(selectedIndex, options.ToArray());
                
                if (newIndex != selectedIndex)
                {
                    targetManager.equippedAbilities[i] = newIndex == 0 ? "" : availableAbilityIds[newIndex - 1];
                    EditorUtility.SetDirty(targetManager);
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUI.indentLevel--;
        }
    }
    
    private void DrawActiveAbilitiesList()
    {
        showActiveAbilities = EditorGUILayout.Foldout(showActiveAbilities, "激活的能力列表", true);
        
        if (showActiveAbilities)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("数量:", GUILayout.Width(60));
            int newCount = EditorGUILayout.IntField(targetManager.activeAbilities.Count);
            if (newCount != targetManager.activeAbilities.Count)
            {
                while (targetManager.activeAbilities.Count < newCount)
                {
                    targetManager.activeAbilities.Add("");
                }
                while (targetManager.activeAbilities.Count > newCount)
                {
                    targetManager.activeAbilities.RemoveAt(targetManager.activeAbilities.Count - 1);
                }
                EditorUtility.SetDirty(targetManager);
            }
            EditorGUILayout.EndHorizontal();
            
            for (int i = 0; i < targetManager.activeAbilities.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"能力 {i}:", GUILayout.Width(60));
                
                string currentAbility = targetManager.activeAbilities[i];
                int selectedIndex = GetAbilityIndex(currentAbility);
                
                List<string> options = new List<string> { "无" };
                options.AddRange(availableAbilityIds);
                
                int newIndex = EditorGUILayout.Popup(selectedIndex, options.ToArray());
                
                if (newIndex != selectedIndex)
                {
                    targetManager.activeAbilities[i] = newIndex == 0 ? "" : availableAbilityIds[newIndex - 1];
                    EditorUtility.SetDirty(targetManager);
                }
                
                // 删除按钮
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    targetManager.activeAbilities.RemoveAt(i);
                    EditorUtility.SetDirty(targetManager);
                    break;
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            // 添加新能力按钮
            if (GUILayout.Button("添加激活能力"))
            {
                targetManager.activeAbilities.Add("");
                EditorUtility.SetDirty(targetManager);
            }
            
            EditorGUI.indentLevel--;
        }
    }
    
    private int GetAbilityIndex(string abilityId)
    {
        if (string.IsNullOrEmpty(abilityId))
            return 0; // "无"
            
        int index = availableAbilityIds.IndexOf(abilityId);
        return index >= 0 ? index + 1 : 0;
    }
    
    private void RefreshAvailableAbilities()
    {
        availableAbilityIds.Clear();
        
        // 尝试从场景中查找PlayerController
        PlayerController playerController = FindObjectOfType<PlayerController>();
        
        if (playerController != null)
        {
            var allAbilities = playerController.GetAllAbilities();
            if (allAbilities != null)
            {
                availableAbilityIds.AddRange(allAbilities.Keys);
                availableAbilityIds.Sort(); // 排序以便查找
            }
        }
        
        // 如果场景中没有PlayerController，使用默认的能力ID列表
        if (availableAbilityIds.Count == 0)
        {
            availableAbilityIds.AddRange(new string[]
            {
                "Movement", "Jump", "IronBlock", "Balloon", 
                "GravityFlip", "IceBlock", "Shrink"
            });
        }
    }
    
    private void ValidateAbilityIds()
    {
        bool hasInvalidIds = false;
        List<string> invalidIds = new List<string>();
        
        // 验证装备能力
        foreach (string abilityId in targetManager.equippedAbilities)
        {
            if (!string.IsNullOrEmpty(abilityId) && !availableAbilityIds.Contains(abilityId))
            {
                hasInvalidIds = true;
                if (!invalidIds.Contains(abilityId))
                    invalidIds.Add(abilityId);
            }
        }
        
        // 验证激活能力
        foreach (string abilityId in targetManager.activeAbilities)
        {
            if (!string.IsNullOrEmpty(abilityId) && !availableAbilityIds.Contains(abilityId))
            {
                hasInvalidIds = true;
                if (!invalidIds.Contains(abilityId))
                    invalidIds.Add(abilityId);
            }
        }
        
        if (hasInvalidIds)
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                $"发现无效的能力ID: {string.Join(", ", invalidIds)}\n" +
                "这些ID在当前已注册的能力中不存在。请检查配置或点击'刷新可用能力列表'。", 
                MessageType.Error);
                
            if (GUILayout.Button("清除无效的能力ID"))
            {
                CleanInvalidAbilityIds();
            }
        }
    }
    
    private void CleanInvalidAbilityIds()
    {
        // 清理装备能力中的无效ID
        for (int i = 0; i < targetManager.equippedAbilities.Count; i++)
        {
            string abilityId = targetManager.equippedAbilities[i];
            if (!string.IsNullOrEmpty(abilityId) && !availableAbilityIds.Contains(abilityId))
            {
                targetManager.equippedAbilities[i] = "";
            }
        }
        
        // 清理激活能力中的无效ID
        targetManager.activeAbilities.RemoveAll(abilityId => 
            !string.IsNullOrEmpty(abilityId) && !availableAbilityIds.Contains(abilityId));
        
        EditorUtility.SetDirty(targetManager);
        Debug.Log("[AbilityManagerEditor] 已清除所有无效的能力ID");
    }

    private void DrawIconConfigsList()
    {
        showIconConfigs = EditorGUILayout.Foldout(showIconConfigs, "图标配置列表", true);

        if (showIconConfigs)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("数量:", GUILayout.Width(60));
            int newCount = EditorGUILayout.IntField(targetManager.abilityIconConfigs.Count);
            if (newCount != targetManager.abilityIconConfigs.Count)
            {
                while (targetManager.abilityIconConfigs.Count < newCount)
                {
                    targetManager.abilityIconConfigs.Add(new AbilityIconConfig());
                }

                while (targetManager.abilityIconConfigs.Count > newCount)
                {
                    targetManager.abilityIconConfigs.RemoveAt(targetManager.abilityIconConfigs.Count - 1);
                }

                EditorUtility.SetDirty(targetManager);
            }

            EditorGUILayout.EndHorizontal();

            for (int i = 0; i < targetManager.abilityIconConfigs.Count; i++)
            {
                var config = targetManager.abilityIconConfigs[i];

                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"配置 {i}:", EditorStyles.boldLabel, GUILayout.Width(60));

                // 删除按钮
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    targetManager.abilityIconConfigs.RemoveAt(i);
                    EditorUtility.SetDirty(targetManager);
                    break;
                }

                EditorGUILayout.EndHorizontal();

                // 能力ID下拉选择
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("能力ID:", GUILayout.Width(60));

                int selectedIndex = GetAbilityIndex(config.abilityTypeId);
                List<string> options = new List<string> { "无" };
                options.AddRange(availableAbilityIds);

                int newIndex = EditorGUILayout.Popup(selectedIndex, options.ToArray());
                if (newIndex != selectedIndex)
                {
                    config.abilityTypeId = newIndex == 0 ? "" : availableAbilityIds[newIndex - 1];
                    EditorUtility.SetDirty(targetManager);
                }

                EditorGUILayout.EndHorizontal();

                // 显示名称
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("显示名:", GUILayout.Width(60));
                string newDisplayName = EditorGUILayout.TextField(config.displayName);
                if (newDisplayName != config.displayName)
                {
                    config.displayName = newDisplayName;
                    EditorUtility.SetDirty(targetManager);
                }

                EditorGUILayout.EndHorizontal();

                // 图标
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("图标:", GUILayout.Width(60));
                Sprite newIcon = (Sprite)EditorGUILayout.ObjectField(config.icon, typeof(Sprite), false);
                if (newIcon != config.icon)
                {
                    config.icon = newIcon;
                    EditorUtility.SetDirty(targetManager);
                }

                EditorGUILayout.EndHorizontal();

                // 颜色
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("颜色:", GUILayout.Width(60));
                Color newColor = EditorGUILayout.ColorField(config.color);
                if (newColor != config.color)
                {
                    config.color = newColor;
                    EditorUtility.SetDirty(targetManager);
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }

            // 添加新配置按钮
            if (GUILayout.Button("添加图标配置"))
            {
                targetManager.abilityIconConfigs.Add(new AbilityIconConfig());
                EditorUtility.SetDirty(targetManager);
            }

            // 同步按钮
            if (GUILayout.Button("从能力列表同步图标配置"))
            {
                targetManager.SyncAbilityIconConfigurations();
                EditorUtility.SetDirty(targetManager);
            }

            EditorGUI.indentLevel--;
        }
    }
}