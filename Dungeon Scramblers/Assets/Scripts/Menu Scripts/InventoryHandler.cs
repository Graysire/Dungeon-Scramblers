using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryHandler : MonoBehaviour
{
    int playerBits;
    Categories.PlayerCategories playerCategory;

    //sets handlers player category
    public void SetSelectedPlayer(Categories.PlayerCategories playerCategory)
    {
        this.playerCategory = playerCategory;
    }

    //retrieves saved player bits
    public void RetrivePlayerBits()
    {
        playerBits = FindObjectOfType<MenuManager>().GetPlayerBits(playerCategory);
    }

    //returns playerbits saved from menumanager
    public int GetPlayerBits()
    {
        return playerBits;
    }
}
