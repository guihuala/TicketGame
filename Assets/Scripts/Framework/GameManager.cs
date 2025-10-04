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
        selectedLevelIndex = PlayerPrefs.GetInt("SelectedLevelIndex", 0);  // 读取选择的关卡索引

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
                Time.timeScale = 1;
                UIManager.Instance.ClosePanel("PausePanel");
                UIManager.Instance.ClosePanel("GameOverPanel");
                break;

            case GameState.Paused:
                Time.timeScale = 0;
                UIManager.Instance.OpenPanel("PausePanel");
                break;

            case GameState.GameOver:
                Time.timeScale = 0;
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
            // 你可以在这里做其他的初始化工作，比如初始化 UI、时间等
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

    // 重新开始当前关卡
    public void RestartCurrentLevel()
    {
        // 重置游戏状态
        SetGameState(GameState.Playing);
        
        // 重新加载当前关卡
        ticketGenerator.SetLevel(selectedLevelIndex);
        
        // 重新开始游戏
        StartGame();
        
        Debug.Log("Restarting current level: " + selectedLevelIndex);
    }

    // 返回主菜单
    public void ReturnToMainMenu()
    {
        SetGameState(GameState.Playing);
        SceneLoader.Instance.LoadScene(GameScene.MainMenu);
    }

    #endregion
}