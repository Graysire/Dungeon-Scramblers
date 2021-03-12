using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BitCategories : MonoBehaviour
{
    public BitCategory playerType = BitCategory.playerType;
    public BitCategory weapon = BitCategory.weapon;
    public BitCategory armor = BitCategory.armor;
    public BitCategory ability1 = BitCategory.ability1;
    public BitCategory ability2 = BitCategory.ability2;

    public enum BitCategory
    {
        playerType = 1,
        weapon = 2,
        armor = 3,
        ability1 = 4,
        ability2 = 5
    }

}

