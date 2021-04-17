using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;
using Photon.Realtime;
using Photon.Pun;
using System;

public class RaiseEvent : MonoBehaviour
{
    public const byte PROJECTILECODE = 1;

    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += NetworkClient_EventReceieved;
    }

  
    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= NetworkClient_EventReceieved;
    }

    private void NetworkClient_EventReceieved(EventData obj)
    {


        //Fire off Event when something happens
       if(obj.Code == PROJECTILECODE)
       {
            //Grab info from obj in event
            object[] data = (object[])obj.CustomData;

            int PhotonID = (int)data[0];

            //Find Gameobject in Scene, Set GO active
            GameObject GOReset = PhotonView.Find(PhotonID).gameObject;

            GOReset.SetActive(true);
            Debug.Log("Gameobject:" + GOReset.name + " has been set to:" + GOReset.active);
 
       }
    }

    private void ProjectileEvent(int PhotonID)
    {
        //Pass Object we want to Set Active into event
        object[] content = new object[] { PhotonID }; // Array contains the target position and the IDs of the selected units
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { InterestGroup = 1 }; // You would have to set the Receivers to All in order to receive this event on the local client as well
        PhotonNetwork.RaiseEvent(PROJECTILECODE, content, raiseEventOptions, SendOptions.SendReliable);
    }
}
