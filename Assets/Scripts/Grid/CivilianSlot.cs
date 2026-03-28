using Unity.Netcode;
using UnityEngine;

public class CivilianSlot : NetworkBehaviour
{
    public NetworkVariable<int> SlotIndex = new NetworkVariable<int>();
}