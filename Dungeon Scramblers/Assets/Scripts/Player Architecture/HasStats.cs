using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[System.Serializable]
public class HasStats : MonoBehaviourPunCallbacks
{
    
    public enum Stats
    {
        health = 0,
        movespeed = 1,
        attackdmg = 2,
        attackspeed = 3,
        abilitydmg = 4,
        abilitycd = 5,
        defense = 6
    }

    [Header("Stats")]
    // Permanent stats - What the player will reference to return to a "normal state" i.e. restoring to full health
    [SerializeField] protected int[] stats = new int[] { 0, 0, 0, 0, 0, 0, 0 };

    //Getter for stats
    public int[] GetStats()
    {
        return stats;
    }
}
