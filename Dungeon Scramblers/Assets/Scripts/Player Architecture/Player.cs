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
        abilitycd = 5,
        defense = 6
    }

    [SerializeField] protected float[] stats = new float[] { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f };
    [SerializeField] protected float[] affectedStats = new float[] { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f };
    [SerializeField] protected bool isDead = false;
    // Movement Variables
    protected CharacterController controller;
    protected Vector3 direction = Vector3.zero;
    protected Vector2 inputDirection = Vector2.zero;
    // Temporary Variables -- will be replaced by official art
    protected SpriteRenderer sr;

    protected virtual void Awake()
    {
        controller = GetComponent<CharacterController>();
        sr = GetComponent<SpriteRenderer>();
    }
    protected virtual void OnEnable()
    {
        UpdateHandler.UpdateOccurred += testerMethod;
        UpdateHandler.UpdateOccurred += Die;
        UpdateHandler.FixedUpdateOccurred += Move;
        UpdateHandler.UpdateOccurred += Attack;
    }
    protected virtual void OnDisable()
    {
        UpdateHandler.UpdateOccurred -= testerMethod;
        UpdateHandler.UpdateOccurred -= Die;
        UpdateHandler.FixedUpdateOccurred -= Move;
        UpdateHandler.UpdateOccurred -= Attack;
    }

    protected void testerMethod() {
        Debug.Log("Health = " + (int)Stats.health);
        Debug.Log("Speed = " + Stats.movespeed.ToString());
    }

    public void OnMove(CallbackContext context){
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
    public virtual void OnAttack(CallbackContext context) {
        if (context.performed)
            Attack();
    }
    protected virtual void Attack() {
        Debug.Log("Attack");
    }

    protected virtual void UseAbility() { 
        // BUTTONS TO USE ABILITY
    }

    protected virtual void Die() {
        if (affectedStats[(int)Stats.health] <= 0 || isDead == true) {
            Debug.Log("Do something that indicates the player is dead...");
            isDead = true;
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0.5f); // temporary color shift
            Debug.Log("Dead");
            // Death animation
        }
    } 
}
