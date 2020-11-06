using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadoutScript : MonoBehaviour
{
    public GameObject ArmorPanel;
    public GameObject WeaponsPanel;
    public GameObject OtherShitPanel;

    public void ShowArmor()
    {
        ArmorPanel.SetActive(true);
        WeaponsPanel.SetActive(false);
        OtherShitPanel.SetActive(false);
    }
    public void ShowWeapons()
    {
        ArmorPanel.SetActive(false);
        WeaponsPanel.SetActive(true);
        OtherShitPanel.SetActive(false);
    }

    public void ShowOtherShit()
    {
        ArmorPanel.SetActive(false);
        WeaponsPanel.SetActive(false);
        OtherShitPanel.SetActive(true);
    }
}
