using Unity.Netcode;

public class PlayerBehaviour : NetworkBehaviour
{
    public PlayerInventory Inventory;
    
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        
        Inventory = FindFirstObjectByType<PlayerInventory>();
    }
    
}