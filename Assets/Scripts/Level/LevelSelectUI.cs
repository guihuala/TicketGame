using UnityEngine;
using UnityEngine.UI;

public class LevelSelectUI : MonoBehaviour
{
    [System.Serializable]
    public class LevelButtonData
    {
        public Button button;
        public GameObject lockIcon;
        public GameObject star1;
        public GameObject star2;
        public GameObject star3;
    }

    [SerializeField] private LevelButtonData[] levelButtons;
    [SerializeField] private Button backButton; // 返回按钮
    [SerializeField] private Button shopButton;

    void Start()
    {
        // 设置返回按钮的点击事件
        if (backButton != null)
            backButton.onClick.AddListener(OnBackButtonClicked);
        if(shopButton != null)
            shopButton.onClick.AddListener(OnShopButtonClicked);
        

        UpdateLevelButtons();
        
        // 为每个按钮添加点击事件
        for (int i = 0; i < levelButtons.Length; i++)
        {
            int index = i;
            if (LevelProgressManager.IsLevelUnlocked(i))
            {
                levelButtons[i].button.onClick.AddListener(() => OnLevelSelected(index));
            }
            else
            {
                levelButtons[i].button.interactable = false;
            }
        }
    }
    
    private void UpdateLevelButtons()
    {
        for (int i = 0; i < levelButtons.Length; i++)
        {
            var buttonData = levelButtons[i];
            if (buttonData == null) continue;

            bool isUnlocked = LevelProgressManager.IsLevelUnlocked(i);
            int stars = LevelProgressManager.GetLevelStars(i);
            
            // 设置按钮交互状态
            buttonData.button.interactable = isUnlocked;
            
            // 显示/隐藏锁图标
            if (buttonData.lockIcon != null)
            {
                buttonData.lockIcon.SetActive(!isUnlocked);
            }
            
            // 更新星星显示
            if (buttonData.star1 != null) buttonData.star1.SetActive(stars >= 1);
            if (buttonData.star2 != null) buttonData.star2.SetActive(stars >= 2);
            if (buttonData.star3 != null) buttonData.star3.SetActive(stars >= 3);
        }
    }
    
    void OnLevelSelected(int levelIndex)
    {
        if (!LevelProgressManager.IsLevelUnlocked(levelIndex))
        {
            Debug.LogWarning($"尝试选择未解锁的关卡: {levelIndex}");
            return;
        }

        Debug.Log("Level selected: " + levelIndex);
        PlayerPrefs.SetInt("SelectedLevelIndex", levelIndex);
        SceneLoader.Instance.LoadScene(GameScene.Gameplay);
    }
    
    private void OnBackButtonClicked()
    {
        SceneLoader.Instance.LoadScene(GameScene.MainMenu);
    }

    private void OnShopButtonClicked()
    {
        UIManager.Instance.OpenPanel("ShopPanel");
    }
}