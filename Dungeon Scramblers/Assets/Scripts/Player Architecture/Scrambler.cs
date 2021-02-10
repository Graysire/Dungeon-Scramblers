using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;
using UnityEngine.InputSystem;

public class Scrambler : Player
{

    protected void Revive(float reviveHP) {
        if (isDead) {
            affectedStats[(int)Stats.health] = 0 + reviveHP;
            isDead = false;
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0.5f); // temporary color shift
        }
    }

    public void LevelUp()
    {
        // Change base stats of heatlh
        stats[0] += stats[0] * 0.1f;
        Debug.Log("Health: " + stats[0]);
        // Change base stats of attack damage
        stats[2] += stats[2] * 0.1f;
        Debug.Log("Attack Damage: " + stats[2]);
        // Change base stats of attack speed
        stats[3] += stats[3] * 0.1f;
        Debug.Log("Attack Speed: " + stats[3]);
        // Change base stats of ability damage
        stats[4] += stats[4] * 0.1f;
        Debug.Log("Ability Damage: " + stats[4]);

        // Refresh the affected stats
        affectedStats[0] += stats[0] * 0.1f;
        // refresh the affected stats upon level up
        affectedStats[2] = stats[2];
        affectedStats[3] = stats[3];
        affectedStats[4] = stats[4];
    }
}
