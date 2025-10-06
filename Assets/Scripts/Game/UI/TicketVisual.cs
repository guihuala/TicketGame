using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using System.Text;

public class TicketVisual : MonoBehaviour
{
    [Header("主票")]
    [SerializeField] private Image ticketBg;
    [SerializeField] private Text titleText;
    [SerializeField] private Text timeText;
    [SerializeField] private Text seatText;
    
    [Header("副券")]
    [SerializeField] private Image stub;
    [SerializeField] private Text stubTitleText;
    [SerializeField] private Text subTimeText;
    [SerializeField] private Text subSeatText;
    [SerializeField] private Text priceText;

    [Header("图片替换")]
    [SerializeField] private Image mainTicketImage;
    [SerializeField] private Image stubImage;
    
    [Header("主票字体设置")]
    [SerializeField] private int mainNormalFontSize = 24;
    [SerializeField] private int mainEmphasizedFontSize = 28; // 主票强调字母的字号
    
    [Header("副券字体设置")]
    [SerializeField] private int stubNormalFontSize = 18;
    [SerializeField] private int stubEmphasizedFontSize = 22; // 副券强调字母的字号
    
    [Header("高亮提示设置")]
    [SerializeField] private GameObject errorHighlightPrefab; // 高亮图标预制件
    [SerializeField] private float highlightDuration = 2f;    // 高亮显示时长
    
    // 高亮目标区域引用
    [SerializeField] private RectTransform filmTitleHighlightArea;
    [SerializeField] private RectTransform showTimeHighlightArea;
    [SerializeField] private RectTransform dateHighlightArea;
    [SerializeField] private RectTransform stubHighlightArea;
    [SerializeField] private RectTransform specialHighlightArea;

    private GameObject currentHighlight; // 当前显示的高亮对象
    
    private Color defaultColor;
    private Vector3 defaultStubPos;
    private string generatedSeatNumber;
    private TicketData currentTicketData;

    void Start()
    {
        if (stub != null)
        {
            defaultStubPos = stub.transform.localPosition;
        }
        
        // 注册消息监听
        MsgCenter.RegisterMsg(MsgConst.MSG_TICKET_HIGHLIGHT, OnTicketHighlight);
        MsgCenter.RegisterMsg(MsgConst.MSG_TICKET_HIGHLIGHT_CLEAR, OnTicketHighlightClear);
    }
    
    void OnDestroy()
    {
        // 移除消息监听
        MsgCenter.UnregisterMsg(MsgConst.MSG_TICKET_HIGHLIGHT, OnTicketHighlight);
        MsgCenter.UnregisterMsg(MsgConst.MSG_TICKET_HIGHLIGHT_CLEAR, OnTicketHighlightClear);
        
        // 清理高亮对象
        if (currentHighlight != null)
        {
            Destroy(currentHighlight);
        }
    }
    
    /// <summary>
    /// 处理票证高亮消息
    /// </summary>
    private void OnTicketHighlight(System.Object data)
    {
        if (data is object[] parameters && parameters.Length >= 2)
        {
            TicketValidator.HighlightType highlightType = (TicketValidator.HighlightType)parameters[0];
            string reason = (string)parameters[1];
            
            ShowErrorHighlight(highlightType, reason);
        }
    }

    /// <summary>
    /// 清除高亮
    /// </summary>
    private void OnTicketHighlightClear(System.Object data)
    {
        ClearErrorHighlight();
    }
    
    private void ShowErrorHighlight(TicketValidator.HighlightType highlightType, string reason)
    {
        // 先清除现有的高亮
        ClearErrorHighlight();
        
        RectTransform targetArea = GetHighlightArea(highlightType);
        if (targetArea != null && errorHighlightPrefab != null)
        {
            // 实例化高亮图标
            currentHighlight = Instantiate(errorHighlightPrefab, transform);
            RectTransform highlightRect = currentHighlight.GetComponent<RectTransform>();
            
            if (highlightRect != null)
            {
                // 设置高亮图标的位置和大小
                highlightRect.SetParent(targetArea, false);
                highlightRect.anchorMin = new Vector2(0.5f, 0.5f);
                highlightRect.anchorMax = new Vector2(0.5f, 0.5f);
                highlightRect.pivot = new Vector2(0.5f, 0.5f);
                highlightRect.anchoredPosition = Vector2.zero;
                highlightRect.sizeDelta = new Vector2(40, 40); // 图标大小
                
                // 添加动画效果
                PlayHighlightAnimation(currentHighlight);
                
                // 定时自动清除
                StartCoroutine(AutoClearHighlight());
                
                Debug.Log($"显示错误高亮: {highlightType}, 原因: {reason}");
            }
        }
    }

    /// <summary>
    /// 获取对应的高亮区域
    /// </summary>
    private RectTransform GetHighlightArea(TicketValidator.HighlightType highlightType)
    {
        switch (highlightType)
        {
            case TicketValidator.HighlightType.FilmTitle:
                return filmTitleHighlightArea;
            case TicketValidator.HighlightType.ShowTime:
                return showTimeHighlightArea;
            case TicketValidator.HighlightType.Date:
                return dateHighlightArea;
            case TicketValidator.HighlightType.Stub:
                return stubHighlightArea;
            case TicketValidator.HighlightType.Special:
                return specialHighlightArea;
            default:
                return specialHighlightArea;
        }
    }

