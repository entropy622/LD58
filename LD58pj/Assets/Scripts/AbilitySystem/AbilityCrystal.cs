using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFramework;

public class AbilityCrystal : MonoBehaviour
{
    [Header("能力配置")]
    public string abilityTypeId;
    
    [Header("视觉效果")]
    public ParticleSystem collectParticles;
    public GameObject visualMesh; // 水晶的视觉模型
    public AudioClip collectSound;
    
    [Header("动画设置")]
    public float floatAmplitude = 0.3f;
    public float floatSpeed = 2f;
    public float rotationSpeed = 30f;
    public float collectAnimationDuration = 0.5f;
    
    private AbilityManager abilityManager;
    private PlayerAbility ability;
    private bool isCollected = false;
    private Vector3 originalPosition;
    private AudioSource audioSource;
    
    void Start()
    {
        abilityManager = AbilityManager.Instance;
        
        // 验证能力ID的有效性
        if (abilityManager == null)
        {
            Debug.LogError("[AbilityCrystal] AbilityManager实例不存在");
            return;
        }
        
        if (!abilityManager.IsValidAbilityId(abilityTypeId))
        {
            Debug.LogWarning($"[AbilityCrystal] 无效的能力ID: {abilityTypeId}");
        }

        // 设置水晶外观（优先使用图标配置系统）
        SetCrystalAppearance();
        
        // 初始化组件
        InitializeComponents();
        
        // 记录初始位置用于浮动动画
        originalPosition = transform.position;
    }
    
