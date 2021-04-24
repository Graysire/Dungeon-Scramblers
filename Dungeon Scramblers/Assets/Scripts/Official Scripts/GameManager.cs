using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    static GameManager _managerInstance;

    public static GameManager ManagerInstance { get { return _managerInstance; } }

    [SerializeField]
    List<GameObject> objectsToNotDestroyOnLoad;      //Stores all the game objects that need to persist between scenes


    [SerializeField]
    string MenuSceneName = "Multiplayer test";   //Name of the scene to transition to after game over
    [SerializeField]
    string BossRoomSceneName = "Minotaur Scene";    //Name of the scene to transition to for boss fight

    bool ready = false; //Stops Update from running till Scramblers are properly loaded


    // Voting Stats
    [SerializeField]
    PerkList perkListPrefab;
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


    //State handling variables
    [Header("Timer Variables")]
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

        Map = FindObjectOfType<MapMaker>();

        if (PhotonNetwork.CurrentRoom == null)
        {
            SetScramblers();
        }
    }


    //Update
    private void Updater()
    {
        if (ready)
        {
            //will generate a new level
            if (createNewLevel)
            {
                //Generate Overlord Level if max rounds exceeded
                if (currentRound == (numberOfRounds + 1))
                {
                    currentRound++;
                    GenerateOverlordLevel();    //Generate the overlord level
                }
                //Generate new round
                else
                {
                    GenerateLevel();  //Generates a new round
                    StartVoteTimer(); //Begins timer for vote stage
                }
                createNewLevel = false;
            }

            // match time over or all scramblers dead then game over
            if (outOfTime || Scramblers.Length == deadScramblers)
            {
                GameOver(false); //Set Game Overstate which transfers to main menu
                DestroyEverything(); //Destroys all Game Objects from Match and the Gmae manager
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
    public void StartVoteTimer()
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

    // Loads Overlord room
    void GenerateOverlordLevel()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.LoadLevel(BossRoomSceneName);
        Map = FindObjectOfType<MapMaker>();
        SetPlayerLocations(Map.rooms[0]);
        timer.InitializeAndStartTimer(bossFightTimeInSeconds, true); //start boss fight timer
    }

    //Generates a level
    void GenerateLevel()
    {
        if (currentRound == 1)
            StartCoroutine(Map.GenerateMap(true));
        else
            StartCoroutine(Map.GenerateMap(false));
        SetPlayerLocations(Map.rooms[0]);
    }

    //Game Over State
    private void GameOver(bool ScramblersWon)
    {
        if (ScramblersWon)
            Debug.Log("TODO: GM: GAME OVER -- SCRAMBLERS WON");
        else
            Debug.Log("TODO: GM: GAME OVER -- OVERLORD WON");

        //Load Main Menu Scene here
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.LoadLevel(MenuSceneName);
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

    //Teleports the scramblers and overlords to their respective starting rooms
    public void SetPlayerLocations(MapMaker.RoomInfo room)
    {
        foreach (Transform p in PlayerTransforms)
        {
            Vector3 newPos = Map.tilemaps[0].CellToWorld(new Vector3Int(Random.Range(room.lowerLeft.x, room.upperRight.x), Random.Range(room.lowerLeft.y, room.upperRight.y), 0));
            p.position = newPos;
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

    //returns a perk from the perkList selectingrandomly if getRevive is false, otehrwise sendsthe Revival perk
    public Perk GetPerk(bool getRevive)
    {
        if (getRevive)
        {
            return perkList.getRevive();
        }
        else
        {
            return perkList.GetPerk();
        }
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

    public void SetScramblers()
    {
        Scramblers = FindObjectsOfType<Scrambler>();
        Map = FindObjectOfType<MapMaker>();

        PlayerTransforms = new Transform[Scramblers.Length];

        for (int i = 0; i < Scramblers.Length; i++)
        {
            PlayerTransforms[i] = Scramblers[i].transform;
        }


        if (perkListPrefab)
            perkList = Instantiate(perkListPrefab, transform);

        if (perkList != null)
        {
            ApplyPerk(perkList.GetPerk());
        }


        //createNewLevel = true;
        ready = true;

        SetObjectsToNotDestroyOnLoad();
        StartVoteTimer(); //initial call to start timer
    }

    // Store all game objects that need to persist to Overlord Room Scene //
    void SetObjectsToNotDestroyOnLoad()
    {
        // Add Scramblers and Overlord here
        for (int i = 0; i < Scramblers.Length; i++)
        {
            objectsToNotDestroyOnLoad.Add(Scramblers[i].gameObject);
            foreach (GameObject go in Scramblers[i].GetAttackObjectsList())
            {
                objectsToNotDestroyOnLoad.Add(go);
            }
        }
        if (Overlord != null)
        {
            objectsToNotDestroyOnLoad.Add(Overlord.gameObject);
        }

        // Add Timer and Canvas here
        objectsToNotDestroyOnLoad.Add(timer.gameObject);
        objectsToNotDestroyOnLoad.Add(GameObject.FindGameObjectWithTag("Canvas"));

        // Make all gathered game objects persistant
        foreach (GameObject go in objectsToNotDestroyOnLoad)
        {
            DontDestroyOnLoad(go);
        }
    }

    // Sets game destory all persistent objects and the Game Manager itself
    void DestroyEverything()
    {
        foreach (GameObject go in objectsToNotDestroyOnLoad)
        {
            Destroy(go);
        }
        Destroy(gameObject);
    }
}
