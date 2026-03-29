using System;
using Unity.Netcode;
using UnityEngine;

public class ModifiersManager : NetworkBehaviour
{
    public static ModifiersManager Instance { get; private set; }

        public NetworkList<TraitModifier> Modifiers;

    [SerializeField] private AnimationCurve spawnInitialCurve = new AnimationCurve(
        new Keyframe(0f, 10f),
        new Keyframe(10f, 13f),
        new Keyframe(20f, 17f),
        new Keyframe(30f, 22f)
    );

    [SerializeField] private AnimationCurve positiveInitialCurve = new AnimationCurve(
        new Keyframe(0f, 1f),
        new Keyframe(10f, 2f),
        new Keyframe(20f, 3f),
        new Keyframe(30f, 5f)
    );

    [SerializeField] private AnimationCurve negativeInitialCurve = new AnimationCurve(
        new Keyframe(0f, 1f),
        new Keyframe(10f, 2f),
        new Keyframe(20f, 3f),
        new Keyframe(30f, 4f)
    );

    [SerializeField] private AnimationCurve spawnChangeCurve = new AnimationCurve(
        new Keyframe(0f, 1f),
        new Keyframe(15f, 2f),
        new Keyframe(30f, 3f)
    );

    [SerializeField] private AnimationCurve positiveChangeCurve = new AnimationCurve(
        new Keyframe(0f, 1f),
        new Keyframe(15f, 1.5f),
        new Keyframe(30f, 2f)
    );

    [SerializeField] private AnimationCurve negativeChangeCurve = new AnimationCurve(
        new Keyframe(0f, 1f),
        new Keyframe(15f, 1.5f),
        new Keyframe(30f, 2f)
    );

    [SerializeField] private AnimationCurve upgradePriceCurve = new AnimationCurve(
        new Keyframe(1f, 10f),
        new Keyframe(5f, 25f),
        new Keyframe(10f, 55f),
        new Keyframe(15f, 95f),
        new Keyframe(20f, 150f),
        new Keyframe(25f, 220f),
        new Keyframe(30f, 300f)
    );

    [Header("Random change multipliers")]
    [SerializeField] private float[] changeMultipliers = { 0.5f, 1.5f };

