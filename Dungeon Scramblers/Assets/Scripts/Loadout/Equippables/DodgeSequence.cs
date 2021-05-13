using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DodgeSequence : DefaultAttackSequence
{
    [SerializeField]
    private float chargingDuration = 0.1f;
    [SerializeField]
    private float launchingForce = 1000;

    private Rigidbody2D unitRigidBody;
    private Vector3 AttackNormal;

    bool bLaunch = false;


    [Photon.Pun.RPC]
    protected override IEnumerator AttackSequence()
    {
        if (PhotonNetwork.CurrentRoom != null)
        {
            photonView.RPC("AttackSequence", RpcTarget.Others);
        }
        Unit.SetAllowedToAttack(false);

        Attacked = true;

        ApplyColorChange();
        ApplySlow();

        Player PlayerUnit = (Player)Unit;

        // get mouse coordinate from camera when clicked and find the ending of the attack with the mouse clicked
        Vector3 AttackEnd = Unit.transform.position + AttackDirection;

        // Normalize the direction of the attack for incrementing the attack movement
        AttackNormal = (AttackEnd - Unit.transform.position).normalized;

        bLaunch = true;
        PlayerUnit.ToggleDashing();
        PlayerUnit.BTargetable = false;

        yield return new WaitForSeconds(chargingDuration);

        bLaunch = false;
        PlayerUnit.ToggleDashing();
        PlayerUnit.BTargetable = true;

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

    private void Update()
    {
        if (bLaunch)
        {
            Unit.GetComponent<Rigidbody2D>().AddForce(AttackNormal * launchingForce);
        }
    }
}
