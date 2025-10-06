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
    private EconomyManager economyManager;
    private int selectedLevelIndex;

    void Start()
    {
        ticketGenerator = FindObjectOfType<TicketGenerator>();
        economyManager = FindObjectOfType<EconomyManager>();
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
            string hintText = "current level:\n" + ticketGenerator.GetCurrentLevelName();
            MsgCenter.SendMsg(MsgConst.MSG_SHOW_HINT, hintText, 3f);
        
            // 统一在这里初始化经济管理器
            if (economyManager != null)
            {
                economyManager.SetCurrentLevel(currentLevel);
                economyManager.ResetIncome();
            }
            else
            {
                Debug.LogWarning("[GameManager] 找不到 EconomyManager");
            }
        
            // 设置时间比例
            TimeManager.Instance.SetTimeFactor(currentLevel.timeScale);
        }
        else
        {
            Debug.LogError("加载关卡失败。");
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

    // 修改游戏结束逻辑
    public void EndGame()
    {
        SetGameState(GameState.GameOver);
        
        // 计算并保存星星
        CalculateAndSaveStars();
    }
    
    // 修改重启关卡逻辑，不保存星星
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
    
    public void CalculateAndSaveStars()
    {
        if (economyManager == null) return;
    
        int totalIncome = economyManager.currentIncome;
        int starsEarned = CalculateStars(totalIncome);
    
        // 保存进度
        LevelProgressManager.CompleteLevel(selectedLevelIndex, starsEarned);
        
        CurrencyManager.AddCoins(totalIncome);
    }
    
    private int CalculateStars(int totalIncome)
    {
        DaySchedule currentLevel = ticketGenerator.GetCurrentDay();
        if (currentLevel == null) return 0;
        
        if (totalIncome >= currentLevel.star3Income) return 3;
        if (totalIncome >= currentLevel.star2Income) return 2;
        if (totalIncome >= currentLevel.star1Income) return 1;
        
        return 0;
    }
}