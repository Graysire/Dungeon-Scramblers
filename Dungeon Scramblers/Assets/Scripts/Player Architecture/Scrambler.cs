using System.Collections;
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
/*    protected override void Attack(Vector2 d) { // TOUCHSCREEN ATTACK INPUT
        Debug.Log("Scrambler attack on phone"); 
        // 
    }*/
/*    public override void UseAbility(Vector2 d)
    {
        Debug.Log("Scrambler ability used on phone");
    }*/

    protected override void UseAbility(float f)
    {
        if (f < 1)
        {
            Debug.Log("Stop ability");
        }
        if (f == 1)
        {
            Debug.Log("Start ability");
        }
    }
}
