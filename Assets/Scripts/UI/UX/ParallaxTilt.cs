using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using System.Collections;

public class ParallaxTilt : MonoBehaviour
{
    [System.Serializable]
    public class Layer
    {
        public RectTransform rect;
        [Tooltip("视差强度（像素）：背景小、前景大，例如 5~40")]
        public float parallax = 15f;
        [Tooltip("限制该图层的最大位移（像素）")]
        public float maxOffset = 40f;

        // 内部：缓存
        [HideInInspector] public Vector2 initialPos;
        [HideInInspector] public Tweener posTweener;
    }

    [Header("Targets")]
    public RectTransform root;
    public List<Layer> layers = new List<Layer>();

    [Header("Tilt (Rotation)")]
    [Tooltip("绕 X 轴最大旋转角（上下点头）")]
    public float maxTiltX = 6f;
    [Tooltip("绕 Y 轴最大旋转角（左右点头）")]
    public float maxTiltY = 8f;
    public float rotFollowTime = 0.18f;
    public Ease rotEase = Ease.OutQuad;

    [Header("Root Move (Position)")]
    [Tooltip("根节点随鼠标移动的最大位移（像素）")]
    public Vector2 rootMoveRange = new Vector2(18f, 12f);
    public float moveFollowTime = 0.14f;
    public Ease moveEase = Ease.OutQuad;

    [Header("Parallax (Layers)")]
    public float parallaxFollowTime = 0.16f;
    public Ease parallaxEase = Ease.OutQuad;
    [Tooltip("视差方向与鼠标同向（关=反向）")]
    public bool parallaxSameDirection = false;

    [Header("Scale (Optional)")]
    [Tooltip("是否启用微缩放")]
    public bool enableScale = false;
    [Tooltip("最大缩放偏移，例如 0.02 表示 1%~2%")]
    public float maxScaleDelta = 0.015f;
    public float scaleFollowTime = 0.2f;
    public Ease scaleEase = Ease.OutQuad;

    [Header("Behavior")]
    [Tooltip("鼠标到屏幕边缘时是否达到最大倾斜/位移")]
    public bool edgeIsMax = true;
    
    [Header("Animation Control")]
    [Tooltip("入场动画控制器")]
    public Animator entranceAnimator;
    [Tooltip("是否等待入场动画结束")]
    public bool waitForEntranceAnimation = true;
    [Tooltip("动画结束后是否禁用动画控制器")]
    public bool disableAnimatorAfterAnimation = true;
    
    private bool _entranceAnimationFinished = false;

    // 缓存
    private Quaternion _initialRot;
    private Vector2 _initialAnchoredPos;
    private Vector3 _initialScale;

    private Tweener _rotTweener;
    private Tweener _moveTweener;
    private Tweener _scaleTweener;

    void Awake()
    {
        if (root == null) root = (RectTransform)transform;

        _initialRot = root.localRotation;
        _initialAnchoredPos = root.anchoredPosition;
        _initialScale = root.localScale;

        // 预创建根节点旋转/平移/缩放的 Tweener（添加 null 检查）
        if (root != null)
        {
            _rotTweener = root.DOLocalRotateQuaternion(_initialRot, rotFollowTime)
                .SetEase(rotEase).SetAutoKill(false).Pause();
            _moveTweener = root.DOAnchorPos(_initialAnchoredPos, moveFollowTime)
                .SetEase(moveEase).SetAutoKill(false).Pause();
            
            if (enableScale)
            {
                _scaleTweener = root.DOScale(_initialScale, scaleFollowTime)
                    .SetEase(scaleEase).SetAutoKill(false).Pause();
            }
        }

        // 记录各图层初始位置并创建 Tweener（添加更严格的检查）
        for (int i = layers.Count - 1; i >= 0; i--)
        {
            var L = layers[i];
            if (L.rect == null)
            {
                Debug.LogWarning($"图层 {i} 的 RectTransform 为 null，已从列表中移除");
                layers.RemoveAt(i);
                continue;
            }
            
            L.initialPos = L.rect.anchoredPosition;
            L.posTweener = L.rect.DOAnchorPos(L.initialPos, parallaxFollowTime)
                .SetEase(parallaxEase).SetAutoKill(false).Pause();
        }
        
        _entranceAnimationFinished = !waitForEntranceAnimation;
        
        // 如果没有指定动画控制器，尝试自动获取
        if (entranceAnimator == null)
        {
            entranceAnimator = GetComponent<Animator>();
            if (entranceAnimator == null)
            {
                entranceAnimator = GetComponentInChildren<Animator>();
            }
        }
    }
    
    void Start()
    {
        // 如果设置了入场动画，启动计时器
        if (waitForEntranceAnimation && entranceAnimator != null)
        {
            StartCoroutine(WaitForEntranceAnimation());
        }
        else if (!waitForEntranceAnimation)
        {
            // 如果不等待动画，立即禁用动画控制器
            DisableAnimator();
        }
    }
    
    private IEnumerator WaitForEntranceAnimation()
    {
        if (entranceAnimator != null)
        {

            AnimatorStateInfo stateInfo = entranceAnimator.GetCurrentAnimatorStateInfo(0);
            yield return new WaitForSeconds(stateInfo.length);
            
            FinishEntranceAnimation();
        }
        else
        {
            // 如果没有动画控制器，直接结束
            _entranceAnimationFinished = true;
            Debug.Log("没有找到动画控制器，直接开始视差效果");
        }
    }
    
