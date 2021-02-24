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
    // Permanent stats - What the player will reference to return to a "normal state" i.e. restoring to full health
    [SerializeField] protected float[] stats = new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };

    //Getter for stats
    public float[] GetStats()
    {
        return stats;
    }
}
