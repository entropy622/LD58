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
    public GameObject deathEffect; // 死亡特效
    public AudioClip deathSound; // 死亡音效
    
    private bool isDead = false;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    
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
        }
        
        // 确保有碰撞器和Rigidbody2D
        EnsureComponents();
    }
    
    /// <summary>
    /// 确保必要的组件存在
    /// </summary>
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
        // 播放死亡特效
        if (deathEffect != null)
        {
            GameObject effect = Instantiate(deathEffect, transform.position, transform.rotation);
            
            // 自动销毁特效（如果没有自己的销毁逻辑）
            var particleSystem = effect.GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                Destroy(effect, particleSystem.main.duration + particleSystem.main.startLifetime.constantMax);
            }
            else
            {
                Destroy(effect, 2f); // 默认2秒后销毁
            }
        }
        
        // 播放死亡音效
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
        
        // 简单的视觉反馈：快速缩小
        if (spriteRenderer != null)
        {
            StartCoroutine(ShrinkAndFade());
        }
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