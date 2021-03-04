using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Perk : Equippable
{
    [SerializeField]
    //this perk is only applied once, not equipped by each player
    protected bool singleApplication;

    public bool GetSingleApplication()
    {
        return singleApplication;
    }


}
