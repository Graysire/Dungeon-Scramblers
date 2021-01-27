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
        }
    }

    protected void UseAbilityQ(float f) { 
    }

    protected void UseAbilityE(float f) { 
    }
}
