using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

//Script to generate a dungeon map
public class MapMaker : MonoBehaviour
{
    [SerializeField]
    int maxIterations = 200;
    //[SerializeField]
    //int newDoorChance = 50;

    [SerializeField]
    //minimum number of doors that can be generated leading from rooms
    int minimumDoors = 1;
    [SerializeField]
    //maximum number of doors that can be generated leading form rooms
    int maximumDoors = 20;
    [SerializeField]
    //value used to non-linearly reduce the chance of new doors being produced
    int doorChanceReducer = 10;
    [SerializeField]
    int currentRoomNum = 0;



    [SerializeField]
    float waitTime = 0f;

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

    //minimum number of floor tiles on one side of a room
    [SerializeField]
    int minRoomSize;
    //maximum number of floor tiles on one side of a room
    [SerializeField]
    int maxRoomSize;

    //minimum length of a corridor
    [SerializeField]
    int minCorridorSize;
    //maximum length of a corridor
    [SerializeField]
    int maxCorridorSize;

    // Start is called before the first frame update
    void Start()
    {
        
        tilemap.SetTile(new Vector3Int(0, 0, 0), doorTile);
        StartCoroutine("GenerateMap");
        //GenerateMap();
    }

    // Update is called once per frame
    void Update()
    {

    }

    //generates a dungeon map, assumes the tilemap is currently empty
    IEnumerator GenerateMap()
    {
        //set current number of rooms to 0
        currentRoomNum = 0;

        List<DoorInfo> doorList = new List<DoorInfo>();

        doorList.Add(new DoorInfo(new Vector3Int(0, 0, 0), Facing.North));

        for (int i = 0; i < maxIterations; i++)
        {
            List<DoorInfo> newDoors = new List<DoorInfo>();
            //generate new rooms from the doors in doorList and add their doors to newDoors
            foreach (DoorInfo door in doorList)
            {
                newDoors.AddRange(GenerateRoom(door));
                yield return new WaitForSeconds(waitTime);
            }
            doorList.Clear();
            //generate new corridors from the doors in newDoors and and add their doors to doorList
            if (i + 1 < maxIterations)
            {
                foreach (DoorInfo door in newDoors)
                {
                    doorList.AddRange(GenerateCorridor(door));
                    yield return new WaitForSeconds(waitTime);
                }
            }
            else
            {
                foreach (DoorInfo door in newDoors)
                {
                    tilemap.SetTile(door.position, wallTile);
                }
            }
        }

        //generate the pathfinding grid
        Pathfinder.CreateGrid(tilemap.GetComponentInParent<Grid>(), tilemap, wallTile);
        yield return null;
    }

