using UnityEngine;

public class UIButtonVisual : MonoBehaviour
{
    public GameObject box;
    public CanvasGroup dotsGroup;
    public RectTransform dotsTransform;

    [Header("Pulse")]
    public float pulseSpeed = 10f;
    public float pulseAmount = 0.02f;

    private Vector3 baseScale;
    private bool isActive = false;

    void Start()
    {
        baseScale = dotsTransform.localScale;
        SetActive(false);
    }

    void Update()
    {
        if (isActive)
        {
            float scale = 1 + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            dotsTransform.localScale = baseScale * scale;
        }
    }

    public void SetActive(bool value)
    {
        isActive = value;

        box.SetActive(value);
        dotsGroup.alpha = value ? 1f : 0f;
    }
}