using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ChargeAttackSequence : DefaultAttackSequence
{
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
