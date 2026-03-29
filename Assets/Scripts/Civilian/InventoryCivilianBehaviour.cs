using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class InventoryCivilianBehaviour : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public int Index;
    public CivilianUI CivilianUI;
    
    public Trait Trait;
    public List<Trait> LikedTraits;
    public List<Trait> DislikedTraits;

    public AudioClip click;
    
    public event Action<int> OnCivilianClicked;
    
    private PlayerInventory _playerInventory;

    private void Awake()
    {
        LikedTraits = new List<Trait>();
        DislikedTraits = new List<Trait>();
    }

    private void Start()
    {
        SelectRandomTraits();
        Debug.Log(Trait);
    }

    private void SelectRandomTraits()
    {
        Trait = (Trait)UnityEngine.Random.Range(0, Enum.GetValues(typeof(Trait)).Length);
        int likedCount = UnityEngine.Random.Range(0, 4); // Randomly like 1 to 3 traits
        int dislikedCount = UnityEngine.Random.Range(0, 4);
        
        while(LikedTraits.Count < likedCount)
        {
            Trait randomTrait = (Trait)UnityEngine.Random.Range(0, Enum.GetValues(typeof(Trait)).Length);
            if(randomTrait != Trait && !LikedTraits.Contains(randomTrait))
            {
                LikedTraits.Add(randomTrait);
            }
        }

        while (DislikedTraits.Count < dislikedCount)
        {
            Trait randomTrait = (Trait)UnityEngine.Random.Range(0, Enum.GetValues(typeof(Trait)).Length);
            if (randomTrait != Trait && !LikedTraits.Contains(randomTrait) && !DislikedTraits.Contains(randomTrait))
            {
                DislikedTraits.Add(randomTrait);
            }
        }
        
        CivilianUI.Initialize(Trait, LikedTraits.ToArray(), DislikedTraits.ToArray());
    }
    
    private bool isBeingDestroyed;

    public void SetScoreTipActive(bool active)
    {
        CivilianUI.SetScoreTextActive(active);
    }
    
    public void SetScoreTip(int score)
    {
        CivilianUI.UpdateScoreText(score);
    }

    public int CalculateScoreTip()
    {
        List<CivilianBehaviour> behaviours = GridManager.Instance.GetAllCivilians();
        Debug.Log(behaviours.Count);
        Debug.Log($"Calculating score tip for trait: {Trait}");
        Debug.Log($"Modifiers for trait: {ModifiersManager.Instance.GetModifierDataForTrait(Trait).Spawn}");
        int score = ModifiersManager.Instance.GetModifierDataForTrait(Trait).Spawn;
        foreach (var behaviour in behaviours)
        {
            if(behaviour == this) continue;
            behaviour.SetScoreTipActive(true);
            behaviour.SetScoreTip(behaviour.ReactToTrait(Trait));
            score += ReactToTrait(behaviour.Trait.Value.Trait);
        }

        if (_playerInventory == null)
        {
            _playerInventory = FindFirstObjectByType<PlayerInventory>();
        }

        List<InventoryCivilianBehaviour> inventoryCivilians = _playerInventory.CivilianBehaviours;
        foreach (var behaviour in inventoryCivilians)
        {
            behaviour.SetScoreTipActive(true);
            behaviour.SetScoreTip(behaviour.ReactToTrait(Trait));
        }

        return score;
    }
    
    public int ReactToTrait(Trait trait)
    {
        int change = 0;

        if (LikedTraits.Contains(trait))
        {
            change = ModifiersManager.Instance.GetModifierDataForTrait(trait).Positive;
        }
        else if (DislikedTraits.Contains(trait))
        {
            change = ModifiersManager.Instance.GetModifierDataForTrait(trait).Negative;
        }

        return change;
    }

    public void DisableScoreTips()
    {
        List<CivilianBehaviour> behaviours = GridManager.Instance.GetAllCivilians();
        foreach (var behaviour in behaviours)
        {
            if(!behaviour) continue;
            behaviour.SetScoreTipActive(false);
        }
        
        List<InventoryCivilianBehaviour> inventoryCivilians = _playerInventory.CivilianBehaviours;
        foreach (var behaviour in inventoryCivilians)
        {
            if(!behaviour) continue;
            behaviour.SetScoreTipActive(false);
        }
    }
    
    public void DestroySelf()
    {
        if (isBeingDestroyed)
            return;

        isBeingDestroyed = true;
        Destroy(gameObject);
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        gameObject.transform.localScale = Vector3.one * 1.2f; // Reset scale on click
        DisableScoreTips();
        SetScoreTipActive(false);
        AudioManager.Instance.PlaySFX(click);
        OnCivilianClicked?.Invoke(Index);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        gameObject.transform.localScale = Vector3.one * 1.1f; // Slightly enlarge the civilian for visual feedback
        CivilianUI.ShowTooltip();
        SetScoreTip(CalculateScoreTip());
        SetScoreTipActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        gameObject.transform.localScale = Vector3.one; // Reset scale
        CivilianUI.HideTooltip();
        DisableScoreTips();
        SetScoreTipActive(false);
    }
}