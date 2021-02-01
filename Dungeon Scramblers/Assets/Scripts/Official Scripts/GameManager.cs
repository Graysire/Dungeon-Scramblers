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
    public Player[] Scramblers; // = new Scrambler class array of 4;
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
}
