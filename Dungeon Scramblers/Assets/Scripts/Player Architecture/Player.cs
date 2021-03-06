using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Photon.Pun;

public class Player : AbstractPlayer
{
    // Placeholder for eventual Attack & Ability Equipables 
    [SerializeField] protected List<GameObject> AttackObjectList;
    protected List<Ability> AttackList;
    // ON SCREEN CONTROLS ONLY
    [SerializeField] protected bool usingOnScreenControls; // Should only be true/false once; Unity current gives errors for using both
                                                           // but real application/build should only be using one or the other
    [SerializeField] protected GameObject PlayerOnScreenControls;
    protected int activeIndependentJoystick = -1;
    // END ON SCREEN CONTROLS ONLY

    protected InputMaster controls;    
    [SerializeField] protected bool isDead = false;
    //Netowkring Variables
    protected Vector2 networkPosition;  //Network position for lag Compensation
    protected Quaternion networkRotation;
    // Movement Variables
    protected Vector2 movement;
    protected Vector2 direction = Vector2.zero;
    protected Rigidbody2D rb;
    // Attack Variables
    protected Vector3 AttackDirection;

    // Art Variables
    protected SpriteRenderer sr;
    [SerializeField][Range(0,2)] protected float playerScale;
    [SerializeField] protected List<GameObject> AnimatorList;
    protected int enabledAnimatorInd = -1;
    protected Animator enabledAnim;

    // Scrambler: [Footstep R, Footstep L, Hurt, Die]
    [SerializeField] protected List<AudioClip> SFXList;
    protected float stepPacer = 0.0f;                               // Prevents footsteps from being spammed 
    [SerializeField] [Range(0, 2)] protected float stepInterval;    // Sets how frequently steps should be played
    protected bool onLeft = false;                                  // Checks for Right and Left footstep variation
    protected float hurtPacer = 0.0f;                               // Prevents hurt sfx from being spammed
    [SerializeField] [Range(0, 2)] protected float hurtInterval;    // Sets how frequently hurt sfx should be played (assuming hurt hurt multiple times in a row)


    //Dodge Rolling/ Chargin Mechanics
    protected bool bDashing = false;
    private bool bTargetable = true;
    public bool BTargetable { get => bTargetable; set => bTargetable = value; }

