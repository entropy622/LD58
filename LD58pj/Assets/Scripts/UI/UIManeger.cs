using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManeger : MonoBehaviour
{
    [Header("能力注册表的key")]
    public List<string> abilityPrefabkeyKeys = new List<string> { "Movement", "Jump", "IronBlock", "IceBlock", "GravityFlip", "Dash", "Balloon" };

    [Header("能力注册表的value")]
    public List<GameObject> abilityPrefabValues = new List<GameObject>();

    [Header("槽位父物体")]
    public List<Transform> abilitySlotParents = new List<Transform>();

    [Header("活跃能力父槽位")]
    public List<Transform> activeAbilityParents = new List<Transform>();



    //这里已经改为非预制体了，请把所有abilityPrefabValues都设置为场景中的物体
    //然后在ShowActiveAbilities中通过SetActive来控制显示和隐藏

    private List<string> lastEquippedAbilities = new List<string>();
    private List<string> lastActiveAbilities = new List<string>();


    // 存储每个父物体的原始子物体   
    private Dictionary<Transform, List<Transform>> abilityOriginalChildren = new Dictionary<Transform, List<Transform>>();
    // 合成后的注册表
    private Dictionary<string, GameObject> abilityPrefabRegistry = new Dictionary<string, GameObject>();

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

        // 存储每个预制体的原始父物体
        changeequippedAbilities();
    }

    void Start()
    {
        lastActiveAbilities = GetActiveAbilities();
        lastEquippedAbilities = GetEquippedAbilities();
        Debug.Log("UIManeger Start" + GetActiveAbilities().Count);
        ShowActiveAbilities();
        changeequippedAbilities();

        // ShowEquippedAbilities();

    }

    void Update()
    {

    
    
        ShowActiveAbilities();

        

        changeequippedAbilities();
            

    
    }




    private void storeabilityOriginalParents()
    {
        abilityOriginalChildren.Clear();
        foreach (Transform parent in abilitySlotParents)
        {
            List<Transform> children = new List<Transform>();
            foreach (Transform child in parent)
            {
                children.Add(child);
            }
            abilityOriginalChildren[parent] = children;
        }
    }


    private void changeequippedAbilities()
    {
        //检测活跃能力父槽位的父类对应的子类和originparents[父槽位]比是否发生了变化
        for (int i = 0; i < activeAbilityParents.Count; i++)
        {
            Transform parent = activeAbilityParents[i];
            //如果父类下有子类
            if (parent.childCount > 0)
            {
                Transform child = parent.GetChild(0);
                //如果原始为空
                if (!abilityOriginalChildren.ContainsKey(parent) || abilityOriginalChildren[parent].Count == 0)
                {
                    //说明是新添加的子类
                    //通过字典反向查找子类对应的key
                    foreach (var kvp in abilityPrefabRegistry)
                    {
                        if (kvp.Value == child.gameObject)
                        {
                            //添加到equippedAbilities中
                            if (!AbilityManager.Instance.equippedAbilities.Contains(kvp.Key))
                            {
                                AbilityManager.Instance.equippedAbilities.Add(kvp.Key);
                                Debug.Log("UIManeger检测到equippedAbilities添加了" + kvp.Key);
                            }
                            break;
                        }
                    }
                    continue;
                }
                //如果原始不为空
                //通过字典反向查找父类的原始子类
                Transform originalChild = abilityOriginalChildren[parent][0];

                //父类的原始子类不是当前子类
                if (originalChild != child)
                {
                    //说明是替换了原始子类
                    //将原始子类从equippedAbilities中移除
                    foreach (var kvp in abilityPrefabRegistry)
                    {
                        if (kvp.Value == originalChild.gameObject)
                        {
                            if (AbilityManager.Instance.equippedAbilities.Contains(kvp.Key))
                            {
                                AbilityManager.Instance.equippedAbilities.Remove(kvp.Key);
                                Debug.Log("UIManeger检测到equippedAbilities移除了" + kvp.Key);
                            }
                            break;
                        }
                    }
                    //通过字典反向查找子类对应的key
                    foreach (var kvp in abilityPrefabRegistry)
                    {
                        if (kvp.Value == child.gameObject)
                        {

                            //添加到equippedAbilities中
                            if (!AbilityManager.Instance.equippedAbilities.Contains(kvp.Key))
                            {
                                AbilityManager.Instance.equippedAbilities.Add(kvp.Key);
                                Debug.Log("UIManeger检测到equippedAbilities添加了" + kvp.Key);
                            }
                            break;
                        }
                    }
                }

            }

        }
        //将原始子类更新为当前子类
        storeabilityOriginalParents();
    
    }



    /*public void ShowEquippedAbilities()
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
    }*/

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
