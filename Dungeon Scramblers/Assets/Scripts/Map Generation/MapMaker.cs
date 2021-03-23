using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Photon.Pun;

//Script to generate a dungeon map
public class MapMaker : MonoBehaviour
{
    //generates a map on intilization of the object
    [SerializeField]
    bool generateMapOnStart = true;
    [SerializeField]
    bool generateAI = false;

    //whether or not the map has finished generating
    bool mapFinished = false;

    [SerializeField]
    int maxIterations = 200;
    //[SerializeField]
    //int newDoorChance = 50;

    [SerializeField]
    //minimum number of doors that can be generated leading from rooms
    int minimumDoors = 1;
    [SerializeField]
    //soft cap on the number of doors that can be generated leading from rooms
    int softMaximumDoors = 20;
    [SerializeField]
    //penalty applied to new door chance, 1 = .01% penalty
    int softMaximumPenalty = 10;
    [SerializeField]
    int currentDoorNum = 0;

    [SerializeField]
    float waitTime = 0f;

    //tilemap to generate dungeon on
    [SerializeField]
    public Tilemap tilemap;

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

    //Frequency of AI spawns (1 Cluster per X tiles in a room)
    [SerializeField]
    int spawnFrequency;

    //the minimum number of tiles the AI can spawn from the edge of a room
    [SerializeField]
    int spawnBuffer = 0;

    //the AI Clusters that can be spawned
    [SerializeField]
    List<AISpawnClusterInfo> spawnAI;

    //the total frequency of all AI Clusters, used to determine which CLuster is placed
    int totalClusterFrequency = 0;
    

    //list of all rooms created
    public List<RoomInfo> rooms;

    //public static int cornerCount = 0;
    //public static int totalCorridors = 0;

    private void Awake()
    {
        foreach (AISpawnClusterInfo cluster in spawnAI)
        {
            totalClusterFrequency += cluster.frequency;
        }
    }

    // Start is called before the first frame update
    void Start()
    {

        //tilemap.SetTile(new Vector3Int(0, 0, 0), doorTile);
        if (generateMapOnStart)
        {
            StartCoroutine("GenerateMap");
        }
        //GenerateMap();
    }

    #region generation functions

    //generates a dungeon map, assumes the tilemap is currently empty
    IEnumerator GenerateMap()
    {
        mapFinished = false;

        //set current number of doors to 1
        currentDoorNum = 1;

        List<DoorInfo> doorList = new List<DoorInfo>();
        rooms = new List<RoomInfo>();

        doorList.Add(new DoorInfo(new Vector3Int(0, 0, 0), Facing.North));

        //generate rooms and corridors
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

        //cleanup rooms
        //Add floors to all rooms
        foreach (RoomInfo room in rooms)
        {
            FillRoom(room, floorTile);
            yield return new WaitForSeconds(waitTime);
        }

        //generate the pathfinding grid
        Pathfinder.CreateGrid(tilemap.GetComponentInParent<Grid>(), tilemap, wallTile);

        //Add doors to all entrances to all rooms
        foreach (RoomInfo room in rooms)
        {
            AddDoors(room, doorTile);
            if (generateAI && spawnAI.Count > 0 && room.lowerLeft != rooms[0].lowerLeft)
            {
                SpawnAIClusters(room);
            }
            yield return new WaitForSeconds(waitTime);
        }
        
        mapFinished = true;
        yield return null;
    }

