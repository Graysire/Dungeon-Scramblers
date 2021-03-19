using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DefaultAttackSequence : Ability
{
    [Header("Attack Sequence Variables")]
    [SerializeField]
    protected ProjectileStats Projectile;
    protected bool Attacked = false;
    protected AbstractPlayer Unit;
    protected Vector3 AttackDirection;
    //protected bool needsMoreInstances;

    private PhotonView pV;
    private void Start()
    {
        // Find object pooler child GameObject
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.CompareTag("Object Pooler"))
            {
                AbilityPooler = child.GetComponent<ObjectPooler>();
                Debug.Log("Found pooler");
                break;
            }
        }
        pV = GetComponent<PhotonView>();
    }

    public override void StartAttack(Vector3 AttackDirection, AbstractPlayer Unit)
    {
        this.AttackDirection = AttackDirection;
        this.Unit = Unit;
        if (!Attacked) StartCoroutine("AttackSequence");
    }

    [PunRPC]
    protected virtual IEnumerator AttackSequence()
    {
        //Call This function on other players

        // Disallow further attacks while the unit is casting
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

        // Normalize the direction of the attack for incrementing the attack movement
        Vector3 AttackNormal = (AttackEnd - Unit.transform.position).normalized;


        // Transform vector with quick if statements for returning offset for attacks
        Vector3 AttackTransform = Unit.transform.position + (RelativeAttackEnd.normalized * Projectile.GetOffsetScale());

        // Get instance of ability from object pooler
        Transform AbilityTransform = AbilityPooler.GetPooledObject(AttackTransform, AttackEnd, Unit.gameObject, AbilityAngle).transform;
        
        SetBulletLayer(AbilityTransform); //set the attack layer based on who creates it
        


        AbilityTransform.GetComponent<ProjectileStats>().SetUp(AttackNormal, abilitySlot == 0? Unit.GetAffectedStats()[(int)Stats.attackdmg] : Unit.GetAffectedStats()[(int)Stats.abilitydmg]);

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

    //Applies the layer for the attacks based on what layer the attack instigator is 
    protected void SetBulletLayer(Transform AbilityTransform)
    {
        if (gameObject.layer == LayerMask.NameToLayer("Scrambler"))
            AbilityTransform.gameObject.layer = LayerMask.NameToLayer("ScramblerBullet");
        else if (gameObject.layer == LayerMask.NameToLayer("Overlord"))
            AbilityTransform.gameObject.layer = LayerMask.NameToLayer("OverlordBullet");
    }


}
