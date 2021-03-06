﻿using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is attached to the Loadout Panel
/// it is used to save Player Character/ Loadout Selections
/// </summary>
public class LoadoutSelection : MonoBehaviourPunCallbacks
{
    public MenuManager Manager;
   

    //Called on button click to select Player Character
    public void SelectPlayer(int PlayerSelectionNumber)
    {
        //Create HashTable with our selection value
        ExitGames.Client.Photon.Hashtable playerSelectionProp = new ExitGames.Client.Photon.Hashtable() { { DungeonScramblersGame.PLAYER_SELECTION_NUMBER, PlayerSelectionNumber } };
        //Add Player Selection to hash table to save for later
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerSelectionProp);

        //GetPlayerLoadout
       int PlayerLoadout = Manager.GetPlayerBits(GetPlayerCategory(PlayerSelectionNumber));
        SetLoadout(PlayerLoadout);
      //  Debug.Log("Player info received");
    }

    //Funciton called when Player's Character Selection changes
    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        //Debug.Log("player: " + targetPlayer + " has changed: " + changedProps);

    }

    public void SetOverlord(int PlayerOverlord)
    {
        ExitGames.Client.Photon.Hashtable playerSelectionProp = new ExitGames.Client.Photon.Hashtable() { { DungeonScramblersGame.PLAYER_OVERLORD, PlayerOverlord } };
        //Add Player Selection to hash table to save for later
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerSelectionProp);
    }


    //Function for gathering PlayerLoadout info

    public void SetLoadout(int PlayerLoadout)
    {
        //Create HashTable with our selection value
        ExitGames.Client.Photon.Hashtable playerSelectionProp = new ExitGames.Client.Photon.Hashtable() { { DungeonScramblersGame.PLAYER_LOADOUT, PlayerLoadout } };
        Debug.Log("Player LoadoutSelection: " + PlayerLoadout);
    }


    public Categories.PlayerCategories GetPlayerCategory(int playerCat)
    {
        Categories.PlayerCategories playerCategory = (Categories.PlayerCategories)playerCat;

        Debug.Log("Player Category: " + playerCategory);
        return playerCategory;
    }
}