    protected override void Awake()
    {
        base.Awake();
        // UNCOMMENT THE SECTION BELOW FOR THE REAL BUILD
        /*if (SystemInfo.deviceType == DeviceType.Handheld)
                    usingOnScreenControls = true;
                else
                    usingOnScreenControls = false;*/

        //Load Attackobjects
        LoadAttackObjects();
        
        // Pool the objects needed for the attack sequence
        AttackList = new List<Ability>();
        for (int i = 0; i < AttackObjectList.Count; i++) {
            //AttackList.Add(AttackObjectList[i].GetComponent<DefaultAttackSequence>());
            AttackObjectList[i].GetComponent<Ability>().Equip(this);
        }
        // Make sure on-screen controls are on/off based on what they should be
        if (PlayerOnScreenControls == null)
        {
            //if (gameObject.layer == 10)
            //{ // If the gameobject layer is a Scrambler 
                PlayerOnScreenControls = GameObject.Find("Player On-Screen Controls");
            //}
        }
        if (usingOnScreenControls && PlayerOnScreenControls != null)
        {
            if (!PlayerOnScreenControls.activeSelf)
                PlayerOnScreenControls.SetActive(true);
        }
        else if (!usingOnScreenControls && PlayerOnScreenControls != null)
        {
            if (PlayerOnScreenControls.activeSelf)
                PlayerOnScreenControls.SetActive(false);
        }
        
           
        controls = new InputMaster();
        controls.PlayerMovement.Movement.performed += ctx => Move(ctx.ReadValue<Vector2>());
        controls.PlayerMovement.Movement.canceled += ctx => Move(ctx.ReadValue<Vector2>());
        if (usingOnScreenControls)
        {
            /* Multiple Joystick Reference: https://forum.unity.com/threads/create-two-virtual-joysticks-touch-with-the-new-input-system.853072/ */
            controls.PlayerMovement.Attack.performed += ctx => Attack(ctx.ReadValue<Vector2>()); // Will need to now fire either attack or ability based on the joystick moved
            controls.PlayerMovement.Attack.canceled += ctx => Attack(ctx.ReadValue<Vector2>());
        }
        else
        {
            controls.PlayerMovement.Attack.performed += ctx => Attack(ctx.ReadValue<float>(), 0);
            controls.PlayerMovement.UseAbility.performed += ctx => Attack(ctx.ReadValue<float>(), 1);
            controls.PlayerMovement.Attack.canceled += ctx => Attack(ctx.ReadValue<float>(), 0);
            controls.PlayerMovement.UseAbility.canceled += ctx => Attack(ctx.ReadValue<float>(), 1);
        }

        //Art Variables
        sr = GetComponent<SpriteRenderer>();
        for (int i = 0; i < AnimatorList.Count; i++)
            AnimatorList[i].transform.localScale = Vector3.zero;
        AnimatorList[0].transform.localScale = new Vector3(playerScale, playerScale, playerScale);
        enabledAnimatorInd = 0;
        enabledAnim = AnimatorList[enabledAnimatorInd].GetComponent<Animator>();


        allowedToAttack = true;
        rb = GetComponent<Rigidbody2D>();
    }
    // Update and Fixed Update handlers (event & delegate)
    protected virtual void OnEnable()
    {
        base.OnEnable();
        controls.Enable();
        //UpdateHandler.UpdateOccurred += Die;
        //UpdateHandler.UpdateOccurred += HealthCheck;
        UpdateHandler.FixedUpdateOccurred += ApplyMove;
        UpdateHandler.FixedUpdateOccurred += PhotonPhysicsUpdate;
    }
    protected virtual void OnDisable()
    {
        base.OnDisable();
        controls.Disable();
        //UpdateHandler.UpdateOccurred -= Die;
        //UpdateHandler.UpdateOccurred -= HealthCheck;
        UpdateHandler.FixedUpdateOccurred -= ApplyMove;
        UpdateHandler.FixedUpdateOccurred -= PhotonPhysicsUpdate;
    }


    protected virtual void HealthCheck()
    {
        if (affectedStats[(int)Stats.health] > stats[(int)Stats.health]) {
            affectedStats[(int)Stats.health] = stats[(int)Stats.health];
        } 
    }

    public override void Damage(int damageTaken) {
        base.Damage(damageTaken);
        // SFX hurt
        if (SFXList.Count >= 3)
        {
            if (Time.time - hurtPacer >= hurtInterval)
            {
                AudioManager.PlaySFX(SFXList[2], 1f);
                hurtPacer = Time.time;
            }
        }
        HealthCheck();
        Die();
    }

