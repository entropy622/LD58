using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManeger : MonoBehaviour
{
    [Header("能力注册表的key")]
    public List<string> abilityPrefabkeyKeys = new List<string> { "Movement", "Jump", "IronBlock", "IceBlock", "GravityFlip", "Dash", "Balloon" };

    [Header("能力注册表的value")]
        public List<GameObject> abilityPrefabValues = new List<GameObject>();



    //这里已经改为非预制体了，请把所有abilityPrefabValues都设置为场景中的物体
    //然后在ShowActiveAbilities中通过SetActive来控制显示和隐藏

    private List<string> lastEquippedAbilities = new List<string>();
    private List<string> lastActiveAbilities = new List<string>();

    private Dictionary<GameObject, Vector2> abilityPositions = new Dictionary<GameObject, Vector2>();

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

        InitializePositions();
    }

    void Start()
    {
        lastActiveAbilities = GetActiveAbilities();
        lastEquippedAbilities = GetEquippedAbilities();
        ShowActiveAbilities();
        ShowEquippedAbilities();

    }

    void Update()
    {
        if (activeablitieschanged())
        {
            ShowActiveAbilities();

        }
        if (equippedablitieschanged())
        {
            ShowEquippedAbilities();

        }
    }

    private void InitializePositions()
    {
        // 存储每个 abilityPrefabValues 场景物体的初始位置
        foreach (GameObject obj in abilityPrefabValues)
        {
            if (obj != null && !abilityPositions.ContainsKey(obj))
            {
                abilityPositions[obj] = obj.transform.position;
            }
        }
    }

    private void ResetPositions()
    {
        // 重置每个 abilityPrefabValues 场景物体的位置
        foreach (var kvp in abilityPositions)
        {
            if (kvp.Key != null)
            {
                kvp.Key.transform.position = kvp.Value;
            }
        }
    }





    public void ShowEquippedAbilities()
    {
        List<string> equippedAbilities = GetEquippedAbilities();
        // 先隐藏所有 abilityPrefabValues 场景物体
        foreach (GameObject obj in abilityPrefabValues)
        {
            if (obj != null)
                obj.SetActive(false);
        }
        // 再显示装备的能力物体
        foreach (string abilityTypeId in equippedAbilities)
        {
            if (!string.IsNullOrEmpty(abilityTypeId) && abilityPrefabRegistry.ContainsKey(abilityTypeId))
            {
                GameObject obj = abilityPrefabRegistry[abilityTypeId];
                if (obj != null)
                    obj.SetActive(true);
            }
        }
    }

    public void ShowActiveAbilities()
    {
        List<string> activeAbilities = GetActiveAbilities();
        // 先隐藏所有 abilityPrefabValues 场景物体
        foreach (GameObject obj in abilityPrefabValues)
        {
            if (obj != null)
                obj.SetActive(false);
        }
        // 再显示激活的能力物体
        foreach (string abilityTypeId in activeAbilities)
        {
            if (!string.IsNullOrEmpty(abilityTypeId) && abilityPrefabRegistry.ContainsKey(abilityTypeId))
            {
                GameObject obj = abilityPrefabRegistry[abilityTypeId];
                if (obj != null)
                    obj.SetActive(true);
            }
        }
    }


    //本maneger只有总计六个输入输出函数
    private List<string> GetEquippedAbilities()
    {
        return AbilityManager.Instance.equippedAbilities;
    }

    private List<string> GetActiveAbilities()
    {
        return AbilityManager.Instance.activeAbilities;
    }

    private bool equippedablitieschanged()
    {
        List<string> currentEquippedAbilities = GetEquippedAbilities();
        if (currentEquippedAbilities.Count != lastEquippedAbilities.Count)
        {
            lastEquippedAbilities = new List<string>(currentEquippedAbilities);
            return true;
        }
        for (int i = 0; i < currentEquippedAbilities.Count; i++)
        {
            if (currentEquippedAbilities[i] != lastEquippedAbilities[i])
            {
                lastEquippedAbilities = new List<string>(currentEquippedAbilities);
                return true;
            }
        }
        return false;
    }
    
    private bool activeablitieschanged()
    {
        List<string> currentActiveAbilities = GetActiveAbilities();
        if (currentActiveAbilities.Count != lastActiveAbilities.Count)
        {
            lastActiveAbilities = new List<string>(currentActiveAbilities);
            return true;
        }
        for (int i = 0; i < currentActiveAbilities.Count; i++)
        {
            if (currentActiveAbilities[i] != lastActiveAbilities[i])
            {
                lastActiveAbilities = new List<string>(currentActiveAbilities);
                return true;
            }
        }
        return false;
    }


 //public List<string> equippedAbilities = new List<string>(); // 装备的能力列表

    //public List<string> activeAbilities = new List<string>(); // 当前激活的能力列表

}
