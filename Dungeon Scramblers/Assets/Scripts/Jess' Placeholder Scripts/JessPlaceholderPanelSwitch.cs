using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class JessPlaceholderPanelSwitch : MonoBehaviour
{
    public GameObject TitleScreen;
    public GameObject LoadoutScreen;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GoToTitleScreen() {
        LoadoutScreen.SetActive(false);
        TitleScreen.SetActive(true);
    }    
    public void GoToLoadoutScreen() {
        TitleScreen.SetActive(false);
        LoadoutScreen.SetActive(true);
    }
}
