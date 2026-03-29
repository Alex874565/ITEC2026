using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "TraitSpritesDatabase", menuName = "ScriptableObjects/TraitSpritesDatabase", order = 1)]
public class TraitSpritesDatabase : ScriptableObject
{
    public List<TraitSprites> TraitSpritesList;
    
    public TraitSprites GetTraitSprites(Trait trait)
    {
        return TraitSpritesList.Find(x => x.Trait == trait);
    }
}