using System;
using Unity.Netcode;

public class ModifiersManager : NetworkBehaviour
{
    public static ModifiersManager Instance { get; private set; }
    public NetworkList<TraitModifier> Modifiers;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        Modifiers = new NetworkList<TraitModifier>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return; // 🔥 IMPORTANT: server initializes

        Modifiers.Clear();

        foreach (Trait trait in Enum.GetValues(typeof(Trait)))
        {
            Modifiers.Add(new TraitModifier
            {
                Trait = trait,
                Positive = 1,
                Negative = 1
            });
        }
    }
    
    public TraitModifier GetModifierDataForTrait(Trait trait)
    {
        foreach (var modifier in Modifiers)
        {
            if (modifier.Trait == trait)
            {
                return modifier;
            }
        }
        return new TraitModifier { Trait = trait, Positive = 1, Negative = 1 };
    }
    
    public void UpdateModifier(Trait trait, int positiveChange, int negativeChange)
    {
        for (int i = 0; i < Modifiers.Count; i++)
        {
            if (Modifiers[i].Trait == trait)
            {
                TraitModifier updatedModifier = Modifiers[i];
                updatedModifier.Positive += positiveChange;
                updatedModifier.Negative += negativeChange;
                Modifiers[i] = updatedModifier; // Update the list with the modified struct
                break;
            }
        }
    }
}