    //generates a room with an entering door at doorposition towards doorDirection
    //returns a list of all new doors created
    List<DoorInfo> GenerateRoom(DoorInfo door)
    {

        List<DoorInfo> newDoors = new List<DoorInfo>();

        //if the position of this door does not contain a door, do not generate a room, unless no rooms exist
        /*if (tilemap.GetTile(door.position) != doorTile && rooms.Count > 0)
        {
            currentDoorNum--;
            return newDoors;
        }*/

        int randX;
        int randY;

        //if this is the first room, make it maximum size, otherwise generate a randomly sized room
        if (rooms.Count > 0)
        {
            //generate a random x size of the floor space of the room
            randX = Random.Range(minRoomSize, maxRoomSize) + 1;// * (doorDirection == 3 ? -1 : 1);
            //generate a random x size of the floor space of the room
            randY = Random.Range(minRoomSize, maxRoomSize) + 1;// * (doorDirection == 2 ? -1 : 1);

            //Debug.Log("Room " + (rooms.Count + 1) + ": X: " + randX + " Y: " + randY);
        }
        else
        {
            randX = maxRoomSize;
            randY = maxRoomSize;
        }

        //set the starting positions to draw tiles from
        int startX = door.position.x; //+ (doorDirection == 1 ? 1 : doorDirection == 3 ? -1 : 0) + (int) Mathf.Ceil(randX / 2f);
        int startY = door.position.y; //+ (doorDirection == 0 ? 1 : doorDirection == 2 ? -1 : 0) + (int) Mathf.Ceil(randY / 2f);

        //based on door facing, modify starting positions 
        switch (door.facing)
        {
            case Facing.North:
                //ensure the first room is not random in its generation location
                if (rooms.Count == 0)
                {
                    startX -= (int)Mathf.Floor(randX / 2f);
                }
                else
                {
                    startX -= Random.Range(1, randX);
                }
                break;
            case Facing.East:
                //startY -= (int)Mathf.Floor(randY / 2f);
                startY -= Random.Range(1, randY);
                break;
            case Facing.South:
                startY -= randY;
                //startX -= (int)Mathf.Floor(randX / 2f);
                startX -= Random.Range(1, randX);
                break;
            case Facing.West:
                startX -= randX;
                //startY -= (int)Mathf.Floor(randY / 2f);
                startY -= Random.Range(1, randY);
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

        //check to make sure it doesn't overlap with the first room
        if (rooms.Count > 0)
        {
            //this statement detects that an overlap exists
            if (startX <= rooms[0].upperRight.x && startY <= rooms[0].upperRight.y && endX >= rooms[0].lowerLeft.x && endY >= rooms[0].lowerLeft.y)
            {
                //5 of the 8 overlap cases require this to be true (overlapping the middle from the top or bottom, and overlapping on the right side
                if (startX >= rooms[0].lowerLeft.x)
                {
                    //3 of the 5 cases require this to be true(overlapping top-middle, top-right, middle-right)
                    if (startY >= rooms[0].lowerLeft.y)
                    {
                        switch (door.facing)
                        {
                            //In all cases, if the room is generated facing north shift the west wall right
                            //This facing should be impossible for the top-middle overlap (which itself should be extremely rare)
                            case Facing.North:
                                startX = rooms[0].upperRight.x + 1;
                                break;
                            //If the room is generated facing south and it overlaps the middle-right of the starting room
                            //shift the west wall right, otherwise shift the south wall up
                            case Facing.South:
                                if (endY <= rooms[0].upperRight.y + minRoomSize)
                                {
                                    startX = rooms[0].upperRight.x + 1;
                                }
                                else
                                {
                                    startY = rooms[0].upperRight.y + 1;
                                }
                                break;
                            //In all cases, if the room is gneerated facing east shift the south wall up
                            //This facing should be impossible for middle-right overlap
                            case Facing.East:
                                startY = rooms[0].upperRight.y + 1;
                                break;
                            //If the room is generated facing west and it overlaps the top-middle of the starting room
                            //shift the south wall up, otherwise shift the west wall right
                            case Facing.West:
                                if (endX <= rooms[0].upperRight.x + minRoomSize)
                                {
                                    startY = rooms[0].upperRight.y + 1;
                                }
                                else
                                {
                                    startX = rooms[0].upperRight.x + 1;
                                }
                                break;
                        }
                    }
                    //the other 2 cases, bottom-middle and bottom-right
                    else
                    {
                        switch (door.facing)
                        {
                            //In all cases, if the room is generated facing north shift the north wall down
                            case Facing.North:
                                endY = rooms[0].lowerLeft.y - 1;
                                break;
                            //In all cases, if the room is generated south shift the west wall right
                            //This facing should be impossible for bottom-middle overlap
                            case Facing.South:
                                startX = rooms[0].upperRight.x + 1;
                                break;
                            //In all cases, if the room is gneerated facing east, shift the north wall down
                            case Facing.East:
                                endY = rooms[0].lowerLeft.y - 1;
                                break;
                            //If the room is generated facing west and it overlaps the lower right corner of the starting room
                            //shift the west wall right, otherwise shift the north wall down
                            case Facing.West:
                                if (endX >= rooms[0].upperRight.x + minRoomSize)
                                {
                                    startX = rooms[0].upperRight.x + 1;
                                }
                                else
                                {
                                    endY = rooms[0].lowerLeft.y - 1;
                                }
                                break;
                        }
                    }
                }
                //The other 3 overlap cases, where the starting room is overlapped on the left side
                else
                {
                    switch (door.facing)
                    {
                        //If the room is generated facing north and it overlaps the lower left corner of the starting room
                        //shift the north wall down, otherwise shift the east wall left
                        case Facing.North:
                            if (startY <= rooms[0].lowerLeft.y + minRoomSize)
                            {
                                endY = rooms[0].lowerLeft.y - 1;
                            }
                            else
                            {
                                endX = rooms[0].lowerLeft.x - 1;
                            }
                            break;
                        //If the room is generated facing south and it overlaps the upper left corner of the starting room
                        //shift the south wall up, otherwise shift the east wall left
                        case Facing.South:
                            if (endY >= rooms[0].upperRight.y + minRoomSize)
                            {
                                startY = rooms[0].upperRight.y + 1;
                            }
                            else
                            {
                                endX = rooms[0].lowerLeft.x - 1;
                            }
                            break;
                        //In all cases, if the room is generate facing east, shift the east wall left
                        case Facing.East:
                            endX = rooms[0].lowerLeft.x - 1;
                            break;
                        //if the room is generated facing west and it overlaps the lower left corner of the starting room
                        //shift the north wall down, otherwise shift the south wall up
                        //This facing should be impossible for the middle-left overlap case
                        case Facing.West:
                            if (startY <= rooms[0].lowerLeft.y + minRoomSize)
                            {
                                endY = rooms[0].lowerLeft.y - 1;
                            }
                            else
                            {
                                startY = rooms[0].upperRight.y + 1;
                            }
                            break;
                    }
                }

                //Debug.Log("Overlap " + startX + " " + startY + " " + endX + " " + endY);
            }





            //if (((startY >= rooms[0].lowerLeft.y && startY <= rooms[0].upperRight.y) || (endY >= rooms[0].lowerLeft.y && endY <= rooms[0].upperRight.y)) &&
            //    ((startX >= rooms[0].lowerLeft.x && startX <= rooms[0].upperRight.x) || (endX >= rooms[0].lowerLeft.x && endX <= rooms[0].upperRight.x)))
            //{
            //    switch (door.facing)
            //    {
            //        case Facing.North:
            //            endY = rooms[0].lowerLeft.y - 1;
            //            break;
            //        case Facing.South:
            //            startY = rooms[0].upperRight.y + 1;
            //            break;
            //        case Facing.East:
            //            endX = rooms[0].lowerLeft.x - 1;
            //            break;
            //        case Facing.West:
            //            startX = rooms[0].upperRight.x + 1;
            //            break;
            //    }
            //}
        }

       //Debug.Log("Start X: " + startX + " Start Y: " + startY + " End X: " + endX + " End Y: " + endY);

        //place all the walls that make up the room
        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                if (x == startX || x == endX || y == startY || y == endY)
                {
                    PlaceTile(new Vector3Int(x, y, 0), wallTile);
                }

                //placing floors during initial room generation is deprecated
                /*//if a non-floor tile is found, check if it has empty spaces around it that are beyond the borders of this room
                //if it does not, then replace the wall with floor
                else if (tilemap.HasTile(new Vector3Int(x,y,0)) && tilemap.GetTile(new Vector3Int(x, y, 0)) != floorTile)
                {
                    for (int x2 = 0; x2 < 2; x2++)
                    {
                        for (int y2 = 0; y2 < 2; y2++)
                        {
                            if (x2 != y2 && x2 * -1 != y2 && x + x2 >= startX && x + x2 <= endX && y + y2 >= startY && y + y2 <= endY)
                            {
                                //tilemap.SetTile(new Vector3Int(x, y, 0), floorTile);
                            }
                        }
                    }
                }
                else
                {
                    //PlaceTile(new Vector3Int(x, y, 0), floorTile);
                }*/

            }
        }


