using System;
using UnityEngine;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    public PlayerBehaviour PlayerBehaviour;
    public GameObject CivilianPrefab;
    private List<InventoryCivilianBehaviour> CivilianBehaviours = new List<InventoryCivilianBehaviour>();
    
    public void Initialize(int civiliansCount)
    {
        for (int i = 0; i < civiliansCount; i++)
        {
            GameObject civilianObj = Instantiate(CivilianPrefab, transform);
            InventoryCivilianBehaviour behaviour = civilianObj.GetComponent<InventoryCivilianBehaviour>();
            behaviour.Index = i;
            behaviour.OnCivilianClicked += SelectCivilian;
            CivilianBehaviours.Add(behaviour);
        }
    }
    
    public void SelectCivilian(int index)
    {
        if(PlayerBehaviour.PlayerNumber != GameManager.Instance.ActivePlayer.Value)
        {
            Debug.LogWarning("It's not your turn to select a civilian.");
            return;
        }
        
        if (index < 0 || index >= CivilianBehaviours.Count)
        {
            Debug.LogError("Invalid civilian index selected.");
            return;
        }
        
        InventoryCivilianBehaviour selectedCivilian = CivilianBehaviours[index];
        // Handle the logic for selecting the civilian, e.g., updating UI or player stats
        Debug.Log($"Selected Civilian with Trait: {selectedCivilian.Trait}");
        
        GameManager.Instance.ChangeActivePlayer();
    }

    public void Clear()
    {
        foreach (InventoryCivilianBehaviour c in CivilianBehaviours)
        {
            c.DestroySelf();
        }
    }
}