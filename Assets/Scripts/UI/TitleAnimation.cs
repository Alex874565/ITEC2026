using UnityEngine;
using DG.Tweening;
using TMPro;

public class TitleAnimation : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup; // assign in inspector
    private Vector3 originalScale;

    [Header("Animation Settings")]
    [SerializeField] private float duration = 1f;
    [SerializeField] private Ease ease = Ease.OutBack;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        originalScale = transform.localScale;

        // Start hidden
        canvasGroup.alpha = 0f;
        transform.localScale = Vector3.zero;
    }

    public void ShowTitle(float delay = 0f)
    {
        // Fade in
        canvasGroup.DOFade(1f, duration).SetDelay(delay);
        // Scale up
        transform.DOScale(originalScale, duration).SetEase(ease).SetDelay(delay);
    }

    public void HideTitle(float delay = 0f)
    {
        // Fade out
        canvasGroup.DOFade(0f, duration).SetDelay(delay);
        // Scale down
        transform.DOScale(Vector3.zero, duration).SetEase(Ease.InBack).SetDelay(delay);
    }
}