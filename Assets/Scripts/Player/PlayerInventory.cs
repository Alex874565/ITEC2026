using System;
using UnityEngine;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    public PlayerBehaviour PlayerBehaviour;
    public GameObject CivilianPrefab;
    public List<InventoryCivilianBehaviour> CivilianBehaviours = new List<InventoryCivilianBehaviour>();
    
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
        
        GameManager.Instance.OnEndWave += Clear;
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
        GridManager.Instance.RequestAddCivilian(selectedCivilian);
        GameManager.Instance.ChangeActivePlayer();
        selectedCivilian.DestroySelf();
    }

    public void Clear()
    {
        for (int i = CivilianBehaviours.Count - 1; i >= 0; i--)
        {
            InventoryCivilianBehaviour civilian = CivilianBehaviours[i];

            if (civilian == null)
            {
                CivilianBehaviours.RemoveAt(i);
                continue;
            }

            civilian.DestroySelf();
            CivilianBehaviours.RemoveAt(i);
        }
        
        GameManager.Instance.OnEndWave -= Clear;
    }

    public void OnDisable()
    {
        foreach (InventoryCivilianBehaviour c in CivilianBehaviours)
        {
            c.OnCivilianClicked -= SelectCivilian;
        }
    }
}