    private void FinishEntranceAnimation()
    {
        _entranceAnimationFinished = true;
        
        // 禁用动画控制器
        if (disableAnimatorAfterAnimation && entranceAnimator != null)
        {
            entranceAnimator.enabled = false;
            Debug.Log("入场动画结束，已禁用动画控制器，开始视差效果");
        }
        else
        {
            Debug.Log("入场动画结束，开始视差效果");
        }
    }
    
    /// <summary>
    /// 禁用动画控制器
    /// </summary>
    private void DisableAnimator()
    {
        if (entranceAnimator != null)
        {
            entranceAnimator.enabled = false;
        }
        _entranceAnimationFinished = true;
    }

void Update()
    {
        if (!_entranceAnimationFinished) return;
        
        // 1) 计算鼠标归一化坐标（中心为 0,0）
        Vector2 mouse = Input.mousePosition;
        float nx, ny;
        if (edgeIsMax)
        {
            nx = Mathf.Clamp((mouse.x / Screen.width) * 2f - 1f, -1f, 1f);
            ny = Mathf.Clamp((mouse.y / Screen.height) * 2f - 1f, -1f, 1f);
        }
        else
        {
            nx = Mathf.Clamp((mouse.x - Screen.width * 0.5f) / (Screen.width * 0.5f), -1f, 1f);
            ny = Mathf.Clamp((mouse.y - Screen.height * 0.5f) / (Screen.height * 0.5f), -1f, 1f);
        }

        // 2) 根节点目标旋转（微透视）- 添加 null 检查
        float tx = -ny * maxTiltX;
        float ty = -nx * maxTiltY;
        Quaternion targetRot = Quaternion.Euler(tx, ty, 0f);

        if (_rotTweener != null && root != null)
        {
            _rotTweener.ChangeEndValue(targetRot, true).Play();
        }

        // 3) 根节点目标平移（整体轻微跟随）- 添加 null 检查
        Vector2 targetRootPos = _initialAnchoredPos + new Vector2(nx * rootMoveRange.x, ny * rootMoveRange.y);
        if (_moveTweener != null && root != null)
        {
            _moveTweener.ChangeEndValue(targetRootPos, true).Play();
        }

        // 4) 视差（各图层位移）- 添加更严格的 null 检查
        float dir = parallaxSameDirection ? 1f : -1f;
        Vector2 mouseVec = new Vector2(nx, ny) * dir;

        for (int i = 0; i < layers.Count; i++)
        {
            var L = layers[i];
            // 检查所有可能为 null 的对象
            if (L.rect == null || L.posTweener == null)
            {
                // 如果图层无效，尝试重新初始化或跳过
                if (L.rect == null)
                {
                    Debug.LogWarning($"图层 {i} 的 RectTransform 为 null，跳过更新");
                    continue;
                }
                else if (L.posTweener == null)
                {
                    Debug.LogWarning($"图层 {i} 的 Tweener 为 null，尝试重新创建");
                    L.posTweener = L.rect.DOAnchorPos(L.initialPos, parallaxFollowTime)
                        .SetEase(parallaxEase).SetAutoKill(false).Pause();
                    if (L.posTweener == null) continue; // 如果创建失败，跳过
                }
            }

            Vector2 offset = mouseVec * L.parallax;
            offset.x = Mathf.Clamp(offset.x, -L.maxOffset, L.maxOffset);
            offset.y = Mathf.Clamp(offset.y, -L.maxOffset, L.maxOffset);

            Vector2 targetPos = L.initialPos + offset;
            
            // 安全地调用 ChangeEndValue
            try
            {
                L.posTweener.ChangeEndValue(targetPos, true).Play();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"更新图层 {i} 的 Tweener 时出错: {e.Message}");
                // 尝试重新创建 Tweener
                L.posTweener = L.rect.DOAnchorPos(targetPos, parallaxFollowTime)
                    .SetEase(parallaxEase).SetAutoKill(false).Pause();
            }
        }

        // 5) 可选：缩放微动 - 添加 null 检查
        if (enableScale && _scaleTweener != null && root != null)
        {
            float strength = Mathf.Clamp01(new Vector2(nx, ny).magnitude);
            float scaleDelta = maxScaleDelta * strength;
            Vector3 targetScale = _initialScale * (1f + scaleDelta);
            _scaleTweener.ChangeEndValue(targetScale, true).Play();
        }
    }

    /// <summary>
    /// 强制结束入场动画（用于调试或特殊情况）
    /// </summary>
    public void ForceFinishEntranceAnimation()
    {
        StopAllCoroutines();
        FinishEntranceAnimation();
        Debug.Log("强制结束入场动画");
    }

    /// <summary>
    /// 重新启用动画（用于场景重置等）
    /// </summary>
    public void ReenableAnimator()
    {
        if (entranceAnimator != null)
        {
            entranceAnimator.enabled = true;
            _entranceAnimationFinished = false;
            StartCoroutine(WaitForEntranceAnimation());
        }
    }

#if UNITY_EDITOR
    [ContextMenu("强制开始视差效果")]
    private void EditorForceStartParallax()
    {
        ForceFinishEntranceAnimation();
    }
#endif

    void OnDisable()
    {
        // 释放中间态，避免停用后残留
        if (_rotTweener != null) _rotTweener.Kill(false);
        if (_moveTweener != null) _moveTweener.Kill(false);
        if (_scaleTweener != null) _scaleTweener.Kill(false);
        foreach (var L in layers)
            if (L.posTweener != null) L.posTweener.Kill(false);
    }
}