using Unity.Netcode;
public class CivilianBehaviour : NetworkBehaviour
{
    public TraitStruct Trait;
    public NetworkList<TraitStruct> LikedTraits = new NetworkList<TraitStruct>();
    public NetworkList<TraitStruct> DislikedTraits = new NetworkList<TraitStruct>();
    
    public NetworkVariable<int> Happiness = new NetworkVariable<int>(0);
    
    public ModifiersManager ModifiersManager = new ModifiersManager();

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

    public void Initialize(InventoryCivilianBehaviour inventoryCivilianBehaviour)
    {
        Trait = new TraitStruct
        {
            Trait = inventoryCivilianBehaviour.Trait
        };
        
        foreach (var trait in inventoryCivilianBehaviour.LikedTraits)
        {
            LikedTraits.Add(new TraitStruct
            {
                Trait = trait
            });
        }
        
        foreach (var trait in inventoryCivilianBehaviour.DislikedTraits)
        {
            DislikedTraits.Add(new TraitStruct
            {
                Trait = trait
            });
        }
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