using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class CivilianBehaviour : NetworkBehaviour
{
    public TraitStruct Trait;
    public NetworkList<TraitStruct> LikedTraits = new NetworkList<TraitStruct>();
    public NetworkList<TraitStruct> DislikedTraits = new NetworkList<TraitStruct>();
    
    public NetworkVariable<int> Happiness = new NetworkVariable<int>(0);
    
    public ModifiersManager ModifiersManager;

    public CivilianUI CivillianUI;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        CivillianUI.Initialize(Trait.Trait);
        
        Happiness.OnValueChanged += (oldValue, newValue) =>
        {
            CivillianUI.UpdateImage(newValue);
        };
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
        if (!IsOwner) return 0;

        int change;
        
        if (LikedTraits.Contains(trait))
        {
            change = ModifiersManager.GetModifierDataForTrait(trait.Trait).Positive;
            UpdateHappiness(change);
        }
        else if (DislikedTraits.Contains(trait))
        {
            change = ModifiersManager.GetModifierDataForTrait(trait.Trait).Negative;
            UpdateHappiness(change);
        }
        else
        {
            change = 0;
        }

        return change;
    }
    
    public void UpdateHappiness(int change)
    {
        if(!IsOwner) return;
        
        Happiness.Value += change;
    }
}