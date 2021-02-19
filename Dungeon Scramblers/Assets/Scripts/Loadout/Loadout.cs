using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Handles the storage of all equippables for a single player
//Game Manager will use this to apply equippables stats and instantiate them
//to the player. 
public class Loadout : MonoBehaviour
{
    public List<Equippable> Equippables;    //Stores all equippables a player has

    //Adds the given equippable into the loadout
    //Should be used for inventory management on main menu
    public void AddEquippable(Equippable e)
    {
        Equippables.Add(e);
    }

    //Removes the equippable from the loadout given the name of the equippable
    //Should be used for inventory management on main menu
    public void DeleteEquippable(string uniqueName)
    {
        for (int i = 0; i < Equippables.Count; ++i)
        {
            if (string.Equals(Equippables[i].GetEquippableName(), uniqueName))
            {
                Equippables.RemoveAt(i);
            }
        }
    }
}
