using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;


//https://gamedevelopment.tutsplus.com/tutorials/how-to-save-and-load-your-players-progress-in-unity--cms-20934

public static class SaveLoad
{
    public static List<Loadout> savedLoadouts = new List<Loadout>(); // Stores the list of loadouts that a player will have


    //Saves the loadout information to a file for future use
    public static void Save()
    {
        /*
        savedLoadouts.Add(savedLoadouts.current); //Will add the current 
        BinaryFormatter bf = new BinaryFormatter();
        FileStream loadoutFile = File.Create(Application.persistentDataPath + "/savedLoadouts.gd");
        bf.Serialize(loadoutFile, SaveLoad.savedLoadouts);
        file.Close();
        */
    }
}
