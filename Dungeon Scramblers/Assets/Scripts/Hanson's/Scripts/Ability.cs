using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability : MonoBehaviour
{
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
    Collider2D Collider;

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
    }


    // This update will be added to update handler when all the ability function is ready to transition
    void Update()
    {
        // Check if Attack Direction exist
        if (AttackDir != null)
        {
            // Increment time passed, position of the object, and position traveled
            totalTime += Time.deltaTime;
            transform.position += AttackDir * Time.deltaTime * MoveSpeed;
            PositionTraveled += AttackDir * Time.deltaTime * MoveSpeed;
            // Check if position traveled or decay time threshold was met and proceed to destroy them
            if (PositionTraveled.magnitude >= Range || totalTime >= DecayTime)
            {
                Destroy(this.gameObject);
            }
        }

    }

    // For Abilities object to collide, the opposing object must have a 2D collider as well as a Rigidbody2D
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Hit " + collision);
        collision.gameObject.GetComponent<IDamageable<float>>().Damage(0.2f);
        
    }

}
