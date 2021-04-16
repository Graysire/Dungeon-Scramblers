using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class GameManager : MonoBehaviour
{
    static GameManager _managerInstance;
    public static GameManager ManagerInstance { get { return _managerInstance; } }

    bool isLoading;

    // Voting Stats
    [SerializeField]
    PerkList perkList;

    // Players stats
    bool isPlayerDead;
    Scrambler[] Scramblers; // = new Scrambler class array of 4;
    Transform[] PlayerTransforms;   //List of Scrambler positions for AI purposes
    Scrambler[] AliveScramblers; // list of alive Scramblers
    Overlord Overlord; // = new Overlord class;

    // Experience Handling Variables
    int level = 1;
    int currentExperience = 0;
    int expToNextLevel = 10000;
    int xpMultiplier = 100;

    //State handling variables
    [SerializeField]
    Timer timer;
    bool outOfTime = false; //Determines if timer ended resulting in game over state

    //Update handler stuff
    protected virtual void OnEnable()
    {
        UpdateHandler.UpdateOccurred += Updater;
    }
    protected virtual void OnDisable()
    {
        UpdateHandler.UpdateOccurred -= Updater;
    }


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

        if (perkList != null)
        {
            
            ApplyPerk(perkList.GetPerk());
        }

        //Begin Timer
        timer.InitializeAndStartTimer(5);

    }

    //Update
    private void Updater()
    {
        if (outOfTime)
        {
            Debug.Log("GM: TIMER ENDED SO GAME OVER");
        }
    }


    //sets out of time so game will end
    public void TimerOver()
    {
        outOfTime = true;
    }

    public void DistributeExperience(int experience)
    {
        currentExperience += experience * xpMultiplier / 100;
        
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
            scrambler.updateExperience(currentExperience / (float)expToNextLevel);
        }
        Debug.Log("Level: " + level + " | Experience: " + currentExperience + "/ " + expToNextLevel);

        
    }

    void GenerateLevel()
    {

    }

    void LoadLoadouts()
    {

    }

    //Applies the perks to each scrambler
    void ApplyPerk(Perk perk)
    {
        if (perk.GetSingleApplication())
        {
            perk.Equip(Scramblers[0]);
        }
        else
        {
            foreach (Scrambler s in Scramblers)
            {
                perk.Equip(s);
                Debug.Log(perk.gameObject.name);
            }
        }
    }

    //Returns a list of ALIVE Scrambler transforms
        //Is called by AIManager to update AI
    public Transform[] GetPlayerTransforms()
    {
        return PlayerTransforms;
    }

    //Returns a list of Scramblers in the game
    public Player[] GetPlayers()
    {
        return Scramblers;
    }

    //gets the current xp multiplier
    public int GetExperienceMultiplier()
    {
        return xpMultiplier;
    }

    //sets the current xp multiplier
    public void SetExperienceMultiplier(int value)
    {
        xpMultiplier = value;
    }
}
