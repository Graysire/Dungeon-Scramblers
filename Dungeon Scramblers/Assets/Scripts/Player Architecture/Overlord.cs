using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Overlord : Player
{
    protected override void Awake()
    {
        base.Awake();
        if (!usingOnScreenControls) {
            controls.PlayerMovement.UseAbilityQ.performed += ctx => UseAbilityQ(ctx.ReadValue<float>());
            controls.PlayerMovement.UseAbilityQ.canceled += ctx => UseAbilityQ(ctx.ReadValue<float>());
            controls.PlayerMovement.UseAbilityE.performed += ctx => UseAbilityE(ctx.ReadValue<float>());
            controls.PlayerMovement.UseAbilityE.canceled += ctx => UseAbilityE(ctx.ReadValue<float>());
        }
    }

    protected void UseAbilityQ(float f) {
        if (f < 1)
            Debug.Log("Stop ability Q");
        else if (f == 1)
            ApplyAttack(f, 2);
    }

    protected void UseAbilityE(float f) {
        if (f < 1)
            Debug.Log("Stop ability E");
        else if (f == 1)
            ApplyAttack(f, 3);
    }
}
