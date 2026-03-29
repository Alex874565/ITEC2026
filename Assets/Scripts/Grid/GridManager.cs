using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GridManager : NetworkBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Setup")]
    [SerializeField] private GameObject civilianPrefab;
    [SerializeField] private GameObject placeholderPrefab;

    [Header("Animation")]
    [SerializeField] private float placeholderAppearStepDelay = 0.08f;
    [SerializeField] private float placeholderAppearDuration = 0.22f;
    [SerializeField] private float placeAnimDuration = 0.35f;
    [SerializeField] private float clearStepDelay = 0.08f;
    [SerializeField] private float clearAnimDuration = 0.25f;
    [SerializeField] private float appearStartScale = 0.75f;
    [SerializeField] private float clearEndScale = 0.75f;

    private Transform civilianContainer;
    private bool isClearingGrid;

    private Coroutine spawnPlaceholdersCoroutine;
    private int waveGeneration;
    private bool isSpawningPlaceholders;
    
    public int CiviliansTargetCount;

    public NetworkVariable<ActiveTraitCivilians> ActiveTraitCivilians = new(
        new ActiveTraitCivilians
        {
            TraitLists = new List<TraitCivilianList>()
        }
    );

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

        waveGeneration++;
        CiviliansTargetCount = newValue + 1;

        StopPlaceholderSpawnRoutine();

        spawnPlaceholdersCoroutine = StartCoroutine(SpawnPlaceholdersRoutine(CiviliansTargetCount, waveGeneration));
    }
    
    private void StopPlaceholderSpawnRoutine()
    {
        if (spawnPlaceholdersCoroutine != null)
        {
            StopCoroutine(spawnPlaceholdersCoroutine);
            spawnPlaceholdersCoroutine = null;
        }

        isSpawningPlaceholders = false;
    }

    private IEnumerator SpawnPlaceholdersRoutine(int count, int generation)
    {
        isSpawningPlaceholders = true;

        var parentNO = civilianContainer.GetComponent<NetworkObject>();
        if (parentNO == null)
        {
            Debug.LogError("civilianContainer must have a NetworkObject.");
            isSpawningPlaceholders = false;
            yield break;
        }

        for (int i = 0; i < count; i++)
        {
            if (!IsServer || isClearingGrid || generation != waveGeneration)
            {
                isSpawningPlaceholders = false;
                spawnPlaceholdersCoroutine = null;
                yield break;
            }

            GameObject placeholder = Instantiate(placeholderPrefab);
            placeholder.name = $"Placeholder_{i}";

            NetworkObject netObj = placeholder.GetComponent<NetworkObject>();
            CivilianSlot slot = placeholder.GetComponent<CivilianSlot>();

            if (netObj == null || slot == null)
            {
                Debug.LogError("Placeholder prefab must have NetworkObject and CivilianSlot.");
                Destroy(placeholder);
                isSpawningPlaceholders = false;
                spawnPlaceholdersCoroutine = null;
                yield break;
            }

            netObj.Spawn();
            netObj.TrySetParent(parentNO, false);

            slot.SlotIndex.Value = i;
            placeholder.transform.SetSiblingIndex(i);

            AnimatePlaceholderAppearClientRpc(new NetworkObjectReference(netObj), i);

            yield return new WaitForSeconds(placeholderAppearStepDelay);
        }

        isSpawningPlaceholders = false;
        spawnPlaceholdersCoroutine = null;
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

        CivilianBehaviour civilian = SpawnCivilian(trait, likedTraits, dislikedTraits);
        if (civilian == null)
            return;

        AddCivilianToList(civilian.gameObject);

        NetworkObject networkObject = civilian.GetComponent<NetworkObject>();
        ApplyTraitReactionsClientRpc(new NetworkObjectReference(networkObject));

        // Wave completion must be checked AFTER adding the civilian to the list.
        if (HasReachedTargetCount() && !isClearingGrid)
        {
            StartCoroutine(EndWaveAfterLastCivilianSpawnRoutine());
        }
    }
    
    private IEnumerator EndWaveAfterLastCivilianSpawnRoutine()
    {
        if (!IsServer || isClearingGrid)
            yield break;

        yield return new WaitForSeconds(placeAnimDuration + 0.1f);

        if (!IsServer || isClearingGrid)
            yield break;

        if (HasReachedTargetCount())
        {
            GameManager.Instance.EndWave();
        }
    }

    private IEnumerator ClearAfterLastCivilianSpawnRoutine()
    {
        if (!IsServer || isClearingGrid)
            yield break;

        // Wait for the place animation to finish.
        // Small extra buffer helps because the client animation starts after the RPC arrives.
        yield return new WaitForSeconds(placeAnimDuration + 0.1f);

        if (!IsServer || isClearingGrid)
            yield break;

        // Make sure the state is still valid.
        if (HasReachedTargetCount())
        {
            StartCoroutine(ClearGridRoutine(true));
        }
    }
    
    private ActiveTraitCivilians CloneData(ActiveTraitCivilians source)
    {
        var clone = new ActiveTraitCivilians
        {
            TraitLists = new List<TraitCivilianList>()
        };

        if (source.TraitLists == null)
            return clone;

        foreach (var traitList in source.TraitLists)
        {
            clone.TraitLists.Add(new TraitCivilianList
            {
                Trait = traitList.Trait,
                Civilians = traitList.Civilians != null
                    ? new List<NetworkObjectReference>(traitList.Civilians)
                    : new List<NetworkObjectReference>()
            });
        }

        return clone;
    }
    
    //[ClientRpc]
    public void AddCivilianToList(GameObject civilianObject)
    {
        CivilianBehaviour civilian = civilianObject.GetComponent<CivilianBehaviour>();
        NetworkObject networkObject = civilian.GetComponent<NetworkObject>();

        var data = CloneData(ActiveTraitCivilians.Value);

        if (data.TraitLists == null)
            data.TraitLists = new List<TraitCivilianList>();

        int traitIndex = GetTraitIndex(data, civilian.Trait.Value.Trait);

        if (traitIndex == -1)
        {
            TraitCivilianList newEntry = new TraitCivilianList
            {
                Trait = civilian.Trait.Value.Trait,
                Civilians = new List<NetworkObjectReference>()
            };

            newEntry.Civilians.Add(new NetworkObjectReference(networkObject));
            data.TraitLists.Add(newEntry);
        }
        else
        {
            var entry = data.TraitLists[traitIndex];

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

        int scoreChange = ModifiersManager.Instance.GetModifierDataForTrait(newCivilian.Trait.Value.Trait).Spawn;

        foreach (CivilianBehaviour existingCivilian in GetAllCivilians())
        {
            if (existingCivilian == null || existingCivilian == newCivilian)
                continue;

            scoreChange += existingCivilian.ReactToTrait(newCivilian.Trait.Value);
            scoreChange += newCivilian.ReactToTrait(existingCivilian.Trait.Value);
        }

        if (IsServer && scoreChange != 0)
        {
            GameManager.Instance.Score.Value += scoreChange;
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
            if (traitCivilians == null)
                continue;

            foreach (var civilianRef in traitCivilians)
            {
                if (civilianRef.TryGet(out NetworkObject networkObject))
                {
                    CivilianBehaviour behaviour = networkObject.GetComponent<CivilianBehaviour>();
                    if (behaviour != null)
                        civilians.Add(behaviour);
                }
            }
        }

        return civilians;
    }
    
    public float GetEstimatedClearDuration()
    {
        int childCount = civilianContainer != null ? civilianContainer.childCount : 0;
        return (childCount * clearStepDelay) + clearAnimDuration + 0.1f;
    }

    public CivilianBehaviour SpawnCivilian(Trait trait, Trait[] likedTraits, Trait[] dislikedTraits)
    {
        GameObject civilian = Instantiate(civilianPrefab);

        CivilianBehaviour behaviour = civilian.GetComponent<CivilianBehaviour>();
        NetworkObject civilianNO = civilian.GetComponent<NetworkObject>();

        if (behaviour == null || civilianNO == null)
        {
            Debug.LogError("Civilian prefab must have CivilianBehaviour and NetworkObject.");
            Destroy(civilian);
            return null;
        }

        civilianNO.Spawn();
        
        behaviour.Initialize(trait, likedTraits, dislikedTraits);

        NetworkObject slotNO = GetNextAvailableSlot();
        if (slotNO != null)
        {
            PlaceCivilianInSlot(civilianNO, slotNO);
        }
        else
        {
            var parentNO = civilianContainer.GetComponent<NetworkObject>();
            if (parentNO != null)
                civilianNO.TrySetParent(parentNO, false);

            Debug.LogWarning("No available slot found.");
        }

        return behaviour;
    }

    private NetworkObject GetNextAvailableSlot()
    {
        for (int i = 0; i < civilianContainer.childCount; i++)
        {
            Transform child = civilianContainer.GetChild(i);
            CivilianSlot slot = child.GetComponent<CivilianSlot>();

            if (slot != null)
            {
                NetworkObject no = child.GetComponent<NetworkObject>();
                if (no != null && no.IsSpawned)
                    return no;
            }
        }

        return null;
    }

    public void PlaceCivilianInSlot(NetworkObject civilianNO, NetworkObject slotNO)
    {
        if (!IsServer)
            return;

        var parentNO = civilianContainer.GetComponent<NetworkObject>();
        if (parentNO == null || civilianNO == null || slotNO == null)
            return;

        CivilianSlot slot = slotNO.GetComponent<CivilianSlot>();
        if (slot == null)
        {
            Debug.LogError("Slot object has no CivilianSlot.");
            return;
        }

        CivilianBehaviour civilian = civilianNO.GetComponent<CivilianBehaviour>();
        if (civilian == null)
        {
            Debug.LogError("Civilian object has no CivilianBehaviour.");
            return;
        }

        int slotIndex = slot.SlotIndex.Value;

        civilianNO.TrySetParent(parentNO, false);
        civilian.SlotIndex.Value = slotIndex;
        civilianNO.transform.SetSiblingIndex(slotIndex);

        slotNO.Despawn(true);

        AnimatePlacedCivilianClientRpc(new NetworkObjectReference(civilianNO), slotIndex);
    }

    public void ClearGrid()
    {
        if (!IsServer || isClearingGrid)
            return;

        StartCoroutine(ClearGridRoutine(false));
    }

    private IEnumerator ClearGridRoutine(bool endWaveAfterClear)
    {
        if (!IsServer || isClearingGrid)
            yield break;

        isClearingGrid = true;
        waveGeneration++;

        StopPlaceholderSpawnRoutine();

        int childCount = civilianContainer != null ? civilianContainer.childCount : 0;

        ClearGridClientRpc(clearStepDelay, clearAnimDuration);

        float waitTime = (childCount * clearStepDelay) + clearAnimDuration + 0.1f;
        yield return new WaitForSeconds(waitTime);

        for (int i = civilianContainer.childCount - 1; i >= 0; i--)
        {
            Transform child = civilianContainer.GetChild(i);
            if (child == null) continue;

            NetworkObject no = child.GetComponent<NetworkObject>();

            if (no != null && no.IsSpawned)
                no.Despawn(true);
            else
                Destroy(child.gameObject);
        }

        ActiveTraitCivilians.Value = new ActiveTraitCivilians
        {
            TraitLists = new List<TraitCivilianList>()
        };
        ActiveTraitCivilians.CheckDirtyState();

        CiviliansTargetCount = 0;
        isClearingGrid = false;

        if (endWaveAfterClear)
        {
            GameManager.Instance.EndWave();
        }
    }

    [ClientRpc]
    private void AnimatePlaceholderAppearClientRpc(NetworkObjectReference placeholderRef, int slotIndex)
    {
        StartCoroutine(AnimatePlaceholderAppearRoutine(placeholderRef, slotIndex));
    }

    private IEnumerator AnimatePlaceholderAppearRoutine(NetworkObjectReference placeholderRef, int slotIndex)
    {
        yield return null;
        yield return null;

        if (!placeholderRef.TryGet(out NetworkObject no))
            yield break;

        Transform t = no.transform;
        if (t == null)
            yield break;

        t.SetSiblingIndex(slotIndex);
        ForceRebuildGridLayout();

        t.DOKill();
        t.localScale = Vector3.one * appearStartScale;

        Image image = t.GetComponent<Image>();
        if (image != null)
        {
            Color c = image.color;
            c.a = 0f;
            image.color = c;
            image.DOFade(.5f, placeholderAppearDuration * 0.9f);
        }

        Sequence seq = DOTween.Sequence();
        seq.Append(t.DOScale(1.05f, placeholderAppearDuration * 0.65f).SetEase(Ease.OutCubic));
        seq.Append(t.DOScale(1f, placeholderAppearDuration * 0.35f).SetEase(Ease.OutSine));
    }

    [ClientRpc]
    private void AnimatePlacedCivilianClientRpc(NetworkObjectReference civilianRef, int slotIndex)
    {
        StartCoroutine(AnimatePlacedCivilianRoutine(civilianRef, slotIndex));
    }

    private IEnumerator AnimatePlacedCivilianRoutine(NetworkObjectReference civilianRef, int slotIndex)
    {
        yield return null;
        yield return null;

        if (!civilianRef.TryGet(out NetworkObject no))
            yield break;

        Transform t = no.transform;
        if (t == null)
            yield break;

        t.SetSiblingIndex(slotIndex);
        ForceRebuildGridLayout();

        t.DOKill();
        t.localScale = Vector3.one * 0.88f;

        Image image = t.GetComponent<Image>();
        if (image != null)
        {
            Color c = image.color;
            c.a = 0f;
            image.color = c;
            image.DOFade(1f, placeAnimDuration * 0.85f);
        }

        Sequence seq = DOTween.Sequence();
        seq.Append(t.DOScale(1.06f, placeAnimDuration * 0.65f).SetEase(Ease.OutCubic));
        seq.Append(t.DOScale(1f, placeAnimDuration * 0.35f).SetEase(Ease.OutSine));
    }

    [ClientRpc]
    private void ClearGridClientRpc(float stepDelay, float animDuration)
    {
        StartCoroutine(ClearGridAnimationRoutine(stepDelay, animDuration));
    }

    private IEnumerator ClearGridAnimationRoutine(float stepDelay, float animDuration)
    {
        ForceRebuildGridLayout();

        List<Transform> children = new List<Transform>();
        for (int i = 0; i < civilianContainer.childCount; i++)
        {
            children.Add(civilianContainer.GetChild(i));
        }

        for (int i = 0; i < children.Count; i++)
        {
            Transform child = children[i];
            if (child == null)
                continue;

            child.DOKill();

            Image image = child.GetComponent<Image>();
            if (image != null)
            {
                image.DOFade(0f, animDuration * 0.85f);
            }

            Sequence seq = DOTween.Sequence();
            seq.Append(child.DOScale(clearEndScale, animDuration * 0.4f).SetEase(Ease.OutSine));
            seq.Append(child.DOScale(0f, animDuration * 0.6f).SetEase(Ease.InCubic));

            yield return new WaitForSeconds(stepDelay);
        }
    }

    private void ForceRebuildGridLayout()
    {
        RectTransform rect = civilianContainer as RectTransform;
        if (rect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        }
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