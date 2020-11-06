using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scrambler : Player
{ 
    protected override void OnEnable()
    {
        UpdateHandler.UpdateOccurred += testerMethod;
        UpdateHandler.UpdateOccurred += Die;
        UpdateHandler.FixedUpdateOccurred += Move;
    }
    protected override void OnDisable()
    {
        UpdateHandler.UpdateOccurred -= testerMethod;
        UpdateHandler.UpdateOccurred -= Die;
        UpdateHandler.FixedUpdateOccurred -= Move;
    }
    protected void Revive(float reviveHP) {
        if (isDead) {
            affectedStats[(int)Stats.health] = 0 + reviveHP;
            isDead = false;
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0.5f); // temporary color shift
        }
    }
}
