using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Doozy.Engine.UI;

public class EquippableBitFlag : BitCategories
{
    // Refer to this link for Bitcode legend: https://docs.google.com/spreadsheets/d/12xuLHZSDCkMI4G0byxqrIRxpuUSsBOtuQ4uvpDbt47w/edit#gid=0

    public int bitEnum;              //Determines the enum type of the ability. Must be 1, 2, 3, 4, or 5. Check BitCategories (Superclass) for representation. 
    public string playerBitFlag;     //Stores which player inventory to change 
    public string bitFlagCode;       //Stores the bits of the equippables this object relates to
    public MenuManager menuManager;  //The menu manager that we want to send the info to

    private BitCategory valueType;   //Used for getting the BitCategory enum val

    //Sends the relevant info to Menu Manager
    public void SendBitInfo()
    {
        //Determine the type of the equippable by using the given bitEnum value
        if (bitEnum == (int)BitCategory.ability2) { valueType = BitCategory.ability2; }
        else if (bitEnum == (int)BitCategory.ability1) { valueType = BitCategory.ability1; }
        else if(bitEnum == (int)BitCategory.armor) { valueType = BitCategory.armor; }
        else if(bitEnum == (int)BitCategory.weapon) { valueType = BitCategory.weapon; }
        else if(bitEnum == (int)BitCategory.playerType) { valueType = BitCategory.playerType; }
        else { Debug.Log("bitEnum not set! Please set to valid value! "); }

        //Send data to menu manager
        menuManager.RetrieveBitInfo(playerBitFlag, bitFlagCode, valueType);
    }
}

