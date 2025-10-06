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
    }

    public void SetTicket(TicketData t)
    {
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
            timeText.text = $"screen . {t.showTime} . {FormatDateWithEmphasizedMonth(t.showDate, true)}";
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
            subTimeText.text = $"screen . {t.showTime} . {FormatDateWithEmphasizedMonth(t.showDate, false)}";
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

    /// <summary>
    /// 格式化日期，将月份的首字母字号放大
    /// </summary>
    /// <param name="date">原始日期（格式：Month/yy/dd）</param>
    /// <param name="isMainTicket">是否为主票（true=主票，false=副券）</param>
    private string FormatDateWithEmphasizedMonth(string date, bool isMainTicket)
    {
        if (string.IsNullOrEmpty(date))
            return date;
        
        int emphasizedSize = stubEmphasizedFontSize;

        // 分割日期部分
        string[] parts = date.Split('/');
        if (parts.Length == 3)
        {
            string month = parts[0];
            string year = parts[1];
            string day = parts[2];

            // 只对月份的首字母进行强调
            if (month.Length > 0)
            {
                string emphasizedMonth = $"<size={emphasizedSize}>{month[0]}</size>" + month.Substring(1);
                return $"{emphasizedMonth}/{year}/{day}";
            }
        }

        // 如果格式不正确，返回原始日期
        return date;
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
            return ticketGenerator.GetCurrentDay();
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
                    return generator.GetCurrentDay();
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