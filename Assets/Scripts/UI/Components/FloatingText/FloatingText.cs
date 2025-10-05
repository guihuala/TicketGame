using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FloatingText : MonoBehaviour
{
    [SerializeField] private Text text;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float floatDistance = 100f;
    [SerializeField] private float duration = 1.5f;

    public void Initialize(string displayText, Color color, Vector3 worldPosition)
    {
        // 设置文本和颜色
        text.text = displayText;
        text.color = color;

        // 设置位置
        transform.position = worldPosition;

        // 浮动动画
        transform.DOMoveY(transform.position.y + floatDistance, duration)
            .SetEase(Ease.OutCubic);

        // 淡出动画
        canvasGroup.DOFade(0f, duration)
            .SetEase(Ease.InQuad)
            .OnComplete(() => Destroy(gameObject));
    }
}