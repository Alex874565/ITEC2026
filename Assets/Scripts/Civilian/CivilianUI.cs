using UnityEngine;
using UnityEngine.UI;

public class CivilianUI : MonoBehaviour
{
    public TraitSpritesDatabase Database;
    public Image Image;
    public Vector2 NeutralRange;

    private Trait _trait;
    
    private TraitSprites _traitSprites;
    
    public void Initialize(Trait trait)
    {
        _trait = trait;
        _traitSprites = Database.TraitSpritesList.Find(x => x.Trait == _trait);
        Image.sprite = _traitSprites.NeutralSprite;
    }

    public void UpdateImage(int traitValue)
    {
        if(traitValue < NeutralRange.x)
        {
            Image.sprite = _traitSprites.AngrySprite;
        }
        else if(traitValue > NeutralRange.y)
        {
            Image.sprite = _traitSprites.HappySprite;
        }
        else
        {
            Image.sprite = _traitSprites.NeutralSprite;
        }
    }
}