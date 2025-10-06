using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Kuchinashi.SceneControl;
using UnityEngine.SceneManagement;
using QFramework;
using TMPro;
using AudioManager = Audio.AudioManager;

public class UIManager : MonoSingleton<UIManager>
{
    private Transform UICanvas;
    private GameObject mDeathMessage;
    private GameObject mDialoguePanel;
    private GameObject mPausePanel;

    // Reference to the Credit Panel
    [SerializeField] private GameObject creditsPanel;

    // Reference to the Close Button (optional)
    [SerializeField] private Button closeButton;

    // Reference to the Credit Button
    // [SerializeField] private Button creditButton;

    // ✅ 【新增】Result 面板
    [Header("Result Panel Components")]
    [SerializeField] private GameObject resultPanel;          // 结算面板
    [SerializeField] private TextMeshProUGUI resultScoreText; // 分数显示
    [SerializeField] private Button restartButton;            // 重开按钮
    [SerializeField] private Button backToTitleButton;        // 返回主菜单按钮（可选）

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        UICanvas = GameObject.Find("UICanvas").transform;
        mDeathMessage = UICanvas.Find("DeathMessage").gameObject;
        mDialoguePanel = UICanvas.Find("DialoguePanel").gameObject;
        mPausePanel = UICanvas.Find("PausePanel").gameObject;

        mDeathMessage.SetActive(false);
        mDialoguePanel.SetActive(false);
        mPausePanel.SetActive(false);

        if (resultPanel != null)
            resultPanel.SetActive(false);

        // 注册 OnLevelResetEvent 事件，用于在关卡重置时隐藏死亡消息
        TypeEventSystem.Global.Register<OnLevelResetEvent>(e =>
        {
            HideDeathMessage();
        }).UnRegisterWhenGameObjectDestroyed(gameObject);
    }

    // 显示死亡消息的方法
    public void ShowDeathMessage()
    {
        if (mDeathMessage != null)
        {
            Debug.Log("显示死亡消息: " + mDeathMessage.name);
            var temp = mDeathMessage.GetComponentInChildren<TextMeshProUGUI>();
            if (temp != null)
            {
                temp.text = "You died";
                temp.text += "\nClick to restart level";
            }
            else
            {
                Debug.LogWarning("DeathMessage 对象中未找到 TextMeshProUGUI 组件。");
            }
            mDeathMessage.SetActive(true);
        }
        else
        {
            Debug.LogWarning("mDeathMessage 未被赋值。无法显示死亡消息。");
        }
    }

    // 隐藏死亡消息的方法
    public void HideDeathMessage()
    {
        if (mDeathMessage != null)
        {
            mDeathMessage.SetActive(false);
            Debug.Log("隐藏死亡消息: " + mDeathMessage.name);
        }
        else
        {
            Debug.LogWarning("mDeathMessage 未被赋值。无法隐藏死亡消息。");
        }
    }

    void Start()
    {
        // Ensure the Credits Panel is hidden at the start
        if (creditsPanel != null)
        {
            creditsPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("Credits Panel 未被赋值！请在 Inspector 中为 UIManager 分配 Credits Panel 对象。");
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HideCredits);
        }
        else
        {
            Debug.LogWarning("Close Button 未被赋值！如不需要关闭按钮，可以忽略此警告。");
        }

        // ✅ 绑定 Result 面板按钮事件
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartButtonClicked);

        if (backToTitleButton != null)
            backToTitleButton.onClick.AddListener(OnBackToTitleClicked);
    }

    // ------------------------ Credit 面板 ------------------------
    // Method to show Credits Panel
    public void ShowCredits()
    {
        if (creditsPanel != null)
        {
            creditsPanel.SetActive(true);
            Audio.AudioManager.Instance.PlayCreditMusic();
        }
    }

    // Method to hide Credits Panel
    public void HideCredits()
    {
        if (creditsPanel != null)
        {
            creditsPanel.SetActive(false);
            Audio.AudioManager.Instance.PlayTitleMusic();
        }
    }

    // ------------------------ Result 面板 ------------------------
    public void ShowResultPanel(int score)
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
            if (resultScoreText != null)
                resultScoreText.text = $"Score: {score}";
            Time.timeScale = 0f; // 暂停游戏
        }
    }

    private void OnRestartButtonClicked()
    {
        Time.timeScale = 1f;
        if (resultPanel != null)
            resultPanel.SetActive(false);
        SceneControl.SwitchSceneWithoutConfirm("chapter 1");
    }

    private void OnBackToTitleClicked()
    {
        Time.timeScale = 1f;
        if (resultPanel != null)
            resultPanel.SetActive(false);
        SceneControl.Instance.hasStart = false;
        Audio.AudioManager.Instance.PlayTitleMusic();
        SceneControl.SwitchSceneWithoutConfirm("MainScene");
    }

    // ------------------------ Pause 面板 ------------------------
    private void Update() {
        if(Input.GetKeyDown(KeyCode.Escape)){
            mPausePanel.SetActive(true);
        }
    }
}