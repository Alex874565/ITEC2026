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
    
    public event Action<int> OnCivilianClicked;

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

    public void CalculateScoreTip()
    {
        
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
        OnCivilianClicked?.Invoke(Index);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        CivilianUI.ShowTooltip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        CivilianUI.HideTooltip();
    }
}