        //fill the area of the room with wall tiles
        //tilemap.BoxFill(new Vector3Int(startX, startY, 0), wallTile, startX - 1, startY - 1, startX + randX, startY + randY);
        //fill the inside of the room with floor tiles
        //tilemap.BoxFill(new Vector3Int(startX, startY, 0), floorTile,startX,startY,startX+randX - 1,startY+randY - 1);


        Vector3Int randDoorLocation = new Vector3Int(0, 0, 0);

        //for each wall of the room except the entering wall, check for creation of new doors and add any new doors to the list of new doors
        for (int i = 0; i < 4; i++)
        {
            //ensure that the first room generates its door the same direction it as generated from(i.e. starting room is generated Northward, so its door is northward)
            if (rooms.Count == 0)
            {
                i = (int)door.facing;
            }

            //if the wall being checked is not the wall being entered from
            if (Mathf.Abs(i - (int)door.facing) != 2)
            {
                //check if new door should be generated
                if (Random.Range(1, 10000) <= GetDoorChance())
                {
                    
                    //determine location of the new door
                    switch ((Facing)i)
                    {
                        case Facing.North:
                            randDoorLocation = new Vector3Int(startX + Random.Range(1, randX - 1), endY, 0);
                            break;
                        case Facing.East:
                            randDoorLocation = new Vector3Int(endX, startY + Random.Range(1, randY - 1), 0);
                            break;
                        case Facing.South:
                            randDoorLocation = new Vector3Int(startX + Random.Range(1, randX - 1), startY, 0);

                            break;
                        case Facing.West:
                            randDoorLocation = new Vector3Int(startX, startY + Random.Range(1, randY - 1), 0);
                            break;
                    }

                    //if this is not the first room and the door would be on one of the walls of the starting room, do not place the door
                    if (rooms.Count > 0 &&
                        randDoorLocation.x >= rooms[0].lowerLeft.x - 1 && randDoorLocation.x <= rooms[0].upperRight.x + 1 &&
                        randDoorLocation.y >= rooms[0].lowerLeft.y - 1 && randDoorLocation.y <= rooms[0].upperRight.y + 1)
                    {
                        continue;
                    }

                    //place the new door as long as the location is inside a wall
                    //add it to the list of new doors
                    if (tilemap.GetTile(randDoorLocation) == wallTile)
                    {
                        tilemap.SetTile(randDoorLocation, floorTile);
                        newDoors.Add(new DoorInfo(randDoorLocation, (Facing)i));
                        currentDoorNum++;
                    }
                    //the first room has only one door, so break
                    if (rooms.Count == 0)
                    {
                        break;
                    }
                }
            }
        }

