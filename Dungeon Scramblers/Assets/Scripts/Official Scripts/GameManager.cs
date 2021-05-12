using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    static GameManager _managerInstance;

    public static GameManager ManagerInstance { get { return _managerInstance; } }

    [SerializeField]
    List<GameObject> objectsToNotDestroyOnLoad;      //Stores all the game objects that need to persist between scenes

    [SerializeField]
    ObjectDatabase OD;  //Stores every attack object for setting up players equiped items


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

    [SerializeField]
    Scrambler[] Scramblers; // = new Scrambler class array of 4;
    Transform[] PlayerTransforms;   //List of Scrambler positions for AI purposes
    List<Scrambler> DeadScramblers = new List<Scrambler>(); // list of alive Scramblers
    [SerializeField]
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
    public MapMaker Map;


    [Header("Player Spawning")]
    public GameObject[] PlayerPrefabs; //List of Playertypes to instantiate
    BitPacket bitPacket = new BitPacket(); // Bitpakcet info for reading player types


    [Header("ItemSpawning")]
    public GameObject[] PlayerItems;
    Random.State s; //State for Map Spawn
    int seed;

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
        if (_managerInstance != null && _managerInstance != this)
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


    void Start()
    {

        //Map.GetComponent<MapMaker>().GenerateMap(false);
        if (perkListPrefab)
            perkList = Instantiate(perkListPrefab, transform);
        //StartCoroutine(Map.GenerateMap(true));

        StartCoroutine(SpawnPlayers());

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
                Debug.Log("Out of Time: " + outOfTime);
                Debug.Log("Scramblers.Length: " + Scramblers.Length);
                Debug.Log("deadScramblers: " + deadScramblers);

                DestroyEverything(); //Destroys all Game Objects from Match and the Game manager
                GameOver(false); //Set Game Overstate which transfers to main menu
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

    //after the level has finishedloading place the scramblers and overlord in their positions
    void finishLevelLoading(Scene scene, LoadSceneMode mode)
    {
        Map = FindObjectOfType<MapMaker>();
        SetPlayerLocations(Map.rooms[0], Map.overlordRoom);
        SceneManager.sceneLoaded -= finishLevelLoading;
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
        // handle voting result and apply perk
        votingSystem.HandleResult();

        // turn on overlord once match start
        if (Overlord != null)
        {
            Overlord.OverviewCam.enabled = false;
            Overlord.NormalCam.enabled = true;
            Overlord.enabled = true;
            PhotonView OPview = gameObject.GetPhotonView();
            int PhotonID = gameObject.GetPhotonView().ViewID;
            object[] arguments = { PhotonID, 1 };
            OPview.RPC("OverLordSetUp", RpcTarget.OthersBuffered, arguments);
            // turn off the Overlord UI
            Overlord.GetComponent<Overlord>().OverlordUI.SetActive(false);
        }

        // start match
        timer.InitializeAndStartTimer(matchTimeInSeconds, true);
        // TODO:
        foreach (GameObject obj in Map.GetBlockers())
        {
            Destroy(obj);
        }
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
        SceneManager.sceneLoaded += finishLevelLoading;

        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.LoadLevel(BossRoomSceneName);

        timer.InitializeAndStartTimer(bossFightTimeInSeconds, true); //start boss fight timer
    }

    //Generates a level
    void GenerateLevel()
    {
        if (currentRound == 1)
            StartCoroutine(Map.GenerateMap(true));
        //Map.GenerateMap(true);
        else
            StartCoroutine(Map.GenerateMap(false));
        //Map.GenerateMap(false);
        SetPlayerLocations(Map.rooms[0], Map.overlordRoom);
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
    public void SetPlayerLocations(MapMaker.RoomInfo scramblerRoom, MapMaker.RoomInfo overlordRoom)
    {
        foreach (Transform p in PlayerTransforms)
        {
            Vector3 newPos = Map.tilemaps[0].CellToWorld(new Vector3Int(Random.Range(scramblerRoom.lowerLeft.x, scramblerRoom.upperRight.x), Random.Range(scramblerRoom.lowerLeft.y, scramblerRoom.upperRight.y), 0));
            p.position = newPos;
        }
        if (Overlord != null)
        {
            Overlord.transform.position = Map.tilemaps[0].CellToWorld(new Vector3Int(Random.Range(overlordRoom.lowerLeft.x, overlordRoom.upperRight.x), Random.Range(overlordRoom.lowerLeft.y, overlordRoom.upperRight.y), 0));

        }
    }

    [PunRPC]
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
        Overlord = FindObjectOfType<Overlord>();

        PlayerTransforms = new Transform[Scramblers.Length];

        for (int i = 0; i < Scramblers.Length; i++)
        {
            PlayerTransforms[i] = Scramblers[i].transform;
            Scramblers[i].SetEXPBar(GameObject.Find("Experience Bar").GetComponent<DisplayBar>());
            Scramblers[i].SetHealthBar(GameObject.Find("HealthBar").GetComponent<DisplayBar>());
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
        Debug.Log("Destroy Everything!");
        foreach (GameObject go in objectsToNotDestroyOnLoad)
        {
            Destroy(go);
        }
        Destroy(gameObject);
    }



    #region PlayerSpawning
    //Coroutine for debugging purposes
    IEnumerator SpawnPlayers()
    {
        yield return new WaitForSeconds(2);
        if (PhotonNetwork.IsConnectedAndReady)
        {
            ////If we aren't the master Client, give us the seed
            //if (!PhotonNetwork.LocalPlayer.IsMasterClient)
            //{
            //    Random.state = s;
            //    Random.InitState(seed);
            //    Debug.Log("Seed for NonMaster Clinet: " + seed);
            //}
            StartCoroutine(Map.GenerateMap(true));

            //Player Spawning
            object PlayerSelectionNumber;
            //Check if player has an available loadout
            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(DungeonScramblersGame.PLAYER_SELECTION_NUMBER, out PlayerSelectionNumber))
            {
                Debug.Log("Player Number: " + (int)PlayerSelectionNumber);

                //Get Player Category, save for later for loadout implementation
                Categories.PlayerCategories SavedPlayerType = GetPlayerCategory((int)PlayerSelectionNumber);


                //Inventory Reading Starts here/////
                int codeWepaon = GetInventoryCode(SavedPlayerType, Categories.ItemCategory.weapon);
                int codeArmor = GetInventoryCode(SavedPlayerType, Categories.ItemCategory.armor);
                int codeAbility1 = GetInventoryCode(SavedPlayerType, Categories.ItemCategory.ability1);
                int codeAbility2 = GetInventoryCode(SavedPlayerType, Categories.ItemCategory.ability2);

                Debug.Log("Saved Player Type:" + SavedPlayerType);

                int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;

                //Start Countdown
                //Spawn Overlord at the Exit door

                    //Check player for Overlord Category
                    if (SavedPlayerType == Categories.PlayerCategories.overlord)
                    {
                        //Debug.Log("Overlord Player selected");
                        Vector3 Spawn = SetupSpawning(true);
                        GameObject PlayerGO = PlayerPrefabs[(int)PlayerSelectionNumber];
                        //Set Player Camera to Map view and turn off regular controls
                        PlayerGO = PhotonNetwork.Instantiate(PlayerGO.name, SetupSpawning(true), Quaternion.identity);
                        PlayerGO.GetComponent<Overlord>().OverviewCam.enabled = true;
                        PlayerGO.GetComponent<Overlord>().NormalCam.enabled = false;
                        PlayerGO.GetComponent<Overlord>().enabled = false;
                        //PlayerGO.GetComponent<SpriteRenderSwitch>().SpritesOff();
                         PhotonView OPview = gameObject.GetPhotonView();
                         int PhotonID = gameObject.GetPhotonView().ViewID;
                         OPview.RPC("OverLordSetUp", RpcTarget.OthersBuffered, PhotonID, 0); 
                    //Start Countdown
                    //Spawn Overlord at the Exit door

                    }       
                    else //All other players spawn like normal
                    {
                        Vector3 Spawn = SetupSpawning(false);
                        Debug.Log(Spawn);
                        GameObject PlayerGO = PhotonNetwork.Instantiate(PlayerPrefabs[(int)PlayerSelectionNumber].name, Spawn, Quaternion.identity);
                        Scrambler scrambler = PlayerGO.GetComponent<Scrambler>();
                        //AddScrambler( scrambler);

                    }
            }
            else
            {
                //Debug.Log("Default Player Spawning");
                Vector3 Spawn = SetupSpawning(false);
                int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
                //yield return new WaitForSeconds(1f);
                Debug.Log(Spawn);
                GameObject PlayerGO = PhotonNetwork.Instantiate(PlayerPrefabs[0].name, Spawn, Quaternion.identity);
            }
        }
        yield return new WaitForSeconds(3f);
        SetPlayerAbilities(); //Sets up all player abilities
        SetScramblers();
    }

    //This returns the vector3 to spawn the player at
    Vector3 SetupSpawning(bool bIsOverlord)
    {
        //Get Start Roomn from MapMaker
        MapMaker.RoomInfo StartRoom = Map.rooms[0];

        // Get Overlord Room
        MapMaker.RoomInfo OverlordRoom = Map.overlordRoom;

        // Set spawn room of the current player
        MapMaker.RoomInfo SpawnRoom;
        if (bIsOverlord)
        {
            SpawnRoom = OverlordRoom;
        }
        else
        {
            SpawnRoom = StartRoom;
        }

        //Get roomsize
        Vector2Int roomSize = new Vector2Int(SpawnRoom.upperRight.x - SpawnRoom.lowerLeft.x + 2, SpawnRoom.upperRight.y - SpawnRoom.lowerLeft.y + 2);
        //Get Random Coords in Room
        int randX = Random.Range((SpawnRoom.lowerLeft.x), (SpawnRoom.upperRight.x) + 1);
        int randY = Random.Range((SpawnRoom.lowerLeft.y), (SpawnRoom.upperRight.y) + 1);
        //Translate to world space
        Vector3Int randLocation = new Vector3Int(randX, randY, 0);

        Vector3 worldLocation = Map.GetComponent<MapMaker>().tilemaps[0].GetCellCenterWorld(randLocation);
        return worldLocation;
    }
    #endregion


    public int PlayerType(Categories.PlayerCategories PC)
    {
        int code = 0;
        //mage
        if (PC == Categories.PlayerCategories.mage)
        {
            Debug.Log("We have:" + PC);
            code = 1;
        }
        //knight
        if (PC == Categories.PlayerCategories.knight)
        {
            Debug.Log("We have:" + PC);
            code = 2;
        }
        //roguea
        if (PC == Categories.PlayerCategories.rogue)
        {
            Debug.Log("We have:" + PC);
            code = 3;
        }
        //overlord
        if (PC == Categories.PlayerCategories.overlord)
        {
            Debug.Log("We have:" + PC);
            code = 4;
        }

        return code;
    }

    #region Inventory Reading


    public Categories.PlayerCategories GetPlayerCategory(int playerCat)
    {
        Categories.PlayerCategories playerCategory = (Categories.PlayerCategories)playerCat;


        return playerCategory;
    }


    //Provided the player enum and the category of the item type wanted, this returns 
    //the item code for the player
    public int GetInventoryCode(Categories.PlayerCategories playerCategory, Categories.ItemCategory category)
    {
        int code = 0;
        //mage
        if (playerCategory == Categories.PlayerCategories.mage)
        {
            code = GetCode(bitPacket.mageInvBitsPacked, category);
        }
        //knight
        if (playerCategory == Categories.PlayerCategories.knight)
        {
            code = GetCode(bitPacket.knightInvBitsPacked, category);
        }
        //rogue
        if (playerCategory == Categories.PlayerCategories.rogue)
        {
            code = GetCode(bitPacket.rogueInvBitsPacked, category);
        }
        //overlord
        if (playerCategory == Categories.PlayerCategories.overlord)
        {
            code = GetCode(bitPacket.overlordInvBitsPacked, category);
        }
        Debug.Log("Code Found: " + code + " for category: " + category);
        return code;
    }

    //Retrieves the code at of the inventory given the category
    private int GetCode(int inventory, Categories.ItemCategory category)
    {
        int code = inventory;
        if (category == Categories.ItemCategory.weapon)
        {
            code = code << 3;
            code = code >> 27;
        }
        if (category == Categories.ItemCategory.armor)
        {
            code = code << 8;
            code = code >> 27;
        }
        if (category == Categories.ItemCategory.ability1)
        {
            code = code << 13;
            code = code >> 26;
        }
        if (category == Categories.ItemCategory.ability2)
        {
            code = code << 19;
            code = code >> 26;
        }
        return code;
    }

    private void SetPlayerAbilities()
    {
        InventoryHandler IH = FindObjectOfType<InventoryHandler>();
        int code = IH.GetPlayerBits();

        int weaponCode = GetCode(code, Categories.ItemCategory.weapon);
        int ability1Code = GetCode(code, Categories.ItemCategory.ability1);

        Categories.PlayerCategories playerCategory = IH.GetPlayerCategory(); //Get reference to Inventory Handler data for one player

        //obtain Ability Objects
        foreach (GameObject GO in OD.GetObjectList())
        {
            Ability a = GO.GetComponent<Ability>();

            //Check for weapons
            if (a != null && a.CompareWith(weaponCode, Categories.ItemCategory.weapon, playerCategory))
            {
                Debug.Log("Found a matching weapon!");

                if (PhotonNetwork.CurrentRoom != null)
                {
                    //TODO
                }
                else
                    Scramblers[0].SetAttackObjectsList(0, GO); //Set this as the new weapon
            }

            //Check for ability1
            if (a != null && a.CompareWith(ability1Code, Categories.ItemCategory.ability1, playerCategory))
            {
                Debug.Log("Found a matching abiliy1!");
                if (PhotonNetwork.CurrentRoom != null)
                {
                    //TODO
                }
                else
                    Scramblers[0].SetAttackObjectsList(1, GO); //Set this as the new ability 1
            }
        }

    }

    #endregion

    #region Overlord Setup
    [PunRPC]
    void OverLordSetUp(int PhotonID, int SetSprite)
    {
        GameObject overlord = PhotonView.Find(PhotonID).gameObject;
       
        if (SetSprite == 1)
        {
            overlord.GetComponent<SpriteRenderSwitch>().SpritesOn();
        }
        else
        {
            overlord.GetComponent<SpriteRenderSwitch>().SpritesOff();
        }
    }

    #endregion
}
