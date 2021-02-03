using System.Collections;
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

    [Header("Inside Room")]
    public Text RoomInfoText;
    public GameObject PlayerListingPrefab;
    public GameObject PlayerListingsPanel;


    [Header("Buttons")]
    public GameObject CreateRoomButton;
    public GameObject Backbutton;
    public GameObject LeaveRoomButton;
    public GameObject StartButton;


    //Dictionary for PlayerRoomListings
    private Dictionary<string, RoomInfo> cachedRoomList;
    //Dictionary for PlayerRoomListings as instantiated
    private Dictionary<string, GameObject>RoomListGameObjects;
    //Dictionary for Player Objects
    private Dictionary<int, GameObject> PlayerListGameObjects;


    private void Start()
    {
        cachedRoomList = new Dictionary<string, RoomInfo>();
        RoomListGameObjects = new Dictionary<string, GameObject>();


        //All clients in the same room will load the same scene
        PhotonNetwork.AutomaticallySyncScene = true;
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
        //Setting correct UI buttons
        RoomNameInputfield.gameObject.SetActive(true);
        MaxPlayerInputfield.gameObject.SetActive(true);
        CreateRoomButton.SetActive(true);
        StartButton.SetActive(false);
        LeaveRoomButton.SetActive(false);
    }

    //When Player decides to Join a Lobby (DEFUNCT)
    public void OnJoinLobbyOptionClicked()
    {
        Debug.Log("OnJoinLobbyButtonClicked was called");
        ////Player must be in Lobby to view rooms
        //if(!PhotonNetwork.InLobby)
        //{
        //    Debug.Log("Joining Lobby Services");
        //    PhotonNetwork.JoinLobby();
        //    GameOptionsPanel.Hide();
        //    JoinRoomListingPanels.Show();
        //}

    }

    //When Player decides to Join a Lobby
    public void OnJoinLobbyOption()
    {
        //Player must be in Lobby to view rooms
        if (!PhotonNetwork.InLobby)
        {
            Debug.Log("Joining Lobby Services");
            PhotonNetwork.JoinLobby();
            GameOptionsPanel.Hide();
            JoinRoomListingPanels.Show();
        }

    }

    //Remove Player from Lobby Networking after pressing back button
    public void OnBackButtonClicked()
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
            Debug.Log("Removing Player from Lobby Finder");
            //Show appropriate Panels
            GameOptionsPanel.Show();
            JoinRoomListingPanels.Hide();
        }

    }

    //When Player leaves room
    public void OnLeaveRoomButtonClicked()
    {
        PhotonNetwork.LeaveRoom();
        GameOptionsPanel.Show();
        CreateRoomPanel.Hide();
    }

    public void OnStartButtonClicked()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Trying to load new Scene");
            //Use Photon to load new Scene
            PhotonNetwork.LoadLevel("MultiplayerGameTest");

            //Add Loading Screen Here
        }

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

    //Callback function for Debug.Log when Creating Room
    public override void OnCreatedRoom()
    {
        Debug.Log(PhotonNetwork.CurrentRoom.Name + " is created");

        //turn off all input fields and Create Room Button Here
        RoomNameInputfield.gameObject.SetActive(false);
        MaxPlayerInputfield.gameObject.SetActive(false);
        CreateRoomButton.SetActive(false);
        //StartButton.SetActive(true);
        LeaveRoomButton.SetActive(true);
        
    }

    //Callback function for Debug.Log when Joining Room
    public override void OnJoinedRoom()
    {
        //Debug.Log for testing
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + " joined to " + PhotonNetwork.CurrentRoom.Name);

        RoomNameInputfield.gameObject.SetActive(false);
        MaxPlayerInputfield.gameObject.SetActive(false);
        CreateRoomButton.SetActive(false);
        //StartButton.SetActive(true);
        LeaveRoomButton.SetActive(true);

        //Display StartButton for Party Leader only
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            StartButton.SetActive(true);
        }
        else
        {
            StartButton.SetActive(false);
        }

        //Change Room Text to display proper info
        RoomInfoText.text = PhotonNetwork.CurrentRoom.Name + " \n"+
            "Players: " + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;

        //Update Dictionary
        if(PlayerListGameObjects == null)
        {
            PlayerListGameObjects = new Dictionary<int, GameObject>();
        }


        //Display PlayerNamePrefab
        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
        {

            GameObject playerListGameObject = Instantiate(PlayerListingPrefab, PlayerListingsPanel.transform);
            //playerListGameObject.transform.localScale = Vector3.one;

            //Specialize PlayerListings to display proper name and PlayerIndicator
            playerListGameObject.transform.Find("PlayerNameText").GetComponent<Text>().text = player.NickName;

            if(player.ActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
            {
                playerListGameObject.transform.Find("PlayerIndicator").gameObject.SetActive(false);
            }

            PlayerListGameObjects.Add(player.ActorNumber,playerListGameObject);
        }

    }


    //Update room lisitings
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

    //This function will most likely be used to update the Game Manager about what weapons each player is carrying
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        GameObject playerListGameObject = Instantiate(PlayerListingPrefab, PlayerListingsPanel.transform);
        //playerListGameObject.transform.localScale = Vector3.one;

        //Specialize PlayerListings to display proper name and PlayerIndicator
        playerListGameObject.transform.Find("PlayerNameText").GetComponent<Text>().text = newPlayer.NickName;

        if (newPlayer.ActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
        {
            playerListGameObject.transform.Find("PlayerIndicator").gameObject.SetActive(false);
        }

        PlayerListGameObjects.Add(newPlayer.ActorNumber, playerListGameObject);


        //Code for Gathering Player info should be here ( i think)
    }

    //When Player Leaves the room, destroy his/her PlayerListingPrefab
    //This function could also be used to remove player from GameManager after leaving a match
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        //Change Room Text to display proper info
        RoomInfoText.text = PhotonNetwork.CurrentRoom.Name + " \n" +
            "Players: " + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;

        //Destroy Player Listing
        Destroy(PlayerListGameObjects[otherPlayer.ActorNumber].gameObject);
        //Remove Player from Dictionary
        PlayerListGameObjects.Remove(otherPlayer.ActorNumber);

        //change ownership of Room to next player
        if(PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            StartButton.SetActive(true);
        }


    }

    //When "we" or the player that started the room leaves the room
    public override void OnLeftRoom()
    {
       //Display Game Options Panel


        //Throw the whole room away
        foreach(GameObject playerlistGameObject in PlayerListGameObjects.Values)
        {
            Destroy(playerlistGameObject);
        }
        PlayerListGameObjects.Clear();
        PlayerListGameObjects = null;

    }

    //Wipe room list to refresh after leaving Lobby Finder
    public override void OnLeftLobby()
    {
        ClearRoomListView();
        cachedRoomList.Clear();
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
        JoinRoomListingPanels.Hide();
        CreateRoomPanel.Show();
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
