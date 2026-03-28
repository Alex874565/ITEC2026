using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class CivilianBehaviour : NetworkBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TraitStruct Trait;
    public NetworkList<TraitStruct> LikedTraits = new NetworkList<TraitStruct>();
    public NetworkList<TraitStruct> DislikedTraits = new NetworkList<TraitStruct>();
    
    public NetworkVariable<int> Happiness = new NetworkVariable<int>(0);
    
    public CivilianUI CivillianUI;
    public NetworkVariable<int> SlotIndex = new NetworkVariable<int>(-1);
    
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

        CivillianUI.Initialize(Trait.Trait, liked, disliked);
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
    
    public void UpdateHappiness(int change)
    {
        if (!IsServer) return;
        Happiness.Value += change;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        CivillianUI.ShowTooltip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        CivillianUI.HideTooltip();
    }
}