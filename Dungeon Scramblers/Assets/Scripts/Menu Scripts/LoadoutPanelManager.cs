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

    [SerializeField]
    private UIButton armorButton;
    [SerializeField]
    private UIButton weaponButton;
    [SerializeField]
    private UIButton primaryAbilityButton;
    [SerializeField]
    private UIButton secondaryAbilityButton;

    [SerializeField]
    private GameObject ArmorPanel;
    [SerializeField]
    private GameObject WeaponsPanel;
    [SerializeField]
    private GameObject PrimaryAbilitiesPanel;
    [SerializeField]
    private GameObject SecondaryAbilitiesPanel;

    public void ShowArmorPanel()
    {
        ArmorPanel.SetActive(true);
        WeaponsPanel.SetActive(false);
        PrimaryAbilitiesPanel.SetActive(false);
        SecondaryAbilitiesPanel.SetActive(false);
    }

    public void ShowWeaponPanel()
    {
        ArmorPanel.SetActive(false);
        WeaponsPanel.SetActive(true);
        PrimaryAbilitiesPanel.SetActive(false);
        SecondaryAbilitiesPanel.SetActive(false);
    }

    public void ShowPrimaryAbilitiesPanel()
    {
        ArmorPanel.SetActive(false);
        WeaponsPanel.SetActive(false);
        PrimaryAbilitiesPanel.SetActive(true);
        SecondaryAbilitiesPanel.SetActive(false);
    }

    public void ShowSecondaryAbilitiesPanel()
    {
        ArmorPanel.SetActive(false);
        WeaponsPanel.SetActive(false);
        PrimaryAbilitiesPanel.SetActive(false);
        SecondaryAbilitiesPanel.SetActive(true);
    }

}
