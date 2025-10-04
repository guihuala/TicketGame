using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelector : MonoBehaviour
{
    [SerializeField] private Button[] levelButtons;

    void Start()
    {
        // 为每个按钮添加点击事件，点击按钮后切换场景并传递关卡索引
        for (int i = 0; i < levelButtons.Length; i++)
        {
            int index = i;
            levelButtons[i].onClick.AddListener(() => OnLevelSelected(index));
        }
    }
    
    void OnLevelSelected(int levelIndex)
    {
        Debug.Log("Level selected: " + levelIndex);

        // 保存选择的关卡索引
        PlayerPrefs.SetInt("SelectedLevelIndex", levelIndex);

        // 加载游戏场景
        SceneLoader.Instance.LoadScene(GameScene.Gameplay);
    }
}