        //add this room's information to the list of rooms
        RoomInfo thisRoom;
        thisRoom.lowerLeft = new Vector3Int(startX + 1, startY + 1, 0);
        thisRoom.upperRight = new Vector3Int(endX - 1, endY - 1, 0);
        rooms.Add(thisRoom);

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

        //calculate end door position and corridor direction vector
        Vector3Int endDoorPosition = door.position + new Vector3Int((length + 1) * xAdjust, (length + 1) * yAdjust, 0);
        Vector3Int corridorDirection = new Vector3Int(xAdjust, yAdjust, 0);

        //vector to check whether this corridor has space to generate a room
        Vector3Int maxOverlapPosition = endDoorPosition + corridorDirection * (minRoomSize+ 1);

        if (maxOverlapPosition.x <= rooms[0].upperRight.x + minRoomSize && maxOverlapPosition.x >= rooms[0].lowerLeft.x - minRoomSize &&
            maxOverlapPosition.y <= rooms[0].upperRight.y + minRoomSize && maxOverlapPosition.y >= rooms[0].lowerLeft.y - minRoomSize)
        {
            tilemap.SetTile(door.position, wallTile);
            currentDoorNum--;
            //Debug.Log("OVERLAP");
            return newDoors;
        }

        //check if this corridor should connect to a room, due to overlap or starting just short
        for (int i = 1; i <= length + 1 + minRoomSize / 2; i++)
        {
            
            if (tilemap.HasTile(door.position + i * corridorDirection)/* && tilemap.HasTile(door.position + (i + 1) * corridorDirection)*/)
            {
                Debug.Log("Connector Case 1");
                endDoorPosition = door.position + i * corridorDirection;
                length = i - 1;
                isConnector = true;
                break;
            }
        }

