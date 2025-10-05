using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManeger : MonoBehaviour
{
    [Header("能力预制体注册表key的key")]
    public List<string> abilityPrefabkeyKeys = new List<string> { "Movement", "Jump", "IronBlock", "IceBlock", "GravityFlip", "Dash", "Balloon" };

    [Header("能力预制体注册表value")]
    public List<GameObject> abilityPrefabValues = new List<GameObject>();

    [Header("UI父节点")]
    public Transform uiParent; // 用于装备的能力UI

    public Transform ui2Parent; // 用于激活的能力UI

    // 合成后的注册表
    public Dictionary<string, GameObject> abilityPrefabRegistry = new Dictionary<string, GameObject>();

    public static UIManeger Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 可选：跨场景保留
        }
        else
        {
            Destroy(gameObject);
        }

        // 初始化能力预制体注册表
        abilityPrefabRegistry.Clear();
        for (int i = 0; i < Mathf.Min(abilityPrefabkeyKeys.Count, abilityPrefabValues.Count); i++)
        {
            if (!string.IsNullOrEmpty(abilityPrefabkeyKeys[i]) && abilityPrefabValues[i] != null)
            {
                abilityPrefabRegistry[abilityPrefabkeyKeys[i]] = abilityPrefabValues[i];
            }
        }
    }

    void Start()
    {
        // ...existing code...
        
    }

    void Update()
    {
        // ...existing code...
    }

    public void ShowEquippedAbilities()
    {
        List<string> equippedAbilities = GetEquippedAbilities();
        //根据equippedAbilities内容，通过abilityPrefabRegistry生成UI
        //  // 请在Inspector中绑定或查找你的UI父节点
        if (uiParent == null)
        {
            Debug.LogWarning("UI父节点未设置，无法生成能力UI");
            return;
        }
        // 清空旧UI
        List<GameObject> children = new List<GameObject>();
        foreach (Transform child in uiParent)
        {
            children.Add(child.gameObject);
        }
        for (int i = 0; i < children.Count; i++)
        {
            Destroy(children[i]);
        }
        // 生成新UI
        foreach (string abilityTypeId in equippedAbilities)
        {
            if (!string.IsNullOrEmpty(abilityTypeId) && abilityPrefabRegistry.ContainsKey(abilityTypeId))
            {
                GameObject prefab = abilityPrefabRegistry[abilityTypeId];
                Instantiate(prefab, uiParent);
            }
        }
    }

    public void ShowActiveAbilities()
    {
        List<string> activeAbilities = GetActiveAbilities();
        //根据activeAbilities内容，通过abilityPrefabRegistry生成UI
         // 请在Inspector中绑定或查找你的UI父节点
        if (ui2Parent == null)
        {
            Debug.LogWarning("UI父节点未设置，无法生成能力UI");
            return;
        }
        // 清空旧UI
        List<GameObject> children = new List<GameObject>();
        foreach (Transform child in ui2Parent)
        {
            children.Add(child.gameObject);
        }
        for (int i = 0; i < children.Count; i++)
        {
            Destroy(children[i]);
        }

        // 生成新UI
        foreach (string abilityTypeId in activeAbilities)
        {
            if (!string.IsNullOrEmpty(abilityTypeId) && abilityPrefabRegistry.ContainsKey(abilityTypeId))
            {
                GameObject prefab = abilityPrefabRegistry[abilityTypeId];
                Instantiate(prefab, ui2Parent);
            }
        }
    }


//本maneger输入和输出只有总计四个输入输出函数
    private List<string> GetEquippedAbilities()
    {
        return AbilityManager.Instance.equippedAbilities;
    }

    private List<string> GetActiveAbilities()
    {
        return AbilityManager.Instance.activeAbilities;
    }

 //public List<string> equippedAbilities = new List<string>(); // 装备的能力列表

    //public List<string> activeAbilities = new List<string>(); // 当前激活的能力列表

}
