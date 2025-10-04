
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using UnityEngine.EventSystems;

public static class CreateSampleContent
{
    [MenuItem("PaperworkGame/Generate Sample Content")]
    public static void GenerateAssets()
    {
        string root = "Assets/PaperworkGame";
        string levelDir = root + "/Levels";
        System.IO.Directory.CreateDirectory(levelDir);

        var l1 = ScriptableObject.CreateInstance<DaySchedule>();
        l1.star1Income = 20; l1.star2Income = 50; l1.star3Income = 90;
        l1.shows.Add(new DaySchedule.Show{ filmTitle = "La La Lamb", startTime = "10:00", audienceCount = 3, special_Early=1 });
        l1.shows.Add(new DaySchedule.Show{ filmTitle = "La La Lamb", startTime = "15:00", audienceCount = 3, special_Early=1 });
        AssetDatabase.CreateAsset(l1, levelDir + "/Level_1.asset");

        var l2 = ScriptableObject.CreateInstance<DaySchedule>();
        l2.star1Income = 20; l2.star2Income = 50; l2.star3Income = 90;
        l2.shows.Add(new DaySchedule.Show{ filmTitle = "La La Lamb", startTime = "10:00", audienceCount = 5, special_Early=1 });
        l2.shows.Add(new DaySchedule.Show{ filmTitle = "La La Lamb", startTime = "13:00", audienceCount = 3, special_Early=1 });
        l2.shows.Add(new DaySchedule.Show{ filmTitle = "La La Lamb", startTime = "15:00", audienceCount = 5, special_Early=1 });
        AssetDatabase.CreateAsset(l2, levelDir + "/Level_2.asset");

        var l3 = ScriptableObject.CreateInstance<DaySchedule>();
        l3.star1Income = 20; l3.star2Income = 50; l3.star3Income = 90;
        l3.shows.Add(new DaySchedule.Show{ filmTitle = "La La Lamb", startTime = "10:00", audienceCount = 7, special_Early=2 });
        l3.shows.Add(new DaySchedule.Show{ filmTitle = "La La Lamb", startTime = "13:00", audienceCount = 5, special_Early=1 });
        l3.shows.Add(new DaySchedule.Show{ filmTitle = "La La Lamb", startTime = "15:00", audienceCount = 7, special_Early=2 });
        AssetDatabase.CreateAsset(l3, levelDir + "/Level_3.asset");

        var db = ScriptableObject.CreateInstance<LevelDatabase>();
        db.levels = new DaySchedule[]{ l1, l2, l3 };
        AssetDatabase.CreateAsset(db, levelDir + "/LevelDatabase.asset");

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("PaperworkGame", "Sample levels created at Assets/PaperworkGame/Levels", "OK");
    }

    [MenuItem("PaperworkGame/Create Minimal Demo Scene")]
    public static void CreateDemoScene()
    {
        var db = AssetDatabase.LoadAssetAtPath<LevelDatabase>("Assets/PaperworkGame/Levels/LevelDatabase.asset");
        if (db == null) GenerateAssets();
        db = AssetDatabase.LoadAssetAtPath<LevelDatabase>("Assets/PaperworkGame/Levels/LevelDatabase.asset");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var root = new GameObject("PaperworkGame_Root");

        var generator = root.AddComponent<TicketGenerator>();
        var validator = root.AddComponent<TicketValidator>();
        var economy = root.AddComponent<EconomyManager>();
        var clock = root.AddComponent<ScheduleClock>();
        if (db != null) generator.SetDatabase(db);

        var ctrlGO = new GameObject("QueueController");
        ctrlGO.transform.SetParent(root.transform);
        var ctrl = ctrlGO.AddComponent<TicketQueueController>();

        var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        var panelGO = new GameObject("TicketPanel", typeof(RectTransform), typeof(Image));
        panelGO.transform.SetParent(canvasGO.transform, false);
        var panelRT = panelGO.GetComponent<RectTransform>();
        panelRT.sizeDelta = new Vector2(600, 320);
        panelRT.anchoredPosition = new Vector2(0, 120);
        var panelImg = panelGO.GetComponent<Image>();
        panelImg.color = new Color(1f, 1f, 1f, 1f);

        var titleGO = new GameObject("FilmTitle", typeof(Text));
        titleGO.transform.SetParent(panelGO.transform, false);
        var titleTxt = titleGO.GetComponent<Text>();
        titleTxt.text = "Film Title";
        titleTxt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        titleTxt.fontSize = 36;
        titleTxt.alignment = TextAnchor.MiddleCenter;
        var titleRT = titleGO.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0.1f, 0.65f);
        titleRT.anchorMax = new Vector2(0.9f, 0.95f);
        titleRT.offsetMin = titleRT.offsetMax = Vector2.zero;

