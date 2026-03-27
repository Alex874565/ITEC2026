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
    
    public void ReactToTrait(TraitStruct trait)
    {
        if(!IsOwner) return;
        
        if (LikedTraits.Contains(trait))
        {
            UpdateHappiness(ModifiersManager.GetModifierDataForTrait(trait.Trait).Positive);
        }
        else if (DislikedTraits.Contains(trait))
        {
            UpdateHappiness(ModifiersManager.GetModifierDataForTrait(trait.Trait).Negative);
        }
    }
    
    public void UpdateHappiness(int change)
    {
        if(!IsOwner) return;
        
        Happiness.Value += change;
    }
}