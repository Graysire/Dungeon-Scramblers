﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Handles A* pathfinding between tiles on a grid
public class Pathfinder : MonoBehaviour
{
    //accessible instance of the pathfinder class
    //[SerializeField]
    //public static Pathfinder pathfinder;

    //array containing all pathing nodes
    static PathNode[,] nodeGrid;

    [SerializeField]
    //size of the grid, can be removed if using constructor
    static Vector2Int gridSize;
    //the grid's offset
    static Vector3Int gridOffset;

    //the tilemap grid that paths are being found on
    [SerializeField]
    static Grid tileGrid;

    //default tile used when no other tiles exists, deprecated by map maker's responsibilities
    //[SerializeField]
    //TileBase defaultTile = null;

    //most recent path generated by the script
    static List<PathNode> mostRecentPath = new List<PathNode>();

    //constructor for singleton pathfinder
    /*Pathfinder()
    {
        tileGrid = null;
        gridSize = new Vector2Int(0,0);
        nodeGrid = null;
    }*/

    //gets the shortest path between two points
    public static List<Vector3> GetPath(Vector3 startPos, Vector3 targetPos, int maxLength = 90000)
    {
        //get pathfinding nodes from positions
        PathNode start = WorldToNode(startPos);
        PathNode target = WorldToNode(targetPos);

        //get list of pathnodes
        List<PathNode> nodeList = GetPath(start, target, maxLength);
        //create list of vectors to return
        List<Vector3> vectorList = new List<Vector3>();

        //convert nodes to vectors
        for (int i = 0; i < nodeList.Count; i++)
        {
            vectorList.Add(NodeToWorld(nodeList[i]));
        }

        return vectorList;
    }

    //gets the shortest path between two points on the grid
    static List<PathNode> GetPath(PathNode startNode, PathNode targetNode, int maxLength)
    {
        //check validity of start and target nodes
        if (startNode == null)
        {
            Debug.Log("Start Position Out of Bounds");
            return null;
        }
        else if (targetNode == null)
        {
            Debug.Log("Target Position Out of Bounds");
            return null;
        }
        else if (startNode == targetNode)
        {
            Debug.Log("Start and Target Position are identical tiles");
            return null;
        }

        //reset gCost of start node
        startNode.gCost = 0;
        //set hCost of start node
        startNode.hCost = GetHCost(startNode, targetNode);

        //list of pathing nodes that have not been checked
        List<PathNode> OpenList = new List<PathNode>();
        //set of pathing nodes that have been checked
        HashSet<PathNode> ClosedList = new HashSet<PathNode>();

        //add first node to unchecked nodes
        OpenList.Add(startNode);

        //while unchecked nodes exist, continue to check nodes
        while (OpenList.Count > 0)
        {
            //initialize node to check as first element of openList
            PathNode currentNode = OpenList[0];

            //compare to every other node, if any node is clsoer to the goal, that node becomes currentNode
            for (int i = 0; i < OpenList.Count; i++)
            {
                if (OpenList[i].FCost < currentNode.FCost || OpenList[i].FCost == currentNode.FCost && OpenList[i].hCost < currentNode.hCost)
                {
                    currentNode = OpenList[i];
                }
            }

            //remove current node from unchecked nodes
            OpenList.Remove(currentNode);
            //add current node to checked nodes
            ClosedList.Add(currentNode);

            //if the current node is the target node, construct and return the path
            if (currentNode == targetNode)
            {
                //initialize path list
                List<PathNode> finalPath = new List<PathNode>();

                //go backwards from the current node until the starting node, adding nodes tothe path
                while (currentNode != startNode)
                {
                    finalPath.Add(currentNode);
                    currentNode = currentNode.previousNode;
                }
                //add the starting node to the path;
                finalPath.Add(currentNode);

                //reverse the final path so it goes fro mstart to end;
                finalPath.Reverse();

                //set most recent path and return the path
                mostRecentPath = finalPath;
                return finalPath;
            }

            //loop through all adjacent nodes, skipping any nodes that have been checked or are obstructed
            foreach (PathNode adjacentNode in GetAdjacentNodes(currentNode))
            {
                if (adjacentNode.isObstructed || ClosedList.Contains(adjacentNode))
                {
                    continue;
                }

                //if the adjacent node was considered farther from the start
                //or if adjacent node is not in the list of unchecked nodes then add it to unchecked nodes
                if (!OpenList.Contains(adjacentNode) || currentNode.gCost + 1 < adjacentNode.gCost)
                {
                    //update gCost and hCost of adjacent node
                    adjacentNode.gCost = currentNode.gCost + 1;
                    adjacentNode.hCost = GetHCost(adjacentNode, targetNode);
                    //point adjacent node's previous to current node to construct path
                    adjacentNode.previousNode = currentNode;
                    //if unchecked nodes does not contain adjacent node and the node is within maxlength, add it to unchecked nodes
                    if (!OpenList.Contains(adjacentNode) && adjacentNode.gCost <= maxLength)
                    {
                        OpenList.Add(adjacentNode);
                    }
                }
            }
        }

        Debug.Log("Movement Failed, Path Impossible or Target Out of Range");
        return null;
    }


    public static void ClearGrid()
    {
        nodeGrid = null;
    }



