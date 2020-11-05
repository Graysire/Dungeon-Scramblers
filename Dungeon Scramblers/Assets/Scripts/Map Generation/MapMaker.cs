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
        GenerateRoom(new Vector3Int(0, 0, 0), 0);
        //GenerateRoom(new Vector3Int(0, 0, 0), 1);
        //GenerateRoom(new Vector3Int(0, 0, 0), 2);
        //GenerateRoom(new Vector3Int(0, 0, 0), 3);
    }

    // Update is called once per frame
    void Update()
    {

    }


    //generates a room with an entering door at doorposition towards doorDirection
    //0 = Up, 1 = Right, 2 = Down, 3 = Left
    //Due to the function of box fill, cannot safely generate multiple rooms aorund one poin
    //Exception to the above is generating 0 and, 1 and 3, 0 then 3 then 2, or 0 then 1 then 2
    void GenerateRoom(Vector3Int doorPosition, int doorDirection)
    {
        int newRoomChance = 35;

        //generate a random x size of the floor space of the room, multiply by negative one if the direction is left
        int randX = Random.Range(minRoomSize, maxRoomSize);// * (doorDirection == 3 ? -1 : 1);
        //generate a random x size of the floor space of the room, multiply by negative one if the direction is down
        int randY = Random.Range(minRoomSize, maxRoomSize);// * (doorDirection == 2 ? -1 : 1);

        //set the starting positions to draw tiles from
        int startX = doorPosition.x; //+ (doorDirection == 1 ? 1 : doorDirection == 3 ? -1 : 0) + (int) Mathf.Ceil(randX / 2f);
        int startY = doorPosition.y; //+ (doorDirection == 0 ? 1 : doorDirection == 2 ? -1 : 0) + (int) Mathf.Ceil(randY / 2f);

        //based on door direction, modify starting positions and set a tile to expand thetilemap to cover the room
        switch (doorDirection)
        {
            case 0:
                startY += 1;
                startX -= (int)Mathf.Floor(randX / 2f);
                tilemap.SetTile(new Vector3Int(startX - 1, startY + randY, 0), wallTile);
                break;
            case 1:
                startX += 1;
                startY -= (int)Mathf.Floor(randY / 2f);
                tilemap.SetTile(new Vector3Int(startX + randX, startY - 1, 0), wallTile);
                break;
            case 2:
                startY -= randY;
                startX -= (int)Mathf.Floor(randX / 2f);
                tilemap.SetTile(new Vector3Int(startX - 1, startY - 1, 0), wallTile);
                break;
            case 3:
                startX -= randX;
                startY -= (int)Mathf.Floor(randY / 2f);
                tilemap.SetTile(new Vector3Int(startX - 1, startY - 1, 0), wallTile);
                break;
        }

        //Debug statements for room sizes
        //Debug.Log("Start X: " + startX + " Start Y: " + startY);
        //Debug.Log("Rand X: " + randX + " Rand Y: " + randY);
        //Debug.Log("End X: " + (startX + randX - 1) + " End Y: " + (startY + randY - 1));

        //if no tile exists, set a wall tile to expandthe tilemap
        if (!tilemap.HasTile(new Vector3Int(startX + randX, startY + randY, 0)))
        { 
            tilemap.SetTile(new Vector3Int(startX + randX, startY + randY, 0), wallTile);
        }

        //fill the area of the room with wall tiles
        tilemap.BoxFill(new Vector3Int(startX, startY, 0), wallTile, startX - 1, startY - 1, startX + randX, startY + randY);
        //fill the inside of the room with floor tiles
        tilemap.BoxFill(new Vector3Int(startX, startY, 0), floorTile,startX,startY,startX+randX - 1,startY+randY - 1);

        Vector3Int randDoorLocation;
        if (doorDirection != 0 && Random.Range(0,100) < newRoomChance)
        {
            randDoorLocation = new Vector3Int(startX + Random.Range(1, randX - 1), startY - 1, 0);
            tilemap.SetTile(randDoorLocation, doorTile);
            GenerateRoom(randDoorLocation, 2);
        }
        if (doorDirection != 1 && Random.Range(0, 100) < newRoomChance)
        {
            randDoorLocation = new Vector3Int(startX - 1, startY + Random.Range(1, randY - 1), 0);
            tilemap.SetTile(randDoorLocation, doorTile);
            GenerateRoom(randDoorLocation, 3);
        }
        if (doorDirection != 2 && Random.Range(0, 100) < newRoomChance)
        {
            randDoorLocation = new Vector3Int(startX + Random.Range(1, randX - 1), startY + randY, 0);
            tilemap.SetTile(randDoorLocation, doorTile);
            GenerateRoom(randDoorLocation, 0);
        }
        if (doorDirection != 3 && Random.Range(0, 100) < newRoomChance)
        {
            randDoorLocation = new Vector3Int(startX + randX, startY + Random.Range(1, randY - 1), 0);
            tilemap.SetTile(randDoorLocation, doorTile);
            GenerateRoom(randDoorLocation, 1);
        }
        /*
        //based on door direction, place doors
        switch (doorDirection)
        {
            case 0:
                if (Random.Range(0, newRoomChance) < 1)
                {
                    Vector3Int randDoorLocation = new Vector3Int(startX - 1, startY + Random.Range(1, randY - 1), 0);
                    tilemap.SetTile(randDoorLocation, doorTile);
                    GenerateRoom(randDoorLocation, 3);
                }
                if (Random.Range(0, newRoomChance) < 1)
                {
                    Vector3Int randDoorLocation = new Vector3Int(startX + randX, startY + Random.Range(1, randY - 1),0);
                    tilemap.SetTile(randDoorLocation, doorTile);
                    GenerateRoom(randDoorLocation, 1);
                }
                if (Random.Range(0, newRoomChance) < 1)
                {
                    Vector3Int randDoorLocation = new Vector3Int(startX + Random.Range(1, randX - 1), startY + randY, 0);
                    tilemap.SetTile(randDoorLocation, doorTile);
                    GenerateRoom(randDoorLocation, 0);
                }
                break;
            case 1:
                if (Random.Range(0, newRoomChance) < 1)
                {
                    Vector3Int randDoorLocation = new Vector3Int(startX + randX, startY + Random.Range(1, randY - 1), 0);
                    tilemap.SetTile(randDoorLocation, doorTile);
                    GenerateRoom(randDoorLocation, 1);
                }
                if (Random.Range(0, newRoomChance) < 1)
                {
                    Vector3Int randDoorLocation = new Vector3Int(startX + Random.Range(1, randX - 1), startY + randY, 0);
                    tilemap.SetTile(randDoorLocation, doorTile);
                    GenerateRoom(randDoorLocation, 0);
                }
                if (Random.Range(0, newRoomChance) < 1)
                {
                    
                }
                break;
            case 2:
                if (Random.Range(0, newRoomChance) < 1)
                {
                    Vector3Int randDoorLocation = new Vector3Int(startX - 1, startY + Random.Range(1, randY - 1), 0);
                    tilemap.SetTile(randDoorLocation, doorTile);
                    GenerateRoom(randDoorLocation, 3);
                }
                if (Random.Range(0, newRoomChance) < 1)
                {
                    Vector3Int randDoorLocation = new Vector3Int(startX + randX, startY + Random.Range(1, randY - 1), 0);
                    tilemap.SetTile(randDoorLocation, doorTile);
                    GenerateRoom(randDoorLocation, 1);
                }
                if (Random.Range(0, newRoomChance) < 1)
                {
                    Vector3Int randDoorLocation = new Vector3Int(startX + Random.Range(1, randX - 1), startY - 1, 0);
                    tilemap.SetTile(randDoorLocation, doorTile);
                    GenerateRoom(randDoorLocation, 2);
                }
                break;
            case 3:
                if (Random.Range(0, newRoomChance) < 1)
                {
                    Vector3Int randDoorLocation = new Vector3Int(startX - 1, startY + Random.Range(1, randY - 1), 0);
                    tilemap.SetTile(randDoorLocation, doorTile);
                    GenerateRoom(randDoorLocation, 3);
                }
                if (Random.Range(0, newRoomChance) < 1)
                {
                    Vector3Int randDoorLocation = new Vector3Int(startX + Random.Range(1, randX - 1), startY + randY, 0);
                    tilemap.SetTile(randDoorLocation, doorTile);
                    GenerateRoom(randDoorLocation, 0);
                }
                if (Random.Range(0, newRoomChance) < 1)
                {
                    Vector3Int randDoorLocation = new Vector3Int(startX + Random.Range(1, randX - 1), startY - 1, 0);
                    tilemap.SetTile(randDoorLocation, doorTile);
                    GenerateRoom(randDoorLocation, 2);
                }
                break;
        }*/
    }


}
