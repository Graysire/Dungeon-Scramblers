using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerListEntryInitializer : MonoBehaviour
{
    [Header("UI References")]
    public Text PlayerNameText;
    public Button PlayerReadyButton;
    public Button PlayerReadyPopUp;
    public Button PlayerIndicator;

    private bool isPlayerReady = false;

    public void Initialize(int playerID, string PlayerName)
    {
        PlayerNameText.text = PlayerName;

        //Hide Player Ready button & Player Indicator
        if(PhotonNetwork.LocalPlayer.ActorNumber != playerID)
        {
            
            PlayerReadyButton.gameObject.SetActive(false);
            PlayerReadyPopUp.gameObject.SetActive(false);
            PlayerIndicator.gameObject.SetActive(false);
        }
        //This is our Player
        else
        {
            PlayerReadyPopUp.gameObject.SetActive(false);
            ExitGames.Client.Photon.Hashtable initialProps = new ExitGames.Client.Photon.Hashtable() { {DungeonScramblersGame.PLAYER_READY,isPlayerReady } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(initialProps);
            //Change Ready value when Ready Button is Pressed
            PlayerReadyButton.onClick.AddListener(() =>
            {
                //Debug.Log("Player Ready Clicked");
                //isPlayerReady = !isPlayerReady;
                //SetPlayerReady(isPlayerReady);

                ////Create new Entry to update hash table
                //ExitGames.Client.Photon.Hashtable newProps = new ExitGames.Client.Photon.Hashtable() { { DungeonScramblersGame.PLAYER_READY, isPlayerReady } };
                ////Update Hash table
                //PhotonNetwork.LocalPlayer.SetCustomProperties(newProps);
            });
        }
    }

    //Turn off Visual indicator for Player Ready Value
    public void SetPlayerReady(bool playerReady)
    {
        PlayerReadyPopUp.gameObject.SetActive(playerReady);

        if (playerReady)
        {
            PlayerReadyButton.GetComponentInChildren<Text>().text = "Ready!";
        }
        else
        {
            PlayerReadyButton.GetComponentInChildren<Text>().text = "Ready?";
        }
    }

    public void IsPlayerReady()
    {
        Debug.Log("Player Ready Clicked");
        isPlayerReady = !isPlayerReady;
        SetPlayerReady(isPlayerReady);

        //Create new Entry to update hash table
        ExitGames.Client.Photon.Hashtable newProps = new ExitGames.Client.Photon.Hashtable() { { DungeonScramblersGame.PLAYER_READY, isPlayerReady } };
        //Update Hash table
        PhotonNetwork.LocalPlayer.SetCustomProperties(newProps);
    }
}
