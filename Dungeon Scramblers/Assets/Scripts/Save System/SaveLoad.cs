using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;


//https://gamedevelopment.tutsplus.com/tutorials/how-to-save-and-load-your-players-progress-in-unity--cms-20934
//https://learn.unity.com/tutorial/persistence-saving-and-loading-data#
//https://www.raywenderlich.com/418-how-to-save-and-load-a-game-in-unity

//https://docs.unity3d.com/Manual/script-Serialization.html
//https://docs.unity3d.com/Manual/script-Serialization-Custom.html
//Bitmasking: https://www.alanzucconi.com/2015/07/26/enum-flags-and-bitwise-operators/ | https://alemil.com/bitmask

[System.Serializable]
public class SaveLoad
{ 
    //Saves the data into a file
    public void Save(BitPacket bp)
    {
        Debug.Log("Saving Data!");

        BinaryFormatter bf = new BinaryFormatter(); //converts data to binary
        FileStream file = File.Open(GetFilePath(), FileMode.Open); //Open file
        bf.Serialize(file, bp); //saves data into file
        file.Close(); //close the file
    }


    //Loads the data from a file
    public BitPacket Load()
    {
        Debug.Log("Loading Data!");

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(GetFilePath(), FileMode.Open); //Open file
        BitPacket bp = (BitPacket)bf.Deserialize(file);   //Load data from file
        file.Close();
        return bp;
    }


    //Checks if the data file exists
    public bool FileExists()
    {
        if (File.Exists(GetFilePath()))
        {
            Debug.Log("File Exists!");
            return true;
        }
        Debug.Log("File Missing!");
        return false;
    }


    //Initializes a file with data
    public void InitializeFile()
    {
        Debug.Log("Initializing File with Data!");

        BitPacket bp = new BitPacket(); //bitpacket to save into file

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(GetFilePath());
        bf.Serialize(file, bp);
        file.Close();
    }
    

    //Deletes the saved file information
    public void DeleteSaveFile()
    {
        Debug.Log("Deleting File");
        File.Delete(GetFilePath());
    }


    //Gets the string of the file path
    private string GetFilePath()
    {
        return Application.persistentDataPath + "/bitPacket.txt";
    }
}
