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

    //list of all rooms created
    List<RoomInfo> rooms;

    // Start is called before the first frame update
    void Start()
    {
        Random.InitState(0);
        //tilemap.SetTile(new Vector3Int(0, 0, 0), doorTile);
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
            for (int x = room.lowerLeft.x; x <= room.upperRight.x; x++)
            {
                    for (int y = room.lowerLeft.y; y <= room.upperRight.y; y++)
                    {
                        /*//check if a door should exist in a location, and place it if so
                        if ((x == room.lowerLeft.x - 1 || x == room.upperRight.x + 1 || y == room.lowerLeft.y - 1 || y == room.upperRight.y + 1))
                        {
                            //if the location is not a floor, no door should exist here
                            if (tilemap.GetTile(new Vector3Int(x, y, 0)) != floorTile)
                            {
                                continue;
                            }
                            //check to see if the tile has 2 adjacent walls, if so place a door
                            int adjacentWalls = 0;
                            for (int x2 = -1; x2 <= 1; x2++)
                            {
                                for (int y2 = -1; y2 <= 1; y2++)
                                {
                                    if (x2 != y2 && tilemap.GetTile(new Vector3Int(x + x2, y + y2, 0)) == wallTile)
                                    {
                                        adjacentWalls++;
                                    }
                                }
                            }

                            if (adjacentWalls == 2)
                            {
                                tilemap.SetTile(new Vector3Int(x, y, 0), doorTile);
                            }
                        }*/
                        //ensure that the starting room is never combined with other rooms and fill each room with floor tiles
                        if (x != rooms[0].lowerLeft.x - 1 || x != rooms[0].upperRight.x + 1 || y != rooms[0].lowerLeft.y - 1 || y != rooms[0].upperRight.y + 1)
                        {
                            //fill the room with floor tiles
                            tilemap.SetTile(new Vector3Int(x, y, 0), floorTile);
                        }
                    }


            }
            yield return new WaitForSeconds(waitTime);
        }

        //Add doors to all entrances to all rooms
        foreach (RoomInfo room in rooms)
        {
            for (int x = room.lowerLeft.x - 1; x <= room.upperRight.x + 1; x++)
            {
                for (int y = room.lowerLeft.y - 1; y <= room.upperRight.y + 1; y += room.upperRight.y + 1 - (room.lowerLeft.y -1))
                {
                    if (tilemap.GetTile(new Vector3Int(x, y, 0)) == floorTile && IsDoorway(x,y))
                    {
                        tilemap.SetTile(new Vector3Int(x, y, 0), doorTile);
                    }
                }
            }

            for (int y = room.lowerLeft.y - 1; y <= room.upperRight.y + 1; y++)
            {
                for (int x = room.lowerLeft.x - 1; x <= room.upperRight.x + 1; x += room.upperRight.x + 1 - (room.lowerLeft.x - 1))
                {
                    if (tilemap.GetTile(new Vector3Int(x, y, 0)) == floorTile && IsDoorway(x, y))
                    {
                        tilemap.SetTile(new Vector3Int(x, y, 0), doorTile);
                    }
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
                //startX -= (int)Mathf.Floor(randX / 2f);
                startX -= Random.Range(1, randX);
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

        //add this room's information to the list of rooms
        RoomInfo thisRoom;
        thisRoom.lowerLeft = new Vector3Int(startX + 1, startY + 1, 0);
        thisRoom.upperRight = new Vector3Int(endX - 1, endY - 1, 0);
        rooms.Add(thisRoom);

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


        Vector3Int randDoorLocation = new Vector3Int(0, 0, 0); ;

        //for each wall of the room except the entering wall, check for creation of new doors and add any new doors to the list of new doors
        for (int i = 0; i < 4; i++)
        {
            //if the wall being checked is not the wall being entered from
            if (Mathf.Abs(i - (int) door.facing) != 2)
            {
                //check if new door should be generated
                if (Random.Range(1, 10000) <= GetDoorChance())
                {
                    currentDoorNum++;
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
                    //add it to the list of new doors
                    if (tilemap.GetTile(randDoorLocation) == wallTile)
                    {
                        tilemap.SetTile(randDoorLocation, floorTile);
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

        //calculate end door position and corridor direction vector
        Vector3Int endDoorPosition = door.position + new Vector3Int((length + 1) * xAdjust, (length + 1) * yAdjust, 0);
        Vector3Int corridorDirection = new Vector3Int(xAdjust, yAdjust, 0);

        //check if this corridor should connect to a room
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

        //checks if this corridor would connect to the corner of another room
        if (isConnector && tilemap.GetTile(endDoorPosition + corridorDirection) == wallTile && length + 1 >= minCorridorSize)
        {
            //checks if this would result in two adjacent door tiles
            if (!tilemap.GetTile(endDoorPosition + corridorDirection * 2) == doorTile)
            {
                tilemap.SetTile(endDoorPosition, floorTile);



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

        if (tilemap.GetTile(door.position + corridorDirection) == floorTile)
        {
            return newDoors;
        }
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

    //gets the chance of a new door being created based on the number of doors that exist
    int GetDoorChance()
    {

        //Debug.Log("Num:" + currentDoorNum + ", C:" + ((100.0 / Mathf.Log(currentDoorNum + 1, minimumDoors)) - (currentDoorNum >= softMaximumDoors ? softMaximumPenalty : 0) / 100) + "%");
        return (int) (100.0 / Mathf.Log(currentDoorNum + 1, minimumDoors) * 100) - (currentDoorNum >= softMaximumDoors? softMaximumPenalty:0);
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
    bool IsDoorway(int startX, int startY)
    {
        //if both tiles adjacent on the X axis are walls or both tiles adjacent on the Y axis are walls, return true
        if (tilemap.GetTile(new Vector3Int(startX - 1, startY, 0)) == wallTile && tilemap.GetTile(new Vector3Int(startX + 1, startY, 0)) == wallTile
            || tilemap.GetTile(new Vector3Int(startX, startY - 1, 0)) == wallTile && tilemap.GetTile(new Vector3Int(startX, startY + 1, 0)) == wallTile)
        {
            return true;
        }
        return false;
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

    struct RoomInfo
    {
        //the position of the lower left floor corner on the tilemap
        public Vector3Int lowerLeft;
        //the position of the upper right floor corner on the tilemap
        public Vector3Int upperRight;
    }

    //the direction a door faces to lead out of a room or corridor
    //North is up, East is right, South is down, West is left
    enum Facing
    {
        North, East, South, West
    }
}
