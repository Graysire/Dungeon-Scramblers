using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [Header("Login Ui Panel")]
    public InputField playerNameInput;
    #region Unity Methods

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #endregion


    #region UI Callbacks

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
        base.OnConnectedToMaster();
    }



    #endregion
}
