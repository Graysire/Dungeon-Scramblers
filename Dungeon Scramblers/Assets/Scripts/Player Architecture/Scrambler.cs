using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;
using UnityEngine.InputSystem;
using Photon.Pun;

public class Scrambler : Player
{
    [SerializeField]
    public DisplayBar ExperienceBar;
    bool escaped = false;

    protected override void OnEnable()
    {
        base.OnEnable();
        UpdateHandler.FixedUpdateOccurred += Escaped;
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        UpdateHandler.FixedUpdateOccurred -= Escaped;
    }

    public void SetEXPBar(DisplayBar expB)
    {
        ExperienceBar = expB;
    }

    public override bool GetUntargetable()
    {
        if (escaped || isDead)
        {
            return true;
        }
        return false;

    }

    public void Escaped()
    {
        if (escaped)
        {
            allowedToAttack = false;
            SetPhysicsLayer(14);
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0.5f);
        }
    }
    public void ResetEscaped()
    {
        allowedToAttack = true;
        SetPhysicsLayer(10);
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0.5f);
    }

    public override void Die()
    {
        if (PhotonNetwork.CurrentRoom != null)
        {
            PhotonView OPview = gameObject.GetPhotonView();
            int PhotonID = gameObject.GetPhotonView().ViewID;
            OPview.RPC("Die", RpcTarget.OthersBuffered, PhotonID);
        }
        base.Die();
        if (isDead)
        {
            GameManager.ManagerInstance.AddScramblerDeath(this);
            SetPhysicsLayer(14);
        }
    }
    [PunRPC]
    public void Die(int PhotonID)
    {
        //Find Player that died
        GameObject Scrambler = PhotonView.Find(PhotonID).gameObject;
        GetComponent<Scrambler>().SetPhysicsLayer(14);
        //OPview.RPC("Die", RpcTarget.OthersBuffered, PhotonID);
    }

    //layer #14 is spectator, #10 is Scrambler
    private void SetPhysicsLayer(int layer)
    {
        gameObject.layer = layer;
    }

    public bool GetEscaped()
    {
        return escaped;
    }
    public void SetEscaped(bool e)
    {
        escaped = e;
    }

    public bool IsAlive()
    {
        return !isDead;
    }

    public void updateExperience(float ratio)
    {
        if (ExperienceBar != null)
        {
            ExperienceBar.SetValue(ratio);
        }
    }

    public void Revive(int reviveHP, bool byPercent) {
        if (isDead) {
            if (byPercent)
            {
                if (reviveHP < 0)                                                            // Check for invalid heath percentages
                    reviveHP = 0;
                else if (reviveHP > 1)
                    reviveHP = 1;
                affectedStats[(int)Stats.health] = (reviveHP / 100) * stats[(int)Stats.health];   // Revive with a PERCENT of your health
            }
            else {
                if (reviveHP < 0)                                                               // Check for invalid health values
                    reviveHP = 0;
                else if (reviveHP > stats[(int)Stats.health])
                    reviveHP = stats[(int)Stats.health];
                affectedStats[(int)Stats.health] = reviveHP;                                // Revive with a STRAIGHT health value
            }
            isDead = false;
            allowedToAttack = true;
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0.5f);                     // temporary color shift
            SetPhysicsLayer(10); // set layer back to scrambler
            GameManager.ManagerInstance.RemoveScramblerDeath(this);
        }
    }

    public void LevelUp()
    {
        int hpIncrease = Mathf.FloorToInt(stats[0] * 0.1f);

        // Change base stats of heatlh
        stats[0] += hpIncrease;
        Debug.Log("Health: " + stats[0]);
        // Change base stats of attack damage
        stats[2] += Mathf.FloorToInt(stats[2] * 0.1f);
        Debug.Log("Attack Damage: " + stats[2]);
        // Change base stats of attack speed
        //stats[3] += Mathf.FloorToInt(stats[3] * 0.1f);
        Debug.Log("Attack Speed: " + stats[3]);
        // Change base stats of ability damage
        stats[4] += Mathf.FloorToInt(stats[4] * 0.1f);
        Debug.Log("Ability Damage: " + stats[4]);

        // Refresh the affected stats
        affectedStats[0] += hpIncrease;
        // refresh the affected stats upon level up
        affectedStats[2] = stats[2];
        //affectedStats[3] = stats[3];
        affectedStats[4] = stats[4];
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (bDashing && (collision.gameObject.layer == 10/*Ignore Scramblers*/ || collision.gameObject.layer == 11 /*Ignore Overlord*/))
        {
            Physics2D.IgnoreCollision(collision.collider, GetComponent<Collider2D>());
        }
    }

}
