using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Perk : Equippable
{
    [SerializeField]
    //this perk is only applied once, not equipped by each player
    protected bool singleApplication;
    [SerializeField]
    //this is the image to be displayed on the perk voting button
    protected Sprite perkButtonSprite;


    public bool GetSingleApplication()
    {
        return singleApplication;
    }

    public Sprite GetSprite()
    {
        return perkButtonSprite;
    }

}
