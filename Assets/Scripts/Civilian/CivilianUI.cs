using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CivilianUI : MonoBehaviour
{
    public TraitSpritesDatabase Database;
    public Image Image;
    public Vector2 NeutralRange;
    public GameObject Tooltip;
    public TextMeshProUGUI TraitContainer;
    public List<TextMeshProUGUI> LikedTraitsContainers;
    public List<TextMeshProUGUI> DislikedTraitsContainers;

    private Trait _trait;
    
    private TraitSprites _traitSprites;
    
    public void Initialize(Trait trait, Trait[] likedTraits, Trait[] dislikedTraits)
    {
        _trait = trait;
        //_traitSprites = Database.GetTraitSprites(trait);
        
        //UpdateImage(0);

        TraitContainer.text = _trait.ToString();
        
        for(int i = 0; i < LikedTraitsContainers.Count; i++)
        {
            if(i < likedTraits.Length)
            {
                LikedTraitsContainers[i].text = likedTraits[i].ToString();
            }
            else
            {
                LikedTraitsContainers[i].text = "";
            }
        }
        
        for(int i = 0; i < DislikedTraitsContainers.Count; i++)
        {
            if(i < dislikedTraits.Length)
            {
                DislikedTraitsContainers[i].text = dislikedTraits[i].ToString();
            }
            else
            {
                DislikedTraitsContainers[i].text = "";
            }
        }

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

    public void ShowTooltip()
    {
        Tooltip.SetActive(true);
    }

    public void HideTooltip()
    {
        Tooltip.SetActive(false);
    }
}