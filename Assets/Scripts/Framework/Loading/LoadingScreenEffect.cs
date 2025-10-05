using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class FallingImage
{
    public RectTransform transform;
    public float fallSpeed;
    public float rotationSpeed;
    public Vector2 size;
}

public class LoadingScreenEffect : MonoBehaviour
{
    [Header("掉落图片设置")]
    [SerializeField] private GameObject imagePrefab; // 图片预制体
    [SerializeField] private Transform fallingImagesParent; // 父物体
    [SerializeField] private Sprite[] fallingSprites; // 可掉落的图片数组
    
    [Header("常规掉落参数")]
    [SerializeField] private int maxConcurrentImages = 10; // 最大同时掉落图片数量
    [SerializeField] private float spawnIntervalMin = 0.5f; // 生成间隔最小值
    [SerializeField] private float spawnIntervalMax = 2f; // 生成间隔最大值
    [SerializeField] private float fallSpeedMin = 50f; // 掉落速度最小值
    [SerializeField] private float fallSpeedMax = 150f; // 掉落速度最大值
    [SerializeField] private float rotationSpeedMin = -30f; // 旋转速度最小值
    [SerializeField] private float rotationSpeedMax = 30f; // 旋转速度最大值
    [SerializeField] private Vector2 sizeMin = new Vector2(30, 30); // 尺寸最小值
    [SerializeField] private Vector2 sizeMax = new Vector2(80, 80); // 尺寸最大值
    
    [Header("批量掉落参数")]
    [SerializeField] private bool enableBurstSpawn = true; // 是否启用批量生成
    [SerializeField] private int burstMinCount = 3; // 批量生成最小数量
    [SerializeField] private int burstMaxCount = 8; // 批量生成最大数量
    [SerializeField] private float burstIntervalMin = 3f; // 批量生成最小间隔
    [SerializeField] private float burstIntervalMax = 6f; // 批量生成最大间隔
    [SerializeField] private float burstSpread = 100f; // 批量生成时的水平散布范围
    
    [Header("屏幕边界")]
    [SerializeField] private float spawnMargin = 20f; // 生成边界距离（减小边界）
    [SerializeField] private float horizontalSpawnRange = 0.8f; // 水平生成范围比例 (0-1)
    
