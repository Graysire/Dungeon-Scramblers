using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class JessPlaceholderPanelSwitch : MonoBehaviour
{
    public GameObject TitleScreen;
    public GameObject LoadoutScreen;

    public void GoToTitleScreen() {
        LoadoutScreen.SetActive(false);
        TitleScreen.SetActive(true);
    }    
    public void GoToLoadoutScreen() {
        TitleScreen.SetActive(false);
        LoadoutScreen.SetActive(true);
    }
}
