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
    public Camera RegCam;
    public AudioBehaviour Listener;
    public bool isOverlord = false;

    private void Start()
    {
        //Get Overlord Cam
        if (GameObject.Find("OverlordCam") != null)
        {
            RegCam = GameObject.Find("OverlordCam").GetComponent<Camera>();
        }
        
        //If this player is not mine
        if (!photonView.IsMine)
        {
            Debug.Log("Camera: " + cam.name + " is disabled");
            cam.enabled = false;
            if (isOverlord)
            {
                RegCam.enabled = false;
                RegCam.gameObject.SetActive(false);
            }
        }
    }

}
