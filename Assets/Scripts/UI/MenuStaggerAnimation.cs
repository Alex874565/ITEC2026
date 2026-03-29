using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class MenuStaggerAnimation : MonoBehaviour
{
    private Dictionary<RectTransform, Vector3> originalScales = new Dictionary<RectTransform, Vector3>();
    [SerializeField] private RectTransform moneyElement;
    [SerializeField] private RectTransform timeElement;
    [SerializeField] private RectTransform lvlElement;
    [Header("Buttons To Animate")]
    [SerializeField] private RectTransform[] buttons;

    public RectTransform[] Buttons => buttons;

    [Header("Animation Settings")]
    [SerializeField] private float duration = 0.4f;
    public float Duration => duration;
    [SerializeField] private float staggerDelay = 0.08f;

    [Header("Audio (Optional)")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip popSound;
    [Range(0f, 1f)] [SerializeField] private float volume = 1f;

    private Sequence currentSequence;

    private void Awake()
    {
        foreach (var button in buttons)
        {
            originalScales[button] = button.localScale; // save original
            button.localScale = Vector3.zero; // hide
        }
    }

    private void Start()
    {
        // if (sfxSource == null)
        // {
        //     sfxSource = ServiceLocator.Instance.AudioManager.SfxSource;
        // }
    }

    public void OpenMenu(System.Action onLvlShown = null, System.Action onTimeShown = null)
    {
        currentSequence?.Kill();
        currentSequence = DOTween.Sequence().SetUpdate(true);

        for (int i = 0; i < buttons.Length; i++)
        {
            RectTransform button = buttons[i];
            button.localScale = Vector3.zero;
            
            float appearanceTime = i * staggerDelay;

            // ONLY play sound if a clip is actually assigned in the Inspector
            if (popSound != null && sfxSource != null)
            {
                currentSequence.InsertCallback(appearanceTime, () => 
                {
                    sfxSource.PlayOneShot(popSound, volume);
                });
            }

            Tween scaleTween = button.DOScale(originalScales[button], duration)
                .SetEase(Ease.OutBack);

            currentSequence.Insert(appearanceTime, scaleTween);

            // Callbacks for Money/Time
            if (button == lvlElement && onLvlShown != null)
            {
                scaleTween.OnComplete(() => onLvlShown.Invoke());
            }

            // if (button == timeElement && onTimeShown != null)
            //     scaleTween.OnComplete(() => onTimeShown.Invoke());
        }
    }

    public void CloseMenu(System.Action onComplete = null)
    {
        currentSequence?.Kill();

        currentSequence = DOTween.Sequence().SetUpdate(true);

        for (int i = buttons.Length - 1; i >= 0; i--)
        {
            RectTransform button = buttons[i];
            float delay = (buttons.Length - 1 - i) * staggerDelay;

            currentSequence.Insert(delay,
                button.DOScale(0f, duration * 0.6f)
                    .SetEase(Ease.InBack)
                    .SetUpdate(true)); // <-- unscaled
        }

        if (onComplete != null)
            currentSequence.OnComplete(() => onComplete.Invoke());
    }
}