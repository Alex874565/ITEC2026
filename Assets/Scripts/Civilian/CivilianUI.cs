using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CivilianUI : MonoBehaviour
{
    public TraitSpritesDatabase Database;
    public Image Image;
    public Vector2 NeutralRange;
    public GameObject Tooltip;
    public TextMeshProUGUI TraitContainer;
    public List<TextMeshProUGUI> LikedTraitsContainers;
    public List<TextMeshProUGUI> DislikedTraitsContainers;
    public TextMeshProUGUI ScoreText;
    public GameObject ScoreObject;
    public Animator animator;

    private Trait _trait;
    private TraitSprites _traitSprites;

    public float minActDelay = 2f;
    public float maxActDelay = 5f;

    private Coroutine actRoutine;

    private void OnEnable()
    {
        actRoutine = StartCoroutine(ActRoutine());
    }

    private void OnDisable()
    {
        if (actRoutine != null)
            StopCoroutine(actRoutine);
    }

    private System.Collections.IEnumerator ActRoutine()
    {
        while (true)
        {
            float delay = Random.Range(minActDelay, maxActDelay);
            yield return new WaitForSeconds(delay);

            if (animator != null)
                animator.SetTrigger("Act");
        }
    }
    
    public void Initialize(Trait trait, Trait[] likedTraits, Trait[] dislikedTraits)
    {
        _trait = trait;
        _traitSprites = Database.GetTraitSprites(trait);

        ApplyVisuals();

        TraitContainer.text = _trait.ToString();

        for (int i = 0; i < LikedTraitsContainers.Count; i++)
        {
            if (i < likedTraits.Length)
                LikedTraitsContainers[i].text = likedTraits[i].ToString();
            else
                LikedTraitsContainers[i].text = "";
        }

        for (int i = 0; i < DislikedTraitsContainers.Count; i++)
        {
            if (i < dislikedTraits.Length)
                DislikedTraitsContainers[i].text = dislikedTraits[i].ToString();
            else
                DislikedTraitsContainers[i].text = "";
        }
    }

    private void ApplyVisuals()
    {
        if (_traitSprites == null)
        {
            Debug.LogWarning($"No TraitSprites found for trait {_trait}", this);
            return;
        }

        if (Image != null && _traitSprites.TraitSprite != null)
            Image.sprite = _traitSprites.TraitSprite;

        if (animator != null && _traitSprites.TraitAnimator != null)
            animator.runtimeAnimatorController = _traitSprites.TraitAnimator;
    }

    public void PlayAct()
    {
        if (animator != null)
            animator.SetTrigger("Act");
    }

    public void UpdateScoreText(int value)
    {
        ScoreText.text = value.ToString();
    }

    public void SetScoreTextActive(bool active)
    {
        ScoreObject.SetActive(active);
    }

    public void ShowTooltip()
    {
        Tooltip.SetActive(true);
    }

    public void HideTooltip()
    {
        Tooltip.SetActive(false);
    }
}