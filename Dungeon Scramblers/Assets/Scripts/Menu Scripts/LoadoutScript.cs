using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadoutScript : MonoBehaviour
{
    public GameObject ArmorPanel;
    public GameObject WeaponsPanel;
    public GameObject PrimaryAbilitiesPanel;
    public GameObject SecondaryAbilitiesPanel;


    public void ShowArmor()
    {
        ArmorPanel.SetActive(true);
        WeaponsPanel.SetActive(false);
        PrimaryAbilitiesPanel.SetActive(false);
        SecondaryAbilitiesPanel.SetActive(false);
    }

    public void ShowWeapons()
    {
        ArmorPanel.SetActive(false);
        WeaponsPanel.SetActive(true);
        PrimaryAbilitiesPanel.SetActive(false);
        SecondaryAbilitiesPanel.SetActive(false);
    }

    public void ShowPrimaryAbilities()
    {
        ArmorPanel.SetActive(false);
        WeaponsPanel.SetActive(false);
        PrimaryAbilitiesPanel.SetActive(true);
        SecondaryAbilitiesPanel.SetActive(false);
    }

    public void ShowSecondaryAbilities()
    {
        ArmorPanel.SetActive(false);
        WeaponsPanel.SetActive(false);
        PrimaryAbilitiesPanel.SetActive(false);
        SecondaryAbilitiesPanel.SetActive(true);
    }
}