    /// <summary>
    /// 播放高亮动画
    /// </summary>
    private void PlayHighlightAnimation(GameObject highlightObj)
    {
        if (highlightObj == null) return;
        
        Image highlightImage = highlightObj.GetComponent<Image>();
        if (highlightImage != null)
        {
            // 重置透明度
            Color color = highlightImage.color;
            color.a = 1f;
            highlightImage.color = color;
            
            // 缩放动画
            highlightObj.transform.localScale = Vector3.zero;
            highlightObj.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
            
            // 闪烁动画
            highlightImage.DOFade(0.3f, 0.5f)
                .SetLoops(6, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }
    }

    /// <summary>
    /// 自动清除高亮
    /// </summary>
    private IEnumerator AutoClearHighlight()
    {
        yield return new WaitForSeconds(highlightDuration);
        ClearErrorHighlight();
    }

    /// <summary>
    /// 清除错误高亮
    /// </summary>
    private void ClearErrorHighlight()
    {
        if (currentHighlight != null)
        {
            // 添加消失动画
            currentHighlight.transform.DOScale(Vector3.zero, 0.2f)
                .SetEase(Ease.InBack)
                .OnComplete(() => {
                    Destroy(currentHighlight);
                    currentHighlight = null;
                });
        }
    }

    public void SetTicket(TicketData t)
    {
        // 清除可能存在的旧高亮
        ClearErrorHighlight();
        
        // 保存当前票数据用于后续查询
        currentTicketData = t;

        ResetVisuals();
        
        generatedSeatNumber = "SEAT:" + GenerateRandomSeatNumber();

        // 检查是否有特殊事件图片
        bool hasSpecialImage = TryApplySpecialEventImage(t);

        if (!hasSpecialImage)
        {
            // 如果没有特殊图片，显示正常文字信息和默认票面
            SetTextInformation(t);
            ShowDefaultTicketAppearance();
        }
        else
        {
            // 如果有特殊图片
            ShowImageTicketAppearance();
        }

        // 设置票根显示
        if (stub) stub.enabled = t.hasStub;
    }

    private bool TryApplySpecialEventImage(TicketData t)
    {
        if (t.special == SpecialEventType.None)
            return false;

        // 从特殊事件配置中获取图片
        Sprite mainImage = GetSpecialEventMainImage(t.special);
        
        // 只有当主票图片存在时才认为有特殊图片
        return mainImage != null;
    }

    private void ShowDefaultTicketAppearance()
    {
        // 显示默认票面：启用背景图片，禁用替换图片
        if (ticketBg != null)
        {
            ticketBg.enabled = true;
        }
        if (mainTicketImage != null)
        {
            mainTicketImage.enabled = false;
        }
        if (stubImage != null)
        {
            stubImage.enabled = false;
        }
    }

    private void ShowImageTicketAppearance()
    {
        // 显示图片票面：禁用默认背景，启用替换图片
        if (ticketBg != null)
        {
            ticketBg.enabled = false;
        }
        
        // 设置主票图片
        if (mainTicketImage != null)
        {
            Sprite mainImage = GetSpecialEventMainImage(currentTicketData.special);
            if (mainImage != null)
            {
                mainTicketImage.sprite = mainImage;
                mainTicketImage.enabled = true;
            }
        }
        
        // 设置票根图片
        if (stubImage != null)
        {
            Sprite stubImg = GetSpecialEventStubImage(currentTicketData.special);
            if (stubImg != null)
            {
                stubImage.sprite = stubImg;
                stubImage.enabled = true;
            }
        }
    }

    private void SetTextInformation(TicketData t)
    {
        // 设置主票信息
        if (titleText) 
        {
            titleText.text = FormatTitleWithEmphasizedLetters(t.filmTitle, true);
            titleText.enabled = true;
        }
        if (timeText)
        {
            timeText.text = $"screen . {t.showTime} . {t.showDate}";
            timeText.enabled = true;
        }
        if (seatText) 
        {
            seatText.text = generatedSeatNumber;
            seatText.enabled = true;
        }

        // 设置副券信息
        if (stubTitleText) 
        {
            stubTitleText.text = FormatTitleWithEmphasizedLetters(t.filmTitle, false);
            stubTitleText.enabled = true;
        }
        if (subTimeText) 
        {
            subTimeText.text = $"screen . {t.showTime} . {t.showDate}";
            subTimeText.enabled = true;
        }
        if (subSeatText) 
        {
            subSeatText.text = generatedSeatNumber;
            subSeatText.enabled = true;
        }
        if (priceText) 
        {
            priceText.text = "PRICE: $" + GetTicketPrice(t);
            priceText.enabled = true;
        }
    }

    /// <summary>
    /// 格式化标题，将首字母和空格后的第一个字母字号放大
    /// </summary>
    /// <param name="title">原始标题</param>
    /// <param name="isMainTicket">是否为主票（true=主票，false=副券）</param>
    private string FormatTitleWithEmphasizedLetters(string title, bool isMainTicket)
    {
        if (string.IsNullOrEmpty(title))
            return title;

        int normalSize = isMainTicket ? mainNormalFontSize : stubNormalFontSize;
        int emphasizedSize = isMainTicket ? mainEmphasizedFontSize : stubEmphasizedFontSize;

        StringBuilder formattedTitle = new StringBuilder();
        bool isFirstChar = true;

        for (int i = 0; i < title.Length; i++)
        {
            char currentChar = title[i];
            
            if (isFirstChar || (i > 0 && title[i - 1] == ' '))
            {
                // 首字母或空格后的第一个字母 - 使用大字号的富文本标签
                formattedTitle.Append($"<size={emphasizedSize}>{currentChar}</size>");
                isFirstChar = false;
            }
            else
            {
                // 其他字母 - 使用正常字号
                formattedTitle.Append(currentChar);
            }
        }

        return formattedTitle.ToString();
    }
    
    private Sprite GetSpecialEventMainImage(SpecialEventType eventType)
    {
        return GetSpecialEventImageFromConfig(eventType, true);
    }

    private Sprite GetSpecialEventStubImage(SpecialEventType eventType)
    {
        return GetSpecialEventImageFromConfig(eventType, false);
    }

    private Sprite GetSpecialEventImageFromConfig(SpecialEventType eventType, bool isMainTicket)
    {
        if (eventType == SpecialEventType.None)
            return null;

        // 获取当前关卡数据
        DaySchedule currentDay = GetCurrentDaySchedule();
        if (currentDay == null)
        {
            Debug.LogWarning("无法获取当前关卡数据");
            return null;
        }

        // 遍历所有场次和特殊事件配置，查找匹配的图片
        foreach (var show in currentDay.shows)
        {
            foreach (var specialEvent in show.specialEvents)
            {
                if (specialEvent.type == eventType)
                {
                    // 根据需求返回主票图片或票根图片
                    if (isMainTicket)
                    {
                        return specialEvent.mainTicketImage;
                    }
                    else
                    {
                        return specialEvent.stubImage;
                    }
                }
            }
        }
        return null;
    }

    private DaySchedule GetCurrentDaySchedule()
    {
        TicketGenerator ticketGenerator = FindObjectOfType<TicketGenerator>();
        if (ticketGenerator != null)
        {
            return ticketGenerator.GetCurrentLevel();
        }
        
        TicketQueueController queueController = FindObjectOfType<TicketQueueController>();
        if (queueController != null)
        {
            // 通过反射或其他方式获取 generator
            var generatorField = typeof(TicketQueueController).GetField("generator", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (generatorField != null)
            {
                TicketGenerator generator = generatorField.GetValue(queueController) as TicketGenerator;
                if (generator != null)
                {
                    return generator.GetCurrentLevel();
                }
            }
        }

        // 最后尝试直接查找 LevelDatabase
        LevelDatabase levelDatabase = FindObjectOfType<LevelDatabase>();
        if (levelDatabase != null && levelDatabase.levels != null && levelDatabase.levels.Length > 0)
        {
            return levelDatabase.levels[0]; // 返回第一个关卡作为备用
        }

        return null;
    }

    private int GetTicketPrice(TicketData t)
    {
        // 从关卡配置中获取实际票价
        DaySchedule currentDay = GetCurrentDaySchedule();
        if (currentDay != null)
        {
            // 查找当前电影场次的票价
            foreach (var show in currentDay.shows)
            {
                if (show.filmTitle == t.filmTitle && show.startTime == t.showTime)
                {
                    return show.ticketPrice;
                }
            }
        }

        // 默认票价
        return Random.Range(12, 21);
    }

    private string GenerateRandomSeatNumber()
    {
        char row = (char)('A' + Random.Range(0, 10)); // A-J排
        int number = Random.Range(1, 21); // 1-20号
        return $"{row}-{number:00}";
    }

    private void ResetVisuals()
    {
        // 重置背景 - 默认启用
        if (ticketBg)
        {
            ticketBg.enabled = true;
        }
        
        // 重置票根
        if (stub)
        {
            stub.enabled = true;
        }

        // 重置图片为禁用状态
        if (mainTicketImage != null)
        {
            mainTicketImage.enabled = false;
            mainTicketImage.sprite = null;
        }
        if (stubImage != null)
        {
            stubImage.enabled = false;
            stubImage.sprite = null;
        }
    }
    
    // 撕票成功时，添加视觉效果
    public void OnTearSuccess()
    {
        if (stub != null)
        {
            StartCoroutine(DropTicketStub());
        }
    }

    private IEnumerator DropTicketStub()
    {
        Vector3 targetPos = new Vector3(defaultStubPos.x - 100, defaultStubPos.y - 200f, defaultStubPos.z);
        float duration = 1f;
        
        stub.transform.DOLocalMove(targetPos, duration)
            .SetEase(Ease.InOutQuad)
            .SetUpdate(true);

        yield return new WaitForSeconds(duration);
    }
}