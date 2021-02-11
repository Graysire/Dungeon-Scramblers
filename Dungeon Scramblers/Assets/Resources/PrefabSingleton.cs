using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabSingleton : MonoBehaviour
{
    public List<PlayerData> Players = new List<PlayerData>();
    private static PrefabSingleton _instance;
    public static PrefabSingleton Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<PrefabSingleton>();
                if (_instance == null)
                {
                    var GO = new GameObject("SingletonPrefab");
                    _instance = GO.AddComponent<PrefabSingleton>();
                }
                DontDestroyOnLoad(_instance.gameObject);
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if(Instance != this)
        {
            Debug.Log($"Singleton Prefab Duplicate, deleting {this.name}", this);
            DestroyImmediate(this.gameObject);
        }
        else
        {
            DontDestroyOnLoad(this.gameObject);
        }
    }

    public void GetPlayers()
    {
        //For players in lobby ,add them to list
        //foreach()
    }

    public void GetPlayerInfo()
    {
        //Get neccessary player info related to their loadouts
    }

    public void AddPlayer(PlayerData player)
    {
        Players.Add(player);
        Debug.Log("Player: " + player.PlayerName + " has been added");
    }
}

