using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFramework;
using TMPro;
using UnityEngine.Serialization;

/// <summary>
/// 游戏进度管理器 - 管理敌人生成、水晶生成和分数系统
/// </summary>
public class SpawnManager : MonoSingleton<SpawnManager>
{
    // ==================== 新增：倒计时功能 ====================
    [Header("倒计时设置")]
    public float gameTimeLimit = 60f; // 游戏总时间（秒）
    public TextMeshProUGUI timerText; // 计时器UI文本引用
    public Color normalTimeColor = Color.white;
    public Color warningTimeColor = Color.red;
    public float warningThreshold = 10f; // 警告阈值（最后10秒）

    private float timerTime;
    private bool isTimerRunning = true;

    // 倒计时事件
    public System.Action OnTimeUp; // 时间结束事件
    public System.Action<float> OnTimeChanged; // 时间变化事件
    // ========================================================

    [Header("游戏设置")]
    public float gameTime = 120f; // 游戏总时间（秒）
    public int enemiesToUpgrade = 5; // 击败多少敌人后升级
    
    [Header("敌人生成设置")]
    public GameObject enemyPrefab;
    public int maxEnemies = 10; // 最大敌人数量
    public float enemySpawnInterval = 2f; // 敌人生成间隔
    public float spawnEnemyAccelerate = 0.9f;
    public Vector2 spawnAreaMin = new Vector2(-8f, -4f); // 生成区域最小坐标
    public Vector2 spawnAreaMax = new Vector2(8f, 4f); // 生成区域最大坐标
    public float minDistanceFromPlayer = 3f; // 距离玩家的最小距离
    
