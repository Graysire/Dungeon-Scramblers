using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MenuManager : MonoBehaviour
{

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
    public void RetrieveBitInfo(Categories.PlayerCategories playerCategory, string bitFlagCode, Categories.ItemCategory category)
    {
        //mage
        if (playerCategory == Categories.PlayerCategories.mage)
        {
            //Clear the region where we apply
            tempMageInvBitsPacked = ClearForApply(tempMageInvBitsPacked, category);
            //apply equippable bits
            tempMageInvBitsPacked |= ApplyBitsToTemp(bitFlagCode, category);
            Debug.Log(Convert.ToString(tempMageInvBitsPacked, 2).PadLeft(32, '0'));
        }
        //knight
        if (playerCategory == Categories.PlayerCategories.knight)
        {
            //Clear the region where we apply
            tempKnightInvBitsPacked = ClearForApply(tempKnightInvBitsPacked, category);
            //apply equippable bits
            tempKnightInvBitsPacked |= ApplyBitsToTemp(bitFlagCode, category);
            Debug.Log(Convert.ToString(tempKnightInvBitsPacked, 2).PadLeft(32, '0'));
        }
        //rogue
        if (playerCategory == Categories.PlayerCategories.rogue)
        {
            //Clear the region where we apply
            tempRogueInvBitsPacked = ClearForApply(tempRogueInvBitsPacked, category);
            //apply equippable bits
            tempRogueInvBitsPacked |= ApplyBitsToTemp(bitFlagCode, category);
            Debug.Log(Convert.ToString(tempRogueInvBitsPacked, 2).PadLeft(32, '0'));
        }
        //overlord
        if (playerCategory == Categories.PlayerCategories.overlord)
        {
            //Clear the region where we apply
            tempOverlordInvBitsPacked = ClearForApply(tempOverlordInvBitsPacked, category);
            //apply equippable bits
            tempOverlordInvBitsPacked |= ApplyBitsToTemp(bitFlagCode, category);
            Debug.Log(Convert.ToString(tempOverlordInvBitsPacked, 2).PadLeft(32, '0'));
        }
    }

    //Clears the bits for the given equippable category so it can be applied without leftover bits from previous applies
    private int ClearForApply(int valToModify, Categories.ItemCategory category)
    {
        string b = "";
        int mask = 0;

        //Creates the mask for the category
        if (category == Categories.ItemCategory.weapon)
        {
            b = "11111";
            int bits = Convert.ToInt32(b, 2);
            mask = (bits << 24);
        }
        if (category == Categories.ItemCategory.armor)
        {
            b = "11111";
            int bits = Convert.ToInt32(b, 2);
            mask = (bits << 19);
        }
        if (category == Categories.ItemCategory.ability1)
        {
            b = "111111";
            int bits = Convert.ToInt32(b, 2);
            mask = (bits << 13);
        }
        if (category == Categories.ItemCategory.ability2)
        {
            b = "111111";
            int bits = Convert.ToInt32(b, 2);
            mask = (bits << 7);
        }

        valToModify &= ~(mask);

        return valToModify;
    }

    //Applies the data to the correct temp bitstring
    private int ApplyBitsToTemp(string bitFlagCode, Categories.ItemCategory category)
    {
        int inventory = 0;

        //convert equippable code to binary
        int bits = Convert.ToInt32(bitFlagCode, 2);

        //Pack bits together
        //Shifts bits to end of 32 bit integer
        if (category == Categories.ItemCategory.weapon)
        {
            inventory = inventory | (bits << 24);
        }
        if (category == Categories.ItemCategory.armor)
        {
            inventory = inventory | (bits << 19);
        }
        if (category == Categories.ItemCategory.ability1)
        {
            inventory = inventory | (bits << 13);
        }
        if (category == Categories.ItemCategory.ability2)
        {
            inventory = inventory | (bits << 7);
        }

        return inventory;
    }


    //Provided the player enum and the category of the item type wanted, this returns 
        //the item code for the player
    public int GetInventoryCode(Categories.PlayerCategories playerCategory, Categories.ItemCategory category)
    {
        int code = 0;
        //mage
        if (playerCategory == Categories.PlayerCategories.mage)
        {
            code = GetCode(bitPacket.mageInvBitsPacked, category);
        }
        //knight
        if (playerCategory == Categories.PlayerCategories.knight)
        {
            code = GetCode(bitPacket.knightInvBitsPacked, category);
        }
        //rogue
        if (playerCategory == Categories.PlayerCategories.rogue)
        {
            code = GetCode(bitPacket.rogueInvBitsPacked, category);
        }
        //overlord
        if (playerCategory == Categories.PlayerCategories.overlord)
        {
            code = GetCode(bitPacket.overlordInvBitsPacked, category);
        }

        return code;
    }


    //Retrieves the code at of the inventory given the category
    private int GetCode(int inventory, Categories.ItemCategory category)
    {
        int code = inventory;
        if (category == Categories.ItemCategory.weapon)
        {
            code = code << 3;
            code = code >> 27;
        }
        if (category == Categories.ItemCategory.armor)
        {
            code = code << 8;
            code = code >> 27;
        }
        if (category == Categories.ItemCategory.ability1)
        {
            code = code << 13;
            code = code >> 26;
        }
        if (category == Categories.ItemCategory.ability2)
        {
            code = code << 19;
            code = code >> 26;
        }
        return code;
    }
}