    [Header("Spawn bounds")]
    [SerializeField] private int minSpawn = 0;
    [SerializeField] private int maxSpawn = 999;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Modifiers = new NetworkList<TraitModifier>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            InitializeModifiers();
        }
    }

    private void InitializeModifiers()
    {
        Modifiers.Clear();

        Trait[] traits = (Trait[])Enum.GetValues(typeof(Trait));

        for (int i = 0; i < traits.Length; i++)
        {
            float t = GetTraitPosition(i);

            int spawnValue = Mathf.Clamp(
                Mathf.RoundToInt(Mathf.Abs(spawnInitialCurve.Evaluate(t))),
                minSpawn,
                maxSpawn
            );

            int positiveValue = Mathf.Max(
                0,
                Mathf.RoundToInt(Mathf.Abs(positiveInitialCurve.Evaluate(t)))
            );

            int negativeValue = Mathf.Min(
                0,
                -Mathf.RoundToInt(Mathf.Abs(negativeInitialCurve.Evaluate(t)))
            );

            Modifiers.Add(new TraitModifier
            {
                Trait = traits[i],
                Spawn = spawnValue,
                Positive = positiveValue,
                Negative = negativeValue
            });
        }
    }

    public TraitModifier GetModifierDataForTrait(Trait trait)
    {
        foreach (var modifier in Modifiers)
        {
            if (modifier.Trait == trait)
                return modifier;
        }

        return new TraitModifier
        {
            Trait = trait,
            Spawn = 1,
            Positive = 1,
            Negative = -1
        };
    }

    public void UpdateModifier(Trait trait, int spawnDelta, int positiveDelta, int negativeDelta)
    {
        if (!IsServer) return;

        for (int i = 0; i < Modifiers.Count; i++)
        {
            if (Modifiers[i].Trait != trait)
                continue;

            TraitModifier updated = Modifiers[i];

            updated.Spawn = Mathf.Clamp(updated.Spawn + spawnDelta, minSpawn, maxSpawn);
            updated.Positive = Mathf.Max(0, updated.Positive + Mathf.Max(0, positiveDelta));
            updated.Negative = Mathf.Min(0, updated.Negative + Mathf.Min(0, negativeDelta));

            Modifiers[i] = updated;
            return;
        }
    }

    public void ApplyUpgrade(ModifierUpgrade upgrade)
    {
        if (!IsServer) return;

        int spawn = 0;
        int positive = 0;
        int negative = 0;

        switch (upgrade.Type)
        {
            case ModifierType.Spawn:
                spawn = upgrade.Value;
                break;

            case ModifierType.Like:
                positive = Mathf.Max(0, upgrade.Value);
                break;

            case ModifierType.Dislike:
                negative = Mathf.Min(0, upgrade.Value);
                break;
        }

        UpdateModifier(upgrade.Trait, spawn, positive, negative);
    }

    public ModifierUpgrade GenerateRandomUpgrade()
    {
        if (!IsServer || Modifiers.Count == 0)
            return default;

        int index = UnityEngine.Random.Range(0, Modifiers.Count);
        ModifierType type = GetRandomModifierType();

        TraitModifier modifier = Modifiers[index];

        float t = GetCurrentWave();
        float multiplier = GetRandomMultiplier();

        int value = 0;

        switch (type)
        {
            case ModifierType.Spawn:
            {
                int baseChange = Mathf.Abs(Mathf.RoundToInt(spawnChangeCurve.Evaluate(t)));
                value = Mathf.Max(1, Mathf.RoundToInt(baseChange * multiplier));

                if (UnityEngine.Random.value < 0.5f)
                    value *= -1;

                break;
            }

            case ModifierType.Like:
            {
                int baseChange = Mathf.Abs(Mathf.RoundToInt(positiveChangeCurve.Evaluate(t)));
                value = Mathf.Max(1, Mathf.RoundToInt(baseChange * multiplier));
                break;
            }

            case ModifierType.Dislike:
            {
                int baseChange = Mathf.Abs(Mathf.RoundToInt(negativeChangeCurve.Evaluate(t)));
                value = -Mathf.Max(1, Mathf.RoundToInt(baseChange * multiplier));
                break;
            }
        }

        int price = GetUpgradePrice() + Mathf.RoundToInt(Mathf.Abs(value) * 0.5f);

        return new ModifierUpgrade
        {
            Trait = modifier.Trait,
            Type = type,
            Value = value
        };
    }

    public void SelectRandomChange()
    {
        if (!IsServer || Modifiers.Count == 0)
            return;

        int randomTraitIndex = UnityEngine.Random.Range(0, Modifiers.Count);
        ModifierType randomModifierType = GetRandomModifierType();

        ApplyRandomChange(randomTraitIndex, randomModifierType);
    }

    public void SelectRandomChange(ModifierType modifierType)
    {
        if (!IsServer || Modifiers.Count == 0)
            return;

        int randomTraitIndex = UnityEngine.Random.Range(0, Modifiers.Count);
        ApplyRandomChange(randomTraitIndex, modifierType);
    }

    public void SelectRandomChange(Trait trait)
    {
        if (!IsServer || Modifiers.Count == 0)
            return;

        int index = GetTraitIndex(trait);
        if (index < 0) return;

        ModifierType randomModifierType = GetRandomModifierType();
        ApplyRandomChange(index, randomModifierType);
    }

    public void SelectRandomChange(Trait trait, ModifierType modifierType)
    {
        if (!IsServer || Modifiers.Count == 0)
            return;

        int index = GetTraitIndex(trait);
        if (index < 0) return;

        ApplyRandomChange(index, modifierType);
    }

    private void ApplyRandomChange(int modifierIndex, ModifierType modifierType)
    {
        TraitModifier modifier = Modifiers[modifierIndex];

        float t = GetCurrentWave();
        float multiplier = GetRandomMultiplier();

        int spawnDelta = 0;
        int positiveDelta = 0;
        int negativeDelta = 0;

        switch (modifierType)
        {
            case ModifierType.Spawn:
            {
                int baseChange = Mathf.Abs(Mathf.RoundToInt(spawnChangeCurve.Evaluate(t)));
                int finalChange = Mathf.Max(1, Mathf.RoundToInt(baseChange * multiplier));

                if (UnityEngine.Random.value < 0.5f)
                    finalChange *= -1;

                spawnDelta = finalChange;
                break;
            }

            case ModifierType.Like:
            {
                int baseChange = Mathf.Abs(Mathf.RoundToInt(positiveChangeCurve.Evaluate(t)));
                int finalChange = Mathf.Max(1, Mathf.RoundToInt(baseChange * multiplier));

                positiveDelta = finalChange;
                break;
            }

            case ModifierType.Dislike:
            {
                int baseChange = Mathf.Abs(Mathf.RoundToInt(negativeChangeCurve.Evaluate(t)));
                int finalChange = Mathf.Max(1, Mathf.RoundToInt(baseChange * multiplier));

                negativeDelta = -finalChange;
                break;
            }
        }

        UpdateModifier(modifier.Trait, spawnDelta, positiveDelta, negativeDelta);
    }

    private ModifierType GetRandomModifierType()
    {
        ModifierType[] values = (ModifierType[])Enum.GetValues(typeof(ModifierType));
        return values[UnityEngine.Random.Range(0, values.Length)];
    }

    private int GetTraitIndex(Trait trait)
    {
        for (int i = 0; i < Modifiers.Count; i++)
        {
            if (Modifiers[i].Trait == trait)
                return i;
        }

        return -1;
    }

    private float GetTraitPosition(int index)
    {
        return index;
    }

    private float GetCurrentWave()
    {
        if (GameManager.Instance == null)
            return 1f;

        return Mathf.Clamp(GameManager.Instance.CurrentWave.Value, 1, 30);
    }

    private float GetRandomMultiplier()
    {
        if (changeMultipliers == null || changeMultipliers.Length == 0)
            return 1f;

        return changeMultipliers[UnityEngine.Random.Range(0, changeMultipliers.Length)];
    }

    public int GetUpgradePrice()
    {
        return Mathf.Max(
            0,
            Mathf.RoundToInt(Mathf.Abs(upgradePriceCurve.Evaluate(GetCurrentWave())))
        );
    }

    public int GetUpgradePrice(int level)
    {
        level = Mathf.Clamp(level, 1, 30);
        return Mathf.Max(0, Mathf.RoundToInt(Mathf.Abs(upgradePriceCurve.Evaluate(level))));
    }
}