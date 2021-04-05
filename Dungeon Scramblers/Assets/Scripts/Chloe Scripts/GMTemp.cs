using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Tilemaps;
using Cinemachine;
//using UnityEditorInternal;

public class GMTemp : MonoBehaviour
{
    public GameObject[] PlayerPrefabs;
    public GameObject EnemyTest;
    public CinemachineVirtualCamera vcam;
    public GameObject Map;
    private Vector2Int roomSize;
    Vector3 worldLocation;
    Random.State s;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SpawnPlayers());
        //Get State of Game
        //Random.State s = Random.state;

    }
    IEnumerator SpawnPlayers()
    {
        //Display StartButton for Party Leader only
        //Get Party Leader Seed for Map Generation
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            //Save seed of MasterClient
            s = Random.state;
            //ExitGames.Client.Photon.Hashtable MapSeed = new ExitGames.Client.Photon.Hashtable() { { DungeonScramblersGame.MASTER_CLIENT_SEED, s } };
            //Add Seed to hash table to save for later
            //PhotonNetwork.LocalPlayer.SetCustomProperties(MapSeed);
            Debug.Log("Seed: " + s);
        }

        yield return new WaitForSeconds(2);
        if (PhotonNetwork.IsConnectedAndReady)
        {
            //If we aren't the master Client, give us the seed
            if(!PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                Random.state = s;
            }
            object PlayerSelectionNumber;
            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(DungeonScramblersGame.PLAYER_SELECTION_NUMBER, out PlayerSelectionNumber))
            {
                Debug.Log("Player Number: " + (int)PlayerSelectionNumber);

                int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;

                GameObject PlayerGO = PhotonNetwork.Instantiate(PlayerPrefabs[(int)PlayerSelectionNumber].name, SetupSpawning(), Quaternion.identity);

            }
            else
            {
                Debug.Log("Default Player Spawning");

                int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
                yield return new WaitForSeconds(1f);
                GameObject PlayerGO = PhotonNetwork.Instantiate(PlayerPrefabs[0].name, SetupSpawning(), Quaternion.identity);
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
        int randX = Random.Range((StartRoom.lowerLeft.x),(StartRoom.upperRight.x) + 1);
        int randY = Random.Range((StartRoom.lowerLeft.y), (StartRoom.upperRight.y) + 1);
        //Translate to world space
        Vector3Int randLocation = new Vector3Int(randX, randY, 0);
        //worldLocation = Map.GetComponent<MapMaker>().tilemap.GetCellCenterWorld(randLocation);
        return worldLocation;
    }



}
