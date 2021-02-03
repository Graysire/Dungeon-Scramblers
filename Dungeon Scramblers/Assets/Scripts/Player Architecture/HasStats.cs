using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class HasStats : MonoBehaviourPunCallbacks
{
    protected enum Stats
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
    [SerializeField] protected float[] stats = new float[] { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f };
    // Temporary stats - The stats that the player currently has through the game i.e. 130/200 health
    [SerializeField] protected float[] affectedStats = new float[] { 0f, 0f, 0f, 0f, 0f, 0f, 0f };
}
