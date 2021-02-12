using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Tilemaps;
using Cinemachine;

public class GMTemp : MonoBehaviour
{
    public GameObject[] PlayerPrefabs;
    public GameObject EnemyTest;
    public CinemachineVirtualCamera vcam;
    public GameObject Map;
    private Vector2Int roomSize;
    Vector3 worldLocation;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SpawnPlayers());

    }
    IEnumerator SpawnPlayers()
    {
        yield return new WaitForSeconds(2);
        if (PhotonNetwork.IsConnectedAndReady)
        {
            //if(PlayerTest != null)
            //{

            //    GameObject PlayerGO = PhotonNetwork.Instantiate(PlayerTest.name, worldLocation, Quaternion.identity);

            //    vcam.m_Follow = PlayerGO.transform;
            //    //Refresh int randX and randY
            //    Debug.Log("Player Spawning");

            //}
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
        int randX = Random.Range(Mathf.FloorToInt(StartRoom.lowerLeft.x), Mathf.FloorToInt(StartRoom.upperRight.x) + 1);
        int randY = Random.Range(Mathf.FloorToInt(StartRoom.lowerLeft.y), Mathf.FloorToInt(StartRoom.upperRight.y) + 1);
        //Translate to world space
        Vector3Int randLocation = new Vector3Int(randX, randY, 0);
        worldLocation = Map.GetComponent<MapMaker>().tilemap.GetCellCenterWorld(randLocation);
        return worldLocation;
    }



}
