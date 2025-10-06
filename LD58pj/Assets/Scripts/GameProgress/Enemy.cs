using UnityEngine;
using QFramework;

/// <summary>
/// 敌人组件 - 简单的活靶子，被玩家碰撞后消失
/// </summary>
public class Enemy : MonoBehaviour
{
    [Header("敌人设置")]
    public int health = 1;
    public float destroyDelay = 0.1f; // 被击败后的销毁延迟
    
    [Header("视觉效果")]
    public GameObject deathEffect; // 死亡特效预制体
    public ParticleSystem deathParticles; // 死亡粒子系统
    
    [Header("音效设置")]
    public AudioClip[] deathSounds; // 死亡音效组
    [Range(0f, 1f)]
    public float soundVolume = 0.7f; // 音效音量
    
    [Header("粒子效果设置")]
    public Color particleColor = Color.red;
    public int particleCount = 20;
    public float particleLifetime = 1f;
    public float particleSpeed = 5f;
    public bool createParticlesIfMissing = true; // 如果没有配置粒子系统，自动创建
    
    private bool isDead = false;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;

    private Vector2 movementDirection;
    private float movementSpeed;

    //得到testplayer的位置

    [Header("Player")]
    public GameObject player;
    void Start()
    {
        // 获取组件
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        // 如果没有AudioSource，创建一个
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = soundVolume;
        }
        else
        {
            audioSource.volume = soundVolume;
        }

        // 隐藏AudioSource组件在游戏视图中的显示
        if (audioSource != null)
        {
            audioSource.hideFlags = HideFlags.HideInInspector;
        }

        // 确保有碰撞器和Rigidbody2D
        EnsureComponents();
        
