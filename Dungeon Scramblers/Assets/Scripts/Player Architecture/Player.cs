using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public class Player : MonoBehaviour
{
    protected enum Stats { 
        health = 0,
        movespeed = 1,
        attackdmg = 2,
        attackspeed = 3,
        abilitydmg = 4,
        defense = 5
    }

    [SerializeField] protected float[] stats = new float[] { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f };
    protected bool isDead = false;
    // Movement Variables
    protected CharacterController controller;
    protected Vector3 direction = Vector3.zero;
    protected Vector2 inputDirection = Vector2.zero;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }
    private void OnEnable()
    {
        UpdateHandler.UpdateOccurred += testerMethod;
        UpdateHandler.FixedUpdateOccurred += Move;
    }    
    private void OnDisable()
    {
        UpdateHandler.UpdateOccurred -= testerMethod;
        UpdateHandler.FixedUpdateOccurred -= Move;
    }

    private void testerMethod() {
        Debug.Log("Health = " + (int)Stats.health);
        Debug.Log("Speed = " + Stats.movespeed.ToString());
    }

    public virtual void OnMove(CallbackContext context){
        SetInputVector(context.ReadValue<Vector2>());
    }

    protected virtual void SetInputVector(Vector2 dir) {
        inputDirection = dir;
    }
    protected virtual void Move() {
        direction = new Vector3(inputDirection.x, inputDirection.y,0);
        direction = transform.TransformDirection(direction);
        direction *= stats[(int)Stats.movespeed];
        controller.Move(direction * Time.deltaTime);
    }

    protected virtual void Attack() { 
        // BUTTONS TO ATTACK
    }

    protected virtual void UseAbility() { 
        // BUTTONS TO USE ABILITY
    }

    protected virtual void Die() {
        if (stats[(int)Stats.health] <= 0) {
            Debug.Log("Do something that indicates the player is dead...");
            isDead = true;
            // Death animation
        }
    }
    protected virtual void Vote() { 
        // was this supposed to be a thing?
    }
}
