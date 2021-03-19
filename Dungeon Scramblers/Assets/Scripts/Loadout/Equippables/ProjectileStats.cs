﻿using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileStats : MonoBehaviourPunCallbacks
{
    // Define the damage of the the ability before any bonuses
    [SerializeField]
    protected int BaseDamage = 5;
    //Define the dmaage of the ability with any obnuses
    int ActualDamage;

    // Define the range of the the ability before destroying itself
    [SerializeField]
    protected float Range;
    // Define the Decay time of the ability before destroying itself
    [SerializeField]
    protected float DecayTime;
    // Define the move speed of the ability in unity units
    [SerializeField]
    protected float MoveSpeed = 10;
    // Define the casting time after calling the ability
    [SerializeField]
    protected float CastingTime;
    // Define the cooldown for every ability before being able to use it again
    [SerializeField]
    protected float CoolDown;
    // Define the offset of the ability from the orgin of the user in the direction of the attack
    [SerializeField]
    protected float OffsetScale;
    //maximum number of targets this projectile can hit
    [SerializeField]
    protected int maxTargetsHit = 1;
    // List of Status Effects to apply to enemy hit
    [SerializeField]
    protected List<StatusEffect> StatusEffects;

    // Use to calculate direction for movement of the ability
    protected Vector3 AttackDir;
    // Use to calculate decay time for the ability
    protected float totalTime = 0;
    //Used to determine if the maximum number of targets has been hitr
    protected int numHits = 0;
    // Use to calculate the position traveled to destroy object
    protected Vector3 PositionTraveled = Vector3.zero;
    // Use to handle collision to effect health or other sysem
    protected Collider2D Collider;
    // Use to handle movement of the projectile and collision
    protected Rigidbody2D rb;

    // Return the casting time for the ability as a float
    // This is an interface function that the player call with nessarily finding the object
    public float GetCastingTime()
    {
        return CastingTime;
    }

    // Return the cooldown time for the ability as a float
    // This is an interface function that the player call with nessarily finding the object
    public float GetCoolDownTime()
    {
        return CoolDown;
    }

    // Return Offset distance from the origin of the player as a float
    // This is an interface function that the player call with nessarily finding the object
    public float GetOffsetScale()
    {
        return OffsetScale;
    }
      
    // Take in the attack direction from the player to move the ability
    public void SetUp(Vector3 AttackDir, int dmg)
    {
        this.AttackDir = AttackDir;
        ActualDamage = BaseDamage + dmg;
        rb = GetComponent<Rigidbody2D>();
        Debug.Log("Set Up has been called");
        //Debug.Log("Attack Dir:" + AttackDir.ToString() + "\n rb: " + rb.ToString());
    }


    // This update will be added to update handler when all the ability function is ready to transition
    //void FixedUpdate()
    //{
    //    // Check if Attack Direction exist
    //    if (AttackDir != null && (PhotonNetwork.CurrentRoom == null || photonView.IsMine))
    //    {
    //        // Increment time passed, position of the object, and position traveled

    //        totalTime += Time.fixedDeltaTime;
    //        Debug.Log("Total Time:" + totalTime);
    //        Debug.Log("Attack Direction:" + AttackDir);
    //        Debug.Log("Position: " + rb.position);
    //        Debug.Log("New Position: " + ((new Vector2(AttackDir.x, AttackDir.y) * Time.fixedDeltaTime * MoveSpeed) + rb.position));

    //        rb.MovePosition(new Vector2(1, 1));

    //        PositionTraveled += AttackDir * Time.fixedDeltaTime * MoveSpeed;
    //        // Check if position traveled or decay time threshold was met and proceed to destroy them
    //        if (PositionTraveled.magnitude >= Range || totalTime >= DecayTime)
    //        {
    //            //Being called multiple Times
    //            ResetProjectiles();
    //        }
    //    }

    //}

    private void Awake()
    {
        if(PhotonNetwork.CurrentRoom == null)
        {
            GetComponent<PhotonRigidbody2DView>().enabled = false;
        }
    }

    void Movement()
    {
        // Check if Attack Direction exist
        if (AttackDir != null && (PhotonNetwork.CurrentRoom == null || photonView.IsMine))
        {
            // Increment time passed, position of the object, and position traveled
            totalTime += Time.fixedDeltaTime;
            //Debug.Log("Total Time:" + totalTime);
            //Debug.Log("Attack Direction:" + AttackDir);
            //Debug.Log("Position: " + rb.position);
            //Debug.Log("New Position: " + ((new Vector2(AttackDir.x, AttackDir.y) * Time.fixedDeltaTime * MoveSpeed) + rb.position));
            rb = GetComponent<Rigidbody2D>();

            rb.MovePosition((new Vector2(AttackDir.x, AttackDir.y) * Time.fixedDeltaTime * MoveSpeed) + rb.position);

            PositionTraveled += AttackDir * Time.fixedDeltaTime * MoveSpeed;
            // Check if position traveled or decay time threshold was met and proceed to destroy them
            if (PositionTraveled.magnitude >= Range || totalTime >= DecayTime)
            {
                //Being called multiple Times
               // ResetProjectiles();
            }
        }
    }
    private void OnDisable()
    {
        UpdateHandler.FixedUpdateOccurred -= Movement;
    }

    private void OnEnable()
    {
        UpdateHandler.FixedUpdateOccurred += Movement;
    }

    // For Abilities object to collide, the opposing object must have a 2D collider as well as a Rigidbody2D
    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        
        if (collision.tag == "Shield")
        {
            ResetProjectiles();
            return;
        }

        //get Damageable from collision object
        IDamageable<int> damageable = collision.GetComponent<IDamageable<int>>();
        //if the collision does not exist or damageable is not on the object
        if (collision == null || damageable == null)  return;

        //If the colliding object inherits from Abstract player then apply the status effect(s)
        AbstractPlayer unit = collision.gameObject.GetComponent<AbstractPlayer>();
        if (unit != null)
        {
            for (int i = 0; i < StatusEffects.Count; ++i)
            {
                unit.AddStatusEffect(StatusEffects[i]);
            }
        }

        Debug.Log("Hit " + collision);

        //Apply damage to object
        damageable.Damage(ActualDamage);
        numHits++;
        if (numHits >= maxTargetsHit)
        {
            ResetProjectiles();
        }
    }

    [PunRPC]
    public void ResetProjectiles()
    {
        if (PhotonNetwork.CurrentRoom != null && photonView.IsMine)
        {
            photonView.RPC("ResetProjectiles", RpcTarget.Others);
            totalTime = 0f;
            numHits = 0;
        }
        else
        {
            totalTime = 0f;
            numHits = 0;
        }

        ActualDamage = BaseDamage;
        PositionTraveled = Vector3.zero;
        this.gameObject.SetActive(false);
    }

    [PunRPC]
    public void ShowProjectile()
    {
        if (PhotonNetwork.CurrentRoom != null && photonView.IsMine)
        {
            photonView.RPC("ShowProjectile", RpcTarget.Others);
        }
        gameObject.SetActive(true);
    }
}
