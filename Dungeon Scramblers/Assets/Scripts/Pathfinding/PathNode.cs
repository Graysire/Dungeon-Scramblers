using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Last Edited: 10/27/2020

//Point on a grid used for A* Pathfinding
public class PathNode
{
    //the x and y positions of this node on the grid
    public readonly int posX;
    public readonly int posY;
    //whether this node is obstructed and cannot be moved through
    public bool isObstructed;

    //the node preceding this node when calculating a path
    public PathNode previousNode;

    //gCost is distance from start of path to this node
    public float gCost;
    //hCost is distance from this node to end of path
    public float hCost;
    //total cost of moving to this node gCost + hCost
    public float FCost { get { return gCost + hCost; } }

    //Constructor taking location as X,Y and obstruction
    public PathNode(int gridX, int gridY, bool isObstruct)
    {
        posX = gridX;
        posY = gridY;
        isObstructed = isObstruct;
    }
}