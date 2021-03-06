﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Pun;

public class Hover : MonoBehaviour
{
    // Making this class a singleton will allow all obstacles to call on the activate function and replace their
    // sprite into the hover instance while the mopuse is down
    private static Hover HoverInstance;
    [SerializeField]
    private Camera OverlordCam;
    private GameObject EnemyInstance;
    private SpriteRenderer obstacle;


    public static Hover GetHover()
    {
        return HoverInstance;
    }

    private void Awake()
    {
        HoverInstance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        obstacle = this.GetComponent<SpriteRenderer>();   
    }


    public void FollowMouse()
    {
        if(obstacle.enabled)
        {
            Vector3 MouseWorldCoord = OverlordCam.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(MouseWorldCoord.x, MouseWorldCoord.y, -10);
        }
    }

    public void Activate(Sprite sprite)
    {
        obstacle.sprite = sprite;
        obstacle.enabled = true;
    }

    public void Deactivate()
    {
        obstacle.enabled = false;
    }

    public void SetEnemyIntance(GameObject EnemyInstance)
    {
        this.EnemyInstance = EnemyInstance;
    }

    public void CreateEnemyInstance()
    {
        if(EnemyInstance != null)
        {

            //Debug.Log(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            //Debug.Log(Input.mousePosition);
            PathNode node = Pathfinder.WorldToNode(OverlordCam.ScreenToWorldPoint(Input.mousePosition));
            Debug.Log(node);

            if (node != null && !node.isObstructed)
            {
                PhotonNetwork.Instantiate(EnemyInstance.name, new Vector3(node.posX , node.posY), Quaternion.identity);
                
            }
            else
            {
                Debug.Log("Nope");
            }

        }
        
    }

    private void OnMouseDown()
    {
        Debug.Log("Clicked");

        CreateEnemyInstance();
    }
}
