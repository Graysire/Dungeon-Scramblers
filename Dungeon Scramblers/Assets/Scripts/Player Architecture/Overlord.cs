using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Overlord : Player
{
    protected override void Awake()
    {
        base.Awake();
        if (!usingOnScreenControls) {
            controls.PlayerMovement.UseAbilityQ.performed += ctx => Attack(ctx.ReadValue<float>(), 2);
            controls.PlayerMovement.UseAbilityQ.canceled += ctx => Attack(ctx.ReadValue<float>(), 2);
            controls.PlayerMovement.UseAbilityE.performed += ctx => Attack(ctx.ReadValue<float>(), 3);
            controls.PlayerMovement.UseAbilityE.canceled += ctx => Attack(ctx.ReadValue<float>(), 3);
        }
    }

}
