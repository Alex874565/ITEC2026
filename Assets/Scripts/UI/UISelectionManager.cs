using UnityEngine;

public class UISelectionManager : MonoBehaviour
{
    public static UISelectionManager Instance;

    private UIButtonVisual current;

    void Awake()
    {
        Instance = this;
    }

    public void SetActiveButton(UIButtonVisual newButton)
    {
        if (current != null)
            current.SetActive(false);

        current = newButton;
        current.SetActive(true);
    }

    public void SetDefault(UIButtonVisual defaultButton)
    {
        current = defaultButton;
        current.SetActive(true);
    }
}