    private List<FallingImage> activeImages = new List<FallingImage>();
    private Canvas canvas;
    private RectTransform canvasRect;
    private bool isActive = false;
    private Coroutine spawnCoroutine;
    private Coroutine burstCoroutine;

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            canvasRect = canvas.GetComponent<RectTransform>();
        }
        
        // 如果没有指定父物体，使用自己
        if (fallingImagesParent == null)
        {
            fallingImagesParent = transform;
        }
    }

    private void OnEnable()
    {
        StartFallingEffect();
    }

    private void OnDisable()
    {
        StopFallingEffect();
    }

    /// <summary>
    /// 开始掉落效果
    /// </summary>
    public void StartFallingEffect()
    {
        if (isActive) return;
        
        isActive = true;
        spawnCoroutine = StartCoroutine(SpawnFallingImages());
        
        // 如果启用批量生成，启动批量协程
        if (enableBurstSpawn)
        {
            burstCoroutine = StartCoroutine(BurstSpawnRoutine());
        }
    }

    /// <summary>
    /// 停止掉落效果
    /// </summary>
    public void StopFallingEffect()
    {
        isActive = false;
        
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
        
        if (burstCoroutine != null)
        {
            StopCoroutine(burstCoroutine);
            burstCoroutine = null;
        }
        
        // 清除所有掉落中的图片
        ClearAllImages();
    }

    private IEnumerator SpawnFallingImages()
    {
        while (isActive)
        {
            if (activeImages.Count < maxConcurrentImages && fallingSprites.Length > 0)
            {
                SpawnSingleImage();
            }
            
            float interval = Random.Range(spawnIntervalMin, spawnIntervalMax);
            yield return new WaitForSeconds(interval);
        }
    }

    private IEnumerator BurstSpawnRoutine()
    {
        while (isActive)
        {
            yield return new WaitForSeconds(Random.Range(burstIntervalMin, burstIntervalMax));
            
            if (isActive && fallingSprites.Length > 0)
            {
                SpawnBurstImages();
            }
        }
    }

    /// <summary>
    /// 生成单个图片
    /// </summary>
    private void SpawnSingleImage()
    {
        if (imagePrefab == null || canvasRect == null) return;
        
        Vector2 spawnPosition = GetRandomSpawnPosition();
        CreateFallingImage(spawnPosition);
    }

    /// <summary>
    /// 批量生成图片
    /// </summary>
    private void SpawnBurstImages()
    {
        if (imagePrefab == null || canvasRect == null) return;
        
        int burstCount = Random.Range(burstMinCount, burstMaxCount + 1);
        float baseX = Random.Range(spawnMargin, canvasRect.rect.width - spawnMargin);
        
        for (int i = 0; i < burstCount; i++)
        {
            if (activeImages.Count >= maxConcurrentImages) break;
            
            // 在基础位置周围散布
            float offsetX = Random.Range(-burstSpread, burstSpread);
            Vector2 spawnPosition = new Vector2(
                Mathf.Clamp(baseX + offsetX, spawnMargin, canvasRect.rect.width - spawnMargin),
                canvasRect.rect.height + spawnMargin
            );
            
            CreateFallingImage(spawnPosition);
        }
        
        Debug.Log($"批量生成了 {burstCount} 个图片");
    }

    /// <summary>
    /// 创建掉落图片实例
    /// </summary>
    private void CreateFallingImage(Vector2 spawnPosition)
    {
        GameObject newImageObj = Instantiate(imagePrefab, fallingImagesParent);
        RectTransform imageRect = newImageObj.GetComponent<RectTransform>();
        Image imageComponent = newImageObj.GetComponent<Image>();
        
        if (imageComponent != null && fallingSprites.Length > 0)
        {
            // 随机选择一张图片
            Sprite randomSprite = fallingSprites[Random.Range(0, fallingSprites.Length)];
            imageComponent.sprite = randomSprite;
        }
        
        // 设置初始位置
        imageRect.anchoredPosition = spawnPosition;
        
        // 设置随机尺寸
        Vector2 imageSize = new Vector2(
            Random.Range(sizeMin.x, sizeMax.x),
            Random.Range(sizeMin.y, sizeMax.y)
        );
        imageRect.sizeDelta = imageSize;
        
        // 设置随机旋转
        float randomRotation = Random.Range(0f, 360f);
        imageRect.rotation = Quaternion.Euler(0, 0, randomRotation);
        
        // 创建 FallingImage 对象
        FallingImage fallingImage = new FallingImage
        {
            transform = imageRect,
            fallSpeed = Random.Range(fallSpeedMin, fallSpeedMax),
            rotationSpeed = Random.Range(rotationSpeedMin, rotationSpeedMax),
            size = imageSize
        };
        
        activeImages.Add(fallingImage);
    }

    /// <summary>
    /// 获取随机生成位置（减少位置偏移）
    /// </summary>
    private Vector2 GetRandomSpawnPosition()
    {
        if (canvasRect == null) return Vector2.zero;
        
        // 使用更集中的水平生成范围
        float canvasWidth = canvasRect.rect.width;
        float horizontalCenter = canvasWidth * 0.5f;
        float horizontalRange = canvasWidth * horizontalSpawnRange;
        
        float spawnX = horizontalCenter + Random.Range(-horizontalRange * 0.5f, horizontalRange * 0.5f);
        spawnX = Mathf.Clamp(spawnX, spawnMargin, canvasWidth - spawnMargin);
        
        return new Vector2(spawnX, canvasRect.rect.height + spawnMargin);
    }

    void Update()
    {
        if (!isActive) return;
        
        // 更新所有活动图片的位置
        for (int i = activeImages.Count - 1; i >= 0; i--)
        {
            FallingImage fallingImage = activeImages[i];
            
            // 更新位置
            fallingImage.transform.anchoredPosition -= new Vector2(0, fallingImage.fallSpeed * Time.deltaTime);
            
            // 更新旋转
            fallingImage.transform.Rotate(0, 0, fallingImage.rotationSpeed * Time.deltaTime);
            
            // 检查是否超出屏幕底部
            if (fallingImage.transform.anchoredPosition.y < -fallingImage.size.y - spawnMargin)
            {
                Destroy(fallingImage.transform.gameObject);
                activeImages.RemoveAt(i);
            }
        }
    }

    private void ClearAllImages()
    {
        foreach (FallingImage fallingImage in activeImages)
        {
            if (fallingImage.transform != null)
            {
                Destroy(fallingImage.transform.gameObject);
            }
        }
        activeImages.Clear();
    }
    
    void OnDestroy()
    {
        StopFallingEffect();
    }
}