    /// <summary>
    /// 设置水晶外观（图标和颜色）
    /// </summary>
    private void SetCrystalAppearance()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogWarning("[AbilityCrystal] 找不到SpriteRenderer组件");
            return;
        }
        
        // 优先使用图标配置系统
        var iconConfig = abilityManager.GetAbilityIconConfig(abilityTypeId);
        if (iconConfig != null && iconConfig.icon != null)
        {
            spriteRenderer.sprite = iconConfig.icon;
            spriteRenderer.color = iconConfig.color;
            Debug.Log($"[AbilityCrystal] 使用图标配置设置水晶外观: {abilityTypeId}");
            return;
        }
        
        // 备用方案：从 playerAbilities 获取
        ability = abilityManager.playerAbilities.Find(x => x.AbilityTypeId == abilityTypeId);
        if (ability != null && ability.icon != null)
        {
            spriteRenderer.sprite = ability.icon;
            spriteRenderer.color = ability.color;
            Debug.Log($"[AbilityCrystal] 使用PlayerAbility设置水晶外观: {abilityTypeId}");
            return;
        }
        
        // 都没有找到，使用默认外观
        Debug.LogWarning($"[AbilityCrystal] 无法为能力ID {abilityTypeId} 找到图标配置，使用默认外观");
        spriteRenderer.color = Color.white; // 默认颜色
    }
    
    void Update()
    {
        // if (!isCollected)
        // {
        //     // 浮动和旋转动画
        //     AnimateCrystal();
        // }
    }
    
    /// <summary>
    /// 初始化组件
    /// </summary>
    private void InitializeComponents()
    {
        // 获取或创建AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = 0.7f;
        }
        
        // 如果没有指定视觉模型，使用自身
        if (visualMesh == null)
        {
            visualMesh = gameObject;
        }
        
        // 如果没有粒子系统，尝试从子对象获取
        if (collectParticles == null)
        {
            collectParticles = GetComponentInChildren<ParticleSystem>();
        }
    }
    
    /// <summary>
    /// 水晶浮动和旋转动画
    /// </summary>
    // private void AnimateCrystal()
    // {
    //     // 浮动动画
    //     float floatY = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
    //     transform.position = originalPosition + Vector3.up * floatY;
    //     
    //     // 旋转动画
    //     transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    //     
    //     // 可选：脉冲缩放效果
    //     float pulseScale = 1f + Mathf.Sin(Time.time * floatSpeed * 2f) * 0.05f;
    //     if (visualMesh != null)
    //     {
    //         visualMesh.transform.localScale = Vector3.one * pulseScale;
    //     }
    // }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isCollected)
        {
            StartCoroutine(CollectCrystal());
        }
    }
    
    /// <summary>
    /// 水晶收集效果协程
    /// </summary>
    private IEnumerator CollectCrystal()
    {
        isCollected = true;
        
        // 禁用碰撞器，防止重复触发
        GetComponent<Collider2D>().enabled = false;
        
        // 播放收集音效
        PlayCollectSound();
        
        // 激活能力
        if (abilityManager != null && !string.IsNullOrEmpty(abilityTypeId))
        {
            bool success = abilityManager.SafeActivateAbility(abilityTypeId);
            if (success)
            {
                Debug.Log($"[AbilityCrystal] 成功激活能力: {abilityTypeId}");
                
                // 发送能力获得事件
                TypeEventSystem.Global.Send(new OnAbilityCollectedEvent
                {
                    abilityTypeId = abilityTypeId,
                    crystalPosition = transform.position
                });
            }
            else
            {
                Debug.LogError($"[AbilityCrystal] 激活能力失败: {abilityTypeId}");
            }
        }
        
        // 播放粒子效果
        PlayCollectParticles();
        
        // 收集动画
        yield return StartCoroutine(PlayCollectAnimation());
        
        // 销毁水晶
        Destroy(gameObject);
    }
    
    /// <summary>
    /// 播放收集音效
    /// </summary>
    private void PlayCollectSound()
    {
        if (audioSource != null && collectSound != null)
        {
            audioSource.PlayOneShot(collectSound);
        }
    }
    
    /// <summary>
    /// 播放收集粒子效果
    /// </summary>
    private void PlayCollectParticles()
    {
        if (collectParticles != null)
        {
            // 设置粒子系统为不随父对象销毁而停止
            collectParticles.transform.SetParent(null);
            
            // 播放粒子效果
            collectParticles.Play();
            
            // 在粒子效果结束后销毁
            float particleDuration = collectParticles.main.duration + collectParticles.main.startLifetime.constantMax;
            Destroy(collectParticles.gameObject, particleDuration);
        }
    }
    
    /// <summary>
    /// 收集动画效果
    /// </summary>
    private IEnumerator PlayCollectAnimation()
    {
        Vector3 originalScale = visualMesh.transform.localScale;
        Vector3 originalPos = transform.position;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < collectAnimationDuration)
        {
            float progress = elapsedTime / collectAnimationDuration;
            
            // 缩放动画：先放大再缩小
            float scaleMultiplier;
            if (progress < 0.3f)
            {
                // 前30%时间放大到1.2倍
                scaleMultiplier = Mathf.Lerp(1f, 1.2f, progress / 0.3f);
            }
            else
            {
                // 后70%时间缩小到0
                scaleMultiplier = Mathf.Lerp(1.2f, 0f, (progress - 0.3f) / 0.7f);
            }
            
            if (visualMesh != null)
            {
                visualMesh.transform.localScale = originalScale * scaleMultiplier;
            }
            
            // 上升动画
            float upwardMovement = Mathf.Sin(progress * Mathf.PI) * 0.5f;
            transform.position = originalPos + Vector3.up * upwardMovement;
            
            // 透明度动画（如果有Renderer组件）
            Renderer renderer = visualMesh.GetComponent<Renderer>();
            if (renderer != null)
            {
                Color color = renderer.material.color;
                color.a = Mathf.Lerp(1f, 0f, progress);
                renderer.material.color = color;
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // 确保最终状态
        if (visualMesh != null)
        {
            visualMesh.transform.localScale = Vector3.zero;
        }
    }
}

/// <summary>
/// 能力收集事件
/// </summary>
public struct OnAbilityCollectedEvent
{
    public string abilityTypeId;
    public Vector3 crystalPosition;
}
