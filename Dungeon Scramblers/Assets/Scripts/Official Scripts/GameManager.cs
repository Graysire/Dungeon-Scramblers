using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    bool isLoading;

    // MapMaker
    MapMaker mapMaker = new MapMaker();

    // Voting Stats
    float timer;

    // Players stats
    bool isPlayerDead;
    Player[] Scramblers; // = new Scrambler class array of 4;
    Player Overlord; // = new Overlord class;

    private void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
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
