using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loadout : MonoBehaviour
{

    public List<Equippable> Equippables;    //Stores all equippables a player has


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Adds the given equippable into the loadout
    public void AddEquippable(Equippable e)
    {
        Equippables.Add(e);
    }

    //Removes the equippable from the loadout given the name of the equippable
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
