﻿using System.Collections;
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

    // ON SCREEN CONTROLS ONLY
    [SerializeField] protected bool usingOnScreenControls; // Should only be true/false once; Unity current gives errors for using both
                                                           // but real application/build should only be using one or the other
    [SerializeField] protected GameObject PlayerOnScreenControls;
    protected List<GameObject> AllIndependentJoysticks; // Morning Jess, please figure out a way to replace specific onscreenstick with generic GOs, ty
    protected GameObject EnabledIndependentJoystick;
    //protected List<UnityEngine.InputSystem.OnScreen.OnScreenStick> AllIndependentJoystickFunctions;
   // protected UnityEngine.InputSystem.OnScreen.OnScreenStick EnabledIndependentJoystickFunction;
    // END ON SCREEN CONTROLS ONLY

    protected InputMaster controls;    
    [SerializeField] protected float[] stats = new float[] { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f };
    [SerializeField] protected float[] affectedStats = new float[] { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f };
    [SerializeField] protected bool isDead = false;
    // Movement Variables
    protected CharacterController controller;
    protected Vector3 direction = Vector3.zero;
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
            AllIndependentJoysticks = new List<GameObject>();
            //AllIndependentJoystickFunctions = new List<UnityEngine.InputSystem.OnScreen.OnScreenStick>();
            foreach (Transform t in PlayerOnScreenControls.transform) {
                if (t.tag == "IndependentJoystick") {
                    AllIndependentJoysticks.Add(t.gameObject);
                    //AllIndependentJoystickFunctions.Add(t.GetComponent<UnityEngine.InputSystem.OnScreen.OnScreenStick>());
                }
            }
            if (AllIndependentJoysticks.Count > 0) {
                AllIndependentJoysticks[0].GetComponentInChildren<Button>().gameObject.SetActive(false);
                EnabledIndependentJoystick = AllIndependentJoysticks[0];
                Debug.Log("Joystick Count: " + AllIndependentJoysticks.Count);
                for (int i = 1; i < AllIndependentJoysticks.Count; i++)
                    AllIndependentJoysticks[i].GetComponent<UnityEngine.InputSystem.OnScreen.OnScreenStick>().enabled = false;
            }else
                Debug.Log("No independent joysticks found");

            /* Multiple Joystick Reference: https://forum.unity.com/threads/create-two-virtual-joysticks-touch-with-the-new-input-system.853072/ */
            controls.PlayerMovement.Attack.performed += ctx => Attack(ctx.ReadValue<Vector2>());
            controls.PlayerMovement.Attack.canceled += ctx => Attack(ctx.ReadValue<Vector2>());
            //controls.PlayerMovement.UseAbility.performed += ctx => UseAbility(ctx.ReadValue<Vector2>());
            //controls.PlayerMovement.UseAbility.canceled += ctx => UseAbility(ctx.ReadValue<Vector2>());

        }
        else {
            controls.PlayerMovement.Attack.performed += ctx => Attack(ctx.ReadValue<float>());
            controls.PlayerMovement.UseAbility.performed += ctx => UseAbility(ctx.ReadValue<float>());
            controls.PlayerMovement.Attack.canceled += ctx => Attack(ctx.ReadValue<float>());
            controls.PlayerMovement.UseAbility.canceled += ctx => UseAbility(ctx.ReadValue<float>());
        }
            
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

    public virtual void SwitchEnabledIndependentJoystick() {
        GameObject thisButton = EventSystem.current.currentSelectedGameObject;
        UnityEngine.InputSystem.OnScreen.OnScreenStick requestedStick = thisButton.GetComponentInParent<UnityEngine.InputSystem.OnScreen.OnScreenStick>();
        if (!requestedStick.enabled) {
            UnityEngine.InputSystem.OnScreen.OnScreenStick tempSwitch = EnabledIndependentJoystick.GetComponent<UnityEngine.InputSystem.OnScreen.OnScreenStick>();
            thisButton.SetActive(false);
            tempSwitch.enabled = false;
            requestedStick.enabled = true;
            EnabledIndependentJoystick.GetComponentInChildren<Button>(true).gameObject.SetActive(true);
            EnabledIndependentJoystick = requestedStick.gameObject;
            Debug.Log("Switched joysticks");
        }
    }

    protected virtual void Attack(Vector2 d) {

        Debug.Log("Attack on Phone");
    }
    protected virtual void Attack(float f) {
        if (f < 1) {
            Debug.Log("Stop attacking");
        }
        if (f == 1) {
            Debug.Log("Start attacking");
        }
    }
    public virtual void UseAbility(Vector2 d)
    {
        Debug.Log("Ability used on phone");
    }
    protected virtual void UseAbility(float f) {
        if (f < 1)
        {
            Debug.Log("Stop ability");
        }
        if (f == 1)
        {
            Debug.Log("Start ability");
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
}


