using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

//Script to generate a dungeon map
public class MapMaker : MonoBehaviour
{
    [SerializeField]
    int maxRooms = 200;
    [SerializeField]
    int newDoorChance = 50;

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
        //GenerateCorridor(new Vector3Int(0, 0, 0), 0);
        //GenerateCorridor(new Vector3Int(0, 0, 0), 1);
        //GenerateCorridor(new Vector3Int(0, 0, 0), 2);
        //GenerateCorridor(new Vector3Int(0, 0, 0), 3);
        StartCoroutine(GenerateRoom(new Vector3Int(0, 0, 0), 0));
        
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
    IEnumerator GenerateRoom(Vector3Int doorPosition, int doorDirection)
    {
        maxRooms--;
        if (maxRooms < 0)
        {
            yield break;
        }

        //generate a random x size of the floor space of the room
        int randX = Random.Range(minRoomSize, maxRoomSize) + 1;// * (doorDirection == 3 ? -1 : 1);
        //generate a random x size of the floor space of the room
        int randY = Random.Range(minRoomSize, maxRoomSize) + 1;// * (doorDirection == 2 ? -1 : 1);

        //set the starting positions to draw tiles from
        int startX = doorPosition.x; //+ (doorDirection == 1 ? 1 : doorDirection == 3 ? -1 : 0) + (int) Mathf.Ceil(randX / 2f);
        int startY = doorPosition.y; //+ (doorDirection == 0 ? 1 : doorDirection == 2 ? -1 : 0) + (int) Mathf.Ceil(randY / 2f);

        //based on door direction, modify starting positions and set a tile to expand thetilemap to cover the room
        switch (doorDirection)
        {
            case 0:
                //startY += 1;
                startX -= (int)Mathf.Floor(randX / 2f);
                //tilemap.SetTile(new Vector3Int(startX - 1, startY + randY, 0), wallTile);
                break;
            case 1:
                //startX += 1;
                startY -= (int)Mathf.Floor(randY / 2f);
                //tilemap.SetTile(new Vector3Int(startX + randX, startY - 1, 0), wallTile);
                break;
            case 2:
                startY -= randY;
                startX -= (int)Mathf.Floor(randX / 2f);
               // tilemap.SetTile(new Vector3Int(startX - 1, startY - 1, 0), wallTile);
                break;
            case 3:
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

        for (int x = startX; x <= startX + randX; x++)
        {
            for (int y = startY; y <= startY + randY; y++)
            {
                if (x == startX || x == startX + randX || y == startY || y == startY + randY)
                {
                    PlaceTile(new Vector3Int(x, y, 0), wallTile);
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

        Vector3Int randDoorLocation;

        //if the door is not facing up, generate a door and corridor facing left
        if (doorDirection != 0 && Random.Range(0,100) < newDoorChance)
        {
            randDoorLocation = new Vector3Int(startX + Random.Range(1, randX - 1), startY, 0);
            if (tilemap.GetTile(randDoorLocation) == wallTile)
            {
                tilemap.SetTile(randDoorLocation, doorTile);
                yield return new WaitForSeconds(2f);
                GenerateCorridor(randDoorLocation, 2);
            }
        }
        //if the door is not facing right, generate a door and corridorfacing left
        if (doorDirection != 1 && Random.Range(0, 100) < newDoorChance)
        {
            randDoorLocation = new Vector3Int(startX, startY + Random.Range(1, randY - 1), 0);
            if (tilemap.GetTile(randDoorLocation) == wallTile)
            {
                tilemap.SetTile(randDoorLocation, doorTile);
                yield return new WaitForSeconds(2f);
                GenerateCorridor(randDoorLocation, 3);
            }
        }
        //if the door is not facing down, generate a door and corridor facing up
        if (doorDirection != 2 && Random.Range(0, 100) < newDoorChance)
        {
            randDoorLocation = new Vector3Int(startX + Random.Range(1, randX - 1), startY + randY, 0);
            if (tilemap.GetTile(randDoorLocation) == wallTile)
            {
                tilemap.SetTile(randDoorLocation, doorTile);
                yield return new WaitForSeconds(2f);
                GenerateCorridor(randDoorLocation, 0);
            }
        }
        //if the door is not facing left, generate a door and corridor facing right
        if (doorDirection != 3 && Random.Range(0, 100) < newDoorChance)
        {
            randDoorLocation = new Vector3Int(startX + randX, startY + Random.Range(1, randY - 1), 0);
            if (tilemap.GetTile(randDoorLocation) == wallTile)
            {
                tilemap.SetTile(randDoorLocation, doorTile);
                yield return new WaitForSeconds(2f);
                GenerateCorridor(randDoorLocation, 1);
            }
        }
    }

    //generates a corridor with an entering door at doorposition towards doorDirection
    //0 = Up, 1 = Right, 2 = Down, 3 = Left
    void GenerateCorridor(Vector3Int doorPosition, int doorDirection)
    {
        if (maxRooms < 0)
        {
            return;
        }
        //generate a random length for the corridor
        int length = Random.Range(minCorridorSize, maxCorridorSize); //* (doorDirection >= 2 ? -1 : 1);

        //initialize adjustment
        int xAdjust = 0;
        int yAdjust = 0;

        //whether this corridor connects to a room that has already been generated
        bool isConnector = false;

        //set the x and y adjustment to tile location based off door direction
        //this decides what direction the tiles are put down
        switch (doorDirection)
        {
            case 0:
                xAdjust = 0;
                yAdjust = 1;

                break;
            case 1:
                xAdjust = 1;
                yAdjust = 0;

                break;
            case 2:
                xAdjust = 0;
                yAdjust = -1;

                break;
            case 3:
                xAdjust = -1;
                yAdjust = 0;

                break;
        }

        Vector3Int endDoorPosition = doorPosition + new Vector3Int((length + 1) * xAdjust, (length + 1) * yAdjust, 0);
        Vector3Int corridorDirection = new Vector3Int(xAdjust, yAdjust, 0);

        for (int i = 1; i <= length + 1; i++)
        {
            if (tilemap.HasTile(doorPosition + i * corridorDirection))
            {
                endDoorPosition = doorPosition + i * corridorDirection;
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

        if (tilemap.GetTile(doorPosition + corridorDirection) == floorTile)
        {
            return;
        }
        else if (length < minCorridorSize)
        {
            tilemap.SetTile(doorPosition, wallTile);
            return;
        }

        tilemap.SetTile(endDoorPosition, doorTile);

        for (int i = 0; i <= length + 1; i++)
        {
            Vector3Int tileLoc = doorPosition + i * corridorDirection;

            PlaceTile(tileLoc, floorTile);
            PlaceTile(tileLoc + new Vector3Int(yAdjust, xAdjust, 0), wallTile);
            PlaceTile(tileLoc + new Vector3Int(-1 * yAdjust, -1 * xAdjust, 0), wallTile);


        }



        if (!isConnector)
        {
            StartCoroutine(GenerateRoom(doorPosition + new Vector3Int((length + 1) * xAdjust, (length + 1) * yAdjust, 0), doorDirection));
        }
    }

    void PlaceTile(Vector3Int position, TileBase tile)
    {
        if (!tilemap.HasTile(position))
        {
            tilemap.SetTile(position, tile);
        }
    }
}