    //initializes pathfinding nodes on the grid based on the bounds of the tilemap
    public static void CreateGrid(Grid grid, UnityEngine.Tilemaps.Tilemap tilemap, UnityEngine.Tilemaps.TileBase wallTile)
    {
        //sets the grid used by the pathfinder
        tileGrid = grid;

        //the adjustment used to calculate the actual positions of new nodes
        //subtract 1 to account for integer rounding down in 
        gridOffset = new Vector3Int(tilemap.cellBounds.x, tilemap.cellBounds.y, 0);

        //calculate the size of the grid needed
        gridSize.x = tilemap.cellBounds.xMax - gridOffset.x;
        gridSize.y = tilemap.cellBounds.yMax - gridOffset.y;

        //instantiates 2D array of nodes
        nodeGrid = new PathNode[gridSize.y, gridSize.x];

        //deprecated by mapmaker
        //Tilemap tilemap = tileGrid.gameObject.GetComponentInChildren<Tilemap>();

        //fills every space on the grid with a node, obstructed if on a wallTile, unobstructed otherwise
        //if a space does not contain a tile, do not initialize a node there
        for (int y = 0; y < gridSize.y; y++)
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                if (tilemap.HasTile(new Vector3Int(x + gridOffset.x, y + gridOffset.y, 0)))
                {
                    //fills every space on the grid that has a tile with a node
                    if (tilemap.GetTile(new Vector3Int(x + gridOffset.x, y + gridOffset.y, 0)) == wallTile)
                    {
                        nodeGrid[y, x] = new PathNode(x + gridOffset.x, y + gridOffset.y, true);
                    }
                    else
                    {
                        nodeGrid[y, x] = new PathNode(x + gridOffset.x, y + gridOffset.y, false);
                    }
                }
                else
                {
                    nodeGrid[y, x] = null;
                }

            }
        }
    }

    //returns pathfinding node at given point in world space
    public static PathNode WorldToNode(Vector3 worldPos)
    {
        //get the cell location from the Tilemap Grid
        Vector3Int cellLocation = tileGrid.WorldToCell(worldPos) - gridOffset;
        //Debug.Log("World Position: " + worldPos + " to CellLocation: " + cellLocation + " " + nodeGrid.GetLength(1));
        //if the index is our of bounds return null, otherwise return the node
        if (cellLocation.x >= nodeGrid.GetLength(1) || cellLocation.x < 0 || cellLocation.y >= nodeGrid.GetLength(0) || cellLocation.y < 0)
        {
            return null;
        }
        else
        {
            return nodeGrid[cellLocation.y, cellLocation.x];
        }
    }

    //returns world space location of pathfinding node
    static Vector3 NodeToWorld(PathNode node)
    {
        //returns the location of the node using the Tilemap Grid
        return tileGrid.CellToWorld(new Vector3Int(node.posX, node.posY, 0));
    }

    //returns a list of all nodes adjacent to this one
    static List<PathNode> GetAdjacentNodes(PathNode centerNode)
    {
        //creates list of nodes to be returned
        List<PathNode> adjacentNodes = new List<PathNode>();


        int truePosX = centerNode.posX - gridOffset.x;
        int truePosY = centerNode.posY - gridOffset.y;

        //iterate through every adjacent node
        for (int y = -1; y <= 1; y++)
        {
            for (int x = -1; x <= 1; x++)
            {
                //if x and y are not both 0, and they do not result in an index out of range exception
                //add the node at centerX + x and centerY + y to the list of adjacent nodes
                if (((x != 0) || (y != x)) && x != y * -1)
                {
                    //Debug.Log("Potential Point " + truePosX + "," + truePosY);


                    //check that the x position is within the bounds of the grid
                    if (truePosX + x >= 0 && truePosX + x < nodeGrid.GetLength(1) 
                        //check that the y position is within the bounds of the grid
                        && truePosY + y >= 0 && truePosY < nodeGrid.GetLength(0) 
                        //check that the target point exists
                        && nodeGrid[truePosY + y,truePosX + x] != null)
                    {
                        
                        adjacentNodes.Add(nodeGrid[truePosY + y, truePosX + x]);
                    }
                }
            }
        }

        //return the list of adjacent nodes
        return adjacentNodes;
    }

    //returns the distance between two nodes for the purposes of pathfinding
    static int GetHCost(PathNode startNode, PathNode targetNode)
    {
        return Mathf.RoundToInt(Mathf.Sqrt(Mathf.Pow(startNode.posX - targetNode.posX, 2f) + Mathf.Pow(startNode.posY - targetNode.posY, 2f)));
    }

    //draws the location of all pathing nodes for the developer view
    void OnDrawGizmos()
    {
        //draws small sphres to represent each pathfinding node
        if (nodeGrid != null)
        {
            for (int y = 0; y < nodeGrid.GetLength(0); y++)
            {
                for (int x = 0; x < nodeGrid.GetLength(1); x++)
                {
                    //draw the start of the most recent path in blue
                    //draw the end of the most recent path in black
                    //draw the body of the most recent path in red
                    //draw obstructed nodes in magenta
                    //draw any other nodes in white
                    if (nodeGrid[y, x] != null)
                    {
                        if (mostRecentPath.Count > 0 && mostRecentPath[mostRecentPath.Count - 1] == nodeGrid[y, x])
                        {
                            Gizmos.color = Color.blue;
                        }
                        else if (mostRecentPath.Count > 0 && mostRecentPath[0] == nodeGrid[y, x])
                        {
                            Gizmos.color = Color.black;
                        }
                        else if (mostRecentPath.Contains(nodeGrid[y, x]))
                        {
                            Gizmos.color = Color.red;
                        }
                        else if (nodeGrid[y, x].isObstructed)
                        {
                            Gizmos.color = Color.magenta;
                        }
                        else
                        {
                            Gizmos.color = Color.white;
                        }
                        Gizmos.DrawSphere(tileGrid.GetCellCenterWorld(new Vector3Int(nodeGrid[y, x].posX, nodeGrid[y, x].posY, 0)), 0.25f);
                    }
                }
            }
        }
    }

}
