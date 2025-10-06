using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

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
        // 设置基本信息
        if (titleText) 
        {
            titleText.text = t.filmTitle;
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
            stubTitleText.text = t.filmTitle;
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

        Debug.Log($"未找到特殊事件 {eventType} 的图片配置");
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