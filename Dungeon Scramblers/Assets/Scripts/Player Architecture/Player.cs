using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Player : MonoBehaviour, IDamageable<float>
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
    // Placeholder for eventual Attack & Ability Equipables 
    [SerializeField] protected List<GameObject> AttackObjectList;
    protected List<DefaultAttackSequence> AttackList;
    // ON SCREEN CONTROLS ONLY
    [SerializeField] protected bool usingOnScreenControls; // Should only be true/false once; Unity current gives errors for using both
                                                           // but real application/build should only be using one or the other
    [SerializeField] protected GameObject PlayerOnScreenControls;
    protected int activeIndependentJoystick = -1;
    // END ON SCREEN CONTROLS ONLY

    protected InputMaster controls;    
    [SerializeField] protected float[] stats = new float[] { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f };
    [SerializeField] protected float[] affectedStats = new float[] { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f };
    [SerializeField] protected bool isDead = false;
    // Movement Variables
    protected CharacterController controller;
    protected Vector3 direction = Vector3.zero;
    // Attack Variables
    protected Vector3 AttackDirection;
    protected bool allowedToAttack;
    // Art Variables
    protected SpriteRenderer sr;
    protected Animator animator;
    protected bool isFacingLeft;

    protected virtual void Awake()
    {
        // UNCOMMENT THE SECTION BELOW FOR THE REAL BUILD
        /*if (SystemInfo.deviceType == DeviceType.Handheld)
                    usingOnScreenControls = true;
                else
                    usingOnScreenControls = false;*/

        // Instantiate attack sequences to reattach the instance to the player
        for(int i = 0; i < AttackObjectList.Count; i++)
        {
            AttackObjectList[i] = Instantiate(AttackObjectList[i], gameObject.transform);
        }
        

        AttackList = new List<DefaultAttackSequence>();
        for (int i = 0; i < AttackObjectList.Count; i++) {
            AttackList.Add(AttackObjectList[i].GetComponent<DefaultAttackSequence>());
        }
        // Make sure on-screen controls are on/off based on what they should be
        if (usingOnScreenControls && PlayerOnScreenControls != null)
        {
            if (!PlayerOnScreenControls.activeSelf)
                PlayerOnScreenControls.SetActive(true);
        }
        else if (!usingOnScreenControls && PlayerOnScreenControls != null) {
            if (PlayerOnScreenControls.activeSelf)
                PlayerOnScreenControls.SetActive(false);
        }   
           
        controls = new InputMaster();
        controls.PlayerMovement.Movement.performed += ctx => Move(ctx.ReadValue<Vector2>());
        controls.PlayerMovement.Movement.canceled += ctx => Move(ctx.ReadValue<Vector2>());
        if (usingOnScreenControls) {
            /* Multiple Joystick Reference: https://forum.unity.com/threads/create-two-virtual-joysticks-touch-with-the-new-input-system.853072/ */
            controls.PlayerMovement.Attack.performed += ctx => Attack(ctx.ReadValue<Vector2>()); // Will need to now fire either attack or ability based on the joystick moved
            controls.PlayerMovement.Attack.canceled += ctx => Attack(ctx.ReadValue<Vector2>());
        }
        else {
            controls.PlayerMovement.Attack.performed += ctx => Attack(ctx.ReadValue<float>());
            controls.PlayerMovement.UseAbility.performed += ctx => UseAbility(ctx.ReadValue<float>());
            controls.PlayerMovement.Attack.canceled += ctx => Attack(ctx.ReadValue<float>());
            controls.PlayerMovement.UseAbility.canceled += ctx => UseAbility(ctx.ReadValue<float>());
        }
            
        controller = GetComponent<CharacterController>();
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        allowedToAttack = true;
        isFacingLeft = false;

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
        // Animation changes
        // Not moving
        if (d.x == 0 && d.y == 0 && !animator.GetBool("idle"))
        {
            if (animator.GetBool("facingLeft")) {
                animator.SetBool("facingLeft", false);
                animator.SetBool("facingFront", true);
            }
            animator.SetBool("idle", true);
        }
        else {
            animator.SetBool("idle", false);
            // Moving Down (front-facing)
            if (d.y < 0 && d.x == 0 && !animator.GetBool("facingFront"))
            {
                animator.SetBool("facingFront", true);
                animator.SetBool("facingBack", false);
                animator.SetBool("facingLeft", false);
            }
            // Moving Up (back-facing)
            else if (d.y > 0 && d.x == 0 && !animator.GetBool("facingBack"))
            {
                animator.SetBool("facingFront", false);
                animator.SetBool("facingBack", true);
                animator.SetBool("facingLeft", false);
            }
            // Moving Sideways & diagonally (side-facing)
            else if (d.x != 0 )
            {
                if (!animator.GetBool("facingLeft")) {
                    animator.SetBool("facingFront", false);
                    animator.SetBool("facingBack", false);
                    animator.SetBool("facingLeft", true);
                }
                if (d.x < 0 && sr.flipX)
                    sr.flipX = false;
                else if (d.x > 0 && !sr.flipX)
                    sr.flipX = true;
            }
        }
        // Actual movement
        direction = new Vector3(d.x, d.y, 0);
        direction = transform.TransformDirection(direction);
        direction *= stats[(int)Stats.movespeed];
    }
    protected virtual void ApplyMove() {
        controller.Move(direction * Time.deltaTime);
    }

    // For On-screen stick usage only
    public virtual void SetActiveIndependentJoystick(int j) {
        activeIndependentJoystick = j;
    }

    protected virtual void Attack(Vector2 d) {
        // Decide what we're attacking with (i.e. attack vs ability)
        // Joystick.getJoystickNumber
        if (activeIndependentJoystick < 0 || activeIndependentJoystick >= AttackList.Count)
        {
            Debug.Log("Invalid Joystick number.");
            return;
        }
        if (d.x == 0 && d.y == 0) { // Stop Attacking if joystick is at <0, 0>
            Debug.Log("End Attack " + activeIndependentJoystick);
            activeIndependentJoystick = -1;
            return;
        }
        AttackDirection = new Vector3(-d.x, d.y, 0);
        RequestAttack(activeIndependentJoystick);
        // Call attack based on joystick number
        Debug.Log("Attack " + activeIndependentJoystick + " on phone");
    }

    protected virtual void Attack(float f) { // Basic attack using mouse
        if (f < 1)
            Debug.Log("Stop ability");
        else if (f == 1)
            ApplyAttack(f, 0);
    }
    protected virtual void UseAbility(float f) { // Ability cast using mouse, different way to implement?
        if (f < 1)
            Debug.Log("Stop ability");
        else if (f == 1)
            ApplyAttack(f, 1);
    }
    protected virtual void ApplyAttack(float f, int abilityIndex) {
        Vector3 MouseWorldCoord = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        AttackDirection = new Vector3(MouseWorldCoord.x - transform.position.x, MouseWorldCoord.y - transform.position.y, 0);
        RequestAttack(abilityIndex);
        // Use ability from list
        Debug.Log("Start attacking");
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
    protected virtual void RequestAttack(int attackListIndex) {
        if (allowedToAttack)
            if(attackListIndex >= 0 && attackListIndex < AttackList.Count)
                AttackList[attackListIndex].StartAttack(GetAttackDirection(), this);
    }
    public Vector3 GetAttackDirection() => AttackDirection;
    public void SetAllowedToAttack(bool tf) => allowedToAttack = tf;
    public void Damage(float damageTaken) => affectedStats[(int)Stats.health] -= damageTaken;
}


