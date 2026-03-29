using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class CivilianBehaviour : NetworkBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TraitStruct Trait;
    public NetworkList<TraitStruct> LikedTraits = new NetworkList<TraitStruct>();
    public NetworkList<TraitStruct> DislikedTraits = new NetworkList<TraitStruct>();
    
    public NetworkVariable<int> Happiness = new NetworkVariable<int>(0);
    
    public CivilianUI CivilianUI;
    public NetworkVariable<int> SlotIndex = new NetworkVariable<int>(-1);
    
    private PlayerInventory _playerInventory;
    
    public override void OnNetworkSpawn()
    {
        SlotIndex.OnValueChanged += (_, newIndex) => ApplySlotIndex(newIndex);
        ApplySlotIndex(SlotIndex.Value);

        Happiness.OnValueChanged += (_, newValue) =>
        {
            // CivillianUI.UpdateImage(newValue);
        };

        LikedTraits.OnListChanged += _ => RefreshUI();
        DislikedTraits.OnListChanged += _ => RefreshUI();

        RefreshUI();
    }

    private void ApplySlotIndex(int slotIndex)
    {
        if (slotIndex < 0 || transform.parent == null)
            return;

        int targetIndex = Mathf.Clamp(slotIndex, 0, transform.parent.childCount - 1);
        transform.SetSiblingIndex(targetIndex);
    }

    private void RefreshUI()
    {
        Trait[] liked = new Trait[LikedTraits.Count];
        Trait[] disliked = new Trait[DislikedTraits.Count];

        for (int i = 0; i < LikedTraits.Count; i++)
            liked[i] = LikedTraits[i].Trait;

        for (int i = 0; i < DislikedTraits.Count; i++)
            disliked[i] = DislikedTraits[i].Trait;

        CivilianUI.Initialize(Trait.Trait, liked, disliked);
    }

    public void Initialize(Trait trait, Trait[] likedTraits, Trait[] dislikedTraits)
    {
        Trait = new TraitStruct
        {
            Trait = trait
        };
        
        foreach (var likedTrait in likedTraits)
        {
            LikedTraits.Add(new TraitStruct
            {
                Trait = likedTrait
            });
        }
        
        foreach (var dislikedTrait in dislikedTraits)
        {
            DislikedTraits.Add(new TraitStruct
            {
                Trait = dislikedTrait
            });
        }
        
        Debug.Log($"Civilian initialized with trait: {Trait.Trait}, liked traits: {LikedTraits.Count}, disliked traits: {DislikedTraits.Count}");
    }
    
    public int ReactToTrait(TraitStruct trait)
    {
        if (!IsServer) return 0;

        int change = 0;

        if (LikedTraits.Contains(trait))
        {
            change = ModifiersManager.Instance.GetModifierDataForTrait(trait.Trait).Positive;
        }
        else if (DislikedTraits.Contains(trait))
        {
            change = ModifiersManager.Instance.GetModifierDataForTrait(trait.Trait).Negative;
        }

        Happiness.Value += change;
        return change;
    }

    public int ReactToTrait(Trait trait)
    {
        TraitStruct traitStruct = new TraitStruct
        {
            Trait = trait
        };
        int change = 0;

        if (LikedTraits.Contains(traitStruct))
        {
            change = ModifiersManager.Instance.GetModifierDataForTrait(trait).Positive;
        }
        else if (DislikedTraits.Contains(traitStruct))
        {
            change = ModifiersManager.Instance.GetModifierDataForTrait(trait).Negative;
        }

        Happiness.Value += change;
        return change;
    }

    public void SetScoreTipActive(bool active)
    {
        CivilianUI.SetScoreTextActive(active);
    }

    public void DisableScoreTips()
    {
        List<CivilianBehaviour> behaviours = GridManager.Instance.GetAllCivilians();
        foreach (var behaviour in behaviours)
        {
            behaviour.SetScoreTipActive(false);
        }
        
        List<InventoryCivilianBehaviour> inventoryCivilians = _playerInventory.CivilianBehaviours;
        foreach (var behaviour in inventoryCivilians)
        {
            behaviour.SetScoreTipActive(false);
        }
    }
    
    public int CalculateScoreTip()
    {
        List<CivilianBehaviour> behaviours = GridManager.Instance.GetAllCivilians();
        int score = 0;
        foreach (var behaviour in behaviours)
        {
            if(behaviour == this) continue;
            behaviour.SetScoreTipActive(true);
            behaviour.SetScoreTip(behaviour.ReactToTrait(Trait.Trait));
            score += ReactToTrait(behaviour.Trait.Trait);
        }

        if (_playerInventory == null)
        {
            _playerInventory = FindFirstObjectByType<PlayerInventory>();
        }

        List<InventoryCivilianBehaviour> inventoryCivilians = _playerInventory.CivilianBehaviours;
        foreach (var behaviour in inventoryCivilians)
        {
            behaviour.SetScoreTipActive(true);
            behaviour.SetScoreTip(behaviour.ReactToTrait(behaviour.Trait));
        }

        return score;
    }
    
    public void SetScoreTip(int value)
    {
        CivilianUI.UpdateScoreText(value);
    }
    
    public void UpdateHappiness(int change)
    {
        if (!IsServer) return;
        Happiness.Value += change;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        CivilianUI.ShowTooltip();
        SetScoreTip(CalculateScoreTip());
        SetScoreTipActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        DisableScoreTips();
        CivilianUI.HideTooltip();
    }
}