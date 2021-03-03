using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Doozy.Engine.UI;

public class LoadoutPanelManager : MonoBehaviour
{
    //https://www.doozyui.com/learn/documentation/#CodeExamples
    //https://www.doozyui.com/code-uibutton/
    //This is attached to each Class Type Loadout View and it references the buttons 
    //that show the panels for each equippable

    public UIButton armorButton;
    public UIButton weaponButton;
    public UIButton primaryAbilityButton;
    public UIButton secondaryAbilityButton;

    public GameObject ArmorPanel;
    public GameObject WeaponsPanel;
    public GameObject PrimaryAbilitiesPanel;
    public GameObject SecondaryAbilitiesPanel;

    private int flag = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (armorButton.IsSelected && flag != 1)
        {
            flag = 1;

            ArmorPanel.SetActive(true);
            WeaponsPanel.SetActive(false);
            PrimaryAbilitiesPanel.SetActive(false);
            SecondaryAbilitiesPanel.SetActive(false);

        }
        if (weaponButton.IsSelected && flag != 2)
        {
            flag = 2;

            ArmorPanel.SetActive(false);
            WeaponsPanel.SetActive(true);
            PrimaryAbilitiesPanel.SetActive(false);
            SecondaryAbilitiesPanel.SetActive(false);

        }
        if (primaryAbilityButton.IsSelected && flag != 3)
        {
            flag = 3;

            ArmorPanel.SetActive(false);
            WeaponsPanel.SetActive(false);
            PrimaryAbilitiesPanel.SetActive(true);
            SecondaryAbilitiesPanel.SetActive(false);

        }
        if (secondaryAbilityButton.IsSelected && flag != 4)
        {
            flag = 4;

            ArmorPanel.SetActive(false);
            WeaponsPanel.SetActive(false);
            PrimaryAbilitiesPanel.SetActive(false);
            SecondaryAbilitiesPanel.SetActive(true);

        }
    }
}
