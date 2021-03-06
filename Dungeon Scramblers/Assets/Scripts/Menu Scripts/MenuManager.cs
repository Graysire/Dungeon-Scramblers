using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MenuManager : BitCategories
{

    /*
     * Responsible for getting player data for Audio settings and Loadout preferences
     * and sending the data to the SaveLoad system.
     */


    //Bit representation document
    //https://docs.google.com/spreadsheets/d/12xuLHZSDCkMI4G0byxqrIRxpuUSsBOtuQ4uvpDbt47w/edit#gid=0

    SaveLoad saveLoad = new SaveLoad();         //Saves and loads data from file
    BitPacket bitPacket = new BitPacket();      //Stores the data to be saved

    private int tempMageInvBitsPacked = 0;      //The saved mage bits
    private int tempKnightInvBitsPacked = 0;    //The saved knight bits
    private int tempRogueInvBitsPacked = 0;     //The saved rogue bits
    private int tempOverlordInvBitsPacked = 0;  //The saved overlord bits

    //Initialize the file if it isn't made
    //or load data from file
    void Awake()
    {
        //saveLoad.DeleteSaveFile();

        //If the file doesn't exist then initialize it
        if (!saveLoad.FileExists())
        {
            saveLoad.InitializeFile();
        }

        LoadData(); //Load the saved data to local variables
    }

    //Loads the data into the temp bits
    private void LoadData()
    {
        bitPacket = saveLoad.Load(); //load our bitpacket from file

        tempMageInvBitsPacked = bitPacket.mageInvBitsPacked;
        tempKnightInvBitsPacked = bitPacket.knightInvBitsPacked;
        tempRogueInvBitsPacked = bitPacket.rogueInvBitsPacked;
        tempOverlordInvBitsPacked = bitPacket.overlordInvBitsPacked;
    }

    //Saves the temp data into a bitpacket which gets sends to SaveLoad to write to file
    public void SaveLoadout()
    {
        bitPacket.mageInvBitsPacked = tempMageInvBitsPacked;
        bitPacket.knightInvBitsPacked = tempKnightInvBitsPacked;
        bitPacket.rogueInvBitsPacked = tempRogueInvBitsPacked;
        bitPacket.overlordInvBitsPacked = tempOverlordInvBitsPacked;

        saveLoad.Save(bitPacket);
    }

    //Recieves the playertype bit flag, equippable bitflag, and BitCategory of the equippaable
    public void GetBitInfo(string playerBitFlag, string bitFlagCode, BitCategory category)
    {
        //mage
        if (playerBitFlag == "000")
        {
            //Clear the region where we apply
            tempMageInvBitsPacked = ClearForApply(tempMageInvBitsPacked, category);
            //apply equippable bits
            tempMageInvBitsPacked |= ApplyBitsToTemp(bitFlagCode, category);
            Debug.Log(Convert.ToString(tempMageInvBitsPacked, 2).PadLeft(32, '0'));
        }
        //knight
        if (playerBitFlag == "001")
        {
            //Clear the region where we apply
            tempKnightInvBitsPacked = ClearForApply(tempKnightInvBitsPacked, category);
            //apply equippable bits
            tempKnightInvBitsPacked |= ApplyBitsToTemp(bitFlagCode, category);
            Debug.Log(Convert.ToString(tempKnightInvBitsPacked, 2).PadLeft(32, '0'));
        }
        //rogue
        if (playerBitFlag == "010")
        {
            //Clear the region where we apply
            tempRogueInvBitsPacked = ClearForApply(tempRogueInvBitsPacked, category);
            //apply equippable bits
            tempRogueInvBitsPacked |= ApplyBitsToTemp(bitFlagCode, category);
            Debug.Log(Convert.ToString(tempRogueInvBitsPacked, 2).PadLeft(32, '0'));
        }
        //overlord
        if (playerBitFlag == "011")
        {
            //Clear the region where we apply
            tempOverlordInvBitsPacked = ClearForApply(tempOverlordInvBitsPacked, category);
            //apply equippable bits
            tempOverlordInvBitsPacked |= ApplyBitsToTemp(bitFlagCode, category);
            Debug.Log(Convert.ToString(tempOverlordInvBitsPacked, 2).PadLeft(32, '0'));
        }
    }

    //Clears the bits for the given equippable category so it can be applied without leftover bits from previous applies
    private int ClearForApply(int valToModify, BitCategory category)
    {
        string b = "1111";
        int bits = Convert.ToInt32(b, 2);
        int mask = 0;

        //Creates the mask for the category
        if (category == BitCategory.weapon)
        {
            mask = (bits << 25);
        }
        if (category == BitCategory.armor)
        {
            mask = (bits << 21);
        }
        if (category == BitCategory.ability1)
        {
            mask = (bits << 17);
        }
        if (category == BitCategory.ability2)
        {
            mask = (bits << 13);
        }

        valToModify &= ~(mask);

        return valToModify;
    }

    //Applies the data to the correct temp bitstring
    private int ApplyBitsToTemp(string bitFlagCode, BitCategory category)
    {
        int inventory = 0;

        //convert equippable code to binary
        int bits = Convert.ToInt32(bitFlagCode, 2);

        //Pack bits together
        //Shifts bits to end of 32 bit integer
        if (category == BitCategory.weapon)
        {
            inventory = inventory | (bits << 25);
        }
        if (category == BitCategory.armor)
        {
            inventory = inventory | (bits << 21);
        }
        if (category == BitCategory.ability1)
        {
            inventory = inventory | (bits << 17);
        }
        if (category == BitCategory.ability2)
        {
            inventory = inventory | (bits << 13);
        }

        return inventory;
    }
}
