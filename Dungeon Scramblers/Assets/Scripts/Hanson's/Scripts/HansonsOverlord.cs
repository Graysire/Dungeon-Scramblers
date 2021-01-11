using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HansonsOverlord : Player
{
    [SerializeField]
    private LineRenderer line;
    [SerializeField]
    private Ability ability;

    bool bIsAttacking = false;

    protected override void OnEnable()
    {
        controls.Enable();
        UpdateHandler.UpdateOccurred += Die;
        UpdateHandler.FixedUpdateOccurred += ApplyMove;

    }
    protected override void OnDisable()
    {
        controls.Disable();
        UpdateHandler.UpdateOccurred -= Die;
        UpdateHandler.FixedUpdateOccurred -= ApplyMove;
    }

    // Override Attack function to start coroutine
    protected override void Attack(float f)
    { // MOUSE ATTACK INPUT
        // Check if the coroutine can be called with account to Castingtime and Cooldown
        if(!bIsAttacking) StartCoroutine("AttackSequence");
    }
    protected override void Attack(Vector2 d)
    { // TOUCHSCREEN ATTACK INPUT
        Debug.Log("Overlord attack on phone");
        
    }

    IEnumerator AttackSequence()
    {
        // Set Is currently attacking
        bIsAttacking = true;
        // Wait for ability casting time before proceeding
        yield return new WaitForSeconds(ability.GetCastingTime());
        // get mouse coordinate from camera when clicked and find the ending of the attack with the mouse clicked
        Vector3 MouseWorldCoord = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 AttackEnd = new Vector3(MouseWorldCoord.x, MouseWorldCoord.y, 0);

        // Get relative position of where the mouse was clicked to correctly calculate the angle for projectile
        Vector3 RelativeAttackEnd = AttackEnd - transform.position;
        float dot = Vector3.Dot(transform.up, RelativeAttackEnd);
        Debug.Log("Dot: " + dot);
        // Calculate the angle of the ability in radians with dot product formula A dot B = |A||B|cos(theta)
        float AbilityAngle = Mathf.Acos(dot / (transform.up.magnitude * RelativeAttackEnd.magnitude)) * Mathf.Rad2Deg;
        Debug.Log("Angle: " + AbilityAngle);
        
        // Normalize the direction of the attack for incrementing the attack movement
        Vector3 AttackDirection = (AttackEnd - transform.position).normalized;
        // Transform vector with quick if statements for returning offset for attacks
        Vector3 AttackTransform = transform.position + (RelativeAttackEnd.normalized * ability.GetOffsetScale());
        Transform AbilityTransform = Instantiate(ability.gameObject, AttackTransform, 
            Quaternion.Euler(0,0, AttackEnd.x >= transform.position.x ? -AbilityAngle : AbilityAngle)).transform;
        AbilityTransform.GetComponent<Ability>().SetUp(AttackDirection);
        yield return new WaitForSeconds(ability.GetCoolDownTime());
        bIsAttacking = false;
    }
}