        //随机设置movementDirection和movementSpeed
        movementDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        movementSpeed = Random.Range(1f, 20f);
    }

    /// <summary>
    /// 确保必要的组件存在
    /// </summary>

    public void Update()
    {  //有概率改变方向
        if (Random.Range(0f, 1f) < 0.005f)
        {
            movementDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        }
        Move();
    }

    private void Move()
    {
        // 根据方向和速度移动敌人
        transform.Translate(movementDirection * movementSpeed * Time.deltaTime);
    }

    private void EnsureComponents()
    {
        // 确保有碰撞器
        if (GetComponent<Collider2D>() == null)
        {
            var collider = gameObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true; // 设为触发器
        }

        // 确保有Rigidbody2D（静态敌人）
        if (GetComponent<Rigidbody2D>() == null)
        {
            var rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic; // 静态，不受物理影响
            rb.gravityScale = 0;
        }

        // 设置标签
        if (gameObject.tag != "Enemy")
        {
            gameObject.tag = "Enemy";
        }
    }
    
    /// <summary>
    /// 碰撞检测 - 当玩家碰撞时
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;
        
        // 检查是否是玩家
        if (other.CompareTag("Player"))
        {
            TakeDamage(1, other.transform.position);
        }
    }
    
    /// <summary>
    /// 受到伤害
    /// </summary>
    public void TakeDamage(int damage, Vector3 attackerPosition)
    {
        if (isDead) return;
        
        health -= damage;
        
        if (health <= 0)
        {
            Die();
        }
        else
        {
            // 受伤效果（可选）
            OnTakeDamage();
        }
    }
    
    /// <summary>
    /// 受伤时的效果
    /// </summary>
    private void OnTakeDamage()
    {
        // 简单的受伤闪烁效果
        if (spriteRenderer != null)
        {
            StartCoroutine(FlashEffect());
        }
    }
    
    /// <summary>
    /// 闪烁效果协程
    /// </summary>
    private System.Collections.IEnumerator FlashEffect()
    {
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        
        yield return new WaitForSeconds(0.1f);
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }
    
    /// <summary>
    /// 死亡
    /// </summary>
    private void Die()
    {
        if (isDead) return;
        
        isDead = true;
        
        // 发送敌人死亡事件
        TypeEventSystem.Global.Send(new OnEnemyKilledEvent
        {
            enemy = gameObject,
            position = transform.position
        });
        
        // 播放死亡效果
        PlayDeathEffects();
        
        // 禁用碰撞器，防止重复触发
        var collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }
        
        // 延迟销毁
        Destroy(gameObject, destroyDelay);
        
        Debug.Log($"[Enemy] 敌人在位置 {transform.position} 被击败");
    }
    
    /// <summary>
    /// 播放死亡特效和音效
    /// </summary>
    private void PlayDeathEffects()
    {
        // 播放死亡粒子效果
        PlayDeathParticles();
        
        // 播放死亡特效预制体
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, transform.rotation);
        }
        
        // 播放死亡音效
        if (audioSource != null && deathSounds != null && deathSounds.Length > 0)
        {
            // 从音效组中随机选择一个
            AudioClip randomDeathSound = deathSounds[Random.Range(0, deathSounds.Length)];
            if (randomDeathSound != null)
            {
                audioSource.PlayOneShot(randomDeathSound, soundVolume);
                Debug.Log($"[Enemy] 播放死亡音效: {randomDeathSound.name}");
            }
        }
        else if (deathSounds == null || deathSounds.Length == 0)
        {
            Debug.LogWarning("[Enemy] 没有配置死亡音效，跳过音效播放");
        }
        
        // 简单的视觉反馈：快速缩小
        if (spriteRenderer != null)
        {
            StartCoroutine(ShrinkAndFade());
        }
    }
    
    /// <summary>
    /// 播放死亡粒子效果
    /// </summary>
    private void PlayDeathParticles()
    {
        ParticleSystem particles = null;
        
        // 优先使用配置的粒子系统
        if (deathParticles != null)
        {
            particles = deathParticles;
        }
        // 如果没有配置但允许自动创建，则创建一个
        else if (createParticlesIfMissing)
        {
            particles = CreateDeathParticleSystem();
        }
        
        if (particles != null)
        {
            // 设置粒子系统位置
            particles.transform.position = transform.position;
            
            // 播放粒子效果
            particles.Play();
            
            Debug.Log($"[Enemy] 播放死亡粒子效果在位置: {transform.position}");
        }
    }
    
    /// <summary>
    /// 创建默认的死亡粒子系统
    /// </summary>
    private ParticleSystem CreateDeathParticleSystem()
    {
        // 创建一个新的GameObject来承载粒子系统
        GameObject particleGO = new GameObject("DeathParticles");
        particleGO.transform.position = transform.position;
        
        // 添加粒子系统组件
        ParticleSystem ps = particleGO.AddComponent<ParticleSystem>();
        
        // 配置主模块
        var main = ps.main;
        main.startLifetime = particleLifetime;
        main.startSpeed = particleSpeed;
        main.startColor = particleColor;
        main.startSize = 0.1f;
        main.maxParticles = particleCount;
        
        // 配置发射模块
        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0; // 不要持续发射
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0.0f, particleCount) // 在开始时一次性发射所有粒子
        });
        
        // 配置形状模块（爆炸效果）
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.1f;
        
        // 配置速度模块（向外扩散）
        var velocityOverLifetime = ps.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(particleSpeed);
        
        // 配置大小模块（随时间变化）
        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f); // 开始时正常大小
        sizeCurve.AddKey(1f, 0f); // 结束时缩小到0
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        
        // 配置透明度模块（淡出效果）
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(particleColor, 0.0f), new GradientColorKey(particleColor, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
        );
        colorOverLifetime.color = gradient;
        
        // 设置自动销毁
        float totalDuration = main.startLifetime.constantMax + 0.5f; // 粒子生命周期 + 0.5秒缓冲
        Destroy(particleGO, totalDuration);
        
        Debug.Log($"[Enemy] 自动创建死亡粒子系统: {particleCount}个粒子，持续{particleLifetime}秒");
        
        return ps;
    }
    
    /// <summary>
    /// 缩小和淡出效果
    /// </summary>
    private System.Collections.IEnumerator ShrinkAndFade()
    {
        Vector3 originalScale = transform.localScale;
        Color originalColor = spriteRenderer.color;
        
        float duration = destroyDelay * 0.8f; // 在销毁前完成动画
        float elapsed = 0f;
        
        while (elapsed < duration && spriteRenderer != null)
        {
            float progress = elapsed / duration;
            
            // 缩小
            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, progress);
            
            // 淡出
            Color color = originalColor;
            color.a = Mathf.Lerp(1f, 0f, progress);
            spriteRenderer.color = color;
            
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
    
    /// <summary>
    /// 强制销毁敌人（清理用）
    /// </summary>
    public void ForceDestroy()
    {
        if (!isDead)
        {
            isDead = true;
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 在Scene视图中显示调试信息
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // 绘制检测范围
        Gizmos.color = Color.yellow;
        var collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            Gizmos.DrawWireCube(transform.position, collider.bounds.size);
        }
        else
        {
            Gizmos.DrawWireCube(transform.position, Vector3.one);
        }
    }
}