    [Header("地面检测设置")]
    public LayerMask groundLayerMask = -1; // 地面层级掩码，默认检测所有层
    public float groundCheckRadius = 0.5f; // 地面检测半径
    public bool avoidGroundTiles = true; // 是否避免在地面上生成
    
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
        InitializeTimer(); // 初始化计时器
    }

    // ==================== 新增：计时器初始化 ====================
    private void InitializeTimer()
    {
        timerTime = gameTimeLimit;
        UpdateTimerDisplay();

        // 如果没有设置timerText，尝试自动查找
        if (timerText == null)
        {
            timerText = FindObjectOfType<TextMeshProUGUI>();
            if (timerText != null)
            {
                Debug.Log("[AbilityManager] 自动找到TimerText: " + timerText.name);
            }
            else
            {
                Debug.LogWarning("[AbilityManager] 未找到TimerText引用，请在Inspector中手动设置");
            }
        }
    }
    // ========================================================

    void Update()
    {
        if (gameActive)
        {
            UpdateGameTime();
            UpdateUI();
            UpdateTimer();
        }
    }

    // ==================== 新增：计时器核心逻辑 ====================
    private void UpdateTimer()
    {
        if (!isTimerRunning) return;

        timerTime -= Time.deltaTime;
        UpdateTimerDisplay();

        // 触发时间变化事件
        OnTimeChanged?.Invoke(timerTime);

        // 检查时间是否结束
        if (timerTime <= 0f)
        {
            timerTime = 0f;
            TimerEnd();
        }
    }

    private void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            timerText.text = FormatTime(timerTime);

            // 时间警告效果
            if (timerTime <= warningThreshold)
            {
                timerText.color = warningTimeColor;
                // 可以在这里添加闪烁效果
            }
            else
            {
                timerText.color = normalTimeColor;
            }
        }
    }

    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void TimerEnd()
    {
        isTimerRunning = false;
        Debug.Log("时间到！游戏结束");

        // 触发时间结束事件
        OnTimeUp?.Invoke();
        // 结束游戏
        EndGame();
        // 调用结果面板
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowResultPanel(currentScore);
        }
    }

    // ==================== 新增：计时器公共API ====================
    /// <summary>
    /// 暂停计时器
    /// </summary>
    public void PauseTimer()
    {
        isTimerRunning = false;
    }

    /// <summary>
    /// 恢复计时器
    /// </summary>
    public void ResumeTimer()
    {
        isTimerRunning = true;
    }

    /// <summary>
    /// 重置计时器
    /// </summary>
    public void ResetTimer()
    {
        timerTime = gameTimeLimit;
        isTimerRunning = true;
        UpdateTimerDisplay();
    }

    /// <summary>
    /// 添加额外时间
    /// </summary>
    public void AddTime(float extraTime)
    {
        timerTime += extraTime;
        UpdateTimerDisplay();
    }

    /// <summary>
    /// 获取剩余时间
    /// </summary>
    public float GetRemainingTime()
    {
        return timerTime;
    }

    /// <summary>
    /// 检查时间是否已到
    /// </summary>
    public bool IsTimeUp()
    {
        return timerTime <= 0f;
    }
    // ========================================================

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
            scoreText.text = $"Score: {currentScore}";
            
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
    /// 获取有效的生成位置（距离玩家足够远且不在地面上）
    /// </summary>
    private Vector2 GetValidSpawnPosition()
    {
        Vector2 spawnPosition;
        int maxAttempts = 50; // 增加尝试次数，因为增加了地面检测
        int attempts = 0;
        
        do
        {
            spawnPosition = new Vector2(
                Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                Random.Range(spawnAreaMin.y, spawnAreaMax.y)
            );
            attempts++;
            
            // 如果超过最大尝试次数，退出循环防止无限循环
            if (attempts >= maxAttempts)
            {
                Debug.LogWarning($"[SpawnManager] 在 {maxAttempts} 次尝试后未能找到合适的生成位置，使用最后一次生成的位置");
                break;
            }
        }
        while (!IsValidSpawnLocation(spawnPosition));
        
        return spawnPosition;
    }
    
    /// <summary>
    /// 检查指定位置是否适合生成
    /// </summary>
    /// <param name="position">要检查的位置</param>
    /// <returns>是否适合生成</returns>
    private bool IsValidSpawnLocation(Vector2 position)
    {
        // 1. 检查与玩家的距离
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector2.Distance(position, playerTransform.position);
            if (distanceToPlayer < minDistanceFromPlayer)
            {
                return false;
            }
        }
        
        // 2. 检查是否在地面上（如果启用了避免地面生成）
        if (avoidGroundTiles && IsOnGround(position))
        {
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// 检查指定位置是否在地面上（有 "Ground" tag 的 TileMap 或其他物体）
    /// </summary>
    /// <param name="position">要检查的位置</param>
    /// <returns>是否在地面上</returns>
    private bool IsOnGround(Vector2 position)
    {
        // 使用 OverlapCircle 检测指定位置是否有地面碰撞体
        Collider2D groundCollider = Physics2D.OverlapCircle(position, groundCheckRadius, groundLayerMask);
        
        if (groundCollider != null)
        {
            // 检查是否有 "Ground" tag
            if (groundCollider.CompareTag("Ground"))
            {
                return true;
            }
            
            // 可选：检查是否是 TileMap（TileMap 组件通常在父物体上）
            if (groundCollider.GetComponent<UnityEngine.Tilemaps.TilemapCollider2D>() != null ||
                groundCollider.GetComponentInParent<UnityEngine.Tilemaps.TilemapCollider2D>() != null)
            {
                return true;
            }
        }
        
        return false;
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
        
        // 获取生成位置（使用相同的地面检测逻辑）
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
            
            currentScore += 20;
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
        
        // 生成能力水晶
        SpawnAbilityCrystal();
        
        // 可选：增加敌人生成速度
        enemySpawnInterval = Mathf.Max(1f, enemySpawnInterval * spawnEnemyAccelerate);
        
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
    /// 在Scene视图中绘制生成区域和调试信息
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
        
        // 绘制地面检测范围示例（在生成区域内的几个点）
        if (avoidGroundTiles)
        {
            Gizmos.color = Color.yellow;
            
            // 在生成区域内绘制几个地面检测点示例
            for (int x = 0; x < 5; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    Vector2 testPos = new Vector2(
                        Mathf.Lerp(spawnAreaMin.x, spawnAreaMax.x, x / 4f),
                        Mathf.Lerp(spawnAreaMin.y, spawnAreaMax.y, y / 2f)
                    );
                    
                    // 如果该位置在地面上，用红色显示；否则用绿色显示
                    if (Application.isPlaying && IsOnGround(testPos))
                    {
                        Gizmos.color = Color.red;
                    }
                    else
                    {
                        Gizmos.color = Color.green;
                    }
                    
                    Gizmos.DrawWireSphere(testPos, groundCheckRadius);
                }
            }
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