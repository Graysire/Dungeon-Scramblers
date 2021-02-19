using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;


//https://gamedevelopment.tutsplus.com/tutorials/how-to-save-and-load-your-players-progress-in-unity--cms-20934
//https://learn.unity.com/tutorial/persistence-saving-and-loading-data#
//https://www.raywenderlich.com/418-how-to-save-and-load-a-game-in-unity


[System.Serializable]
public class SaveLoad
{
    /*
     * This class will couple with the Menu Manager which will run calls onto here
     */


    public Loadout[] savedLoadouts = new Loadout[4]; // Stores the list of loadouts that a player will have

    //Saves the data into a file
    public void Save()
    {
        BinaryFormatter bf = new BinaryFormatter(); //converts data to binary
        FileStream file = File.Open(Application.persistentDataPath + "/savedLoadouts.dat", FileMode.Open); //Opens the file to save into
        bf.Serialize(file, savedLoadouts); //saves this class info into the file
        file.Close(); //close the file
    }

    //Loads the data from a file
    public void Load()
    {
        if (!FileExists())
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/savedLoadouts.dat", FileMode.Open);
            savedLoadouts = (Loadout[])bf.Deserialize(file);
            file.Close();
        }
    }

    //Checks if the data file exists
    public bool FileExists()
    {
        if (File.Exists(Application.persistentDataPath + "/savedLoadouts.dat"))
        {
            return true;
        }
        return false;
    }

    //Initializes a file with data if a file doesn't already exist
    public void InitializeFile()
    {
        if (!FileExists())
        {
            //TO DO: Fill savedLoadouts with default data
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(Application.persistentDataPath + "/savedLoadouts.dat");
            bf.Serialize(file, savedLoadouts);
            file.Close();
        }
    }
}
