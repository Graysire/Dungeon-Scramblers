using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MultiAttackSequence : DefaultAttackSequence
{

    [PunRPC]
    protected override IEnumerator AttackSequence()
    {
        Unit.SetAllowedToAttack(false);

        Attacked = true;

        // Wait for ability casting time before proceeding
        yield return new WaitForSeconds(Projectile.GetCastingTime());

        // get mouse coordinate from camera when clicked and find the ending of the attack with the mouse clicked
        Vector3 AttackEnd = Unit.transform.position + AttackDirection;

        // Get relative position of where the mouse was clicked to correctly calculate the angle for projectile
        Vector3 RelativeAttackEnd = AttackEnd - Unit.transform.position;


        float dot = Vector3.Dot(Unit.transform.right, RelativeAttackEnd);

        // Calculate the angle of the ability in radians with dot product formula A dot B = |A||B|cos(theta)
        float angle = Mathf.Acos(dot / (Unit.transform.right.magnitude * RelativeAttackEnd.magnitude)) * Mathf.Rad2Deg;

       
        // Set the angle to be positive if on the right of the object. The angle will be convertred to negative if on the left side of the object
        // This is because angle from dot product return the result in perspective with the reference vector, unit right vector in this case and they're not negative
        float AbilityAngle = AttackEnd.y >= Unit.transform.position.y ? angle : -angle;
        float AbilityAngle2 = AbilityAngle + 15;
        float AbilityAngle3 = AbilityAngle - 15;

        Debug.Log("Angle 1: " + AbilityAngle);
        Debug.Log("Angle 2: " + AbilityAngle2);
        Debug.Log("Angle 3: " + AbilityAngle3);


        Vector3 RelativeAttackEnd2 = new Vector3(RelativeAttackEnd.magnitude * Mathf.Cos(AbilityAngle2 * Mathf.Deg2Rad), RelativeAttackEnd.magnitude * Mathf.Sin(AbilityAngle2 * Mathf.Deg2Rad), 0);
        Vector3 RelativeAttackEnd3 = new Vector3(RelativeAttackEnd.magnitude * Mathf.Cos(AbilityAngle3 * Mathf.Deg2Rad), RelativeAttackEnd.magnitude * Mathf.Sin(AbilityAngle3 * Mathf.Deg2Rad), 0);
        
        Vector3 AttackEnd2 = Unit.transform.position + RelativeAttackEnd2;
        Vector3 AttackEnd3 = Unit.transform.position + RelativeAttackEnd3;
        

        Debug.Log("RAE 1: " + RelativeAttackEnd);
        Debug.Log("RAE 2: " + RelativeAttackEnd2);
        Debug.Log("RAE 3: " + RelativeAttackEnd3);

        Debug.Log("AE 1: " + AttackEnd);
        Debug.Log("AE 2: " + AttackEnd2);
        Debug.Log("AE 3: " + AttackEnd3);

        Debug.Log("Mag: " + AttackDirection.magnitude);

        // Normalize the direction of the attack for incrementing the attack movement
        Vector3 AttackNormal = (AttackEnd - Unit.transform.position).normalized ;
        Vector3 AttackNormal2 = (AttackEnd2 - Unit.transform.position).normalized;
        Vector3 AttackNormal3 = (AttackEnd3 - Unit.transform.position).normalized;

        // Transform vector with quick if statements for returning offset for attacks
        Vector3 AttackTransform = Unit.transform.position + (RelativeAttackEnd.normalized * Projectile.GetOffsetScale());
        Vector3 AttackTransform2 = Unit.transform.position + (RelativeAttackEnd2.normalized * Projectile.GetOffsetScale());
        Vector3 AttackTransform3 = Unit.transform.position + (RelativeAttackEnd3.normalized * Projectile.GetOffsetScale());
        
        // Get instance of ability from object pooler
        Transform AbilityTransform = AbilityPooler.GetPooledObject(AttackTransform, AttackEnd, Unit.gameObject, AbilityAngle).transform;

        Transform AbilityTransform2 = AbilityPooler.GetPooledObject(AttackTransform2, AttackEnd2, Unit.gameObject, AbilityAngle2).transform;

        Transform AbilityTransform3 = AbilityPooler.GetPooledObject(AttackTransform2, AttackEnd3, Unit.gameObject, AbilityAngle3).transform;

        SetBulletLayer(AbilityTransform); //set the attack layer based on who creates it
        SetBulletLayer(AbilityTransform2);
        SetBulletLayer(AbilityTransform3);



        AbilityTransform.GetComponent<ProjectileStats>().SetUp(AttackNormal, abilitySlot == 0 ? Unit.GetAffectedStats()[(int)Stats.attackdmg] : Unit.GetAffectedStats()[(int)Stats.abilitydmg]);
        AbilityTransform2.GetComponent<ProjectileStats>().SetUp(AttackNormal2, abilitySlot == 0 ? Unit.GetAffectedStats()[(int)Stats.attackdmg] : Unit.GetAffectedStats()[(int)Stats.abilitydmg]);
        AbilityTransform3.GetComponent<ProjectileStats>().SetUp(AttackNormal3, abilitySlot == 0 ? Unit.GetAffectedStats()[(int)Stats.attackdmg] : Unit.GetAffectedStats()[(int)Stats.abilitydmg]);




        //allow the player to attack after casting is finished
        Unit.SetAllowedToAttack(true);

        if (abilitySlot == 0)
        {
            yield return new WaitForSeconds(Projectile.GetCoolDownTime() * (Unit.GetAffectedStats()[(int)Stats.attackspeed] / 100));
        }
        else
        {
            yield return new WaitForSeconds(Projectile.GetCoolDownTime() * (Unit.GetAffectedStats()[(int)Stats.abilitycd] / 100));
        }

        Attacked = false;
    }
}
