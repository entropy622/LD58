using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFramework;
using TMPro;

/// <summary>
/// 游戏进度管理器 - 管理敌人生成、水晶生成和分数系统
/// </summary>
public class SpawnManager : MonoSingleton<SpawnManager>
{
    [Header("游戏设置")]
    public float gameTime = 120f; // 游戏总时间（秒）
    public int enemiesToUpgrade = 5; // 击败多少敌人后升级
    
    [Header("敌人生成设置")]
    public GameObject enemyPrefab;
    public int maxEnemies = 10; // 最大敌人数量
    public float enemySpawnInterval = 2f; // 敌人生成间隔
    public Vector2 spawnAreaMin = new Vector2(-8f, -4f); // 生成区域最小坐标
    public Vector2 spawnAreaMax = new Vector2(8f, 4f); // 生成区域最大坐标
    public float minDistanceFromPlayer = 3f; // 距离玩家的最小距离
    
    [Header("水晶生成设置")]
    public GameObject abilityCrystalPrefab;
    public float crystalSpawnDelay = 1f; // 升级后水晶生成延迟
    
    [Header("UI设置")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI enemiesKilledText;
    
    // 游戏状态
    private float currentTime;
    private int currentScore = 0;
    private int currentLevel = 1;
    private int enemiesKilled = 0;
    private int totalEnemiesKilled = 0;
    private bool gameActive = false;
    
    // 敌人管理
    private List<GameObject> activeEnemies = new List<GameObject>();
    private Coroutine enemySpawnCoroutine;
    
    // 能力管理
    private List<string> availableAbilities = new List<string>();
    private List<string> obtainedAbilities = new List<string>();
    
    // 玩家引用
    private Transform playerTransform;
    private AbilityManager abilityManager;
    
    protected void Awake()
    {
        // 注册事件
        TypeEventSystem.Global.Register<OnEnemyKilledEvent>(OnEnemyKilled).UnRegisterWhenGameObjectDestroyed(gameObject);
        TypeEventSystem.Global.Register<OnAbilityCollectedEvent>(OnAbilityCollected).UnRegisterWhenGameObjectDestroyed(gameObject);
    }
    
    void Start()
    {
        InitializeGame();
    }
    
    void Update()
    {
        if (gameActive)
        {
            UpdateGameTime();
            UpdateUI();
        }
    }
    
    /// <summary>
    /// 初始化游戏
    /// </summary>
    private void InitializeGame()
    {
        // 获取玩家引用
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerTransform == null)
        {
            Debug.LogError("[GameProgressManager] 未找到玩家对象！");
            return;
        }
        
        // 获取能力管理器
        abilityManager = AbilityManager.Instance;
        if (abilityManager != null)
        {
            InitializeAvailableAbilities();
        }
        
        // 初始化游戏状态
        currentTime = gameTime;
        currentScore = 0;
        currentLevel = 1;
        enemiesKilled = 0;
        totalEnemiesKilled = 0;
        gameActive = true;
        
        // 开始生成敌人
        StartEnemySpawning();
        
        // 更新UI
        UpdateUI();
        
        Debug.Log("[GameProgressManager] 游戏开始！");
    }
    
    /// <summary>
    /// 初始化可用能力列表
    /// </summary>
    private void InitializeAvailableAbilities()
    {
        availableAbilities.Clear();
        obtainedAbilities.Clear();
        
        // 从能力管理器获取所有可用能力
        if (abilityManager != null)
        {
            var validAbilities = abilityManager.GetValidAbilityIds();
            availableAbilities.AddRange(validAbilities);
            
            // 排除已装备的能力
            var equippedAbilities = abilityManager.GetEquippedAbilities();
            foreach (string equipped in equippedAbilities)
            {
                if (!string.IsNullOrEmpty(equipped) && availableAbilities.Contains(equipped))
                {
                    availableAbilities.Remove(equipped);
                    obtainedAbilities.Add(equipped);
                }
            }
        }
        else
        {
            // 如果没有能力管理器，使用默认列表
            availableAbilities.AddRange(new string[] 
            {
                "IronBlock", "Balloon", "GravityFlip", "IceBlock", "Shrink"
            });
        }
        
        Debug.Log($"[GameProgressManager] 初始化能力列表，可获取能力: {string.Join(", ", availableAbilities)}");
    }
    
