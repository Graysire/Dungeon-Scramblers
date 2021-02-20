using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MenuManager : MonoBehaviour
{
    /*
     * Responsible for getting player data for Audio settings and Loadout preferences
     * and sending the data to the SaveLoad system.
     */


    //https://www.doozyui.com/script-reference/


    SaveLoad dataManager = new SaveLoad();      //Saves and loads data from file

    public enum FileType
    {
        Mage = 0,
        Knight = 1,
        Rogue = 2,
        Overlord = 3,
        //Audio = 4
    }


    //Initialize the file if it isn't made
    //or load data from file
    void Awake()
    {
        //Load each of the file types into the system
        foreach (FileType type in (FileType[])Enum.GetValues(typeof(FileType)))
        {
            //Delete the file for now -- REMOVE LATER WHEN FULLY IMPLEMENTED
            dataManager.DeleteSaveFile((int)type);

            //If the file doesn't exist then initialize it
            if (!dataManager.FileExists((int)type))
                dataManager.InitializeFile((int)type);
            //If the file exists load it 
            else
                dataManager.Load((int)type);
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
