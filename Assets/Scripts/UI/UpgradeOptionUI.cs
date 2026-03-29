using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class UpgradeOptionUI : OptionUI
{
    [SerializeField] private int slotIndex;

    public List<ModifierUpgrade> UpgradesData = new();
    public int Price;

    public TextMeshProUGUI ModifierUI;
    public TextMeshProUGUI PriceUI;

    private NetworkVariable<int> clicksVar;
    private NetworkVariable<bool> usedVar;
    private NetworkVariable<int> priceVar;
    private NetworkList<ModifierUpgrade> upgradeList;

    public override void Initialize()
    {
        ForceReset();

        clicksVar = null;
        usedVar = null;
        priceVar = null;
        upgradeList = null;

        switch (slotIndex)
        {
            case 1:
                clicksVar = UpgradesManager.Instance.Slot1Clicks;
                usedVar = UpgradesManager.Instance.Slot1Used;
                priceVar = UpgradesManager.Instance.UpgradePrice1;
                upgradeList = UpgradesManager.Instance.UpgradeSlot1;
                break;
            case 2:
                clicksVar = UpgradesManager.Instance.Slot2Clicks;
                usedVar = UpgradesManager.Instance.Slot2Used;
                priceVar = UpgradesManager.Instance.UpgradePrice2;
                upgradeList = UpgradesManager.Instance.UpgradeSlot2;
                break;
            case 3:
                clicksVar = UpgradesManager.Instance.Slot3Clicks;
                usedVar = UpgradesManager.Instance.Slot3Used;
                priceVar = UpgradesManager.Instance.UpgradePrice3;
                upgradeList = UpgradesManager.Instance.UpgradeSlot3;
                break;
        }

        if (clicksVar == null || usedVar == null || priceVar == null || upgradeList == null)
        {
            Debug.LogError($"[UpgradeOptionUI] Invalid slotIndex: {slotIndex}");
            return;
        }

        clicksVar.OnValueChanged += OnClicksChanged;
        usedVar.OnValueChanged += OnUsedChanged;
        priceVar.OnValueChanged += OnPriceChanged;
        upgradeList.OnListChanged += OnUpgradeListChanged;
        GameManager.Instance.TotalPoints.OnValueChanged += OnTotalPointsChanged;

        RefreshUpgradeUI(upgradeList, priceVar.Value);
        UpdateClicksUI(clicksVar.Value);
        UpdateUsedState(usedVar.Value);
        RefreshCanActivate();
    }

    private void OnDestroy()
    {
        if (clicksVar != null) clicksVar.OnValueChanged -= OnClicksChanged;
        if (usedVar != null) usedVar.OnValueChanged -= OnUsedChanged;
        if (priceVar != null) priceVar.OnValueChanged -= OnPriceChanged;
        if (upgradeList != null) upgradeList.OnListChanged -= OnUpgradeListChanged;

        if (GameManager.Instance != null)
            GameManager.Instance.TotalPoints.OnValueChanged -= OnTotalPointsChanged;
    }

    private void OnClicksChanged(int oldValue, int newValue)
    {
        UpdateClicksUI(newValue);
    }

    private void OnUsedChanged(bool oldValue, bool newValue)
    {
        UpdateUsedState(newValue);
        RefreshCanActivate();
    }

    private void OnPriceChanged(int oldValue, int newValue)
    {
        RefreshUpgradeUI(upgradeList, newValue);
        RefreshCanActivate();
    }

    private void OnUpgradeListChanged(NetworkListEvent<ModifierUpgrade> changeEvent)
    {
        RefreshUpgradeUI(upgradeList, priceVar.Value);
        RefreshCanActivate();
    }

    private void OnTotalPointsChanged(int oldValue, int newValue)
    {
        RefreshCanActivate();
    }

    private void RefreshUpgradeUI(NetworkList<ModifierUpgrade> upgrades, int price)
    {
        UpgradesData.Clear();

        foreach (ModifierUpgrade mod in upgrades)
            UpgradesData.Add(mod);

        Price = price;

        UpdateModifierUI();
        UpdatePriceUI();
    }

    private void RefreshCanActivate()
    {
        bool isUsed = usedVar != null && usedVar.Value;
        bool canAfford = GameManager.Instance.TotalPoints.Value >= Price;

        CanActivate = !isUsed && canAfford;

        if (Cover != null)
            Cover.SetActive(!CanActivate || HasVotedLocally);
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        RefreshCanActivate();

        if (!CanActivate || HasVotedLocally)
            return;

        HasVotedLocally = true;
        RefreshCanActivate();
        UpgradesManager.Instance.RegisterUpgradeClickServerRpc(slotIndex);
    }

    public override void Activate()
    {
        base.Activate();
        // Server-side apply is handled in UpgradesManager.
    }

    public void UpdateModifierUI()
    {
        string text = "";

        foreach (ModifierUpgrade mod in UpgradesData)
            text += GetUpgradeDisplayText(mod) + "\n";

        ModifierUI.text = text.TrimEnd('\n');
    }

    public void UpdatePriceUI()
    {
        PriceUI.text = Price.ToString();
    }

    private string GetUpgradeDisplayText(ModifierUpgrade upgrade)
    {
        bool isPositive = upgrade.Value > 0;

        string color = isPositive ? "#66FF66" : "#FF5252";
        string signedValue = upgrade.Value > 0 ? $"+{upgrade.Value}" : upgrade.Value.ToString();
        string coloredValue = $"<color={color}>{signedValue}</color>";

        return $"{upgrade.Trait}: {coloredValue} {upgrade.Type} Value";
    }
}