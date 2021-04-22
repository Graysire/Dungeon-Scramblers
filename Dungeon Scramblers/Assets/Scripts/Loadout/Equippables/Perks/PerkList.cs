using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Stores list of perks that can be selected
public class PerkList : MonoBehaviour
{
    [SerializeField]
    List<Perk> perkList;

    [SerializeField]
    //special perk that always spawns
    Perk revivePerk;

    //list of perks that are unavailable
    List<Perk> unavailablePerkList;

    private void Awake()
    {
        unavailablePerkList = new List<Perk>();
    }

    //returns a random perk from the list of perks
    public Perk GetPerk()
    {
        int rand = Random.Range(0, perkList.Count);
        Perk selection = perkList[rand];
        unavailablePerkList.Add(selection);
        perkList.Remove(selection);

        return selection;
    }

    //reutnrs therevive perk
    public Perk getRevive()
    {
        return revivePerk;
    }

    //Resets the list of available perks, excluding selection
    public void ResetPerks(Perk selection = null)
    {
        for (int i = 0; i < unavailablePerkList.Count; i++)
        {
            //add any perks that are not the selection back into the main pool
            if (unavailablePerkList[i] != selection)
            {
                perkList.Add(unavailablePerkList[i]);
            }
        }
        //clear the remaining perks so that they cannot be selected anymore
        unavailablePerkList.Clear();
    }
}
