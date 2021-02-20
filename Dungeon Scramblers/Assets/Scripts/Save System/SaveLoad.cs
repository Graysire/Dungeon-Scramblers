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

[System.Serializable]
public class SaveLoad
{
    /*
     * This class will couple with the Menu Manager which will run calls onto here
     */

    public Loadout[] savedLoadouts = new Loadout[4]; // Stores the list of loadouts that a player will have
    /*  Index  |  Class Stored at Index
     *    0             Mage
     *    1             Knight
     *    2             Rogue
     *    3             Overlord
     */

    /*
     * Bug: Cannot save Jagged arrays. Must find way to save each loadout and read from files
     * Possible Fix: Make a file for each loadout and other file types
     */

    //Saves the data into a file
    public void Save(int num)
    {
        Debug.Log("Saving Data!");

        BinaryFormatter bf = new BinaryFormatter(); //converts data to binary
        FileStream file = File.Open(GetFilePath(num), FileMode.Open); //Open file
        bf.Serialize(file, savedLoadouts); //saves data into file
        file.Close(); //close the file
    }

    //Loads the data from a file
    public void Load(int num)
    {
        Debug.Log("Loading Data!");

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(GetFilePath(num), FileMode.Open); //Open file
        savedLoadouts = (Loadout[])bf.Deserialize(file); //Load data from file
        file.Close();
    }

    //Checks if the data file exists
    public bool FileExists(int num)
    {
        if (File.Exists(GetFilePath(num)))
        {
            Debug.Log("File Exists!");
            return true;
        }
        Debug.Log("File Missing!");
        return false;
    }

    //Initializes a file with data
    public void InitializeFile(int num)
    {
        Debug.Log("Initializing File with Data!");
        //TO DO: Fill savedLoadouts with default data

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(GetFilePath(num));
        bf.Serialize(file, savedLoadouts);
        file.Close();
    }

    //Sets the data of the inventory at the given index
    public void ChangeData(int index, Loadout loadout)
    {
        if (index < savedLoadouts.Length && index >= 0)
        {
            Debug.Log("Editing data at i: " + index);
            savedLoadouts[index] = loadout;
        }
    }
    
    //Deletes the saved file information
    public void DeleteSaveFile(int num)
    {
        Debug.Log("Deleting File");
        File.Delete(GetFilePath(num));
    }

    //Gets the string of the file path
    private string GetFilePath(int num)
    {
        return Application.persistentDataPath + "/savedLoadouts" + num + ".txt";
    }
}
