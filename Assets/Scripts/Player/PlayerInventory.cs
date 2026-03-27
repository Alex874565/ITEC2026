using System;
using UnityEngine;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    public GameObject CivilianPrefab;
    private List<InventoryCivilianBehaviour> CivilianBehaviours;

    private void Awake()
    {
        CivilianBehaviours = new List<InventoryCivilianBehaviour>();
    }
    
    public void Initialize(int civiliansCount)
    {
        for (int i = 0; i < civiliansCount; i++)
        {
            GameObject civilianObj = Instantiate(CivilianPrefab, transform);
            InventoryCivilianBehaviour behaviour = civilianObj.GetComponent<InventoryCivilianBehaviour>();
            CivilianBehaviours.Add(behaviour);
        }
    }

    public void Clear()
    {
        foreach (InventoryCivilianBehaviour c in CivilianBehaviours)
        {
            c.DestroySelf();
        }
    }
}