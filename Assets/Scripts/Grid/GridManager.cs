using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GridManager : NetworkBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Setup")]
    [SerializeField] private GameObject civilianPrefab;

    private Transform civilianContainer;

    public int CiviliansTargetCount;

    public NetworkVariable<ActiveTraitCivilians> ActiveTraitCivilians = new(
        new ActiveTraitCivilians
        {
            TraitLists = new List<TraitCivilianList>()
        });

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        var grid = FindFirstObjectByType<CiviliansGrid>(FindObjectsInactive.Include);
        if (grid != null)
            civilianContainer = grid.transform;
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log($"GridManager spawned | IsServer={IsServer} IsHost={IsHost} IsSpawned={IsSpawned}");

        ActiveTraitCivilians.OnValueChanged += OnActiveTraitCiviliansChanged;

        if (IsServer)
        {
            var data = ActiveTraitCivilians.Value;
            if (data.TraitLists == null)
            {
                data.TraitLists = new List<TraitCivilianList>();
                ActiveTraitCivilians.Value = data;
                ActiveTraitCivilians.CheckDirtyState();
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        ActiveTraitCivilians.OnValueChanged -= OnActiveTraitCiviliansChanged;
    }

    private void OnActiveTraitCiviliansChanged(ActiveTraitCivilians oldValue, ActiveTraitCivilians newValue)
    {
        Debug.Log($"ActiveTraitCivilians updated. Total civilians: {GetAllCiviliansCount()}");
    }

    public void OnWaveStarted(int oldValue, int newValue)
    {
        if (!IsServer)
            return;

        CiviliansTargetCount = newValue + 1;
    }

    public void RequestAddCivilian(InventoryCivilianBehaviour inventoryCivilian)
    {
        if (inventoryCivilian == null)
        {
            Debug.LogError("RequestAddCivilian: inventoryCivilian is null.");
            return;
        }

        Trait trait = inventoryCivilian.Trait;
        Trait[] likedTraits = inventoryCivilian.LikedTraits?.ToArray() ?? new Trait[0];
        Trait[] dislikedTraits = inventoryCivilian.DislikedTraits?.ToArray() ?? new Trait[0];

        if (IsServer)
        {
            AddCivilianInternal(trait, likedTraits, dislikedTraits);
        }
        else
        {
            AddCivilianServerRpc(trait, likedTraits, dislikedTraits);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void AddCivilianServerRpc(Trait trait, Trait[] likedTraits, Trait[] dislikedTraits)
    {
        AddCivilianInternal(trait, likedTraits, dislikedTraits);
    }

    private void AddCivilianInternal(Trait trait, Trait[] likedTraits, Trait[] dislikedTraits)
    {
        if (!IsSpawned)
        {
            Debug.LogError("GridManager not spawned yet.");
            return;
        }

        if (!IsServer)
        {
            Debug.LogError("AddCivilianInternal called on non-server instance.");
            return;
        }

        if (civilianPrefab == null)
        {
            Debug.LogError("GridManager: civilianPrefab is missing.");
            return;
        }

        Debug.Log("Spawning civilian...");

        CivilianBehaviour civilian = SpawnCivilian(trait, likedTraits, dislikedTraits);
        AddCivilianToList(civilian.gameObject);

        NetworkObject networkObject = civilian.GetComponent<NetworkObject>();
        ApplyTraitReactionsClientRpc(new NetworkObjectReference(networkObject));
    }

    public void AddCivilianToList(GameObject civilianObject)
    {
        CivilianBehaviour civilian = civilianObject.GetComponent<CivilianBehaviour>();
        NetworkObject networkObject = civilian.GetComponent<NetworkObject>();

        var data = ActiveTraitCivilians.Value;
        if (data.TraitLists == null)
            data.TraitLists = new List<TraitCivilianList>();

        int traitIndex = GetTraitIndex(data, civilian.Trait.Trait);

        if (traitIndex == -1)
        {
            TraitCivilianList newEntry = new TraitCivilianList
            {
                Trait = civilian.Trait.Trait,
                Civilians = new List<NetworkObjectReference>()
            };

            newEntry.Civilians.Add(new NetworkObjectReference(networkObject));
            data.TraitLists.Add(newEntry);
        }
        else
        {
            TraitCivilianList entry = data.TraitLists[traitIndex];

            if (entry.Civilians == null)
                entry.Civilians = new List<NetworkObjectReference>();

            entry.Civilians.Add(new NetworkObjectReference(networkObject));
            data.TraitLists[traitIndex] = entry;
        }

        ActiveTraitCivilians.Value = data;
        ActiveTraitCivilians.CheckDirtyState();
    }

    [ClientRpc]
    private void ApplyTraitReactionsClientRpc(NetworkObjectReference newCivilianRef)
    {
        if (!newCivilianRef.TryGet(out NetworkObject networkObject))
        {
            Debug.LogWarning("Could not resolve new civilian NetworkObject on client.");
            return;
        }

        CivilianBehaviour newCivilian = networkObject.GetComponent<CivilianBehaviour>();
        if (newCivilian == null)
        {
            Debug.LogWarning("Resolved NetworkObject has no CivilianBehaviour.");
            return;
        }

        foreach (CivilianBehaviour existingCivilian in GetAllCivilians())
        {
            if (existingCivilian == null || existingCivilian == newCivilian)
                continue;

            existingCivilian.ReactToTrait(newCivilian.Trait);
            newCivilian.ReactToTrait(existingCivilian.Trait);
        }
    }

    public int GetAllCiviliansCount()
    {
        var data = ActiveTraitCivilians.Value;

        if (data.TraitLists == null)
            return 0;

        int total = 0;

        for (int i = 0; i < data.TraitLists.Count; i++)
        {
            total += data.TraitLists[i].Civilians?.Count ?? 0;
        }

        return total;
    }

    public List<CivilianBehaviour> GetAllCivilians()
    {
        var data = ActiveTraitCivilians.Value;
        List<CivilianBehaviour> civilians = new List<CivilianBehaviour>();

        if (data.TraitLists == null)
            return civilians;

        for (int i = 0; i < data.TraitLists.Count; i++)
        {
            var traitCivilians = data.TraitLists[i].Civilians;

            if (traitCivilians != null)
            {
                foreach (var civilianRef in traitCivilians)
                {
                    if (civilianRef.TryGet(out NetworkObject networkObject))
                    {
                        CivilianBehaviour behaviour = networkObject.GetComponent<CivilianBehaviour>();
                        if (behaviour != null)
                        {
                            civilians.Add(behaviour);
                        }
                    }
                }
            }
        }

        return civilians;
    }

    public CivilianBehaviour SpawnCivilian(Trait trait, Trait[] likedTraits, Trait[] dislikedTraits)
    {
        Debug.Log("Spawning civilian on server...");

        GameObject civilian = Instantiate(civilianPrefab);
        CivilianBehaviour behaviour = civilian.GetComponent<CivilianBehaviour>();
        NetworkObject networkObject = civilian.GetComponent<NetworkObject>();

        behaviour.Initialize(trait, likedTraits, dislikedTraits);

        networkObject.Spawn();

        if (civilianContainer != null)
        {
            var parentNetworkObject = civilianContainer.GetComponent<NetworkObject>();
            if (parentNetworkObject != null)
            {
                networkObject.TrySetParent(parentNetworkObject, false);
            }
            else
            {
                Debug.LogError("civilianContainer must have a spawned NetworkObject for synced parenting.");
            }
        }

        return behaviour;
    }

    public List<NetworkObjectReference> GetTraitCivilians(Trait trait)
    {
        var data = ActiveTraitCivilians.Value;
        int traitIndex = GetTraitIndex(data, trait);

        if (traitIndex == -1)
            return new List<NetworkObjectReference>();

        return data.TraitLists[traitIndex].Civilians ?? new List<NetworkObjectReference>();
    }

    public bool HasReachedTargetCount()
    {
        return GetAllCiviliansCount() >= CiviliansTargetCount;
    }

    private int GetTraitIndex(ActiveTraitCivilians data, Trait trait)
    {
        if (data.TraitLists == null)
            return -1;

        for (int i = 0; i < data.TraitLists.Count; i++)
        {
            if (data.TraitLists[i].Trait.Equals(trait))
                return i;
        }

        return -1;
    }
}