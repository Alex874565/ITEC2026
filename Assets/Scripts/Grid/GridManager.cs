using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GridManager : NetworkBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Setup")] [SerializeField] private GameObject civilianPrefab;
    [SerializeField] private Transform civilianContainer;

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
    }

    public override void OnNetworkSpawn()
    {
        ActiveTraitCivilians.OnValueChanged += OnActiveTraitCiviliansChanged;

        if (IsServer)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CurrentWave.OnValueChanged += OnWaveStarted;
            }

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

        if (IsServer && GameManager.Instance != null)
        {
            GameManager.Instance.CurrentWave.OnValueChanged -= OnWaveStarted;
        }
    }

    private void OnActiveTraitCiviliansChanged(ActiveTraitCivilians oldValue, ActiveTraitCivilians newValue)
    {
        Debug.Log($"ActiveTraitCivilians updated. Total civilians: {GetAllCiviliansCount()}");
    }

    public void OnWaveStarted(int oldValue, int newValue)
    {
        if (!IsServer)
            return;

        CiviliansTargetCount = newValue;
    }

    public void AddCivilian(InventoryCivilianBehaviour inventoryCivilian)
    {
        if (!IsServer)
            return;

        if (civilianPrefab == null)
        {
            Debug.LogError("GridManager: civilianPrefab is missing.");
            return;
        }

        CivilianBehaviour civilian = SpawnCivilian(inventoryCivilian);
        AddCivilianToList(civilian.gameObject);
    }

    public void AddCivilianToList(GameObject civilianPrefab)
    {
        CivilianBehaviour civilian = civilianPrefab.GetComponent<CivilianBehaviour>();
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

    public CivilianBehaviour SpawnCivilian(InventoryCivilianBehaviour inventoryCivilian)
    {
        GameObject civilian = Instantiate(civilianPrefab, civilianContainer);

        CivilianBehaviour behaviour = civilian.GetComponent<CivilianBehaviour>();
        
        NetworkObject networkObject = civilian.GetComponent<NetworkObject>();
        
        networkObject.Spawn();
        
        behaviour.Initialize(inventoryCivilian);
        
        foreach(CivilianBehaviour existingCivilian in GetAllCivilians())
        {
            int scoreChange = 0;
            if (existingCivilian != behaviour)
            {
                scoreChange += existingCivilian.ReactToTrait(behaviour.Trait);
                scoreChange += behaviour.ReactToTrait(existingCivilian.Trait);
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