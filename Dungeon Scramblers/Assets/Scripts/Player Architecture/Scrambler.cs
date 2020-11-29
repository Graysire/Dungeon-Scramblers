using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;
using UnityEngine.InputSystem;

public class Scrambler : Player
{
/*    protected override void Awake()
    {
        controls = new InputMaster();
        controls.PlayerMovement.Movement.performed += ctx => Move(ctx.ReadValue<Vector2>());
        controls.PlayerMovement.Movement.canceled += ctx => Move(ctx.ReadValue<Vector2>());
        if(usingOnScreenControls)
            controls.PlayerMovement.Attack.performed += ctx => Attack(ctx.ReadValue<Vector2>());
        else
            controls.PlayerMovement.Attack.performed += ctx => Attack();
        controls.PlayerMovement.UseActive.performed += _ => UseAbility();
        controller = GetComponent<CharacterController>();
        sr = GetComponent<SpriteRenderer>();

    }*/
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
    }
    protected override void Attack(Vector2 d) { // TOUCHSCREEN ATTACK INPUT
        
    }
}
