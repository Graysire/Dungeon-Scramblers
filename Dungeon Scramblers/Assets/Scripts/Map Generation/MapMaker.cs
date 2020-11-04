using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

//Script to generate a dungeon map
public class MapMaker : MonoBehaviour
{
    //tilemap to generate dungeon on
    [SerializeField]
    Tilemap tilemap;

    //default tile used for the floor
    [SerializeField]
    TileBase floorTile;
    //default tile used for walls
    [SerializeField]
    TileBase wallTile;
    //default tile used for doors
    [SerializeField]
    TileBase doorTile;

    //minimum number of floor tiles on on e side of a room
    [SerializeField]
    int minRoomSize;
    //maximum number of floor tiles on one side of a room
    [SerializeField]
    int maxRoomSize;

    // Start is called before the first frame update
    void Start()
    {
        
        tilemap.SetTile(new Vector3Int(0, 0, 0), doorTile);
        GenerateRoom(new Vector3Int(0, 0, 0), 3);
    }

    // Update is called once per frame
    void Update()
    {

    }


    //generates a room with an entering door at doorposition towards doorDirection
    //0 = Up, 1 = Right, 2 = Down, 3 = Left
    void GenerateRoom(Vector3Int doorPosition, int doorDirection)
    {
        //generate a random x size of the floor space of the room, multiply by negative one if the direction is left
        int randX = Random.Range(minRoomSize, maxRoomSize);// * (doorDirection == 3 ? -1 : 1);
        //generate a random x size of the floor space of the room, multiply by negative one if the direction is down
        int randY = Random.Range(minRoomSize, maxRoomSize);// * (doorDirection == 2 ? -1 : 1);


        int startX = doorPosition.x; //+ (doorDirection == 1 ? 1 : doorDirection == 3 ? -1 : 0) + (int) Mathf.Ceil(randX / 2f);
        int startY = doorPosition.y; //+ (doorDirection == 0 ? 1 : doorDirection == 2 ? -1 : 0) + (int) Mathf.Ceil(randY / 2f);

        switch (doorDirection)
        {
            case 0:
                startY += 1;
                startX -= (int) Mathf.Floor(randX / 2f);
                break;
            case 1:
                startX += 1;
                startY -= (int)Mathf.Floor(randY / 2f);
                break;
            case 2:
                startY -= randY;
                startX -= (int)Mathf.Floor(randX / 2f);
                break;
            case 3:
                startX -= randX;
                startY -= (int)Mathf.Floor(randY / 2f);
                break;
        }

        Debug.Log("Start X: " + startX);
        Debug.Log("Start Y: " + startY);
        Debug.Log("Rand X: " + randX);
        Debug.Log("Rand Y: " + randY);
        Debug.Log("End X: " + (startX + randX - 1));
        Debug.Log("End Y: " + (startY + randY - 1));

        tilemap.SetTile(new Vector3Int(startX + randX, startY + randY, 0), wallTile);
        //tilemap.SetTile(new Vector3Int(startX, startY, 0), wallTile);

        tilemap.BoxFill(new Vector3Int(startX, startY, 0), floorTile,startX,startY,startX+randX - 1,startY+randY - 1);
        //tilemap.BoxFill(new Vector3Int(startX, startY, 0), floorTile, startX, startY + randY + 1, startX + randX - 1, startY);
        //tilemap.BoxFill(new Vector3Int(startX, startY, 0), wallTile, startX - 1, startY - 1, startX + randX, startY + randY);
    }


}
