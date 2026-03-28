using UnityEngine;
using DG.Tweening;
using TMPro;

public class TitleAnimation : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup; // attach CanvasGroup
    private Vector3 originalScale;

    [Header("Animation Settings")]
    [SerializeField] private float duration = 1f;
    [SerializeField] private Ease ease = Ease.OutBack;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        originalScale = transform.localScale;

        // Start invisible and scaled down
        canvasGroup.alpha = 0f;
        transform.localScale = Vector3.zero;
    }

    public void ShowTitle()
    {
        // Animate alpha
        canvasGroup.DOFade(1f, duration);

        // Animate scale to original
        transform.DOScale(originalScale, duration).SetEase(ease);
    }
}