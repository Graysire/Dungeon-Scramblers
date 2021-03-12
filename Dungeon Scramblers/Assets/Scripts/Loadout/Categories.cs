using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Categories : MonoBehaviour
{
    public enum PlayerCategories
    {
        mage = 1,
        knight = 2,
        rogue = 3,
        overlord = 4,
    }

    public enum BitCategory
    {
        playerType = 1,
        weapon = 2,
        armor = 3,
        ability1 = 4,
        ability2 = 5
    }
}
