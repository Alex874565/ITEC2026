using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIButtonPressEffect : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform target; // the visual root
    [SerializeField] private Image image; // background image (for color flash)

    [Header("Settings")]
    [SerializeField] private float pressScale = 0.7f;
    [SerializeField] private float duration = 0.15f;
    [SerializeField] private Color flashColor = Color.white;

    private Vector3 originalScale;
    private Color originalColor;

    private void Awake()
    {
        if (target == null)
            target = transform as RectTransform;

        if (image != null)
            originalColor = image.color;

        originalScale = target.localScale;
    }

    public void PlayPress()
    {
        // Kill previous animations (important if spam clicking)
        target.DOKill();
        if (image != null) image.DOKill();

        Sequence seq = DOTween.Sequence();

        // Scale down
        seq.Append(target.DOScale(originalScale * pressScale, duration).SetEase(Ease.OutQuad));

        // Scale back up
        seq.Append(target.DOScale(originalScale, duration).SetEase(Ease.OutBack));

        // Color flash (parallel)
        if (image != null)
        {
            seq.Join(
                image.DOColor(flashColor, duration)
                     .OnComplete(() =>
                     {
                         image.DOColor(originalColor, duration);
                     })
            );
        }
    }
}