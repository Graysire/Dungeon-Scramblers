using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ChargeAttackSequence : DefaultAttackSequence
{
    private Rigidbody2D rigidBody;
    public float LaunchingForce;
    private void Awake()
    {
        rigidBody = Unit.GetComponent<Rigidbody2D>();
    }

    [PunRPC]
    protected override IEnumerator AttackSequence()
    {
        if (PhotonNetwork.CurrentRoom != null)
        {
            photonView.RPC("AttackSequence", RpcTarget.Others);
        }
        Unit.SetAllowedToAttack(false);

        Attacked = true;

        // Get the overlord initial speed to change the 
        int PlayerSpeed = (Unit.GetAffectedStats()[(int)Stats.movespeed] / 100);

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

        // Normalize the direction of the attack for incrementing the attack movement
        Vector3 AttackNormal = (AttackEnd - Unit.transform.position).normalized;


        // Transform vector with quick if statements for returning offset for attacks
        // In chargin attack case, this will act as an indicator for where the minilord is charging
        Vector3 AttackTransform = Unit.transform.position + (RelativeAttackEnd.normalized * Projectile.GetOffsetScale());

        // Get instance of ability from object pooler
        Transform AbilityTransform = AbilityPooler.GetPooledObject(AttackTransform, AttackEnd, Unit.gameObject, AbilityAngle).transform;

        AbilityTransform.GetComponent<ProjectileStats>().SetUp(AttackNormal, 0);

        // Wait for ability casting time before proceeding
        yield return new WaitForSeconds(Projectile.GetCastingTime());

        rigidBody.AddForce(AttackNormal * LaunchingForce);



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

        // Update is called once per frame
        void Update()
    {
        
    }
}
