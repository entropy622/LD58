using QFramework;
using UnityEngine;
using System.Collections;

namespace Audio
{
    public class AudioManager : MonoSingleton<AudioManager>
    {
        [Header("音频设置")]
        public AudioClip bgmClip;           // 游戏背景音乐
        public AudioClip titleClip;         // 标题音乐
        public AudioClip creditClip;        // 制作人员音乐
        
        [Header("音频参数")]
        [Range(0f, 2f)]
        public float musicVolume = 1f;    // 音乐音量
        
        [Range(0f, 1f)]
        public float fadeDuration = 0.5f;   // 音乐淡入淡出时间
        
        private AudioSource musicSource;    // 音乐音频源
        private Coroutine fadeCoroutine;    // 淡入淡出协程
        private MusicType currentMusicType = MusicType.None; // 当前播放的音乐类型
        
        /// <summary>
        /// 音乐类型枚举
        /// </summary>
        public enum MusicType
        {
            None,
            BGM,
            Title,
            Credit
        }
        
        protected void Awake()
        {
            // 初始化音频源
            InitializeAudioSource();
        }
        
        private void Start()
        {
            // 设置音频源参数
            if (musicSource != null)
            {
                musicSource.volume = musicVolume;
                musicSource.loop = true;
            }
            PlayTitleMusic();
        }
        
        /// <summary>
        /// 初始化音频源
        /// </summary>
        private void InitializeAudioSource()
        {
            // 添加音频源组件
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
            musicSource.volume = musicVolume;
        }
        
        /// <summary>
        /// 播放游戏背景音乐
        /// </summary>
        public void PlayBGM()
        {
            if (bgmClip != null && currentMusicType != MusicType.BGM)
            {
                StartCoroutine(FadeToNewMusic(bgmClip, MusicType.BGM));
            }
        }
        
        /// <summary>
        /// 播放标题音乐
        /// </summary>
        public void PlayTitleMusic()
        {
            if (titleClip != null && currentMusicType != MusicType.Title)
            {
                StartCoroutine(FadeToNewMusic(titleClip, MusicType.Title));
            }
        }
        
        /// <summary>
        /// 播放制作人员音乐
        /// </summary>
        public void PlayCreditMusic()
        {
            if (creditClip != null && currentMusicType != MusicType.Credit)
            {
                StartCoroutine(FadeToNewMusic(creditClip, MusicType.Credit));
            }
        }
        
        /// <summary>
        /// 停止当前音乐
        /// </summary>
        public void StopMusic()
        {
            if (musicSource != null && musicSource.isPlaying)
            {
                if (fadeCoroutine != null)
                {
                    StopCoroutine(fadeCoroutine);
                }
                
                StartCoroutine(FadeOutMusic());
                currentMusicType = MusicType.None;
            }
        }
        
        /// <summary>
        /// 淡入淡出到新音乐
        /// </summary>
        private IEnumerator FadeToNewMusic(AudioClip newClip, MusicType newType)
        {
            // 如果当前正在播放音乐，先淡出
            if (musicSource != null && musicSource.isPlaying)
            {
                yield return StartCoroutine(FadeOutMusic());
            }
            
            // 设置新音乐
            musicSource.clip = newClip;
            currentMusicType = newType;
            
            // 播放新音乐并淡入
            musicSource.Play();
            yield return StartCoroutine(FadeInMusic());
        }
        
        /// <summary>
        /// 淡入音乐
        /// </summary>
        private IEnumerator FadeInMusic()
        {
            if (musicSource == null) yield break;
            
            float startVolume = 0f;
            float endVolume = musicVolume;
            float duration = fadeDuration;
            float elapsed = 0f;
            
            musicSource.volume = startVolume;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, endVolume, elapsed / duration);
                yield return null;
            }
            
            musicSource.volume = endVolume;
        }
        
        /// <summary>
        /// 淡出音乐
        /// </summary>
        private IEnumerator FadeOutMusic()
        {
            if (musicSource == null) yield break;
            
            float startVolume = musicSource.volume;
            float endVolume = 0f;
            float duration = fadeDuration;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, endVolume, elapsed / duration);
                yield return null;
            }
            
            musicSource.Stop();
            musicSource.volume = musicVolume; // 恢复默认音量
        }
        
        /// <summary>
        /// 设置音乐音量
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            if (musicSource != null)
            {
                musicSource.volume = musicVolume;
            }
        }
        
        /// <summary>
        /// 获取当前播放的音乐类型
        /// </summary>
        public MusicType GetCurrentMusicType()
        {
            return currentMusicType;
        }
        
        /// <summary>
        /// 检查音乐是否正在播放
        /// </summary>
        public bool IsMusicPlaying()
        {
            return musicSource != null && musicSource.isPlaying;
        }
    }
}