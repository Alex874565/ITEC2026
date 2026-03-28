using UnityEngine;
using DG.Tweening;

public class TitleAnimation : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float duration = 1f; // animation duration
    [SerializeField] private Ease ease = Ease.OutBack;

    private Vector3 originalScale;

    public float Duration => duration; // <-- add this line

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        originalScale = transform.localScale;

        canvasGroup.alpha = 0f;
        transform.localScale = Vector3.zero;
    }

    public void Show(float delay = 0f)
    {
        canvasGroup.DOFade(1f, duration).SetDelay(delay);
        transform.DOScale(originalScale, duration).SetEase(ease).SetDelay(delay);
    }

    public void Hide(float delay = 0f)
    {
        canvasGroup.DOFade(0f, duration).SetDelay(delay);
        transform.DOScale(Vector3.zero, duration).SetEase(Ease.InBack).SetDelay(delay);
    }
}