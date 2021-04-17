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

    [SerializeField]
    PhysicalVotingSystem votingSystem;

    // Players stats
    int deadScramblers = 0;
    Scrambler[] Scramblers; // = new Scrambler class array of 4;
    Transform[] PlayerTransforms;   //List of Scrambler positions for AI purposes
    List<Scrambler> DeadScramblers = new List<Scrambler>(); // list of alive Scramblers
    Overlord Overlord; // = new Overlord class;

    // Experience Handling Variables
    int level = 1;
    int currentExperience = 0;
    int expToNextLevel = 10000;
    int xpMultiplier = 100;

    [Header("Timer Variables")]
    //State handling variables
    [SerializeField]
    Timer timer;
    [SerializeField]
    int voteTimeInSeconds = 60;
    [SerializeField]
    int matchTimeInSeconds = 60;
    [SerializeField]
    int bossFightTimeInSeconds = 60;
    bool outOfTime = false; //Determines if timer ended resulting in game over state

    [Header("Match Variables")]
    [SerializeField]
    int numberOfRounds = 3;  //The number of rounds to play out before the overlord boss fight
    bool createNewLevel = false;
    int escapedScramblers = 0;
    int currentRound = 1;   //The current round being played
    MapMaker Map;

    
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
        Map = FindObjectOfType<MapMaker>();

        PlayerTransforms = new Transform[Scramblers.Length];

        for (int i = 0; i < Scramblers.Length; i++)
        {
            PlayerTransforms[i] = Scramblers[i].transform;
        }

        if (perkList != null)
        {
            ApplyPerk(perkList.GetPerk());
        }

        createNewLevel = true;
    }

    //Update
    private void Updater()
    {
        //will generate a new level
        if (createNewLevel)
        {
            //Generate Overlord Level if max rounds exceeded
            if (currentRound > numberOfRounds)
            {
                GenerateOverlordLevel();    //Generate the overlord level

            }
            //Generate new round
            else
            {
                GenerateLevel();  //Generates a new round
                StartVoteTimer(); //Begins timer for vote stage
                createNewLevel = false; 
            }
        }

        // match time over or all scramblers dead then game over
        if (outOfTime || Scramblers.Length == deadScramblers)
        {
            GameOver();
        }

        //If all remaining Scramblers escaped then Match completed
        if (escapedScramblers == (Scramblers.Length - deadScramblers))
        {
            Debug.Log("GM: ROUND COMPLETED");
            timer.DisableTimer(false, false); //forces timer to end

            currentRound++; //increment current round number

            createNewLevel = true; //creates a new level to play on

            escapedScramblers = 0;  //reset number of escaped scramblers

            Map.ClearMap(); //Clear the current map

            SetAllAliveScramblersActive(); //Sets all escaped specating players back to active
        }
    }

    private void SetAllAliveScramblersActive()
    {
        for (int i = 0; i < Scramblers.Length; i++)
        {
            if (Scramblers[i].IsAlive())
            {
                Scramblers[i].ResetEscaped();
                Scramblers[i].SetEscaped(false);
            }
        }
    }

    //Starts timer for voting and overlord setup
        //On end then match timer will begin
    private void StartVoteTimer()
    {
        //Begin Timer
        timer.InitializeAndStartTimer(voteTimeInSeconds, false);
    }



    //Handles data for when setup stage is over
    //Will start match timer and open doors
    public void BeginMatchTimer()
    {
        timer.InitializeAndStartTimer(matchTimeInSeconds, true);
        // TODO:
        //  -Unlock doors to rest of level
        //  -Change Overlord to minilord mode
    }

    //sets out of time so game will end
    public void TimerOver()
    {
        outOfTime = true;
    }

    void GenerateOverlordLevel()
    {
        Debug.Log("TODO: Generating Overlord Level");
        timer.InitializeAndStartTimer(bossFightTimeInSeconds, true); //start boss fight timer
    }

    //Generates a level
    void GenerateLevel()
    {
        if (currentRound == 1)
            StartCoroutine(Map.GenerateMap(true));
        else
            StartCoroutine(Map.GenerateMap(false));
    }

    //Game Over State
    private void GameOver()
    {
        Debug.Log("TODO: GM: GAME OVER -- OVERLORD WON");
    }

    //adds scrambler to list of dead scramblers
    public void AddScramblerDeath(Scrambler sc)
    {
        bool scramblerAlreadyAdded = false;
        for (int i = 0; i < DeadScramblers.Count; ++i)
        {
            if (sc == DeadScramblers[i])
            {
                scramblerAlreadyAdded = true;
            }
        }
        if (!scramblerAlreadyAdded)
        {
            DeadScramblers.Add(sc);
            deadScramblers++;
        }
    }

    //Removes scrambler from list of dead scramblers
    public void RemoveScramblerDeath(Scrambler sc)
    {
        for (int i = 0; i < DeadScramblers.Count; ++i)
        {
            if (sc == DeadScramblers[i])
            {
                deadScramblers--;
                DeadScramblers.RemoveAt(i);
                break;
            }
        }
    }

    public void IncrementEscapedScramblers()
    {
        escapedScramblers++; 
    }


    public void IncrementButton(VoteButton button)
    {
        votingSystem.IncrementButton(button);
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

    //apply player loadouts
    void LoadLoadouts()
    {
        
    }

    public Perk GetPerk()
    {
        return perkList.GetPerk();
    }

    //Applies the perks to each scrambler
    public void ApplyPerk(Perk perk)
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
        perkList.ResetPerks(perk);
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