    //generates a room with an entering door at doorposition towards doorDirection
    //returns a list of all new doors created
    List<DoorInfo> GenerateRoom(DoorInfo door)
    {

        List<DoorInfo> newDoors = new List<DoorInfo>();

        //generate a random x size of the floor space of the room
        int randX = Random.Range(minRoomSize, maxRoomSize) + 1;// * (doorDirection == 3 ? -1 : 1);
        //generate a random x size of the floor space of the room
        int randY = Random.Range(minRoomSize, maxRoomSize) + 1;// * (doorDirection == 2 ? -1 : 1);

        //set the starting positions to draw tiles from
        int startX = door.position.x; //+ (doorDirection == 1 ? 1 : doorDirection == 3 ? -1 : 0) + (int) Mathf.Ceil(randX / 2f);
        int startY = door.position.y; //+ (doorDirection == 0 ? 1 : doorDirection == 2 ? -1 : 0) + (int) Mathf.Ceil(randY / 2f);

        //based on door facing, modify starting positions 
        switch (door.facing)
        {
            case Facing.North:
                //startY += 1;
                startX -= (int)Mathf.Floor(randX / 2f);
                //tilemap.SetTile(new Vector3Int(startX - 1, startY + randY, 0), wallTile);
                break;
            case Facing.East:
                //startX += 1;
                startY -= (int)Mathf.Floor(randY / 2f);
                //tilemap.SetTile(new Vector3Int(startX + randX, startY - 1, 0), wallTile);
                break;
            case Facing.South:
                startY -= randY;
                startX -= (int)Mathf.Floor(randX / 2f);
               // tilemap.SetTile(new Vector3Int(startX - 1, startY - 1, 0), wallTile);
                break;
            case Facing.West:
                startX -= randX;
                startY -= (int)Mathf.Floor(randY / 2f);
                //tilemap.SetTile(new Vector3Int(startX - 1, startY - 1, 0), wallTile);
                break;
        }

        //Debug statements for room sizes
        //Debug.Log("Start X: " + startX + " Start Y: " + startY);
        //Debug.Log("Rand X: " + randX + " Rand Y: " + randY);
        //Debug.Log("End X: " + (startX + randX - 1) + " End Y: " + (startY + randY - 1));

        //if no tile exists, set a wall tile to expandthe tilemap
        /*if (!tilemap.HasTile(new Vector3Int(startX + randX, startY + randY, 0)))
        { 
            tilemap.SetTile(new Vector3Int(startX + randX, startY + randY, 0), wallTile);
        }*/


        //the end locations of the room
        int endX = startX + randX;
        int endY = startY + randY;

        //place all the tiles that make up the room
        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                if (x == startX || x == endX || y == startY || y == endY)
                {
                    PlaceTile(new Vector3Int(x, y, 0), wallTile);
                }
                //if a non-floor tile is found, check if it has empty spaces around it that are beyond the borders of this room
                //if it does not, then replace the wall with floor
                else if (tilemap.HasTile(new Vector3Int(x,y,0)) && tilemap.GetTile(new Vector3Int(x, y, 0)) != floorTile)
                {
                    for (int x2 = 0; x2 < 2; x2++)
                    {
                        for (int y2 = 0; y2 < 2; y2++)
                        {
                            if (x2 != y2 && x2 * -1 != y2 && (!tilemap.HasTile(new Vector3Int(x + x2, y + y2, 0)) || (x + x2 >= startX && x + x2 <= endX && y + y2 >= startY && y + y2 <= endY)))
                            {
                                tilemap.SetTile(new Vector3Int(x, y, 0), floorTile);
                            }
                        }
                    }
                }
                else
                {
                    PlaceTile(new Vector3Int(x, y, 0), floorTile);
                }
                
            }
        }

        
        //fill the area of the room with wall tiles
        //tilemap.BoxFill(new Vector3Int(startX, startY, 0), wallTile, startX - 1, startY - 1, startX + randX, startY + randY);
        //fill the inside of the room with floor tiles
        //tilemap.BoxFill(new Vector3Int(startX, startY, 0), floorTile,startX,startY,startX+randX - 1,startY+randY - 1);


        Vector3Int randDoorLocation = new Vector3Int(0, 0, 0); ;

        //for each wall of the room except the entering wall, check for creation of new doors and add any new doors to the list of new doors
        for (int i = 0; i < 4; i++)
        {
            //if the wall being checked is not the wall being entered from
            if (Mathf.Abs(i - (int) door.facing) != 2)
            {
                //check if new door should be generated
                if (Random.Range(1, 100) <= GetDoorChance())
                {
                    currentRoomNum++;
                    //determine location of the new door
                    switch ((Facing) i)
                    {
                        case Facing.North:
                            randDoorLocation = new Vector3Int(startX + Random.Range(1, randX - 1), startY + randY, 0);
                            break;
                        case Facing.East:
                            randDoorLocation = new Vector3Int(startX + randX, startY + Random.Range(1, randY - 1), 0);
                            break;
                        case Facing.South:
                            randDoorLocation = new Vector3Int(startX + Random.Range(1, randX - 1), startY, 0);
                            
                            break;
                        case Facing.West:
                            randDoorLocation = new Vector3Int(startX, startY + Random.Range(1, randY - 1), 0);
                            break;
                    }

                    //place the new door as long as the location is inside a wall
                    //add it to the lsit of new doors
                    if (tilemap.GetTile(randDoorLocation) == wallTile)
                    {
                        tilemap.SetTile(randDoorLocation, doorTile);
                        newDoors.Add(new DoorInfo(randDoorLocation, (Facing) i));
                    }
                }
            } 
        }

        return newDoors;
    }

    //generates a corridor continuing away form the given door and returns a new door at the end of the corridor
    //if the corridor connects to another room or corridor, it will not return any doors
    //returns doors as a list to facilitate possible T intersections
    List<DoorInfo> GenerateCorridor(DoorInfo door)
    {
        List<DoorInfo> newDoors = new List<DoorInfo>();

        /*if (maxRooms < 0)
        {
            tilemap.SetTile(doorPosition, wallTile);
            return;
        }*/


        //generate a random length for the corridor
        int length = Random.Range(minCorridorSize, maxCorridorSize); //* (doorDirection >= 2 ? -1 : 1);

        //initialize adjustment
        int xAdjust = 0;
        int yAdjust = 0;

        //whether this corridor connects to a room that has already been generated
        bool isConnector = false;

        //set the x and y adjustment to tile location based off door direction
        //this decides what direction the tiles are put down
        switch (door.facing)
        {
            case Facing.North:
                xAdjust = 0;
                yAdjust = 1;

                break;
            case Facing.East:
                xAdjust = 1;
                yAdjust = 0;

                break;
            case Facing.South:
                xAdjust = 0;
                yAdjust = -1;

                break;
            case Facing.West:
                xAdjust = -1;
                yAdjust = 0;

                break;
        }

        Vector3Int endDoorPosition = door.position + new Vector3Int((length + 1) * xAdjust, (length + 1) * yAdjust, 0);
        Vector3Int corridorDirection = new Vector3Int(xAdjust, yAdjust, 0);

        for (int i = 1; i <= length + 1; i++)
        {
            if (tilemap.HasTile(door.position + i * corridorDirection))
            {
                endDoorPosition = door.position + i * corridorDirection;
                length = i - 1;
                isConnector = true;
                break;
            }
        }

        if (tilemap.HasTile(endDoorPosition + corridorDirection) && !isConnector)
        {
            endDoorPosition += corridorDirection;
            isConnector = true;
            length++;
        }
        else if (tilemap.HasTile(endDoorPosition + 2 * corridorDirection) && !isConnector)
        {
            endDoorPosition += 2 * corridorDirection;
            isConnector = true;
            length += 2;
        }

        if (isConnector && tilemap.GetTile(endDoorPosition + corridorDirection) == wallTile && length + 1 >= minCorridorSize)
        {
            tilemap.SetTile(endDoorPosition, floorTile);



            endDoorPosition += corridorDirection;
            length+= 2;
        }

        /* while (tilemap.HasTile(endDoorPosition))
         {
             if (tilemap.GetTile(endDoorPosition) != wallTile)
             {
                 length--;
                 endDoorPosition.y--;
             }
             else if(tilemap.GetTile(endDoorPosition + new Vector3Int(xAdjust,yAdjust,0)) == wallTile)
             {
                 tilemap.SetTile(endDoorPosition, floorTile);
                 length++;
                 endDoorPosition.y++;
             }
         }*/

        if (tilemap.GetTile(door.position + corridorDirection) == floorTile)
        {
            return newDoors;
        }
        else if (length < minCorridorSize)
        {
            tilemap.SetTile(door.position, wallTile);
            return newDoors;
        }

        tilemap.SetTile(endDoorPosition, doorTile);

        for (int i = 0; i <= length + 1; i++)
        {
            Vector3Int tileLoc = door.position + i * corridorDirection;

            PlaceTile(tileLoc, floorTile);
            PlaceTile(tileLoc + new Vector3Int(yAdjust, xAdjust, 0), wallTile);
            PlaceTile(tileLoc + new Vector3Int(-1 * yAdjust, -1 * xAdjust, 0), wallTile);


        }

        //if this corridor is not a connector, add it's ending door to the lsit of new doors
        if (!isConnector)
        {
            newDoors.Add(new DoorInfo(endDoorPosition, door.facing));
            //StartCoroutine(GenerateRoom(doorPosition + new Vector3Int((length + 1) * xAdjust, (length + 1) * yAdjust, 0), doorDirection));
        }
        return newDoors;
    }

    void PlaceTile(Vector3Int position, TileBase tile)
    {
        if (!tilemap.HasTile(position))
        {
            tilemap.SetTile(position, tile);
        }
    }

    int GetDoorChance()
    {
        if (currentRoomNum < minimumDoors)
        {
            return 100;
        }
        else if(currentRoomNum >= maximumDoors)
        {
            return 0;
        }
        else
        {
            Debug.Log("Num:" + currentRoomNum + ", C:" + (Mathf.RoundToInt(100 - (currentRoomNum * 100.0f) / (currentRoomNum + doorChanceReducer)) + "%"));
            return Mathf.RoundToInt(100 - (currentRoomNum * 100.0f) / (currentRoomNum + doorChanceReducer));
        }
    }




    //The position and facing of a door
    struct DoorInfo
    {
        //the position of the door on the tilemap
        public Vector3Int position;
        //the facing of the door to leave its room
        public Facing facing;

        public DoorInfo(Vector3Int pos, Facing face)
        {
            position = pos;
            facing = face;
        }

    }

    //the direction a door faces to lead out of a room or corridor
    //North is up, East is right, South is down, West is left
    enum Facing
    {
        North, East, South, West
    }
}
