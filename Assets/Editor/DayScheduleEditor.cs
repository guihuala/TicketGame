using UnityEngine;
using UnityEditor;
using System.Linq;

#if UNITY_EDITOR
[CustomEditor(typeof(DaySchedule))]
public class DayScheduleEditor : Editor
{
    private DaySchedule schedule;
    private Vector2 scrollPosition;
    private bool showBasicInfo = true;
    private bool showTimeSettings = true;
    private bool showShows = true;

    private void OnEnable()
    {
        schedule = (DaySchedule)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Check my tickets - 关卡配置", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // 基本信息折叠面板
        showBasicInfo = EditorGUILayout.Foldout(showBasicInfo, "关卡基本信息", true);
        if (showBasicInfo)
        {
            EditorGUI.indentLevel++;
            DrawBasicInfo();
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
        }

        // 时间设置折叠面板
        showTimeSettings = EditorGUILayout.Foldout(showTimeSettings, "时间设置", true);
        if (showTimeSettings)
        {
            EditorGUI.indentLevel++;
            DrawTimeSettings();
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
        }

        // 场次配置折叠面板
        showShows = EditorGUILayout.Foldout(showShows, $"场次配置 (共{schedule.shows.Count}场)", true);
        if (showShows)
        {
            EditorGUI.indentLevel++;
            DrawShowsConfiguration();
            EditorGUI.indentLevel--;
        }

        // 工具按钮
        EditorGUILayout.Space();
        DrawTools();

        // 检查是否有修改并自动标记为dirty
        if (GUI.changed)
        {
            MarkAsDirty();
        }

        serializedObject.ApplyModifiedProperties();
    }
    
    // 标记SO为已修改并保存
    private void MarkAsDirty()
    {
        EditorUtility.SetDirty(schedule);
    }

    // 立即保存到磁盘
    private void SaveImmediate()
    {
        MarkAsDirty();
        AssetDatabase.SaveAssets();
        Debug.Log($"已保存: {schedule.name}");
    }