        var timeGO = new GameObject("ShowTime", typeof(Text));
        timeGO.transform.SetParent(panelGO.transform, false);
        var timeTxt = timeGO.GetComponent<Text>();
        timeTxt.text = "10:00";
        timeTxt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        timeTxt.fontSize = 28;
        timeTxt.alignment = TextAnchor.MiddleCenter;
        var timeRT = timeGO.GetComponent<RectTransform>();
        timeRT.anchorMin = new Vector2(0.1f, 0.4f);
        timeRT.anchorMax = new Vector2(0.9f, 0.6f);
        timeRT.offsetMin = timeRT.offsetMax = Vector2.zero;

        var specGO = new GameObject("Special", typeof(Text));
        specGO.transform.SetParent(panelGO.transform, false);
        var specTxt = specGO.GetComponent<Text>();
        specTxt.text = "";
        specTxt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        specTxt.fontSize = 24;
        specTxt.alignment = TextAnchor.MiddleCenter;
        var specRT = specGO.GetComponent<RectTransform>();
        specRT.anchorMin = new Vector2(0.1f, 0.15f);
        specRT.anchorMax = new Vector2(0.9f, 0.35f);
        specRT.offsetMin = specRT.offsetMax = Vector2.zero;

        var stubGO = new GameObject("Stub", typeof(Image));
        stubGO.transform.SetParent(panelGO.transform, false);
        var stubImg = stubGO.GetComponent<Image>();
        stubImg.color = new Color(0.9f, 0.9f, 0.9f, 1f);
        var stubRT = stubGO.GetComponent<RectTransform>();
        stubRT.anchorMin = new Vector2(0.9f, 0.1f);
        stubRT.anchorMax = new Vector2(0.98f, 0.9f);
        stubRT.offsetMin = stubRT.offsetMax = Vector2.zero;

        var btnGO = new GameObject("RejectButton", typeof(Image), typeof(Button));
        btnGO.transform.SetParent(canvasGO.transform, false);
        var btnImg = btnGO.GetComponent<Image>(); btnImg.color = new Color(0.8f,0.3f,0.3f,1f);
        var btn = btnGO.GetComponent<Button>();
        var btnRT = btnGO.GetComponent<RectTransform>();
        btnRT.sizeDelta = new Vector2(220, 80);
        btnRT.anchoredPosition = new Vector2(0, -300);

        var btnTextGO = new GameObject("Text", typeof(Text));
        btnTextGO.transform.SetParent(btnGO.transform, false);
        var btnText = btnTextGO.GetComponent<Text>();
        btnText.text = "拒绝 (R)";
        btnText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        btnText.fontSize = 28;
        btnText.alignment = TextAnchor.MiddleCenter;
        var btnTextRT = btnTextGO.GetComponent<RectTransform>();
        btnTextRT.anchorMin = Vector2.zero; btnTextRT.anchorMax = Vector2.one;
        btnTextRT.offsetMin = btnTextRT.offsetMax = Vector2.zero;

        // Logic components
        var uiGO = new GameObject("TicketUI");
        uiGO.transform.SetParent(root.transform);
        var ui = uiGO.AddComponent<TicketUI>();
        var tear = panelGO.AddComponent<TearInteraction>();
        var visual = panelGO.AddComponent<TicketVisual>();

        visual.GetType().GetField("ticketBg", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(visual, panelImg);
        visual.GetType().GetField("titleText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(visual, titleTxt);
        visual.GetType().GetField("timeText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(visual, timeTxt);
        visual.GetType().GetField("specialText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(visual, specTxt);
        visual.GetType().GetField("stub", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(visual, stubImg);

        typeof(TicketUI).GetField("queue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(ui, ctrl);
        typeof(TicketUI).GetField("tear", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(ui, tear);
        typeof(TicketUI).GetField("visual", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(ui, visual);

        typeof(TicketQueueController).GetField("generator", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(ctrl, generator);
        typeof(TicketQueueController).GetField("validator", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(ctrl, validator);
        typeof(TicketQueueController).GetField("ticketUI", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(ctrl, ui);
        typeof(TicketQueueController).GetField("economy", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(ctrl, economy);
        typeof(TicketQueueController).GetField("scheduleClock", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(ctrl, clock);

        btn.onClick.AddListener(() => ui.OnRejectClicked());

        System.IO.Directory.CreateDirectory("Assets/PaperworkGame/Scenes");
        EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene(), "Assets/PaperworkGame/Scenes/PaperworkGame_Demo.unity");

        EditorUtility.DisplayDialog("PaperworkGame", "Demo scene created with visual ticket & interactions.", "Nice!");
    }
}
#endif
