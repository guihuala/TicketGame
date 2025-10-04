using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public enum GameState
    {
        Playing, // 游戏进行中
        Paused, // 游戏暂停
        GameOver // 游戏结束
    }

    private GameState currentState;
    private TicketGenerator ticketGenerator;
    private int selectedLevelIndex;

    void Start()
    {
        ticketGenerator = FindObjectOfType<TicketGenerator>();
        selectedLevelIndex = PlayerPrefs.GetInt("SelectedLevelIndex", 0);

        // 初始化时间管理器
        TimeManager.Instance.SetTimeFactor(1f);
        TimeManager.Instance.ResumeTime();

        // 加载对应的关卡数据
        ticketGenerator.SetLevel(selectedLevelIndex);

        // 启动游戏
        StartGame();
    }
    
    public void SetGameState(GameState newState)
    {
        currentState = newState;

        switch (newState)
        {
            case GameState.Playing:
                TimeManager.Instance.ResumeTime();
                InputController.Instance.EnableInput(true);
                UIManager.Instance.ClosePanel("PausePanel");
                UIManager.Instance.ClosePanel("GameOverPanel");
                break;

            case GameState.Paused:
                TimeManager.Instance.PauseTime();
                InputController.Instance.EnableInput(false);
                UIManager.Instance.OpenPanel("PausePanel");
                break;

            case GameState.GameOver:
                TimeManager.Instance.PauseTime();
                InputController.Instance.EnableInput(false);
                UIManager.Instance.OpenPanel("GameOverPanel");
                break;
        }
    }
    
    #region 状态控制

    // 游戏开始
    private void StartGame()
    {
        DaySchedule currentLevel = ticketGenerator.GetCurrentDay();
        if (currentLevel != null)
        {
            Debug.Log("Starting level: " + currentLevel.name);
            // 重置时间
            var scheduleClock = FindObjectOfType<ScheduleClock>();
            if (scheduleClock != null)
            {
                // todo.根据关卡设置初始时间
            }
        }
        else
        {
            Debug.LogError("Failed to load level.");
        }
    }

    // 暂停游戏
    public void PauseGame()
    {
        if (currentState == GameState.Playing)
        {
            SetGameState(GameState.Paused);
        }
    }

    // 恢复游戏
    public void ResumeGame()
    {
        if (currentState == GameState.Paused)
        {
            SetGameState(GameState.Playing);
        }
    }

    // 游戏结束
    public void EndGame()
    {
        SetGameState(GameState.GameOver);
    }
    
    public void RestartCurrentLevel()
    {
        SetGameState(GameState.Playing);
        
        PlayerPrefs.SetInt("SelectedLevelIndex", selectedLevelIndex);
        PlayerPrefs.Save();
        
        // 重新加载当前游戏场景
        SceneLoader.Instance.ReloadCurrentScene();
    }

    // 返回主菜单
    public void ReturnToMainMenu()
    {
        SetGameState(GameState.Playing);
        SceneLoader.Instance.LoadScene(GameScene.MainMenu);
    }

    #endregion
}