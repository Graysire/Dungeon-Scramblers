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

    protected override void FixedUpdate()
    {
        if (AttackDir != null && (PhotonNetwork.CurrentRoom == null || photonView.IsMine))
        {
            // Increment time passed, position of the object, and position traveled
            totalTime += Time.fixedDeltaTime;

            PositionTraveled += AttackDir * Time.fixedDeltaTime * MoveSpeed;
            rb = GetComponent<Rigidbody2D>();

            rb.MovePosition(ChargingPlayer.transform.position + (AttackDir * GetOffsetScale()));

            // Check if position traveled or decay time threshold was met and proceed to destroy them
            if (PositionTraveled.magnitude >= Range || totalTime >= DecayTime)
            {
                //Being called multiple Times
                //ResetProjectiles();
                if (PhotonNetwork.CurrentRoom != null && photonView.IsMine)
                {
                    int PhotonID = gameObject.GetPhotonView().ViewID;
                    TurnOffProjectile(PhotonID);
                }
                else
                {
                    ResetProjectiles();
                }
            }
        }
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 10)
        {
            Debug.Log("Collide with Scrambler");
        }
        base.OnTriggerEnter2D(collision);
    }
}