    // 强制保存并刷新
    private void ForceSave()
    {
        MarkAsDirty();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"已强制保存并刷新: {schedule.name}");
    }

    private void DrawBasicInfo()
    {
        EditorGUILayout.LabelField("关卡名称", EditorStyles.miniLabel);
        schedule.levelName = EditorGUILayout.TextField("名称", schedule.levelName);
        
        // 提供一些预设关卡名按钮
        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("早班", GUILayout.Width(50))) 
            { 
                schedule.levelName = "早班检票";
                MarkAsDirty();
            }
            if (GUILayout.Button("午班", GUILayout.Width(50))) 
            { 
                schedule.levelName = "午班检票";
                MarkAsDirty();
            }
            if (GUILayout.Button("晚班", GUILayout.Width(50))) 
            { 
                schedule.levelName = "晚班检票";
                MarkAsDirty();
            }
            if (GUILayout.Button("周末", GUILayout.Width(50))) 
            { 
                schedule.levelName = "周末高峰";
                MarkAsDirty();
            }
            if (GUILayout.Button("节日", GUILayout.Width(50))) 
            { 
                schedule.levelName = "节日特场";
                MarkAsDirty();
            }
            GUILayout.FlexibleSpace();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        // 关卡日期
        EditorGUILayout.LabelField("关卡日期", EditorStyles.miniLabel);
        EditorGUILayout.BeginHorizontal();
        {
            schedule.levelDate = EditorGUILayout.TextField("日期(MM/dd/yy)", schedule.levelDate);

            // 验证日期格式
            if (!schedule.IsDateValid())
            {
                EditorGUILayout.LabelField("格式错误", GUILayout.Width(80));
            }
            else
            {
                EditorGUILayout.LabelField("格式正确", GUILayout.Width(80));
            }
        }
        EditorGUILayout.EndHorizontal();

        // 提供快速选择常用日期的按钮
        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("04/10/25", GUILayout.Width(80))) 
            { 
                schedule.levelDate = "04/10/25";
                MarkAsDirty();
            }
            if (GUILayout.Button("04/11/25", GUILayout.Width(80))) 
            { 
                schedule.levelDate = "04/11/25";
                MarkAsDirty();
            }
            if (GUILayout.Button("04/12/25", GUILayout.Width(80))) 
            { 
                schedule.levelDate = "04/12/25";
                MarkAsDirty();
            }
            if (GUILayout.Button("04/13/25", GUILayout.Width(80))) 
            { 
                schedule.levelDate = "04/13/25";
                MarkAsDirty();
            }
            GUILayout.FlexibleSpace();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox($"关卡: {schedule.levelName} | 日期: {schedule.levelDate}", MessageType.Info);
    }

    private void DrawTimeSettings()
    {
        // 关卡开始时间
        EditorGUILayout.LabelField("关卡开始时间", EditorStyles.miniLabel);

        EditorGUILayout.BeginHorizontal();
        {
            schedule.levelStartTime = EditorGUILayout.TextField("开始时间(HH:mm)", schedule.levelStartTime);

            // 验证时间格式
            if (!schedule.IsStartTimeValid())
            {
                EditorGUILayout.LabelField("格式错误", GUILayout.Width(80));
            }
            else
            {
                EditorGUILayout.LabelField("格式正确", GUILayout.Width(80));
            }
        }
        EditorGUILayout.EndHorizontal();

        // 提供快速选择常用时间的按钮
        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("08:00", GUILayout.Width(60))) 
            { 
                schedule.levelStartTime = "08:00";
                MarkAsDirty();
            }
            if (GUILayout.Button("09:30", GUILayout.Width(60))) 
            { 
                schedule.levelStartTime = "09:30";
                MarkAsDirty();
            }
            if (GUILayout.Button("11:00", GUILayout.Width(60))) 
            { 
                schedule.levelStartTime = "11:00";
                MarkAsDirty();
            }
            if (GUILayout.Button("13:15", GUILayout.Width(60))) 
            { 
                schedule.levelStartTime = "13:15";
                MarkAsDirty();
            }
            if (GUILayout.Button("14:45", GUILayout.Width(60))) 
            { 
                schedule.levelStartTime = "14:45";
                MarkAsDirty();
            }
            GUILayout.FlexibleSpace();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("时间流速", EditorStyles.miniLabel);
        schedule.timeScale = EditorGUILayout.Slider("时间比例", schedule.timeScale, 0.1f, 5f);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("时间间隔", EditorStyles.miniLabel);
        schedule.timeBetweenShows = EditorGUILayout.FloatField("场次间隔(秒)", schedule.timeBetweenShows);
        schedule.timeBetweenTickets = EditorGUILayout.FloatField("票间隔(秒)", schedule.timeBetweenTickets);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("动画时间", EditorStyles.miniLabel);
        schedule.ticketSlideInDuration = EditorGUILayout.FloatField("滑入动画(秒)", schedule.ticketSlideInDuration);
        schedule.ticketSlideOutDuration = EditorGUILayout.FloatField("滑出动画(秒)", schedule.ticketSlideOutDuration);
        schedule.initialTicketDelay = EditorGUILayout.FloatField("初始延迟(秒)", schedule.initialTicketDelay);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("评分标准", EditorStyles.miniLabel);
        schedule.star1Income = EditorGUILayout.IntField("1星收入", schedule.star1Income);
        schedule.star2Income = EditorGUILayout.IntField("2星收入", schedule.star2Income);
        schedule.star3Income = EditorGUILayout.IntField("3星收入", schedule.star3Income);
    }

    private void DrawShowsConfiguration()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(400));

        for (int i = 0; i < schedule.shows.Count; i++)
        {
            DrawShow(i, schedule.shows[i]);
            EditorGUILayout.Space();
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("添加场次"))
        {
            schedule.shows.Add(new DaySchedule.Show());
            MarkAsDirty();
        }
        if (GUILayout.Button("清空场次"))
        {
            if (EditorUtility.DisplayDialog("确认清空", "确定要清空所有场次吗？", "确定", "取消"))
            {
                schedule.shows.Clear();
                MarkAsDirty();
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawShow(int index, DaySchedule.Show show)
    {
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.LabelField($"场次 {index + 1}", EditorStyles.boldLabel);

        // 基本信息
        show.filmTitle = EditorGUILayout.TextField("电影名称", show.filmTitle);
        show.startTime = EditorGUILayout.TextField("开始时间", show.startTime);
        show.audienceCount = EditorGUILayout.IntSlider("观众数量", show.audienceCount, 0, 200);
    
        // 票价配置
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("票价设置", EditorStyles.miniLabel);
        show.ticketPrice = EditorGUILayout.IntSlider("单张票价", show.ticketPrice, 1, 20);

        // 特殊事件
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("特殊事件", EditorStyles.miniLabel);

        for (int i = 0; i < show.specialEvents.Count; i++)
        {
            DrawSpecialEvent(i, show.specialEvents[i], show);
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("添加特殊事件", GUILayout.Width(120)))
        {
            show.specialEvents.Add(new DaySchedule.SpecialEventConfig());
            MarkAsDirty();
        }

        if (GUILayout.Button("删除场次", GUILayout.Width(80)))
        {
            schedule.shows.RemoveAt(index);
            MarkAsDirty();
            return;
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }
    
    private void DrawSpecialEvent(int index, DaySchedule.SpecialEventConfig config, DaySchedule.Show show)
    {
        EditorGUILayout.BeginVertical("box");

        // 第一行：事件类型和删除按钮
        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.LabelField($"事件 {index + 1}", GUILayout.Width(60));
            config.type = (SpecialEventType)EditorGUILayout.EnumPopup(config.type, GUILayout.Width(150));

            // 右对齐删除按钮
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("删除", GUILayout.Width(50)))
            {
                show.specialEvents.RemoveAt(index);
                MarkAsDirty();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                return;
            }
        }
        EditorGUILayout.EndHorizontal();

        // 第二行：数量配置
        EditorGUILayout.BeginHorizontal();
        {
            config.count = EditorGUILayout.IntField("数量", config.count, GUILayout.Width(150));
            GUILayout.FlexibleSpace();
        }
        EditorGUILayout.EndHorizontal();

        // 第三行：是否应该接受
        EditorGUILayout.BeginHorizontal();
        {
            config.shouldAccept = EditorGUILayout.Toggle("应该接受", config.shouldAccept, GUILayout.Width(150));
            GUILayout.FlexibleSpace();
        }
        EditorGUILayout.EndHorizontal();

        // 图片配置
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("图片配置", EditorStyles.miniLabel);
        
        EditorGUILayout.BeginHorizontal();
        {
            config.mainTicketImage = (Sprite)EditorGUILayout.ObjectField("主票图片", config.mainTicketImage, typeof(Sprite), false);
            GUILayout.FlexibleSpace();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        {
            config.stubImage = (Sprite)EditorGUILayout.ObjectField("票根图片", config.stubImage, typeof(Sprite), false);
            GUILayout.FlexibleSpace();
        }
        EditorGUILayout.EndHorizontal();

        // 根据事件类型显示额外配置
        switch (config.type)
        {
            case SpecialEventType.WrongNameSpelling:
                EditorGUILayout.BeginHorizontal();
            {
                config.customFilmTitle = EditorGUILayout.TextField("错误电影名", config.customFilmTitle);
                GUILayout.FlexibleSpace();
            }
                EditorGUILayout.EndHorizontal();
                break;

            case SpecialEventType.OldTicket:
                EditorGUILayout.BeginHorizontal();
            {
                config.customShowTime = EditorGUILayout.TextField("旧票时间", config.customShowTime);
                GUILayout.FlexibleSpace();
            }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
            {
                config.customShowDate = EditorGUILayout.TextField("旧票日期", config.customShowDate);
                GUILayout.FlexibleSpace();
            }
                EditorGUILayout.EndHorizontal();
                break;

            case SpecialEventType.EarlyCheck:
                // 提前检票：可以选择其他场次的票
                EditorGUILayout.LabelField("提前检票配置", EditorStyles.miniLabel);

                EditorGUILayout.BeginHorizontal();
            {
                config.targetFilmTitle = EditorGUILayout.TextField("目标电影", config.targetFilmTitle);
                GUILayout.FlexibleSpace();
            }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
            {
                config.targetShowTime = EditorGUILayout.TextField("目标时间", config.targetShowTime);
                GUILayout.FlexibleSpace();
            }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
            {
                config.targetShowDate = EditorGUILayout.TextField("目标日期", config.targetShowDate);
                GUILayout.FlexibleSpace();
            }
                EditorGUILayout.EndHorizontal();

                // 自动同步到自定义字段
                if (!string.IsNullOrEmpty(config.targetFilmTitle) && !string.IsNullOrEmpty(config.targetShowTime))
                {
                    config.customFilmTitle = config.targetFilmTitle;
                    config.customShowTime = config.targetShowTime;
                    config.customShowDate = config.targetShowDate;
                }

                // 提供快速选择当前关卡其他场次的按钮
                if (schedule.shows.Count > 1)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("选择其他场次", GUILayout.Width(120)))
                        {
                            ShowOtherShowsMenu(config);
                        }

                        GUILayout.FlexibleSpace();
                    }
                    EditorGUILayout.EndHorizontal();
                }

                break;
        }

        // 显示事件描述
        string description = GetEventDescription(config, show);
        EditorGUILayout.HelpBox(description, MessageType.Info);

        EditorGUILayout.EndVertical();
    }

    // 显示其他场次选择菜单
    private void ShowOtherShowsMenu(DaySchedule.SpecialEventConfig config)
    {
        GenericMenu menu = new GenericMenu();

        foreach (var otherShow in schedule.shows)
        {
            string menuItem = $"{otherShow.filmTitle} {otherShow.startTime}";
            menu.AddItem(new GUIContent(menuItem), false, () =>
            {
                config.targetFilmTitle = otherShow.filmTitle;
                config.targetShowTime = otherShow.startTime;
                config.targetShowDate = schedule.levelDate;
                // 自动同步到自定义字段
                config.customFilmTitle = otherShow.filmTitle;
                config.customShowTime = otherShow.startTime;
                config.customShowDate = schedule.levelDate;
                MarkAsDirty();
            });
        }

        menu.ShowAsContext();
    }

    private string GetEventDescription(DaySchedule.SpecialEventConfig config, DaySchedule.Show show)
    {
        string baseDescription = "";
        switch (config.type)
        {
            case SpecialEventType.EarlyCheck:
                if (!string.IsNullOrEmpty(config.targetFilmTitle) && !string.IsNullOrEmpty(config.targetShowTime))
                {
                    string targetDate = string.IsNullOrEmpty(config.targetShowDate) ? schedule.levelDate : config.targetShowDate;
                    baseDescription = $"提前检票：持有 {config.targetFilmTitle} {targetDate} {config.targetShowTime} 的票\n" +
                                     $"当前场次：{show.filmTitle} {schedule.levelDate} {show.startTime}\n" +
                                     $"应该{(config.shouldAccept ? "接受" : "拒绝")}";
                }
                else
                {
                    baseDescription = $"提前检票：电影 {show.filmTitle} {show.startTime}";
                }
                break;
            case SpecialEventType.OldTicket:
                string oldDate = string.IsNullOrEmpty(config.customShowDate) ? schedule.levelDate : config.customShowDate;
                baseDescription = $"旧影票：正确电影 {show.filmTitle}，但时间是 {oldDate} {config.customShowTime}";
                break;
            case SpecialEventType.WrongNameSpelling:
                baseDescription = $"错误命名：显示为 '{config.customFilmTitle}'，正确应该是 '{show.filmTitle}'";
                break;
            case SpecialEventType.DamagedTicket:
                baseDescription = $"受损影票：信息正确但有污渍";
                break;
            case SpecialEventType.MissingStub:
                baseDescription = $"缺失票根：信息正确但无票根";
                break;
            default:
                baseDescription = $"{config.type}";
                break;
        }

        // 添加图片信息
        if (config.mainTicketImage != null)
        {
            baseDescription += $"\n使用图片: {config.mainTicketImage.name}";
        }

        return baseDescription;
    }

    private void DrawTools()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("🔧 配置工具", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("验证配置"))
        {
            ValidateConfiguration();
        }
        
        // 添加保存按钮
        if (GUILayout.Button("立即保存"))
        {
            SaveImmediate();
        }
        
        // 添加强制保存按钮
        if (GUILayout.Button("强制保存"))
        {
            ForceSave();
        }
        EditorGUILayout.EndHorizontal();

        // 显示保存状态
        EditorGUILayout.HelpBox("提示：修改后会自动标记为需要保存。点击'立即保存'将更改写入磁盘。", MessageType.Info);

        EditorGUILayout.EndVertical();
    }

    private void ValidateConfiguration()
    {
        int totalAudience = schedule.shows.Sum(show => show.audienceCount);
        int totalSpecialEvents = schedule.shows.Sum(show => show.specialEvents.Sum(e => e.count));
        int totalImages = schedule.shows.Sum(show => show.specialEvents.Count(e => e.mainTicketImage != null));

        string message = $"配置验证结果：\n" +
                         $"总场次：{schedule.shows.Count}\n" +
                         $"总观众：{totalAudience}\n" +
                         $"总特殊事件：{totalSpecialEvents}\n" +
                         $"带图片事件：{totalImages}\n" +
                         $"时间比例：{schedule.timeScale}x\n" +
                         $"开始时间：{schedule.levelStartTime}\n" +
                         $"关卡日期：{schedule.levelDate}";

        // 检查开始时间格式
        if (!schedule.IsStartTimeValid())
        {
            message += $"\n开始时间格式错误，请使用 HH:mm 格式";
        }

        // 检查日期格式
        if (!schedule.IsDateValid())
        {
            message += $"\n日期格式错误，请使用 MM/dd/yy 格式";
        }

        // 检查配置问题
        foreach (var show in schedule.shows)
        {
            int showSpecialEvents = show.specialEvents.Sum(e => e.count);
            if (showSpecialEvents > show.audienceCount)
            {
                message += $"\n场次 '{show.filmTitle}' 特殊事件数量({showSpecialEvents})超过观众数({show.audienceCount})";
            }
        }

        EditorUtility.DisplayDialog("配置验证", message, "确定");
    }
}
#endif