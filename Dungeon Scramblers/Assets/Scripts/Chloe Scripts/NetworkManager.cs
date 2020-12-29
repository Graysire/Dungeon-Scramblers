﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Doozy.Engine.UI;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [Header("Login UI Panel")]
    public UIView PlayerNamePanel;
    public InputField playerNameInput;
    #region Unity Methods

    [Header("RoomListings")]
    public GameObject RoomLisitngsView; //"Content" panel to display RoomListPrefabs
    public GameObject RoomListingsPrefab; //Prefab of Room listings

    [Header("GameOptions")]
    public UIView GameOptionsPanel;

    [Header("Join Room UI Panel")]
    public UIView JoinRoomListingPanels;

    [Header("Inside Room UI Panel")]
    public UIView InsideRoomUIPanel;

    [Header("Create Room Panel UI")]
    public UIView CreateRoomPanel;
    public InputField RoomNameInputfield;
    public InputField MaxPlayerInputfield;


    //Dictionary for PlayerRoomListings
    private Dictionary<string, RoomInfo> cachedRoomList;
    //Dictionary for PlayerRoomListings as instantiated
    private Dictionary<string, GameObject>RoomListGameObjects;



    private void Start()
    {
        cachedRoomList = new Dictionary<string, RoomInfo>();
        RoomListGameObjects = new Dictionary<string, GameObject>();
    }
    #endregion


    #region UI Callbacks

    //Connects Player to Photon network when entered name
    public void OnLoginButtonClicked()
    {
        string playerName = playerNameInput.text;
        if(!string.IsNullOrEmpty(playerName))
        {
            PhotonNetwork.LocalPlayer.NickName = playerName;
            PhotonNetwork.ConnectUsingSettings();


        }
        else
        {
            Debug.Log("Invalid Name: " + playerName + " please try again");
        }
    }


    //Creates room on Create Room Button
    public void OnCreateRoomButtonClicked()
    {
        string RoomName = RoomNameInputfield.text;
        //If Player doesn't give a valid room name, give them a random name
        if(string.IsNullOrEmpty(RoomName))
        {
            RoomName = "Room" + Random.Range(1000, 10000);
        }
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = (byte) int.Parse(MaxPlayerInputfield.text); //Converts input field to int value for Max room
        PhotonNetwork.CreateRoom(RoomName,roomOptions);
    }

    #endregion

    #region UI Panel Callbacks

    //When Player decides to create a Room
    public void CreatRoomOptionClicked()
    {
        GameOptionsPanel.Hide();
        CreateRoomPanel.Show();
    }

    //When Player decides to Join a Lobby
    public void OnJoinLobbyOptionClicked()
    {

        
        //Player must be in Lobby to view rooms
        if(!PhotonNetwork.InLobby)
        {
            Debug.Log("Joining Lobby Services");
            PhotonNetwork.JoinLobby();
            GameOptionsPanel.Hide();
            JoinRoomListingPanels.Show();
        }
        GameOptionsPanel.Hide();
        JoinRoomListingPanels.Show();
    }



    #endregion

    #region Photon Callbacks

    public override void OnConnected()
    {
        Debug.Log("Player has connected to internet");
        base.OnConnected();
    }

    public override void OnConnectedToMaster()
    {

        Debug.Log("Player: " + PhotonNetwork.LocalPlayer.NickName + " has been connected to Photon Services");
        //
        //Turn on GameOptions canvas Here
        //
        PlayerNamePanel.Hide();
        GameOptionsPanel.Show();
        base.OnConnectedToMaster();
    }

    public override void OnCreatedRoom()
    {
        Debug.Log(PhotonNetwork.CurrentRoom.Name + " is created");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + " joined to " + PhotonNetwork.CurrentRoom.Name);
        
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        ClearRoomListView();

       foreach(RoomInfo room in roomList)
        {
            Debug.Log("room.Name: " + room.Name);

            if(!room.IsOpen || !room.IsVisible || room.RemovedFromList) //If room is no longer available, remove it from the list
            {

                if(cachedRoomList.ContainsKey(room.Name))
                {

                    cachedRoomList.Remove(room.Name);

                }
            }
            else //If the room is available, display it on the list
            {
                if(cachedRoomList.ContainsKey(room.Name)) //If room already exists, update the information
                {
                    cachedRoomList[room.Name] = room;
                }
                else
                {
                    //Add new room to list
                    cachedRoomList.Add(room.Name, room);
                }

              
            }
          
        }

       //Create Room Listing Obj for every room accounted for in the dictionary
       foreach(RoomInfo room in cachedRoomList.Values)
        {
            //Instantiate roomListGameobject and parent it to the RoomListingsView
            GameObject roomListEntryGameobject = Instantiate(RoomListingsPrefab, RoomLisitngsView.transform);
            roomListEntryGameobject.transform.localScale = Vector3.one;

            //change Attirbutes on RoomListPrefab to show Room name and room size
            roomListEntryGameobject.transform.Find("RoomNameText").GetComponent<Text>().text = room.Name;
            roomListEntryGameobject.transform.Find("RoomPlayersText").GetComponent<Text>().text = room.PlayerCount + " / " + room.MaxPlayers;

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //THERE MIGHT BE A BETTER WAY TO DO THIS W/ DOOZY, BUT FOR THE SAKE OF SIMPLICITY, I'M KEEPING IT UNTIL FURHTER NOTICE////

            roomListEntryGameobject.transform.Find("JoinRoomButton").GetComponent<Button>().onClick.AddListener(() =>OnJoinRoomButtonClicked(room.Name));

            //Add instantiation to dictionary
            RoomListGameObjects.Add(room.Name, roomListEntryGameobject);

        }
    }
    #endregion


    #region Private methods

    void OnJoinRoomButtonClicked(string _roomName)
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }
        PhotonNetwork.JoinRoom(_roomName); //Join Lobby thorugh PhotonNetwork

         //Display Inside RoomPanel
    }

    void ClearRoomListView()
    {
        //Loop through instantiated game objects and destroy them when updated
        foreach (var roomListGameobject in RoomListGameObjects.Values)
        {
            Destroy(roomListGameobject);
        }

        RoomListGameObjects.Clear();
    }
    #endregion
}
