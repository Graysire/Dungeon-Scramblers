using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Stores list of perks that can be selected
public class PerkList : MonoBehaviour
{
    [SerializeField]
    List<Equippable> perkList;

    //returns a random perk from the list of perks
    public Equippable GetPerk()
    {
        return perkList[Random.Range(0, perkList.Count)];
    }
}
