using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Tilemaps;
using Cinemachine;

public class GMTemp : MonoBehaviour
{
    public GameObject PlayerTest;
    public GameObject EnemyTest;
    public CinemachineVirtualCamera vcam;
    public GameObject Map;
    private Vector2Int roomSize;
    Vector3 worldLocation;
    // Start is called before the first frame update
    void Start()
    {

        SetupSpawning();

        if (PhotonNetwork.IsConnectedAndReady)
        {
            if(PlayerTest != null)
            {
               
                GameObject PlayerGO = PhotonNetwork.Instantiate(PlayerTest.name, worldLocation, Quaternion.identity);

                vcam.m_Follow = PlayerGO.transform;
                //Refresh int randX and randY
                Debug.Log("Player Spawning");
              
            }
            
        }
    }

    void SetupSpawning()
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
    }

}