        //checks if this corridor would connect to the corner of another room
        if (isConnector && tilemap.GetTile(endDoorPosition + corridorDirection) == wallTile && length + 1 >= minCorridorSize)
        {
            //cornerCount++;

            //checks if this would result in two adjacent door tiles
            if (tilemap.GetTile(endDoorPosition + corridorDirection * 2) != doorTile)
            {
                //Debug.Log("Corner case");
                tilemap.SetTile(endDoorPosition, floorTile);
                //tilemap.SetTile(endDoorPosition + corridorDirection, floorTile);


                endDoorPosition += corridorDirection;
                length += 2;
            }
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

        //if there is floor immediately after the initial door, do not generate the corridor
        if (tilemap.GetTile(door.position + corridorDirection) == floorTile)
        {
            return newDoors;
        }
        //if the corridor is too small, do not generate it
        else if (length < minCorridorSize)
        {
            tilemap.SetTile(door.position, wallTile);
            return newDoors;
        }

        //tilemap.SetTile(endDoorPosition, doorTile);
        tilemap.SetTile(endDoorPosition, floorTile);

        for (int i = 0; i <= length + 1; i++)
        {
            Vector3Int tileLoc = door.position + i * corridorDirection;

            //tilemap.SetTile(tileLoc, floorTile);
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

        //totalCorridors++;
        return newDoors;
    }

    void SpawnAIClusters(RoomInfo room)
    {
        //Debug.Log("Starting AI Spawn");

        //the size of the room in x and y directions
        Vector2Int roomSize = new Vector2Int(room.upperRight.x - room.lowerLeft.x + 1, room.upperRight.y - room.lowerLeft.y + 1);
        //total size of the room
        int totalTiles = roomSize.x * roomSize.y;

        //the total number of AI Clusters being spawned
        int numSpawns = totalTiles / spawnFrequency;
        //any fraction in the number of clusters to spawn is used as a percent chance to spawn
        if (Random.Range(1, 1000) <= ((totalTiles / (float) spawnFrequency) % 1) * 1000)
        {
            numSpawns++;
        }

        if (numSpawns == 0)
        {
            return;
        }

        //the room is divided into equal size rectangles, one for each cluster being spawned
        //this vector is the value added to the starting corners of the spawn area
        Vector2 spawnAreaSize = new Vector2(roomSize.x > roomSize.y? ((roomSize.x - spawnBuffer * 2) / (float)numSpawns) : 0, roomSize.x <= roomSize.y ? ((roomSize.y - spawnBuffer * 2) / (float)numSpawns) : 0);
        //Debug.Log(spawnAreaSize);
        //*DEPRECATED* divide up the room based on its longest edge
        /*if (roomSize.x > roomSize.y)
        {
            spawnAreaSize.x /= numSpawns;
        }
        else
        {
            spawnAreaSize.y /= numSpawns;
        }*/

        //if the spawn area is too small, report the error and associated information
        if (spawnAreaSize.x <= 0 && spawnAreaSize.y <= 0)
        {
            Debug.Log("Spawn Area too small to spawn AI, aborting AI spawning for this room");
            Debug.Log("Room Size: " + roomSize.x + ", " + roomSize.y + ", TOTAL: " + totalTiles + "\n" +
            "Num Spawns: " + (totalTiles / (float)spawnFrequency) + ", Actual Spawns: " + numSpawns + "\n" +
            "Spawn Area Estimated Size: " + (roomSize.x / (float)numSpawns) + ", " + (roomSize.y / (float)numSpawns));
        }

        
        //starts at the lower left corner of the room
        Vector2 currentLowerLeft = new Vector2(room.lowerLeft.x + spawnBuffer, room.lowerLeft.y + spawnBuffer);
        //starts as the upper right of the spawn area, if the x area is greater than y, then the x value is based on SpawnAreaSize and the y is just the height of the room and vice versa if x <= y
        Vector2 currentUpperRight = new Vector2(roomSize.x > roomSize.y ? (currentLowerLeft.x + spawnAreaSize.x - 1) : (room.upperRight.x - spawnBuffer), 
            roomSize.x <= roomSize.y ? (currentLowerLeft.y + spawnAreaSize.y - 1) : (room.upperRight.y - spawnBuffer));
        int spawnLimit = (roomSize.x - spawnBuffer * 2) * (roomSize.y - spawnBuffer * 2) / numSpawns; 


        //for each cluster that must be spawned
        for (int i = 0; i < numSpawns; i++)
        {
            if (currentUpperRight.x >= room.upperRight.x - spawnBuffer)
            {
                currentUpperRight.x = room.upperRight.x - spawnBuffer;
                if (currentLowerLeft.x >= currentUpperRight.x)
                {
                    Debug.Log("Not enough space to spawn");
                    break;
                }
            }
            else if (currentUpperRight.y >= room.upperRight.y - spawnBuffer)
            {
                currentUpperRight.y = room.upperRight.y - spawnBuffer;
                if (currentLowerLeft.y >= currentUpperRight.y)
                {
                    Debug.Log("Not enough space to spawn");
                    break;
                }
            }

            

            //Debug.Log("Starting Cluster Spawn");
            //randomly select a cluster to spawn
            int clusterSelect = Random.Range(1, totalClusterFrequency + 1);
            //Debug.Log("Cluster: " + clusterSelect);
            //if no cluster is selected due to error, select the first cluster
            AISpawnClusterInfo selectedCluster = spawnAI[0];
            for (int x = 0; x < spawnAI.Count; x++)
            {
                clusterSelect -= spawnAI[x].frequency;
                if (clusterSelect <= 0)
                {
                    selectedCluster = spawnAI[x];
                    break;
                }
            }

            //list of locations that an ai has already been sapwned in
            List<Vector3> closedLocations = new List<Vector3>();
            int numSpawned = 0;

            //if (room.lowerLeft == rooms[12].lowerLeft)
            //{
                //tilemap.SetTile(new Vector3Int(Mathf.FloorToInt(currentLowerLeft.x), Mathf.FloorToInt(currentLowerLeft.y), 0), doorTile);
                //tilemap.SetTile(new Vector3Int(Mathf.FloorToInt(currentUpperRight.x), Mathf.FloorToInt(currentUpperRight.y), 0), doorTile);
                //Debug.Log(currentLowerLeft + " " + currentUpperRight);
            //}

            //for each ai in the cluster, generate how many should be placed
            foreach (AISpawnInfo ai in selectedCluster.spawns)
            {
                //Debug.Log("Starting Individual Spawn");
                int numToSpawn = Random.Range(ai.minSpawned, ai.maxSpawned);
                

                for (int x = 0; x < numToSpawn; x++)
                {
                    if (numSpawned >= spawnLimit)
                    {
                        break;
                    }

                    //generate random spawn location
                    int randX = Random.Range(Mathf.FloorToInt(currentLowerLeft.x), Mathf.FloorToInt(currentUpperRight.x) + 1);
                    int randY = Random.Range(Mathf.FloorToInt(currentLowerLeft.y), Mathf.FloorToInt(currentUpperRight.y) + 1);
                    Vector3Int randLocation = new Vector3Int(randX, randY, 0);

                    int count = 0;

                    //if the spawn location has been used, generate a new one
                    while (closedLocations.Contains(randLocation))
                    {
                        //Debug.Log("relocate");
                        randX = Random.Range(Mathf.FloorToInt(currentLowerLeft.x), Mathf.FloorToInt(currentUpperRight.x) + 1);
                        randY = Random.Range(Mathf.FloorToInt(currentLowerLeft.y), Mathf.FloorToInt(currentUpperRight.y) + 1);
                        randLocation = new Vector3Int(randX, randY, 0);
                        count++;
                        if (count > 50)
                        {
                            Debug.Log("50 Attempts");
                            break;
                        }
                    }

                    //convert random location to world location
                    Vector3 worldLocation = tilemap.GetCellCenterWorld(randLocation);

                    //if (room.lowerLeft == rooms[12].lowerLeft)
                    //{
                    //    Debug.Log(randLocation + " " + worldLocation);
                    //}

                    //instantiate AI
                    if (PhotonNetwork.CurrentRoom != null)
                    {
                        PhotonNetwork.Instantiate(ai.spawnAI.name, worldLocation, new Quaternion());
                    }
                    else
                    {
                        Instantiate(ai.spawnAI, worldLocation, new Quaternion());
                    }
                    closedLocations.Add(randLocation);
                }
                if (numSpawned >= spawnLimit)
                {
                    break;
                }
            }

            currentLowerLeft += spawnAreaSize;
            currentUpperRight += spawnAreaSize;
            
        }

        //Debug.Log("[" + room.lowerLeft.x  + ", " + room.lowerLeft.y + "], [" + room.upperRight.x + ", " + room.upperRight.y + "]");
        //Debug.Log("Room Size: " + roomSize.x + ", " + roomSize.y + ", TOTAL: " + totalTiles + "\n" +
        //    "Num Spawns: " + (totalTiles / (float) spawnFrequency) + ", Actual Spawns: " + numSpawns + "\n" +
        //    "Spawn Area Size: " + spawnAreaSize.x + ", " + spawnAreaSize.y);//*/

    }

    #endregion

    #region helper functions
    void PlaceTile(Vector3Int position, TileBase tile)
    {
        if (!tilemap.HasTile(position))
        {
            tilemap.SetTile(position, tile);
        }
    }

    //fills the given room with the given tiles
    void FillRoom(RoomInfo room, TileBase tile)
    {
        for (int x = room.lowerLeft.x; x <= room.upperRight.x; x++)
        {
            for (int y = room.lowerLeft.y; y <= room.upperRight.y; y++)
            {
                //ensure that the starting room is never combined with other rooms and fill each room with tiles
                //if (x != rooms[0].lowerLeft.x - 1 || x != rooms[0].upperRight.x + 1 || y != rooms[0].lowerLeft.y - 1 || y != rooms[0].upperRight.y + 1)
                if (x < rooms[0].lowerLeft.x - 1 || x > rooms[0].upperRight.x + 1 || y < rooms[0].lowerLeft.y - 1 || y > rooms[0].upperRight.y + 1 || room.lowerLeft == rooms[0].lowerLeft)
                {
                    //fill the room with floor tiles
                    tilemap.SetTile(new Vector3Int(x, y, 0), tile);
                }
            }
        }
    }

    //adds doors to a given room using the given tile
    void AddDoors(RoomInfo room, TileBase tile)
    {
        for (int x = room.lowerLeft.x - 1; x <= room.upperRight.x + 1; x++)
        {
            for (int y = room.lowerLeft.y - 1; y <= room.upperRight.y + 1; y += room.upperRight.y + 1 - (room.lowerLeft.y - 1))
            {
                if (tilemap.GetTile(new Vector3Int(x, y, 0)) == floorTile)
                {
                    PlaceDoor(x, y, tile);
                }
            }
        }

        for (int y = room.lowerLeft.y - 1; y <= room.upperRight.y + 1; y++)
        {
            for (int x = room.lowerLeft.x - 1; x <= room.upperRight.x + 1; x += room.upperRight.x + 1 - (room.lowerLeft.x - 1))
            {
                if (tilemap.GetTile(new Vector3Int(x, y, 0)) == floorTile)
                {
                    PlaceDoor(x, y, tile);
                }
            }
        }
    }

    //gets the chance of a new door being created based on the number of doors that exist
    int GetDoorChance()
    {

        //Debug.Log("Num:" + currentDoorNum + ", C:" + ((100.0 / Mathf.Log(currentDoorNum + 1, minimumDoors)) - (currentDoorNum >= softMaximumDoors ? softMaximumPenalty : 0) / 100) + "%");
        return (int) (100.0 / Mathf.Log(currentDoorNum + 1, minimumDoors) * 100) - (currentDoorNum >= softMaximumDoors ? softMaximumPenalty : 0);
    }

    // 100 / ((1-minDoors) * x)
    // 100 / (logBase MinDoors X)

    //checks cardinally adjacent tiles, returns the number that match the given tile
    int CheckAdjacentTiles(int startX, int startY, TileBase tile)
    {
        int adjacent = 0;
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x != y && x != y * -1 && tilemap.GetTile(new Vector3Int(startX + x, startY + y, 0)) == tile)
                {
                    adjacent++;
                }
            }
        }

        return adjacent;
    }

    //checks if a given tile is a doorway, meaning that one wall tile exists on either side of it
    //if the door would lead to nowhere place a wall, otherwise place a door
    void PlaceDoor(int startX, int startY, TileBase door)
    {


        //if both tiles adjacent on the X axis are walls or both tiles adjacent on the Y axis are walls
        if (tilemap.GetTile(new Vector3Int(startX - 1, startY, 0)) == wallTile && tilemap.GetTile(new Vector3Int(startX + 1, startY, 0)) == wallTile
            || tilemap.GetTile(new Vector3Int(startX, startY - 1, 0)) == wallTile && tilemap.GetTile(new Vector3Int(startX, startY + 1, 0)) == wallTile)
        {
            //check to ensure all cardinally adjacent tiles are filled, if yes, return true, otherwise return false and replace this doorway with a wall
            if (tilemap.HasTile(new Vector3Int(startX - 1, startY, 0)) && tilemap.HasTile(new Vector3Int(startX + 1, startY, 0))
                && tilemap.HasTile(new Vector3Int(startX, startY - 1, 0)) && tilemap.HasTile(new Vector3Int(startX, startY + 1, 0)))
            {
                tilemap.SetTile(new Vector3Int(startX, startY, 0), door);
            }
            else
            {
                tilemap.SetTile(new Vector3Int(startX, startY, 0), wallTile);
            }
        }
    }
    #endregion

    //returns whether the map has finished generating
    public bool IsMapFinished()
    {
        return mapFinished;
    }

    //clears the map and pathfinder
    public void ClearMap()
    {
        tilemap.ClearAllTiles();
        tilemap.CompressBounds();
        Pathfinder.ClearGrid();
        foreach (GameObject ai in GameObject.FindGameObjectsWithTag("AI"))
        {
           Destroy(ai);
        }
    }

    #region structs and enums
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

    
    //the location of a rooms floor corners
    public struct RoomInfo
    {
        //the position of the lower left floor corner on the tilemap
        public Vector3Int lowerLeft;
        //the position of the upper right floor corner on the tilemap
        public Vector3Int upperRight;
    }

    //struct containing the information used to spawn groups of AI
    [System.Serializable]
    struct AISpawnClusterInfo
    {
        //list of all AI that will be spawned and their number
        public List<AISpawnInfo> spawns;
        //the relative frequency of this AI Cluster (i.e. frequency = 2 occurs twice as often as frequency = 1)
        public int frequency;
    }

    //struct containing information used to spawn an individual AI
    [System.Serializable]
    struct AISpawnInfo
    {
        //the AI gameobject to instantiate
        public GameObject spawnAI;
        //the minimum number to spawn
        public int minSpawned;
        //the maximum number to spawn
        public int maxSpawned;
    }


    //the direction a door faces to lead out of a room or corridor
    //North is up, East is right, South is down, West is left
    enum Facing
    {
        North, East, South, West
    }
    #endregion
}
