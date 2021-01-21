using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HansonsOverlord : Player
{
    enum AttackSequences
    {
        Basic
    };


    [SerializeField]
    private LineRenderer line;
    [SerializeField]
    private Ability ability;
    [SerializeField]
    private AttackSequences sequence;

    private Attack attack;

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

    protected virtual void Awake()
    {
        base.Awake();
        attack = this.gameObject.AddComponent<Attack>();
    }

    // Override Attack function to start coroutine
    protected override void Attack(float f)
    { // MOUSE ATTACK INPUT
        // Check if the coroutine can be called with account to Castingtime and Cooldown
        if (usingOnScreenControls) return;
  
        Vector3 MouseWorldCoord = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 AttackDirection = new Vector3(MouseWorldCoord.x - transform.position.x , MouseWorldCoord.y - transform.position.y, 0);
        if (!bIsAttacking) attack.StartAttack(AttackDirection, ability, this);
    }
    protected override void Attack(Vector2 d)
    { // TOUCHSCREEN ATTACK INPUT
        if (!usingOnScreenControls) return;
        Debug.Log("Overlord attack on phone");
        Vector3 AttackDirection = new Vector3(-d.x, d.y, 0);
        if (!bIsAttacking) attack.StartAttack(AttackDirection, ability, this);
    }

    public void toggleIsAttacking()
    {
        bIsAttacking = !bIsAttacking;
    }

    //IEnumerator AttackSequence()
    //{
    //    // Set Is currently attacking
    //    bIsAttacking = true;
    //    // Wait for ability casting time before proceeding
    //    yield return new WaitForSeconds(ability.GetCastingTime());
    //    // get mouse coordinate from camera when clicked and find the ending of the attack with the mouse clicked

    //    AttackEnd = transform.position + AttackDirection;
    //    // Get relative position of where the mouse was clicked to correctly calculate the angle for projectile
    //    Vector3 RelativeAttackEnd = AttackEnd - transform.position;
    //    float dot = Vector3.Dot(transform.up, RelativeAttackEnd);

    //    // Calculate the angle of the ability in radians with dot product formula A dot B = |A||B|cos(theta)
    //    float AbilityAngle = Mathf.Acos(dot / (transform.up.magnitude * RelativeAttackEnd.magnitude)) * Mathf.Rad2Deg;

        
    //    // Normalize the direction of the attack for incrementing the attack movement
    //    Vector3 AttackNormal = (AttackEnd - transform.position).normalized;
    //    // Transform vector with quick if statements for returning offset for attacks
    //    Vector3 AttackTransform = transform.position + (RelativeAttackEnd.normalized * ability.GetOffsetScale());
    //    Transform AbilityTransform = Instantiate(ability.gameObject, AttackTransform, 
    //        Quaternion.Euler(0,0, AttackEnd.x >= transform.position.x ? -AbilityAngle : AbilityAngle)).transform;
    //    AbilityTransform.GetComponent<Ability>().SetUp(AttackNormal);
    //    yield return new WaitForSeconds(ability.GetCoolDownTime());
    //    bIsAttacking = false;
    //}
}
