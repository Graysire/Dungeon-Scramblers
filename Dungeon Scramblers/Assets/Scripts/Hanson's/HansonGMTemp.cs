using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Tilemaps;
using Cinemachine;

public class HansonGMTemp : MonoBehaviour
{
    BitPacket bitPacket = new BitPacket();
    public GameObject[] PlayerPrefabs;
    public GameObject EnemyTest;
    public CinemachineVirtualCamera OverlordCam;
    public GameObject Map;
    private Vector2Int roomSize;
    Vector3 worldLocation;
    Random.State s;
    // Start is called before the first frame update

    void Start()
    {
        StartCoroutine(SpawnPlayers());

    }

    //Coroutine for debugging purposes
    IEnumerator SpawnPlayers()
    {
        //Display StartButton for Party Leader only
        //Get Party Leader Seed for Map Generation
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            //Save seed of MasterClient
            s = Random.state;
            //Add Seed to hash table to save for later
            //Debug.Log("Seed: " + s);
        }

        yield return new WaitForSeconds(2);
        if (PhotonNetwork.IsConnectedAndReady)
        {
            //If we aren't the master Client, give us the seed
            if (!PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                Random.state = s;
            }

            //Player Spawning
            object PlayerSelectionNumber;
            //Check if player has an available loadout
            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(DungeonScramblersGame.PLAYER_SELECTION_NUMBER, out PlayerSelectionNumber))
            {
                Debug.Log("Player Number: " + (int)PlayerSelectionNumber);

                //Get Player Category, save for later for loadout implementation
                Categories.PlayerCategories SavedPlayerType = GetPlayerCategory((int)PlayerSelectionNumber);

                Debug.Log("Saved Player Type:" + SavedPlayerType);

                int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
                Vector3 Spawn = SetupSpawning();
                GameObject PlayerGO = PhotonNetwork.Instantiate(PlayerPrefabs[(int)PlayerSelectionNumber].name, Spawn, Quaternion.identity);

                //Check player for Overlord Category
                if (SavedPlayerType == Categories.PlayerCategories.overlord)
                {
                    Debug.Log("Overlord Player selected");

                    //Set Player Camera to Map view

                    //Set Player Controls to Map Controls

                    //Start Countdown
                }


            }
            else
            {
                Debug.Log("Default Player Spawning");
                Vector3 Spawn = SetupSpawning();
                int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
                yield return new WaitForSeconds(1f);
                GameObject PlayerGO = PhotonNetwork.Instantiate(PlayerPrefabs[0].name, Spawn, Quaternion.identity);
            }
        }
    }

    Vector3 SetupSpawning()
    {
        //Get Start Roomn from MapMaker
        MapMaker.RoomInfo StartRoom = Map.GetComponent<MapMaker>().rooms[0];
        //Get roomsize
        roomSize = new Vector2Int(StartRoom.upperRight.x - StartRoom.lowerLeft.x + 1, StartRoom.upperRight.y - StartRoom.lowerLeft.y + 1);
        //Get Random Coords in Room
        int randX = Random.Range((StartRoom.lowerLeft.x), (StartRoom.upperRight.x) + 1);
        int randY = Random.Range((StartRoom.lowerLeft.y), (StartRoom.upperRight.y) + 1);
        //Translate to world space
        Vector3Int randLocation = new Vector3Int(randX, randY, 0);
        //worldLocation = Map.GetComponent<MapMaker>().tilemap.GetCellCenterWorld(randLocation);
        return worldLocation;
    }


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

    #endregion

    #region Overlord Setup
    IEnumerator OverLordSetUp()
    {
        yield return new WaitForSeconds(30f);
    }

    #endregion
}
