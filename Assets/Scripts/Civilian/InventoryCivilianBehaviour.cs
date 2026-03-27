using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryCivilianBehaviour : MonoBehaviour
{
    public CivilianUI CivilianUI;
    
    private Trait _trait;
    private List<Trait> _likedTraits;
    private List<Trait> _dislikedTraits;

    private void Awake()
    {
        _likedTraits = new List<Trait>();
        _dislikedTraits = new List<Trait>();
        
        SelectRandomTraits();
    }

    private void SelectRandomTraits()
    {
        _trait = (Trait)UnityEngine.Random.Range(0, Enum.GetValues(typeof(Trait)).Length);
        int likedCount = UnityEngine.Random.Range(0, 4); // Randomly like 1 to 3 traits
        int dislikedCount = UnityEngine.Random.Range(0, 4);
        
        while(_likedTraits.Count < likedCount)
        {
            Trait randomTrait = (Trait)UnityEngine.Random.Range(0, Enum.GetValues(typeof(Trait)).Length);
            if(randomTrait != _trait && !_likedTraits.Contains(randomTrait))
            {
                _likedTraits.Add(randomTrait);
            }
        }

        while (_dislikedTraits.Count < dislikedCount)
        {
            Trait randomTrait = (Trait)UnityEngine.Random.Range(0, Enum.GetValues(typeof(Trait)).Length);
            if (randomTrait != _trait && !_likedTraits.Contains(randomTrait) && !_dislikedTraits.Contains(randomTrait))
            {
                _dislikedTraits.Add(randomTrait);
            }
        }
    }
    
    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}