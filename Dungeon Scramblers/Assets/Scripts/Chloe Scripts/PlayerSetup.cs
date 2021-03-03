using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Cinemachine;

//This script will be called at the start of the game
// checks if this is our "local" player and turns off everyone else's camera
public class PlayerSetup : MonoBehaviourPunCallbacks
{
    public CinemachineVirtualCamera cam;
    // Start is called before the first frame update
    void Awake()
    {
        //If this player is not mine
        if (!photonView.IsMine)
        {
            Debug.Log("Camera: " + cam.name + " is disabled");
            cam.enabled = false;
        }
    }

}
