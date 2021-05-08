using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DefaultAttackSequence : Ability
{
    [Header("Attack Sequence Variables")]
    [SerializeField]
    protected ProjectileStats Projectile;
    [SerializeField]
    protected bool SlowPlayerOnCast = false;
    [SerializeField]
    protected bool ChangePlayerColorOnCast;
    [SerializeField]
    protected Color PlayerCastColor;

    [SerializeField]
    protected bool SelfAffectingAbility = false; //If this is true then the ability layer will be treated as enemy layer so that it can hit own layer

    protected SpriteRenderer PlayerSprite;
    protected Color PlayerOriginalColor;
    protected bool Attacked = false;
    protected AbstractPlayer Unit;
    protected Vector3 AttackDirection;
    protected int originalSpeed;
    //protected bool needsMoreInstances;



    [PunRPC]
    public override void StartAttack(Vector3 AttackDirection, AbstractPlayer Unit)
    {
        //Call This function on other players
        if (PhotonNetwork.CurrentRoom != null && photonView.IsMine)
        {
           // photonView.RPC("StartAttack", RpcTarget.Others, new object[] { AttackDirection });
        }
        this.AttackDirection = AttackDirection;
        this.Unit = Unit;
        if (!Attacked) StartCoroutine("AttackSequence");
    }

    
    protected virtual IEnumerator AttackSequence()
    {
        //photonView.RPC("AttackSequence", RpcTarget.Others );
        // Disallow further attacks while the unit is casting
        Unit.SetAllowedToAttack(false);
        

        Attacked = true;

        ApplyColorChange();
        ApplySlow();


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
        Transform AbilityTransform = AbilityPooler.GetPooledObject(AttackTransform, AttackEnd, AbilityAngle).transform;
        

        SetBulletLayer(AbilityTransform); //set the attack layer based on who creates it
        

        //Send indicator of the charge direction
        AbilityTransform.GetComponent<ProjectileStats>().SetUp(Unit, AttackNormal, abilitySlot == 0? Unit.GetAffectedStats()[(int)Stats.attackdmg] : Unit.GetAffectedStats()[(int)Stats.abilitydmg]);


        RevertColorChange();
        RevertSlow();

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
        if (gameObject.layer == LayerMask.NameToLayer("Scrambler") || (gameObject.layer == LayerMask.NameToLayer("Overlord") && SelfAffectingAbility))
            AbilityTransform.gameObject.layer = LayerMask.NameToLayer("ScramblerBullet");
        if (gameObject.layer == LayerMask.NameToLayer("Overlord") || (gameObject.layer == LayerMask.NameToLayer("Scrambler") && SelfAffectingAbility))
            AbilityTransform.gameObject.layer = LayerMask.NameToLayer("OverlordBullet");
    }

    protected void ApplyColorChange()
    {
        //Change player sprite color for cast
        if (ChangePlayerColorOnCast)
        {
            //Apply color on single sprite
            PlayerSprite = Unit.GetComponent<SpriteRenderer>();
            if (PlayerSprite != null)
            {
                PlayerOriginalColor = PlayerSprite.color;
                PlayerSprite.color = PlayerCastColor;
            }

            foreach (SpriteRenderer r in Unit.GetComponentsInChildren<SpriteRenderer>(true))
            {
                PlayerOriginalColor = r.material.color;
                r.material.color = PlayerCastColor;
            }
        }
    }

    protected void ApplySlow()
    {
        //If slow on cast then reduce speed by half
        if (SlowPlayerOnCast)
        {
            originalSpeed = Unit.GetAffectedStats()[1];
            Unit.GetAffectedStats()[1] = originalSpeed / 2;
        }
    }

    protected void RevertColorChange()
    {
        //Change player sprite color back
        if (ChangePlayerColorOnCast)
        {
            //Revert single sprite color
            if (PlayerSprite != null)
            {
                PlayerSprite.color = PlayerOriginalColor;
            }

            foreach (SpriteRenderer r in Unit.GetComponentsInChildren<SpriteRenderer>(true))
            {
                r.material.color = PlayerOriginalColor;
            }
        }
    }

    protected void RevertSlow()
    {
        //Reset original speed
        if (SlowPlayerOnCast)
        {
            Unit.GetAffectedStats()[1] = originalSpeed;
        }
    }

    public override void Equip(Player player)
    {
        base.Equip(player);
        Unit = player;
    }

    public override void Unequip(Player player)
    {
        base.Unequip(player);
        Unit = null;
    }

    public virtual void Equip(AI player)
    {
        Unit = player;
    }
}