    /// <summary>
    /// 更新游戏时间
    /// </summary>
    private void UpdateGameTime()
    {
        currentTime -= Time.deltaTime;
        
        if (currentTime <= 0)
        {
            currentTime = 0;
            EndGame();
        }
    }
    
    /// <summary>
    /// 更新UI显示
    /// </summary>
    private void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = $"分数: {currentScore}";
            
        if (timeText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60);
            int seconds = Mathf.FloorToInt(currentTime % 60);
            timeText.text = $"时间: {minutes:00}:{seconds:00}";
        }
        
        if (levelText != null)
            levelText.text = $"等级: {currentLevel}";
            
        if (enemiesKilledText != null)
            enemiesKilledText.text = $"击败敌人: {enemiesKilled}/{enemiesToUpgrade}";
    }
    
    #region 敌人生成系统
    
    /// <summary>
    /// 开始生成敌人
    /// </summary>
    private void StartEnemySpawning()
    {
        if (enemySpawnCoroutine != null)
        {
            StopCoroutine(enemySpawnCoroutine);
        }
        
        enemySpawnCoroutine = StartCoroutine(SpawnEnemiesCoroutine());
    }
    
    /// <summary>
    /// 敌人生成协程
    /// </summary>
    private IEnumerator SpawnEnemiesCoroutine()
    {
        while (gameActive)
        {
            // 清理已销毁的敌人
            activeEnemies.RemoveAll(enemy => enemy == null);
            
            // 如果敌人数量未达到上限，生成新敌人
            if (activeEnemies.Count < maxEnemies)
            {
                SpawnEnemy();
            }
            
            yield return new WaitForSeconds(enemySpawnInterval);
        }
    }
    
    /// <summary>
    /// 生成单个敌人
    /// </summary>
    private void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("[GameProgressManager] 敌人预制体未设置！");
            return;
        }
        
        Vector2 spawnPosition = GetValidSpawnPosition();
        
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        
        // 确保敌人有Enemy组件
        if (enemy.GetComponent<Enemy>() == null)
        {
            enemy.AddComponent<Enemy>();
        }
        
        activeEnemies.Add(enemy);
        
        Debug.Log($"[GameProgressManager] 生成敌人于位置: {spawnPosition}");
    }
    
    /// <summary>
    /// 获取有效的生成位置（距离玩家足够远）
    /// </summary>
    private Vector2 GetValidSpawnPosition()
    {
        Vector2 spawnPosition;
        int maxAttempts = 20;
        int attempts = 0;
        
        do
        {
            spawnPosition = new Vector2(
                Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                Random.Range(spawnAreaMin.y, spawnAreaMax.y)
            );
            attempts++;
        }
        while (playerTransform != null && 
               Vector2.Distance(spawnPosition, playerTransform.position) < minDistanceFromPlayer && 
               attempts < maxAttempts);
        
        return spawnPosition;
    }
    
    #endregion
    
    #region 水晶生成系统
    
    /// <summary>
    /// 生成能力水晶
    /// </summary>
    private void SpawnAbilityCrystal()
    {
        if (abilityCrystalPrefab == null)
        {
            Debug.LogWarning("[GameProgressManager] 能力水晶预制体未设置！");
            return;
        }
        
        if (availableAbilities.Count == 0)
        {
            Debug.Log("[GameProgressManager] 没有可获取的能力了！");
            return;
        }
        
        StartCoroutine(SpawnCrystalCoroutine());
    }
    
    /// <summary>
    /// 生成水晶协程
    /// </summary>
    private IEnumerator SpawnCrystalCoroutine()
    {
        yield return new WaitForSeconds(crystalSpawnDelay);
        
        // 随机选择一个可用能力
        string randomAbility = availableAbilities[Random.Range(0, availableAbilities.Count)];
        
        // 获取生成位置
        Vector2 spawnPosition = GetValidSpawnPosition();
        
        // 生成水晶
        GameObject crystal = Instantiate(abilityCrystalPrefab, spawnPosition, Quaternion.identity);
        
        // 设置水晶的能力类型
        AbilityCrystal crystalComponent = crystal.GetComponent<AbilityCrystal>();
        if (crystalComponent != null)
        {
            crystalComponent.abilityTypeId = randomAbility;
        }
        
        Debug.Log($"[GameProgressManager] 生成能力水晶 '{randomAbility}' 于位置: {spawnPosition}");
    }
    
    #endregion
    
    #region 事件处理
    
    /// <summary>
    /// 处理敌人被击败事件
    /// </summary>
    private void OnEnemyKilled(OnEnemyKilledEvent e)
    {
        if (!gameActive) return;
        
        // 增加分数
        currentScore += 10; // 每个敌人10分
        enemiesKilled++;
        totalEnemiesKilled++;
        
        // 从活跃敌人列表中移除
        if (e.enemy != null)
        {
            activeEnemies.Remove(e.enemy);
        }
        
        // 检查是否升级
        if (enemiesKilled >= enemiesToUpgrade)
        {
            LevelUp();
        }
        
        Debug.Log($"[GameProgressManager] 敌人被击败！当前分数: {currentScore}, 击败数: {enemiesKilled}/{enemiesToUpgrade}");
    }
    
    /// <summary>
    /// 处理能力收集事件
    /// </summary>
    private void OnAbilityCollected(OnAbilityCollectedEvent e)
    {
        if (availableAbilities.Contains(e.abilityTypeId))
        {
            availableAbilities.Remove(e.abilityTypeId);
            obtainedAbilities.Add(e.abilityTypeId);
            
            Debug.Log($"[GameProgressManager] 收集到能力: {e.abilityTypeId}");
        }
    }
    
    #endregion
    
    /// <summary>
    /// 升级
    /// </summary>
    private void LevelUp()
    {
        currentLevel++;
        enemiesKilled = 0;
        
        // 增加升级奖励分数
        currentScore += currentLevel * 50;
        
        // 生成能力水晶
        SpawnAbilityCrystal();
        
        // 可选：增加敌人生成速度
        enemySpawnInterval = Mathf.Max(1f, enemySpawnInterval * 0.9f);
        
        Debug.Log($"[GameProgressManager] 升级到等级 {currentLevel}！");
    }
    
    /// <summary>
    /// 结束游戏
    /// </summary>
    private void EndGame()
    {
        gameActive = false;
        
        // 停止生成敌人
        if (enemySpawnCoroutine != null)
        {
            StopCoroutine(enemySpawnCoroutine);
        }
        
        // 显示最终分数
        Debug.Log($"[GameProgressManager] 游戏结束！最终分数: {currentScore}, 总击败敌人: {totalEnemiesKilled}, 最高等级: {currentLevel}");
        
        // 这里可以显示游戏结束UI
        ShowGameOverUI();
    }
    
    /// <summary>
    /// 显示游戏结束UI
    /// </summary>
    private void ShowGameOverUI()
    {
        // TODO: 实现游戏结束UI显示逻辑
        if (UIManager.Instance != null)
        {
            // 可以扩展UIManager来显示游戏结束界面
        }
    }
    
    /// <summary>
    /// 重新开始游戏
    /// </summary>
    public void RestartGame()
    {
        // 清理所有敌人
        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        activeEnemies.Clear();
        
        // 重新初始化
        InitializeGame();
    }
    
    /// <summary>
    /// 获取当前游戏统计信息
    /// </summary>
    public GameStats GetGameStats()
    {
        return new GameStats
        {
            score = currentScore,
            level = currentLevel,
            enemiesKilled = totalEnemiesKilled,
            timeRemaining = currentTime,
            obtainedAbilities = new List<string>(obtainedAbilities)
        };
    }
    
    #region Debug方法
    
    /// <summary>
    /// 在Scene视图中绘制生成区域
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // 绘制敌人生成区域
        Gizmos.color = Color.red;
        Vector3 center = new Vector3((spawnAreaMin.x + spawnAreaMax.x) / 2, (spawnAreaMin.y + spawnAreaMax.y) / 2, 0);
        Vector3 size = new Vector3(spawnAreaMax.x - spawnAreaMin.x, spawnAreaMax.y - spawnAreaMin.y, 0);
        Gizmos.DrawWireCube(center, size);
        
        // 绘制距离玩家的最小距离
        if (playerTransform != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(playerTransform.position, minDistanceFromPlayer);
        }
    }
    
    #endregion
}

/// <summary>
/// 游戏统计信息结构
/// </summary>
[System.Serializable]
public struct GameStats
{
    public int score;
    public int level;
    public int enemiesKilled;
    public float timeRemaining;
    public List<string> obtainedAbilities;
}

/// <summary>
/// 敌人被击败事件
/// </summary>
public struct OnEnemyKilledEvent
{
    public GameObject enemy;
    public Vector3 position;
}