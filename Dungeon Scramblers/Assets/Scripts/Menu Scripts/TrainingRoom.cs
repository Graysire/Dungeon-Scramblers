using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TrainingRoom : MonoBehaviour
{
    public Categories.PlayerCategories playerCategories;
    //Loads the training room
    public void EnterTrainingRoom()
    {
        FindObjectOfType<InventoryHandler>().SetSelectedPlayer(playerCategories); //sets the player category to select category
        SceneManager.LoadScene(3);  //load single player scene
    }
}
