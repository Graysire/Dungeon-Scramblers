using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;
using UnityEngine.InputSystem;

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
    protected InputMaster controls;
    [SerializeField] protected float[] stats = new float[] { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f };
    [SerializeField] protected float[] affectedStats = new float[] { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f };
    [SerializeField] protected bool isDead = false;
    // Movement Variables
    protected CharacterController controller;
    protected Vector3 direction = Vector3.zero;
    // Temporary Variables -- will be replaced by official art
    protected SpriteRenderer sr;

    protected virtual void Awake()
    {
        controls = new InputMaster();
        controls.PlayerMovement.Movement.performed += ctx => Move(ctx.ReadValue<Vector2>());
        controls.PlayerMovement.Movement.canceled += ctx => Move(ctx.ReadValue<Vector2>());
        controls.PlayerMovement.Attack.performed += _ => Attack();
        controls.PlayerMovement.UseActive.performed += _ => UseAbility();
        controller = GetComponent<CharacterController>();
        sr = GetComponent<SpriteRenderer>();
    }
    protected virtual void OnEnable()
    {
        controls.Enable();
        UpdateHandler.UpdateOccurred += Die;
        UpdateHandler.FixedUpdateOccurred += ApplyMove;
    }
    protected virtual void OnDisable()
    {
        controls.Disable();
        UpdateHandler.UpdateOccurred -= Die;
        UpdateHandler.FixedUpdateOccurred -= ApplyMove;
    }

    protected virtual void Move(Vector2 d) {
        direction = new Vector3(d.x, d.y,0);
        direction = transform.TransformDirection(direction);
        direction *= stats[(int)Stats.movespeed];
    }

    protected virtual void ApplyMove() {
        controller.Move(direction * Time.deltaTime);
    }

    protected virtual void Attack() {
        Debug.Log("Attack");
    }

    protected virtual void UseAbility() {
        Debug.Log("Ability Used");
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
