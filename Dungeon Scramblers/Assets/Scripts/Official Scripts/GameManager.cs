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
    Player[] Scramblers; // = new Scrambler class array of 4;
    Transform[] PlayerTransforms;   //List of Scrambler positions for AI purposes
    Player[] AliveScramblers; // list of alive Scramblers
    Player Overlord; // = new Overlord class;

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
        foreach(Scrambler scrambler in Scramblers)
        {
            scrambler.AddExperience(experience);
        }
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
