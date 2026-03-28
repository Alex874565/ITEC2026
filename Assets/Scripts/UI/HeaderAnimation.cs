using UnityEngine;
using DG.Tweening;

public class HeaderAnimator : MonoBehaviour
{
    [SerializeField] private RectTransform headerRect;
    [SerializeField] private float dropDuration = 0.5f;
    [SerializeField] private float overshootAmount = 20f; // how far it goes past final
    [SerializeField] private float bounceBackAmount = 10f; // bounce up a little

    private Vector2 finalPos;

    private void Awake()
    {
        finalPos = headerRect.anchoredPosition;
    }

    public void ShowHeader()
    {
        // start off-screen above
        headerRect.anchoredPosition = new Vector2(finalPos.x, finalPos.y + 200f);

        // Sequence for spring effect
        Sequence seq = DOTween.Sequence();

        // 1️⃣ Move past final position (overshoot)
        seq.Append(headerRect.DOAnchorPosY(finalPos.y - overshootAmount, dropDuration * 0.6f).SetEase(Ease.OutCubic));

        // 2️⃣ Move a little up (bounce)
        seq.Append(headerRect.DOAnchorPosY(finalPos.y + bounceBackAmount, dropDuration * 0.2f).SetEase(Ease.InOutCubic));

        // 3️⃣ Settle to final position
        seq.Append(headerRect.DOAnchorPosY(finalPos.y, dropDuration * 0.2f).SetEase(Ease.OutCubic));
    }
}