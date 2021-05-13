using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryHandler : MonoBehaviour
{
    [Header("Player Items Mage")]
    public GameObject[] PlayerItemsMage;
    [Header("Player Items Knight")]
    public GameObject[] PlayerItemsKnight;
    [Header("Player Items Rogue")]
    public GameObject[] PlayerItemsRogue;
    [Header("Player Items Overlord")]
    public GameObject[] PlayerItemsOverlord;
    int playerBits;
    Categories.PlayerCategories playerCategory;

    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    //sets handlers player category
    public void SetSelectedPlayer(Categories.PlayerCategories playerCategory)
    {
        this.playerCategory = playerCategory;
        RetrivePlayerBits();    //Updates the player bit information for selected player type
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
    public Categories.PlayerCategories GetPlayerCategory()
    {
        return playerCategory;
    }

    public void SetLoadout(Player player, int index)
    {

        //player.SetAttackObjectsList(index, Attack);
        Debug.Log("Attack Updated: " + player.GetAttackObjectsList());
    }

    void GetLoadoutAtIndex(int index)
    {

    }

}
