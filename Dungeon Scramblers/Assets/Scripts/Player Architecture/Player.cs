using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
    // Placeholder for eventual Attack & Ability Equipables 
    [SerializeField] protected List<DefaultAttackSequence> AttackList;
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
    // Temporary Variables -- will be replaced by official art
    protected SpriteRenderer sr;
    // Art variables
    [SerializeField] protected Sprite[] charactersheet;
    // Down, right, up, left
    protected bool[] facingCheckers = new bool[] { true, false, false, false};
    protected int trueFaceIndex = 0;

    protected virtual void Awake()
    {
        // UNCOMMENT THE SECTION BELOW FOR THE REAL BUILD
        /*if (SystemInfo.deviceType == DeviceType.Handheld)
                    usingOnScreenControls = true;
                else
                    usingOnScreenControls = false;*/
        if (usingOnScreenControls && PlayerOnScreenControls != null) {
            PlayerOnScreenControls.SetActive(true);
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
        allowedToAttack = true;
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
        if (d.y < 0 && d.x == 0 && !facingCheckers[0])
            SwitchSpritesOnMove(0);
        else if (d.y > 0 && d.x == 0 && !facingCheckers[2])
            SwitchSpritesOnMove(2);
        else if (d.x > 0 && !facingCheckers[1])
            SwitchSpritesOnMove(1);
        else if (d.x < 0 && !facingCheckers[3])
            SwitchSpritesOnMove(3);
        direction = new Vector3(d.x, d.y,0);
        direction = transform.TransformDirection(direction);
        direction *= stats[(int)Stats.movespeed];
    }
    protected virtual void SwitchSpritesOnMove(int ind) {
        facingCheckers[ind] = true;
        facingCheckers[trueFaceIndex] = false;
        trueFaceIndex = ind;
        sr.sprite = charactersheet[ind];
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
        if (f < 1) {
            Debug.Log("Stop attacking");
        }
        if (f == 1) {
            Vector3 MouseWorldCoord = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            AttackDirection = new Vector3(MouseWorldCoord.x - transform.position.x, MouseWorldCoord.y - transform.position.y, 0);
            RequestAttack(0);
            Debug.Log("Start attacking");
        }
    }
    protected virtual void UseAbility(float f) { // Ability cast using mouse, different way to implement?
        if (f < 1)
        {
            Debug.Log("Stop ability");
        }
        if (f == 1)
        {
            Vector3 MouseWorldCoord = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            AttackDirection = new Vector3(MouseWorldCoord.x - transform.position.x, MouseWorldCoord.y - transform.position.y, 0);
            RequestAttack(1);
            // Use ability from list
            Debug.Log("Start attacking");
        }
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
}


