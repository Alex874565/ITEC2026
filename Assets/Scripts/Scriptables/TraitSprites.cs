using UnityEngine;

[CreateAssetMenu(fileName = "TraitSprites", menuName = "ScriptableObjects/TraitSprites", order = 1)]
public class TraitSprites : ScriptableObject
{
    public Trait Trait;
    public Sprite TraitSprite;
    public Sprite NeutralSprite;
    public Sprite HappySprite;
    public Sprite AngrySprite;
}