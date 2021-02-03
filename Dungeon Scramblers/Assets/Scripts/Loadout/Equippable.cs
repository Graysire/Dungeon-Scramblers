using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equippable : MonoBehaviour
{
    protected string uniqueName;   //Name for the equippable -- *MUST BE UNIQUE NAME*

    //Enum for determining the type of the equippable
    [SerializeField]
    protected enum EquippableType
    {
        ability = 0,    //Identifier for basic abilities a player can use
        perk = 1,       //Identifier for passive ability or perk that players can equip
        weapon = 2,     //Identifier for weapons that player can equip
        armor = 3       //Identifier for armor types the player can equip
    }

    //Returns the name of the Equippable
    public string GetEquippableName()
    {
        return uniqueName;
    }
}
