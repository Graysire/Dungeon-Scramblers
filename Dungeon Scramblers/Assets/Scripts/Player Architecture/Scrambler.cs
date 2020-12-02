﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;
using UnityEngine.InputSystem;

public class Scrambler : Player
{
    protected override void OnEnable()
    {
        controls.Enable();
        UpdateHandler.UpdateOccurred += Die;
        UpdateHandler.FixedUpdateOccurred += ApplyMove;
    }
    protected override void OnDisable()
    {
        controls.Disable();
        UpdateHandler.UpdateOccurred -= Die;
        UpdateHandler.FixedUpdateOccurred -= ApplyMove;
    }
    protected void Revive(float reviveHP) {
        if (isDead) {
            affectedStats[(int)Stats.health] = 0 + reviveHP;
            isDead = false;
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0.5f); // temporary color shift
        }
    }
    protected override void Attack(float f){ // MOUSE ATTACK INPUT
        if (f < 1)
        {
            Debug.Log("Scrambler Stop attacking");
        }
        if (f == 1)
        {
            Debug.Log("Scramber Start attacking");
        }
    }
    protected override void Attack(Vector2 d) { // TOUCHSCREEN ATTACK INPUT
        Debug.Log("Scrambler attack on phone"); 
        // Implement something directional here and above when you wake up Jess
        // Oh yea find a different joystick input for Ability too lmao
    }
}
