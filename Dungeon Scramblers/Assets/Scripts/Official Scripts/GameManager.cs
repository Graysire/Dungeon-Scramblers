using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class GameManager : MonoBehaviour
{
    static GameManager _managerInstance;
    public static GameManager ManagerInstance { get { return _managerInstance; } }

    bool isLoading;

    // Voting Stats
    float timer;

    // Players stats
    bool isPlayerDead;
    Scrambler[] Scramblers; // = new Scrambler class array of 4;
    Transform[] PlayerTransforms;   //List of Scrambler positions for AI purposes
    Scrambler[] AliveScramblers; // list of alive Scramblers
    Overlord Overlord; // = new Overlord class;

    // Experience Handling Variables
    int level = 1;
    float currentExperience = 0.0f;
    float expToNextLevel = 100.0f;

    private void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
        if(_managerInstance != null && _managerInstance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _managerInstance = this;
        }
        Scramblers = FindObjectsOfType<Scrambler>();


        PlayerTransforms = new Transform[Scramblers.Length];

        for (int i = 0; i < Scramblers.Length; i++)
        {
            PlayerTransforms[i] = Scramblers[i].transform;
        }
    }

    public void DistributeExperience(float experience)
    {
        currentExperience += experience;
        
        if (currentExperience >= expToNextLevel)
        {
            level++;
            // Change the value of base stats
            foreach (Scrambler scrambler in Scramblers)
            {
                scrambler.LevelUp();
            }

            currentExperience = 0;

        }

        foreach (Scrambler scrambler in Scramblers)
        {
            scrambler.updateExperience(currentExperience / expToNextLevel);
        }
        Debug.Log("Level: " + level + " | Experience: " + currentExperience + "/ " + expToNextLevel);

        
    }

    void GenerateLevel()
    {

    }

    void LoadLoadouts()
    {

    }

    void ApplyPerk()
    {
        
    }

    //Returns a list of ALIVE Scrambler transforms
        //Is called by AIManager to update AI
    public Transform[] GetPlayerTransforms()
    {
        return PlayerTransforms;
    }
}
