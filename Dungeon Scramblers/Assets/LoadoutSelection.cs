using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is attached to the Loadout Panel
/// it is used to save Player Loadout Selections
/// </summary>
public class LoadoutSelection : MonoBehaviourPunCallbacks
{ 

    public void SelectPlayer(int PlayerSelectionNumber)
    {
        ExitGames.Client.Photon.Hashtable playerSelectionProp = new ExitGames.Client.Photon.Hashtable() { {DungeonScramblersGame.PLAYER_SELECTION_NUMBER, PlayerSelectionNumber } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerSelectionProp);
        //playerSelectionProp.Bro
        Debug.Log("Player info received");
    }

    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        Debug.Log("player: " + targetPlayer + " has changed: " + changedProps);
        
    }
}
