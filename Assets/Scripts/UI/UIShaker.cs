using System.Collections;
using UnityEngine;

public class UIShaker : MonoBehaviour
{
    public static UIShaker Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    [Header("Target")]
    public RectTransform target;

    [Header("Shake")]
    public float duration = 0.2f;
    public float strength = 10f;

    [Header("Hover")]
    public float hoverAmplitude = 3f;
    public float hoverFrequency = 1f;

    Vector2 originalPos;
    Vector2 currentShakeOffset;
    bool isShaking = false;

    private void Start()
    {
        originalPos = target.anchoredPosition;
    }

    private void Update()
    {
        // Hover motion (always active)
        float x = Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude;
        float y = Mathf.Cos(Time.time * hoverFrequency * 0.8f) * hoverAmplitude;
        Vector2 hoverOffset = new Vector2(x, y);

        // Combine hover + shake
        target.anchoredPosition = originalPos + hoverOffset + currentShakeOffset;
    }

    public void Shake()
    {
        StopAllCoroutines();
        StartCoroutine(DoShake());
    }

    IEnumerator DoShake()
    {
        isShaking = true;
        float t = 0f;

        while (t < duration)
        {
            currentShakeOffset = Random.insideUnitCircle * strength;
            t += Time.deltaTime;
            yield return null;
        }

        currentShakeOffset = Vector2.zero;
        isShaking = false;
    }
}