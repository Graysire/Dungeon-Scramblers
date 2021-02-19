using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;


//https://gamedevelopment.tutsplus.com/tutorials/how-to-save-and-load-your-players-progress-in-unity--cms-20934
//https://www.raywenderlich.com/418-how-to-save-and-load-a-game-in-unity
/*
 * PlayerPrefs: 
 * This is a special caching system to keep track of simple settings for the player between game sessions. 
 * Many new programmers make the mistake of thinking they can use this as a save game system as well, but it is bad practice to do so. 
 * This should only be used for keeping track of simple things like graphics, 
 * sound settings, login info, or other basic user-related data.
 *
 *Serialization: 
 *This is the magic that makes Unity work. 
 *Serialization is the conversion of an object into a stream of bytes. 
 *That might seem vague but take a quick look at this graphic:
 *
 *Deserialization: 
 *This is exactly what it sounds like. 
 *It’s the opposite of serialization, namely the conversion of a stream of bytes into an object.
 */

//This is known as an attribute and it is metadata for your code. 
//This tells Unity that this class can be serialized, 
//which means you can turn it into a stream of bytes and save it to a file on disk.
[System.Serializable]
public class SaveLoad
{
    public Loadout[] savedLoadouts = new Loadout[4]; // Stores the list of loadouts that a player will have


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
