using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Doozy.Engine.UI;

public class EquippableBitFlag : MonoBehaviour
{
    // Refer to this link for Bitcode legend: https://docs.google.com/spreadsheets/d/12xuLHZSDCkMI4G0byxqrIRxpuUSsBOtuQ4uvpDbt47w/edit#gid=0

    public int bitEnum;                                 //Determines the enum type of the ability. Must be 1, 2, 3, 4, or 5. Check BitCategories (Superclass) for representation. 
    public Categories.PlayerCategories bitCategory;     //Stores which player inventory category to change 
    public string bitFlagCode;                          //Stores the bits of the equippables this object relates to
    public MenuManager menuManager;                     //The menu manager that we want to send the info to

    private Categories.BitCategory valueType;           //Used for getting the BitCategory enum val

    //Sends the relevant info to Menu Manager
    public void SendBitInfo()
    {
        //Determine the type of the equippable by using the given bitEnum value
        if (bitEnum == (int)Categories.BitCategory.ability2) { valueType = Categories.BitCategory.ability2; }
        else if (bitEnum == (int)Categories.BitCategory.ability1) { valueType = Categories.BitCategory.ability1; }
        else if(bitEnum == (int)Categories.BitCategory.armor) { valueType = Categories.BitCategory.armor; }
        else if(bitEnum == (int)Categories.BitCategory.weapon) { valueType = Categories.BitCategory.weapon; }
        else if(bitEnum == (int)Categories.BitCategory.playerType) { valueType = Categories.BitCategory.playerType; }
        else { Debug.Log("bitEnum not set! Please set to valid value! "); }

        //Send data to menu manager
        menuManager.RetrieveBitInfo(bitCategory, bitFlagCode, valueType);
    }
}

