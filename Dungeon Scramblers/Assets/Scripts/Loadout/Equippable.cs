using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Parent class of DefaultAttackSequence, Armor and Perk classes.
//Will be used to identify the equippable type and name so that it can be retrived
//by Game Manager for player stat/abilities instantiation

public class Equippable : HasStats
{
    [SerializeField]
    protected string uniqueName;   //Name for the equippable -- *MUST BE UNIQUE NAME FOR LOADOUT SYSTEM*


    // Refer to this link for Bitcode legend: https://docs.google.com/spreadsheets/d/12xuLHZSDCkMI4G0byxqrIRxpuUSsBOtuQ4uvpDbt47w/edit#gid=0
    [SerializeField]
    protected string equippableBitFlag;

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

    //applies all changes to the player such as changing their stats
    public virtual void Equip(Player player)
    {
        for (int i = 0; i < stats.Length; i++)
        {
            player.GetStats()[i] += stats[i];
            player.GetAffectedStats()[i] += stats[i];
        }
    }

    //reverses all changes to the playersuch as changing their stats
    public virtual void Unequip(Player player)
    {
        for (int i = 0; i < stats.Length; i++)
        {
            player.GetStats()[i] -= stats[i];
            player.GetAffectedStats()[i] -= stats[i];
        }
    }
}
