using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Kuchinashi.SceneControl;

public class StartPanelControl : MonoBehaviour
{
    private Button mStartButton;
    private Button mExitButton;
    private Button mCreditButton;
    private void Start()
    {
        mStartButton = transform.Find("StartButton").GetComponent<Button>();
        mExitButton = transform.Find("ExitButton").GetComponent<Button>();
        mCreditButton = transform.Find("CreditButton").GetComponent<Button>();
        
        Audio.AudioManager.Instance.PlayTitleMusic();

        mStartButton.onClick.AddListener(() =>
        {
            if (SceneControl.Instance.hasStart)
            {
                return;
            }
            SceneControl.Instance.hasStart = true;
            SceneControl.SwitchSceneWithoutConfirm("chapter 1", () => Audio.AudioManager.Instance.PlayBGM());
        });
        mExitButton.onClick.AddListener(() =>
        {
            if (SceneControl.Instance.hasStart)
            {
                return;
            }
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        });
        mCreditButton.onClick.AddListener(()=>{
            if (SceneControl.Instance.hasStart)
            {
                return;
            }
            UIManager.Instance.ShowCredits();
        });
        
        Audio.AudioManager.Instance.PlayTitleMusic();
    }
}
