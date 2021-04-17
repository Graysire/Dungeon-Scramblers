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

    //tilemaps to generate dungeon on
    //tilemap 0 should be floor
    //tilemap 1 should be walls
    //tilemap 2 should be decoration
    [SerializeField]
    public Tilemap[] tilemaps;

    //default tile used for the floor
    [SerializeField]
    TileBase floorTile;
    //default tile used for walls, assumed ot be aruletile that includes a shaded wall
    [SerializeField]
    TileBase wallTile;
    //default tile used for doors
    [SerializeField]
    TileBase doorTile;
    //default tile used for decorations scattered through the rooms
    [SerializeField]
    TileBase decorationTile;
    //expected number of tiles per floor decoration, lower to increase number of decorations
    [Min(1)]
    [SerializeField]
    int decorationFrequency;
    //tiles used for shadowing the walls
    //NOTE: Light source is assumed to be Northeast
    //code is based on those assumptions
    [SerializeField]
    ShadowInfo wallShadowTiles;
    
    

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

    public int numAttemptsTotal = 0;

    [SerializeField]
    GameObject nextLevelTeleport;
    [SerializeField]
    //the number of attempts to place the exitDoor that should be made before expanding the list of possible rooms
    int exitDoorAttemptThreshold;
    [SerializeField]
    //the minimum shortest path between the starting door and exit door
    int exitDoorDistanceThreshold;

    //the AI Clusters that can be spawned
    [SerializeField]
    List<AISpawnClusterInfo> spawnAI;

    //the total frequency of all AI Clusters, used to determine which Cluster is placed
    int totalClusterFrequency = 0;
    

    //list of all rooms created
    public List<RoomInfo> rooms;
    //list of all corridors created
    public List<CorridorInfo> corridors;

    //the number of perks to spawn
    [SerializeField]
    int numPerks = 3;
    //object used for perk voting
    [SerializeField]
    GameObject perkObject;

    //whether or not this is the first map to be generated and thus whether perks must be spawned in the first room
    bool firstMap;
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
            StartCoroutine(GenerateMap(true));
        }
        //GenerateMap();
    }

    #region generation functions

    //generates a dungeon map, assumes the tilemap is currently empty
    public IEnumerator GenerateMap(bool isFirstMap = true)
    {
        firstMap = isFirstMap;
        mapFinished = false;
        //log an error if the number of tilemaps is unexpected
        if (tilemaps.Length != 4)
        {
            Debug.LogError("Expected 4 Tilemaps, actual count: " + tilemaps.Length);
        }

        //set current number of doors to 1
        currentDoorNum = 1;
        //the number of rooms in the final iteration that runs
        int finalRooms = 0;

        //initialize lists of doors, rooms, and corridors
        List<DoorInfo> doorList = new List<DoorInfo>();
        rooms = new List<RoomInfo>();
        corridors = new List<CorridorInfo>();

        //add initial door
        doorList.Add(new DoorInfo(new Vector3Int(0, 0, 0), Facing.North));
        //declare variable to stor the location of the first generated Door
        DoorInfo startDoor = new DoorInfo();

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
            //generate new corridors from the doors in newDoors and and add their doors to doorList if doors exist
            if (newDoors.Count > 0)
            {
                //if only oneroom exists, recordthe starting door
                if (rooms.Count == 1)
                {
                    startDoor = newDoors[0];
                }
                if (i + 1 < maxIterations)
                {
                    finalRooms = newDoors.Count;
                    foreach (DoorInfo door in newDoors)
                    {
                        doorList.AddRange(GenerateCorridor(door));
                        yield return new WaitForSeconds(waitTime);
                    }
                }
                //if the iterations are about to end, remove all extraneous doors
                else
                {
                    Debug.Log("MaxIter");
                    foreach (DoorInfo door in newDoors)
                    {
                        tilemaps[1].SetTile(door.position, wallTile);
                    }
                }
            }
            //if no new doors exist then break
            else
            {
                //Debug.Log(finalRooms);
                break;
            }
        }

        //cleanup rooms
        //Add floors to all rooms
        foreach (RoomInfo room in rooms)
        {
            FillRoom(room, floorTile);
            yield return new WaitForSeconds(waitTime);
        }

        int attempts = 0;

        //generate the pathfinding grid
        Pathfinder.CreateGrid(tilemaps[0].GetComponentInParent<Grid>(), tilemaps[0], tilemaps[1]);


        //place the exit door in one of the final rooms
        while (true)
        {

            //if the door cannot be placed break
            if (attempts == 100)
            {
                Debug.Log("NO LOCATION" + finalRooms);
                break;
            }
            //log an attempt having been made
            attempts++;
            //if we've done many attempts and failed to create an exit, increase the pool of rooms available
            if (attempts % exitDoorAttemptThreshold == 0)
            {
                finalRooms++;
            }


            //pick exit room and wall for the exitDoor
            RoomInfo exitRoom = rooms[Random.Range(rooms.Count - finalRooms, rooms.Count)];
            Facing facing = (Facing)Random.Range(1, 4);

            int x = 0;
            int y = 0;
            //generate location based on Facing
            switch (facing)
            {
                case Facing.North:
                    y = exitRoom.upperRight.y + 1;
                    x = Random.Range(exitRoom.lowerLeft.x, exitRoom.upperRight.x + 1);
                    break;
                case Facing.South:
                    y = exitRoom.lowerLeft.y - 1;
                    x = Random.Range(exitRoom.lowerLeft.x, exitRoom.upperRight.x + 1);
                    break;
                case Facing.East:
                    x = exitRoom.upperRight.x + 1;
                    y = Random.Range(exitRoom.lowerLeft.y, exitRoom.upperRight.y);
                    break;
                case Facing.West:
                    x = exitRoom.lowerLeft.x - 1;
                    y = Random.Range(exitRoom.lowerLeft.y, exitRoom.upperRight.y);
                    break;
            }

            //check if the exit is too close to the entrance, if so, go to the next iteration
            if (Pathfinder.GetPath(tilemaps[1].CellToWorld(new Vector3Int(x, y, 0)), tilemaps[1].CellToWorld(startDoor.position)).Count < exitDoorDistanceThreshold)
            {
                continue;
            }


            //if this is a valid location to place an exit door (it is north/south, or it is east/west without a corridor above it
            if (tilemaps[1].HasTile(new Vector3Int(x, y, 0)) && (facing == Facing.North || facing == Facing.South || 
                (facing == Facing.East || facing == Facing.West) && !tilemaps[0].HasTile(new Vector3Int(x, y + 2, 0)) && !tilemaps[0].HasTile(new Vector3Int(x, y + 1, 0)) && !tilemaps[0].HasTile(new Vector3Int(x, y - 2, 0))))
            {
                //place the floor for the exit to be placed on
                tilemaps[0].SetTile(new Vector3Int(x, y, 0), floorTile);

                //Spawn level exit object
                if (nextLevelTeleport != null)
                {
                    Instantiate(nextLevelTeleport, tilemaps[0].GetCellCenterWorld(new Vector3Int(x, y, 0)), new Quaternion());
                }

                //place additional tiles based on the facing
                switch (facing)
                {
                    case Facing.North:
                        //place extra wall tiles
                        for (int xa = -1; xa <= 1; xa++)
                        {
                            tilemaps[1].SetTile(new Vector3Int(xa + x, y + 1, 0), wallTile);
                        }
                        //Place corresponding shadow tiles
                        tilemaps[2].SetTile(new Vector3Int(x,y - 1,0), wallShadowTiles.concaveCorner);
                        tilemaps[2].SetTile(new Vector3Int(x - 2, y + 1, 0), ChooseWallShadowTile(new Vector3Int(x - 2, y + 1, 0)));
                        tilemaps[2].SetTile(new Vector3Int(x - 2, y, 0), ChooseWallShadowTile(new Vector3Int(x - 2, y, 0)));

                        
                        break;
                    case Facing.South:
                        //remove blocking wall
                        tilemaps[1].SetTile(new Vector3Int(x, y, 0), null);
                        //add wall and corresponding floor tiles
                        tilemaps[1].SetTile(new Vector3Int(x, y - 1, 0), wallTile);
                        tilemaps[0].SetTile(new Vector3Int(x, y - 1, 0), floorTile);

                        //add shadowing
                        tilemaps[2].SetTile(new Vector3Int(x, y, 0), wallShadowTiles.topRight);
                        tilemaps[2].SetTile(new Vector3Int(x, y - 1, 0), wallShadowTiles.midRight);

                        //add wall shading
                        for (int xa = -1; xa <= 1; xa++)
                        {
                            tilemaps[1].SetTile(new Vector3Int(xa + x, y - 2, 0), wallTile);
                            
                        }
                        //add wall shadows
                        for (int xa = -2; xa <= 1; xa++)
                        {
                            tilemaps[2].SetTile(new Vector3Int(xa + x, y - 3, 0), ChooseWallShadowTile(new Vector3Int(xa + x, y - 3, 0)));

                        }

                        break;
                    case Facing.East:
                        //remove blocking wall
                        tilemaps[1].SetTile(new Vector3Int(x, y, 0), null);
                        //place floor tile
                        tilemaps[0].SetTile(new Vector3Int(x, y - 1, 0), floorTile);

                        

                        //ensure no wall errors occur
                        tilemaps[1].SetTile(new Vector3Int(x, y - 2, 0), wallTile);

                        //add wall shading
                        for (int ya = -2; ya <= 2; ya++)
                        {
                            //tilemaps[1].SetTile(new Vector3Int(x + 1, y + ya, 0), wallTile);
                            PlaceTile(new Vector3Int(x + 1, y + ya, 0), tilemaps[1], wallTile);
                        }

                        //stopgap measure to prevent area being erased by PlaceDoors until it is refactored
                        tilemaps[0].SetTile(new Vector3Int(x + 1, y, 0), floorTile);

                        //add shadowing
                        tilemaps[2].SetTile(new Vector3Int(x, y, 0), wallShadowTiles.concaveCorner);
                        tilemaps[2].SetTile(new Vector3Int(x, y - 1, 0), wallShadowTiles.midRight);
                        tilemaps[2].SetTile(new Vector3Int(x + 1, y - 3, 0), wallShadowTiles.rightTop);

                        break;
                    case Facing.West:
                        //remove blocking wall
                        tilemaps[1].SetTile(new Vector3Int(x, y, 0), null);
                        //place floor tile
                        tilemaps[0].SetTile(new Vector3Int(x, y - 1, 0), floorTile);

                        //ensure no wall errors occur
                        tilemaps[1].SetTile(new Vector3Int(x, y - 2, 0), wallTile);

                        //add wall shading
                        for (int ya = -2; ya <= 2; ya++)
                        {
                            //tilemaps[1].SetTile(new Vector3Int(x - 1, y + ya, 0), wallTile);
                            PlaceTile(new Vector3Int(x - 1, y + ya, 0), tilemaps[1], wallTile);
                        }

                        //stopgap measure to prevent area being erased by PlaceDoors until it is refactored
                        tilemaps[0].SetTile(new Vector3Int(x - 1, y, 0), floorTile);

                        //add wall shadowing
                        for (int ya = -3; ya <= 2; ya++)
                        {
                            tilemaps[2].SetTile(new Vector3Int(x - 2, y + ya, 0), ChooseWallShadowTile(new Vector3Int(x - 2, y + ya, 0)));

                        }

                        //add shadowing
                        tilemaps[2].SetTile(new Vector3Int(x, y, 0), wallShadowTiles.rightTop);
                        tilemaps[2].SetTile(new Vector3Int(x - 1, y - 3, 0), wallShadowTiles.concaveCorner);

                        break;
                }

                break;
            }
        }
        Debug.Log("Attempts: " + attempts);
        numAttemptsTotal += attempts;


        //Add doors, spawn AI, room shading
        foreach (RoomInfo room in rooms)
        {
            AddDoors(room, doorTile);
            AddWallShading(room);
            
            if (generateAI && spawnAI.Count > 0 && room.lowerLeft != rooms[0].lowerLeft)
            {
                SpawnAIClusters(room);
            }
            yield return new WaitForSeconds(waitTime);
        }
        //Add shading, shadows and decoration to corridors
        foreach (CorridorInfo corridor in corridors)
        {
            AddWallShading(corridor);
            AddWallShadows(corridor);
            Decorate(corridor);
            yield return new WaitForSeconds(waitTime);
        }
        //Add shadows and decoration to rooms
        foreach (RoomInfo room in rooms)
        {
            AddWallShadows(room);
            Decorate(room);
            yield return new WaitForSeconds(waitTime);
        }

        //Decorate all corridors
        //foreach (CorridorInfo corridor in corridors)
        //{
        //    //AddWallDecorations(corridor);
        //    yield return new WaitForSeconds(waitTime);
        //}
        
        mapFinished = true;


        tilemaps[1].RefreshAllTiles();

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
                startY -= Random.Range(1, randY - 1);
                break;
            case Facing.South:
                startY -= randY;
                //startX -= (int)Mathf.Floor(randX / 2f);
                startX -= Random.Range(1, randX);
                break;
            case Facing.West:
                startX -= randX;
                //startY -= (int)Mathf.Floor(randY / 2f);
                startY -= Random.Range(1, randY - 1);
                break;
        }

        //Debug statements for room sizes
        //Debug.Log("Start X: " + startX + " Start Y: " + startY);
        //Debug.Log("Rand X: " + randX + " Rand Y: " + randY);
        //Debug.Log("End X: " + (startX + randX - 1) + " End Y: " + (startY + randY - 1));

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
                //ensure the starting door isn't overwritten
                if ((x == startX || x == endX || y == startY || y == endY) && (x != door.position.x || y != door.position.y || rooms.Count == 0))
                {
                    //PlaceTile(new Vector3Int(x, y, 0), tilemaps[1] wallTile);
                    if (!tilemaps[0].HasTile(new Vector3Int(x, y, 0)))
                    {
                        tilemaps[1].SetTile(new Vector3Int(x, y, 0), wallTile);
                    }
                }

            }
        }

        //place the perk selection options ifthis isn't the first map generated and this is the first room
        if (rooms.Count == 0 && !firstMap)
        {
            int dif = (endX - startX) / (numPerks + 1);
            for (int i = 1; i <= numPerks; i++)
            {
                Instantiate(perkObject, new Vector3(startX + dif * i, startY + (endY - startY) / 2, 0), new Quaternion());
            }
        }

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

                    //place the new door as long as the location is inside a wall, door is placed currenlty by removing the wall
                    //add it to the list of new doors
                    if (tilemaps[1].GetTile(randDoorLocation) == wallTile)
                    {
                        tilemaps[1].SetTile(randDoorLocation, null);
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
        Vector3Int maxOverlapPosition = endDoorPosition + corridorDirection * (minRoomSize + 1);

        if (maxOverlapPosition.x <= rooms[0].upperRight.x + minRoomSize && maxOverlapPosition.x >= rooms[0].lowerLeft.x - minRoomSize &&
            maxOverlapPosition.y <= rooms[0].upperRight.y + minRoomSize && maxOverlapPosition.y >= rooms[0].lowerLeft.y - minRoomSize)
        {
            tilemaps[1].SetTile(door.position, wallTile);
            currentDoorNum--;
            //Debug.Log("OVERLAP");
            return newDoors;
        }

        //check if this corridor should connect to a room, due to overlap or starting just short
        for (int i = 1; i <= length + 1 + minRoomSize / 2; i++)
        {

            if (tilemaps[1].HasTile(door.position + i * corridorDirection)/* && tilemap.HasTile(door.position + (i + 1) * corridorDirection)*/)
            {
                Debug.Log("Connector Case 1");
                endDoorPosition = door.position + i * corridorDirection;
                length = i - 1;
                isConnector = true;

                //if this connection would have issues with connecting tothe corenr of a room
                if (tilemaps[1].HasTile(new Vector3Int(endDoorPosition.x, endDoorPosition.y + 1, 0)) && !tilemaps[1].HasTile(new Vector3Int(endDoorPosition.x, endDoorPosition.y + 2, 0)))
                {
                    tilemaps[1].SetTile(door.position, wallTile);
                    currentDoorNum--;
                    return newDoors;
                }

                break;
            }
        }

        //checks if this corridor would connect to the corner of another room
        //NOTE: with recent changes to the tiling system this corner case may cause issues
        //NOTE: it's causing issues
        if (isConnector && tilemaps[1].GetTile(endDoorPosition + corridorDirection) == wallTile && length + 1 >= minCorridorSize)
        {
            //cornerCount++;
            tilemaps[1].SetTile(door.position, wallTile);
            return newDoors;
            //checks if this would result in two adjacent door tiles

            if (tilemaps[1].HasTile(endDoorPosition + corridorDirection * 2))
            {
                //Deprecated form of above if statement for doors being placed
                /*tilemaps[1].GetTile(endDoorPosition + corridorDirection * 2) != doorTile*/
                //Debug.Log("Corner case");
                //remove the wall tile to signify a door
                tilemaps[1].SetTile(endDoorPosition, null);
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
        //NOTE: this statement is likely deprecated
        if (tilemaps[0].GetTile(door.position + corridorDirection) == floorTile)
        {
            return newDoors;
        }
        //if the corridor is too small, do not generate it
        else if (length < minCorridorSize)
        {
            tilemaps[1].SetTile(door.position, wallTile);
            return newDoors;
        }

        //tilemap.SetTile(endDoorPosition, doorTile);
        //ensure the end door position is empty as expected
        tilemaps[1].SetTile(endDoorPosition, null);

        //place the tiles that form the corridor
        for (int i = 0; i <= length + 1; i++)
        {
            Vector3Int tileLoc = door.position + i * corridorDirection;

            //tilemap.SetTile(tileLoc, floorTile);
            //add floor and both walls
            PlaceTile(tileLoc, tilemaps[0], floorTile);
            PlaceTile(tileLoc + new Vector3Int(yAdjust, xAdjust, 0), tilemaps[1], wallTile);
            PlaceTile(tileLoc + new Vector3Int(-1 * yAdjust, -1 * xAdjust, 0), tilemaps[1], wallTile);



        }

        //if this corridor is not a connector, add it's ending door to the lsit of new doors
        if (!isConnector)
        {
            newDoors.Add(new DoorInfo(endDoorPosition, door.facing));
            //StartCoroutine(GenerateRoom(doorPosition + new Vector3Int((length + 1) * xAdjust, (length + 1) * yAdjust, 0), doorDirection));
        }

        CorridorInfo corridor;
        if (door.position.x < endDoorPosition.x || door.position.y < endDoorPosition.y)
        {
            corridor.start = door.position;
            corridor.end = endDoorPosition;
        }
        else
        {
            corridor.start = endDoorPosition;
            corridor.end = door.position;
        }
        corridors.Add(corridor);


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
                    Vector3 worldLocation = tilemaps[0].GetCellCenterWorld(randLocation);

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
    void PlaceTile(Vector3Int position, Tilemap tilemap, TileBase tile)
    {
        for (int t = 0; t <= 1; t++)
        {
            if (tilemaps[t].HasTile(position))
            {
                return;
            }
        }
        //foreach (Tilemap t in tilemaps)
        //{
        //    if (t.HasTile(position))
        //    {
        //        return;
        //    }
        //}
        tilemap.SetTile(position, tile);

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
                    //remove any outstanding wall and decoration tiles
                    tilemaps[1].SetTile(new Vector3Int(x, y, 0), null);
                    tilemaps[2].SetTile(new Vector3Int(x, y, 0), null);
                    //fill the room with floor tiles
                    tilemaps[0].SetTile(new Vector3Int(x, y, 0), tile);
                    
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
                //if no wall exists at theedge of aroom, it must be a door
                if (tilemaps[1].GetTile(new Vector3Int(x, y, 0)) == null)
                {
                    PlaceDoor(x, y, tile);
                }
            }
        }

        for (int y = room.lowerLeft.y - 1; y <= room.upperRight.y + 1; y++)
        {
            for (int x = room.lowerLeft.x - 1; x <= room.upperRight.x + 1; x += room.upperRight.x + 1 - (room.lowerLeft.x - 1))
            {
                //if no wall exists at theedge of aroom, it must be a door
                if (tilemaps[1].GetTile(new Vector3Int(x, y, 0)) == null)
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

    //checks if a given tile is a doorway, meaning that one wall tile exists on either side of it
    //if the door would lead to nowhere place a wall, otherwise place a door
    void PlaceDoor(int startX, int startY, TileBase door)
    {


        //if both tiles adjacent on the X axis are walls
        if (tilemaps[1].HasTile(new Vector3Int(startX - 1, startY, 0)) && tilemaps[1].HasTile(new Vector3Int(startX + 1, startY, 0)))
        {
            //check to ensure that floors exist above and below this tile
            if (tilemaps[0].HasTile(new Vector3Int(startX, startY - 1, 0)) && tilemaps[0].HasTile(new Vector3Int(startX, startY + 1, 0)))
            {
                //place the door and exit
                tilemaps[0].SetTile(new Vector3Int(startX, startY, 0), door);
                return;
            }
            else
            {
                tilemaps[0].SetTile(new Vector3Int(startX, startY, 0), null);
                tilemaps[1].SetTile(new Vector3Int(startX, startY, 0), wallTile);
            }

        }
        //if both tiles adjacent on the Y axis are walls
        else if (tilemaps[1].HasTile(new Vector3Int(startX, startY - 1, 0)) && tilemaps[1].HasTile(new Vector3Int(startX, startY + 1, 0)))
        {
            //check to ensure that floorsexist on iether side of this tile
            if (tilemaps[0].HasTile(new Vector3Int(startX - 1, startY, 0)) && tilemaps[0].HasTile(new Vector3Int(startX + 1, startY, 0)))
            {
                //place the door and exit
                tilemaps[0].SetTile(new Vector3Int(startX, startY, 0), door);
                return;
            }
            else
            {
                tilemaps[1].SetTile(new Vector3Int(startX, startY, 0), wallTile);
            }
        }

        
    }

    //Adds extra tiles to add depth and details to the walls of a room
    //assumes that wallTile is a ruletile that includes shading
    void AddWallShading(RoomInfo room)
    {
        for (int x = room.lowerLeft.x - 1; x <= room.upperRight.x + 1; x++)
        {
            for (int y = room.lowerLeft.y - 2; y <= room.upperRight.y; y += room.upperRight.y - room.lowerLeft.y + 2)
            {
                //if a wall exists above this spot and no floor exists above this spot, add shaded wall
                if (tilemaps[1].HasTile(new Vector3Int(x, y + 1, 0)) && !tilemaps[0].HasTile(new Vector3Int(x, y + 1, 0)) && (!tilemaps[0].HasTile(new Vector3Int(x + 1, y, 0)) || x <= room.upperRight.x))
                {
                    //places the extra wallTile which will convert to shaded wall
                    tilemaps[1].SetTile(new Vector3Int(x, y, 0), wallTile);
                    //places decorative floor tiles to ensure proper coverage of the background, only needed if floor tile does not match the background
                    if (y < room.upperRight.y)
                    {
                        tilemaps[0].SetTile(new Vector3Int(x, y + 1, 0), floorTile);
                    }
                }      
            }
        }
    }

    //Adds extra tiles to add depth and details to the walls of a corridor
    //only needed for East/West corridors
    void AddWallShading(CorridorInfo corridor)
    {
        //if this is a North/South corridor, return
        if (corridor.start.x == corridor.end.x)
        {
            return;
        }

        for (int x = corridor.start.x; x <= corridor.end.x; x++)
        {
            
            if (tilemaps[1].HasTile(new Vector3Int(x, corridor.start.y + 1, 0)))
            {
                PlaceTile(new Vector3Int(x, corridor.start.y + 2, 0), tilemaps[1], wallTile);
            }
            //if southern wall tiles should be placed, do so
            if (tilemaps[1].HasTile(new Vector3Int(x, corridor.start.y - 1, 0)))
            {
                PlaceTile(new Vector3Int(x, corridor.start.y - 2, 0), tilemaps[1], wallTile);
            }
            //place floor tiles to ensure proper background coverage
            tilemaps[0].SetTile(new Vector3Int(x, corridor.start.y - 1, 0), floorTile);
        }
    }

    //Adds shadows to the corridor to provide depth
    //Assumedto go before AddWallShadows(RoomInfo)
    void AddWallShadows(CorridorInfo corridor)
    {
        //if the corridor is North/South and a wall exists to the east
        //draw a line of shadows through the middle and on the west side of the west wall
        if (corridor.start.x == corridor.end.x)
        {
            for (int y = corridor.start.y - 1; y <= corridor.end.y; y++)
            {
                //draw the line of shadows through the middle of the corridor
                tilemaps[2].SetTile(new Vector3Int(corridor.start.x, y, 0), ChooseWallShadowTile(new Vector3Int(corridor.start.x, y, 0)));
                //draw the line to the west of the west side
                if (y + 2 < corridor.end.y && y >= corridor.start.y)
                {
                    tilemaps[2].SetTile(new Vector3Int(corridor.start.x - 2, y, 0), ChooseWallShadowTile(new Vector3Int(corridor.start.x - 2, y, 0)));
                }
            }
        }
        //else the corridor is East/West
        //draw a line of shadows through the middle and on the south side of the south wall
        else
        {
            for (int x = corridor.start.x; x <= corridor.end.x; x++)
            {
                //draw the line of shadws through the middle of the corridor
                tilemaps[2].SetTile(new Vector3Int(x, corridor.start.y, 0), ChooseWallShadowTile(new Vector3Int(x, corridor.start.y, 0)));
                //draw the line to the south of the south side
                if (x > corridor.start.x || x < corridor.end.x)
                {
                    tilemaps[2].SetTile(new Vector3Int(x, corridor.start.y - 3, 0), ChooseWallShadowTile(new Vector3Int(x, corridor.start.y - 3, 0)));
                }
            }
        }


       
    }

    //Adds shadows to the corridor to provide depth
    void AddWallShadows(RoomInfo room)
    {
        //draw a line of shadows across the south side of the north and south walls
        for (int x = room.lowerLeft.x - 1; x <= room.upperRight.x + 1; x++)
        {
            //draw the north wall shadow
            if (x >= room.lowerLeft.x && x <= room.upperRight.x)
            {
                tilemaps[2].SetTile(new Vector3Int(x, room.upperRight.y - 1, 0), ChooseWallShadowTile(new Vector3Int(x, room.upperRight.y - 1, 0)));
            }
            //draw the south wall shadow ignoring tiles where a wall exists
            if (!tilemaps[1].HasTile(new Vector3Int(x, room.lowerLeft.y - 3, 0)))
            {
                tilemaps[2].SetTile(new Vector3Int(x, room.lowerLeft.y - 3, 0), ChooseWallShadowTile(new Vector3Int(x, room.lowerLeft.y - 3, 0)));
            }
        }

        //draw a line of shadows across the west side of the west and east walls
        for (int y = room.lowerLeft.y - 3; y <= room.upperRight.y + 1; y++)
        {
            //draw the east wall shadow
            if (y >= room.lowerLeft.y - 1 && y <= room.upperRight.y - 1)
            {
                tilemaps[2].SetTile(new Vector3Int(room.upperRight.x, y, 0), ChooseWallShadowTile(new Vector3Int(room.upperRight.x, y, 0)));
            }
            //draw the south wall shadow including the corner
            tilemaps[2].SetTile(new Vector3Int(room.lowerLeft.x - 2, y, 0), ChooseWallShadowTile(new Vector3Int(room.lowerLeft.x - 2, y, 0)));

        }
    }

    //takes in a vector3Int tile location and returns a shadow decoration based on the location of walls around that point
    //returns null if no shadow sohuld be placed
    TileBase ChooseWallShadowTile(Vector3Int location)
    {
        //if a wall exists above this tile, add a shadow for the top
        if (tilemaps[1].HasTile(location + new Vector3Int(0, 1, 0)))
        {
            //if no wall exists to the up-right of this tile return a rightTop edge
            if (!tilemaps[1].HasTile(location + new Vector3Int(1, 1, 0)))
            {
                return wallShadowTiles.rightTop;
            }
            //if a wall exists to the east of this tile, returen a concave corner
            else if (tilemaps[1].HasTile(location + new Vector3Int(1, 0, 0)))
            {
                return wallShadowTiles.concaveCorner;
            }
            //otherwise return the middle top tile
            else
            {
                return wallShadowTiles.midTop;
            }
        }
        //else if a wall exists east of this tile, add a shadow for the east
        else if (tilemaps[1].HasTile(location + new Vector3Int(1, 0, 0)))
        {
            //if no wall exists to the northeast of this tile, return a topright corner
            if (!tilemaps[1].HasTile(location + new Vector3Int(1, 1, 0)))
            {
                return wallShadowTiles.topRight;
            }
            //otherwise return a middle right tile
            else
            {
                return wallShadowTiles.midRight;
            }
        }
        //else if a tile exists to the northeast of this tile return a convex corner
        else if (tilemaps[1].HasTile(location + new Vector3Int(1, 1, 0)))
        {
            return wallShadowTiles.convexCorner;
        }

        //if none of theabove are true, no shadow should exist, reutrn null
        return null;
    }

    //Adds random decorations to a room
    void Decorate(RoomInfo room)
    {
        //calculate area of the room for use in deocration frequency
        int area = (room.upperRight.x - room.lowerLeft.x) * (room.upperRight.y - room.lowerLeft.y);
        //place random decorations based on area / frequency
        for (int i = 0; i < area / decorationFrequency; i++)
        {
            int x = Random.Range(room.lowerLeft.x, room.upperRight.x);
            int y = Random.Range(room.lowerLeft.y, room.upperRight.y);
            tilemaps[3].SetTile(new Vector3Int(x, y, 0), decorationTile);
        }
    }

    //Adds random decorations to a corridor
    void Decorate(CorridorInfo corridor)
    {
        //calculate area of the corridor for use in deocration frequency
        int area = corridor.end.x - corridor.start.x + corridor.end.y - corridor.start.y;
        int x, y;
        //place random decorations based on area / frequency
        for (int i = 0; i < area / decorationFrequency; i++)
        {
            //for a north south corridor generate y so that decorations can be placed on either door and below the south door to account for shading
            if (corridor.start.x == corridor.end.x)
            {
                x = corridor.start.x;
                y = Random.Range(corridor.start.y - 1, corridor.end.y + 1);
                
            }
            //for east west corridors ensure that deocrations can be placed on theeastern door
            else
            {
                x = Random.Range(corridor.start.x, corridor.end.x + 1);
                y = corridor.start.y;
            } 
            tilemaps[3].SetTile(new Vector3Int(x, y, 0), decorationTile);
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
        foreach (Tilemap tilemap in tilemaps)
        {
            tilemap.ClearAllTiles();
            tilemap.CompressBounds();
        }
        Pathfinder.ClearGrid();
        foreach (GameObject ai in GameObject.FindGameObjectsWithTag("AI"))
        {
           Destroy(ai);
        }
        
        foreach (GameObject clear in GameObject.FindGameObjectsWithTag("Cleared on New Map"))
        {
            Destroy(clear);
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

    //the location of a room's floor corners
    public struct RoomInfo
    {
        //the position of the lower left floor corner on the tilemap
        public Vector3Int lowerLeft;
        //the position of the upper right floor corner on the tilemap
        public Vector3Int upperRight;
    }

    //location of a corridor's start door and end door
    public struct CorridorInfo
    {
        //the position of the first door in the corridor
        public Vector3Int start;
        //the position of the second door in the corridor
        public Vector3Int end;

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

    //struct containing tile information used to add shadows
    [System.Serializable]
    struct ShadowInfo
    {
        public TileBase midTop;
        public TileBase rightTop;

        public TileBase topRight;
        public TileBase midRight;

        public TileBase concaveCorner;
        public TileBase convexCorner;

    }


    //the direction a door faces to lead out of a room or corridor
    //North is up, East is right, South is down, West is left
    enum Facing
    {
        North, East, South, West
    }
    #endregion
}
