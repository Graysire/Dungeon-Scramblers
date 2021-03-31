using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargingProjectileStats : ProjectileStats
{
    [SerializeField]
    protected AbstractPlayer ChargingPlayer;

    public void SetPlayer(AbstractPlayer ChargingPlayer)
    {
        this.ChargingPlayer = ChargingPlayer;
    }

    protected override void Update()
    {
        if (AttackDir != null && (PhotonNetwork.CurrentRoom == null || photonView.IsMine))
        {

            rb = GetComponent<Rigidbody2D>();

            rb.MovePosition(ChargingPlayer.transform.position + (AttackDir * GetOffsetScale()));

            // Check if position traveled or decay time threshold was met and proceed to destroy them
            if (PositionTraveled.magnitude >= Range || totalTime >= DecayTime)
            {
                //Being called multiple Times
                ResetProjectiles();
                int PhotonID = gameObject.GetPhotonView().ViewID;
                TurnOffProjectile(PhotonID);
            }
        }
    }
}