    #region Pun/Unity Callbacks
    protected void PhotonPhysicsUpdate() {
/*        UNCOMMENT IN BUILD WITH NETWORKING
 *        if (!photonView.IsMine)
        {
            rb.position = Vector3.MoveTowards(rb.position, networkPosition, Time.fixedDeltaTime);
            //rb.rotation = Quaternion.RotateTowards(rb., networkRotation, Time.fixedDeltaTime * 100.0f);
        }*/
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(this.rb.position);
            stream.SendNext(this.rb.rotation);
            stream.SendNext(this.rb.velocity);
        }
        else
        {
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
            rb.velocity = (Vector3)stream.ReceiveNext();

            float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.timestamp));
            networkPosition += (this.rb.velocity * lag);
        }
    }
    #endregion
    protected virtual void Move(Vector2 d) {
        // Animation changes
        // Not moving
        // Front [0], Back [1], Side [2]
        if (d.x == 0 && d.y == 0)
        {
            if(!enabledAnim.GetBool("idle"))
                enabledAnim.SetBool("idle", true);
        }
        else {
            //enabledAnim.SetBool("idle", true);
            // Moving Down (front-facing)
            if (d.y < 0 && d.x == 0 && enabledAnimatorInd != 0)
            {
                SwitchAnimatorGO(0);
            }
            // Moving Up (back-facing)
            else if (d.y > 0 && d.x == 0 && enabledAnimatorInd != 1)
            {
                SwitchAnimatorGO(1);
            }
            // Moving Sideways & diagonally (side-facing)
            else if (d.x != 0 )
            {
                if (enabledAnimatorInd != 2) 
                    SwitchAnimatorGO(2);
                // If needs to face left
                if ((d.x < 0 && AnimatorList[enabledAnimatorInd].transform.localScale.x > 0))
                    AnimatorList[enabledAnimatorInd].transform.localScale = new Vector3(playerScale * -1, playerScale, playerScale);
                // Else if needs to face right
                else if ((d.x > 0 && AnimatorList[enabledAnimatorInd].transform.localScale.x < 0)) 
                    AnimatorList[enabledAnimatorInd].transform.localScale = new Vector3(playerScale, playerScale, playerScale);   
            }
            if(enabledAnim.GetBool("idle"))
                enabledAnim.SetBool("idle", false);
        }
        // Actual movement
        direction = new Vector2(d.x, d.y);
        direction = transform.TransformDirection(direction);
        
    }

    protected virtual void ApplyMove() {

        /* 
         * Moved "(affectedStats[(int)Stats.movespeed] / 100f)" into this because when dash ability
         *  added new movement speed the player would have to change direction to see the effect.
         *  With this modification that is no longer the case and when ability starts player doesn'y need
         *  to change direction to see effect.
         */
        Vector2 dirc = (direction * Time.fixedDeltaTime * (affectedStats[(int)Stats.movespeed] / 100f)) ;
        rb.MovePosition(dirc + rb.position);
        if (dirc != Vector2.zero) {
            // SFX Footsteps
            if (SFXList.Count >= 2)
            {
                if (Time.time - stepPacer >= stepInterval)
                {
                    if (onLeft)
                    {
                        AudioManager.PlaySFX(SFXList[0], 0.4f);
                        onLeft = false;
                    }

                    else
                    {
                        AudioManager.PlaySFX(SFXList[1], 0.4f);
                        onLeft = true;
                    }
                    stepPacer = Time.time;
                }
            }
        }
    }

    // For On-screen stick usage only
    public virtual void SetActiveIndependentJoystick(int j) {
        activeIndependentJoystick = j;
    }

    public void LoadAttackObjects()
    {

        // Instantiate attack sequences to reattach the instance to the player
        for (int i = 0; i < AttackObjectList.Count; i++)
        {
            if (PhotonNetwork.CurrentRoom != null)
            {
                AttackObjectList[i] = PhotonNetwork.Instantiate(AttackObjectList[i].name, gameObject.transform.position, new Quaternion());
                AttackObjectList[i].layer = gameObject.layer;


            }
            else
            {
                AttackObjectList[i] = Instantiate(AttackObjectList[i], gameObject.transform);
                AttackObjectList[i].layer = gameObject.layer;
            }

        }

    }

    protected virtual void Attack(Vector2 d) {
        // Decide what we're attacking with (i.e. attack vs ability)
        // Joystick.getJoystickNumber
        if (activeIndependentJoystick < 0 || activeIndependentJoystick >= AttackList.Count)
        {
            Debug.Log("Invalid Joystick number: " + activeIndependentJoystick);
            return;
        }
        if (d.x == 0 && d.y == 0) { // Stop Attacking if joystick is at <0, 0>
            activeIndependentJoystick = -1;
            return;
        }
        AttackDirection = new Vector3(d.x, d.y, 0);
        Debug.Log("Direction: " + AttackDirection);
        if(allowedToAttack && !disarmed)
            RequestAttack(activeIndependentJoystick);
    }
    
    protected virtual void Attack(float f, int abilityIndex) { // Basic attack using mouse
        if (f < 1)
            Debug.Log("Stop ability");
        if ((photonView == null || photonView.IsMine))
        { 
            Vector3 MouseWorldCoord = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            AttackDirection = new Vector3(MouseWorldCoord.x - transform.position.x, MouseWorldCoord.y - transform.position.y, 0);
            RequestAttack(abilityIndex);
        }
    }


    public override void Die() {
        if (affectedStats[(int)Stats.health] <= 0 || isDead == true) {
            allowedToAttack = false;
            Debug.Log("Do something that indicates the player is dead...");
            isDead = true;
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0.5f); // temporary color shift
            Debug.Log("Dead");
            // Death animation
            // Death SFX
            if (SFXList.Count >= 4)
            {
                AudioManager.PlaySFX(SFXList[3], 1f);
            }
        }
    }

    public virtual bool GetUntargetable()
    {
        if (isDead)
        {
            return true;
        }
        return false;
    }
    
    protected virtual void RequestAttack(int attackListIndex) {
        if (allowedToAttack && !disarmed)
            if(attackListIndex >= 0 && attackListIndex < AttackList.Count)
                AttackList[attackListIndex].StartAttack(GetAttackDirection(), this);
    }
    public Vector3 GetAttackDirection() => AttackDirection;

    //adds an attack ot the list of attacks at a specified index
    //index 0 is basic attack, index 1 is Ability 1 and so on
    public void AddAttack(Ability atk, int abilitySlot)
    {
        //while the AttackList is too short to add the ability, add placeholder nulls
        while (AttackList.Count <= abilitySlot)
        {
            AttackList.Add(null);
        }
        //Add the attack at its designated slot
        AttackList[abilitySlot] = atk;
        
    }

    //Removes the attack from the list and setting its ability slot to null
    public void RemoveAttack(Ability atk, int abilitySlot)
    {
        if (AttackList.Count > abilitySlot)
        {
            AttackList[abilitySlot] = null;
        }
        else
        {
            Debug.Log("No Attack to Remove");
        }
    }

    protected void SwitchAnimatorGO(int enAind) {
        //AnimatorList[enabledAnimatorInd].SetActive(false);
        AnimatorList[enabledAnimatorInd].transform.localScale = Vector3.zero;
        enabledAnimatorInd = enAind;
        //AnimatorList[enabledAnimatorInd].SetActive(true);
        AnimatorList[enabledAnimatorInd].transform.localScale = new Vector3(playerScale,playerScale,playerScale);
        enabledAnim = AnimatorList[enabledAnimatorInd].GetComponent<Animator>();
    }

    public virtual List<GameObject> GetAttackObjectsList()
    {
        return AttackObjectList;
    }

    //Sets the index with the game object and sets up for use
    public virtual void SetAttackObjectsList(int index, GameObject go)
    {
        //if index is in bounds
        if (index >= 0 && index < AttackObjectList.Count)
        {
            GameObject g = AttackObjectList[index];

            AttackObjectList[index] = go; //set new object

            g.GetComponent<Ability>().Unequip(this); //unequip ability
            Destroy(g); //remove previously instantiated object

            //Instantiate object
            if (PhotonNetwork.CurrentRoom != null)
            {
                AttackObjectList[index]= PhotonNetwork.Instantiate(AttackObjectList[index].name, gameObject.transform.position, new Quaternion());
                AttackObjectList[index].layer = gameObject.layer;
            }
            else
            {
                AttackObjectList[index] = Instantiate(AttackObjectList[index], gameObject.transform);
                AttackObjectList[index].layer = gameObject.layer;
            }

            AttackObjectList[index].GetComponent<Ability>().Equip(this);
        }
    }

    // Dashing 
    public void  ToggleDashing()
    {
        bDashing = !bDashing;
    }
}


