using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileStats : MonoBehaviour
{
    // Define the damage of the the ability
    [SerializeField]
    private float Damage = 5.0f;
    // Define the range of the the ability before destroying itself
    [SerializeField]
    private float Range;
    // Define the Decay time of the ability before destroying itself
    [SerializeField]
    private float DecayTime;
    // Define the move speed of the ability in unity units
    [SerializeField]
    private float MoveSpeed;
    // Define the casting time after calling the ability
    [SerializeField]
    private float CastingTime;
    // Define the cooldown for every ability before being able to use it again
    [SerializeField]
    private float CoolDown;
    // Define the offset of the ability from the orgin of the user in the direction of the attack
    [SerializeField]
    private float OffsetScale;



    // Use to calculate direction for movement of the ability
    private Vector3 AttackDir;
    // Use to calculate decay time for the ability
    private float totalTime = 0;
    // Use to calculate the position traveled to destroy object
    private Vector3 PositionTraveled = Vector3.zero;
    // Use to handle collision to effect health or other sysem
    private Collider2D Collider;
    // Use to handle movement of the projectile and collision
    private Rigidbody2D rb;

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
    public void SetUp(Vector3 AttackDir)
    {
        this.AttackDir = AttackDir;
        rb = GetComponent<Rigidbody2D>();
    }


    // This update will be added to update handler when all the ability function is ready to transition
    void FixedUpdate()
    {
        // Check if Attack Direction exist
        if (AttackDir != null)
        {
            // Increment time passed, position of the object, and position traveled
            totalTime += Time.fixedDeltaTime;
            rb.MovePosition((new Vector2(AttackDir.x, AttackDir.y) * Time.fixedDeltaTime * MoveSpeed) + rb.position);
            PositionTraveled += AttackDir * Time.fixedDeltaTime * MoveSpeed;
            // Check if position traveled or decay time threshold was met and proceed to destroy them
            if (PositionTraveled.magnitude >= Range || totalTime >= DecayTime)
            {
                this.gameObject.SetActive(false);
                totalTime = 0f;
                PositionTraveled = Vector3.zero;
            }
        }

    }

    // For Abilities object to collide, the opposing object must have a 2D collider as well as a Rigidbody2D
    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        //get Damageable from collision object
        IDamageable<float> damageable = collision.GetComponent<IDamageable<float>>();

        //if the collision does not exist or damageable is not on the object
        if (collision == null || damageable == null)  return;

        Debug.Log("Hit " + collision);

        //Apply damage to object
        damageable.Damage(Damage);
        
    }
}
