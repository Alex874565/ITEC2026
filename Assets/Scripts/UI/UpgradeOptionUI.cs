using TMPro;
using Unity.Netcode;

public class UpgradeOptionUI : OptionUI
{
    public TextMeshProUGUI ModifiersUI;

    public NetworkVariable<string> ModifiersText = new("");
    public NetworkVariable<int> TraitValue = new(0);
    public NetworkVariable<int> ModifierTypeValue = new(0);
    public NetworkVariable<int> UpgradeValue = new(0);

    private ModifierUpgrade UpgradeData;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        ModifiersText.OnValueChanged += UpdateModifiers;
        UpdateModifiers("", ModifiersText.Value);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        ModifiersText.OnValueChanged -= UpdateModifiers;
    }

    public override void Initialize()
    {
        UpgradeData = ModifiersManager.Instance.GenerateRandomUpgrade();

        TraitValue.Value = (int)UpgradeData.Trait;
        ModifierTypeValue.Value = (int)UpgradeData.Type;
        UpgradeValue.Value = UpgradeData.Value;
        Price.Value = UpgradeData.Price;

        ModifiersText.Value = GetUpgradeDisplayText(UpgradeData);
        CanActivate.Value = true;
        Clicks.Value = 0;
    }

    public void UpdateModifiers(string oldValue, string newValue)
    {
        if (ModifiersUI != null)
            ModifiersUI.text = newValue;
    }

    public override void Activate()
    {
        if (!IsServer)
            return;

        UpgradeData = new ModifierUpgrade
        {
            Trait = (Trait)TraitValue.Value,
            Type = (ModifierType)ModifierTypeValue.Value,
            Value = UpgradeValue.Value,
            Price = Price.Value
        };

        ModifiersManager.Instance.ApplyUpgrade(UpgradeData);

        base.Activate();
    }

    private string GetUpgradeDisplayText(ModifierUpgrade upgrade)
    {
        string signedValue = upgrade.Value > 0
            ? $"+{upgrade.Value}"
            : upgrade.Value.ToString();

        return $"{upgrade.Trait} {upgrade.Type} {signedValue}";
    }
}