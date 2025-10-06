using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Serialization;

public class TitleUIController : MonoBehaviour
{
    public Button startButton;
    public Button settingsButton;
    public Button exitButton;
    public Button tutorialButton;
    public Button creditsButton;
    
    private void Awake()
    {
        startButton.onClick.AddListener(OnStartButtonClicked);
        settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        exitButton.onClick.AddListener(OnExitButtonClicked);
        tutorialButton.onClick.AddListener(OnClickTutorialBtn);
        creditsButton.onClick.AddListener(OnClickCreditBtn);
    }

    public void OnStartButtonClicked()
    {
        SceneLoader.Instance.LoadScene(GameScene.LevelSelection);
    }

    public void OnSettingsButtonClicked()
    {
        UIManager.Instance.OpenPanel("SettingPanel");
    }

    public void OnClickTutorialBtn()
    {
        UIManager.Instance.OpenPanel("TutorialPanel");
    }

    public void OnClickCreditBtn()
    {
        UIManager.Instance.OpenPanel("CreditPanel");
    }

    public void OnExitButtonClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}