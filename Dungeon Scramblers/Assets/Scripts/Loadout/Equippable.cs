﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//Parent class of DefaultAttackSequence, Armor and Perk classes.
//Will be used to identify the equippable type and name so that it can be retrived
//by Game Manager for player stat/abilities instantiation

public class Equippable : HasStats
{
    [Header("Equippable Variables")]
    [SerializeField]
    protected string uniqueName;   //Name for the equippable -- *MUST BE UNIQUE NAME FOR LOADOUT SYSTEM*

    // Refer to this link for Bitcode legend: https://docs.google.com/spreadsheets/d/12xuLHZSDCkMI4G0byxqrIRxpuUSsBOtuQ4uvpDbt47w/edit#gid=0
    [SerializeField]
    private string equippableBitFlag;

    // Contains categories for equippable
    [SerializeField]
    private Categories.ItemCategory bitCategory;

    [SerializeField]
    private Categories.PlayerCategories playerCategory;

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


    //Given the bit code from inventory, category of code, and player category it associates to
        //compare with this equippable to see if they match
    public virtual bool CompareWith(int bitCode, Categories.ItemCategory bitCategory, Categories.PlayerCategories playerCategory)
    {
        int thisCode = Convert.ToInt32(equippableBitFlag, 2);
        if (thisCode == bitCode && bitCategory == this.bitCategory && playerCategory == this.playerCategory) { return true; }
        return false;
    }
}
