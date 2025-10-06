using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public enum GameState
    {
        Initializing, // 新增：初始化状态
        Playing, // 游戏进行中
        Paused, // 游戏暂停
        GameOver // 游戏结束
    }

    private GameState currentState;
    private TicketGenerator ticketGenerator;
    private EconomyManager economyManager;
    private TicketQueueController ticketQueueController;
    private ScheduleClock scheduleClock;
    private int selectedLevelIndex;

    void Start()
    {
        currentState = GameState.Initializing;
        InitializeGame();
    }
    
    /// <summary>
    /// 统一初始化所有模块
    /// </summary>
    private void InitializeGame()
    {
        Debug.Log("[GameManager] 开始初始化游戏模块...");
        
        // 1. 获取组件引用
        ticketGenerator = FindObjectOfType<TicketGenerator>();
        economyManager = FindObjectOfType<EconomyManager>();
        ticketQueueController = FindObjectOfType<TicketQueueController>();
        scheduleClock = FindObjectOfType<ScheduleClock>();
        
        // 2. 设置关卡索引
        selectedLevelIndex = PlayerPrefs.GetInt("SelectedLevelIndex", 0);
        Debug.Log($"[GameManager] 设置关卡索引: {selectedLevelIndex}");
        
        // 3. 配置 TicketGenerator
        if (ticketGenerator != null)
        {
            ticketGenerator.SetLevel(selectedLevelIndex);
            Debug.Log("[GameManager] TicketGenerator 配置完成");
        }
        else
        {
            Debug.LogError("[GameManager] 找不到 TicketGenerator");
        }
        
        // 4. 初始化时间管理器
        TimeManager.Instance.SetTimeFactor(1f);
        TimeManager.Instance.ResumeTime();
        Debug.Log("[GameManager] 时间管理器初始化完成");
        
        // 5. 初始化经济管理器
        if (economyManager != null)
        {
            var currentLevel = ticketGenerator.GetCurrentLevel();
            if (currentLevel != null)
            {
                economyManager.SetCurrentLevel(currentLevel);
                economyManager.ResetIncome();
                Debug.Log("[GameManager] EconomyManager 初始化完成");
            }
        }
        
        // 6. 启动 TicketQueueController
        if (ticketQueueController != null)
        {
            ticketQueueController.Initialize();
            Debug.Log("[GameManager] TicketQueueController 启动完成");
        }
        
        // 7. 开始游戏
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
        DaySchedule currentLevel = ticketGenerator.GetCurrentLevel();
        if (currentLevel != null)
        {
            string hintText = "current level:\n" + ticketGenerator.GetCurrentLevelName();
            MsgCenter.SendMsg(MsgConst.MSG_SHOW_HINT, hintText, 3f);
        
            // 设置时间比例
            TimeManager.Instance.SetTimeFactor(currentLevel.timeScale);
            
            Debug.Log($"[GameManager] 游戏开始: {currentLevel.levelName}, 开始时间: {currentLevel.levelStartTime}");
            
            // 设置游戏状态为进行中
            SetGameState(GameState.Playing);
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
    
    /// <summary>
    /// 重新开始当前关卡
    /// </summary>
    public void RestartCurrentLevel()
    {
        SetGameState(GameState.Playing);
        LoadLevel(selectedLevelIndex);
    }

    /// <summary>
    /// 加载指定关卡
    /// </summary>
    /// <param name="levelIndex">关卡索引</param>
    public void LoadLevel(int levelIndex)
    {
        SetGameState(GameState.Playing);
        Debug.Log($"[GameManager] 加载关卡: {levelIndex}");
        
        // 更新当前选中的关卡索引
        selectedLevelIndex = levelIndex;
        
        // 保存到PlayerPrefs
        PlayerPrefs.SetInt("SelectedLevelIndex", levelIndex);
        PlayerPrefs.Save();
        
        // 重新加载游戏场景
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
        DaySchedule currentLevel = ticketGenerator.GetCurrentLevel();
        if (currentLevel == null) return 0;
        
        if (totalIncome >= currentLevel.star3Income) return 3;
        if (totalIncome >= currentLevel.star2Income) return 2;
        if (totalIncome >= currentLevel.star1Income) return 1;
        
        return 0;
    }
}