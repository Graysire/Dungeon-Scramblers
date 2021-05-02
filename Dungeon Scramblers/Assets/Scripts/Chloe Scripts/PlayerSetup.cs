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
    //// Start is called before the first frame update
    //void Awake()
    //{
    //    //If this player is not mine
    //    if (!photonView.IsMine)
    //    {
    //        //Debug.Log("Camera: " + cam.name + " is disabled");
    //        if(!CheckNull(cam))
    //        {
    //            cam.enabled = false;
    //        }
    //        if (!CheckNull(RegCam))
    //        {
    //            RegCam.enabled = false;
    //        }
    //        if(!CheckNull(Listener))
    //        {
    //            Listener.enabled = false;
    //        }
    //    }
    //}

    //bool CheckNull<T>(T obj)
    //{
    //    if (obj == null)
    //        return false;
    //    else
    //    {
    //        return true;
    //    }
    //}
        // Start is called before the first frame update
        void Awake()
        {

        }
    private void Start()
    {
        //Get Overlord Cam
        RegCam = GameObject.Find("OverlordCam").GetComponent<Camera>(); ;           
        //If this player is not mine
        if (!photonView.IsMine)
        {
            Debug.Log("Camera: " + cam.name + " is disabled");
            cam.enabled = false;
            if (RegCam != null)
            {
                RegCam.enabled = false;
            }
        }
    }

}
