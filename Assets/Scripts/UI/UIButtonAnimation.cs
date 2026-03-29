using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class UIButtonAnimation : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [Header("Animation Settings")]
    [SerializeField] private float hoverMultiplier = 1.08f;
    [SerializeField] private float pressMultiplier = 0.9f;
    [SerializeField] private float duration = 0.15f;

    private Vector3 originalScale;
    private bool hasCustomBaseScale = false;

    private Tween currentTween;
    private Button button;
    private bool isHovered;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        // If parent DIDN'T set scale (no stagger), capture it here
        if (!hasCustomBaseScale)
        {
            originalScale = transform.localScale;
        }
    }

    /// <summary>
    /// Called by parent (MenuStaggerAnimation)
    /// </summary>
    public void SetBaseScale(Vector3 scale)
    {
        originalScale = scale;
        hasCustomBaseScale = true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button != null && !button.interactable)
            return;

        isHovered = true;
        currentTween?.Kill();

        currentTween = transform.DOScale(originalScale * hoverMultiplier, duration)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        currentTween?.Kill();

        currentTween = transform.DOScale(originalScale, duration)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (button != null && !button.interactable)
            return;

        currentTween?.Kill();

        currentTween = transform.DOScale(originalScale * pressMultiplier, duration * 0.6f)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (button != null && !button.interactable)
            return;

        currentTween?.Kill();

        float targetMultiplier = isHovered ? hoverMultiplier : 1f;

        currentTween = transform.DOScale(originalScale * targetMultiplier, duration)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);
    }

    private void OnDisable()
    {
        currentTween?.Kill();
        transform.localScale = originalScale;
    }
}