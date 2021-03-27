using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Overlord : Player
{
    [Header("Overlord Values")]
    [SerializeField]
    public bool HasRetributionAbility = false;  //refers to an effect that gets applied to players that hit them.
    [SerializeField]
    protected int RetributionAbilityIndex;  // The index of the retribution ability or status effect to apply

    protected override void Awake()
    {
        base.Awake();
        if (!usingOnScreenControls) {
            controls.PlayerMovement.UseAbilityQ.performed += ctx => Attack(ctx.ReadValue<float>(), 2);
            controls.PlayerMovement.UseAbilityQ.canceled += ctx => Attack(ctx.ReadValue<float>(), 2);
            controls.PlayerMovement.UseAbilityE.performed += ctx => Attack(ctx.ReadValue<float>(), 3);
            controls.PlayerMovement.UseAbilityE.canceled += ctx => Attack(ctx.ReadValue<float>(), 3);
        }
    }

    //Applies a status effect to the player that hits this overlord
    public void ApplyRetributionAbility(AbstractPlayer player)
    {
        player.AddStatusEffect((StatusEffect)AttackList[RetributionAbilityIndex]);
    }


protected override void Move(Vector2 d)
    {
        // Animation changes
        // Not moving
        // Front [0], Back [1], Side [2]
        //if (d.x == 0 && d.y == 0 && !enabledAnim.GetBool("idle"))
        if (d.x == 0 && d.y == 0)
        {
            /*if (animator.GetBool("facingLeft")) {
                animator.SetBool("facingLeft", false);
                animator.SetBool("facingFront", true);
            }
            animator.SetBool("idle", true);*/
            /*if (enabledAnimatorInd == 2) 
                SwitchAnimatorGO(0, false);*/
            //enabledAnim.SetBool("idle", true);
        }
        else
        {
            //enabledAnim.SetBool("idle", true);
            // Moving Down (front-facing)
            if (d.y < 0 && d.x == 0 && enabledAnimatorInd != 0)
            {
                /*animator.SetBool("facingFront", true);
                animator.SetBool("facingBack", false);
                animator.SetBool("facingLeft", false);*/
                //SwitchAnimatorGO(0, false);
            }
            // Moving Up (back-facing)
            else if (d.y > 0 && d.x == 0 && enabledAnimatorInd != 1)
            {
                /*animator.SetBool("facingFront", false);
                animator.SetBool("facingBack", true);
                animator.SetBool("facingLeft", false);*/
                //SwitchAnimatorGO(1, false);
            }
            // Moving Sideways & diagonally (side-facing)
            else if (d.x != 0)
            {
                if (enabledAnimatorInd != 3)
                {
                    /*animator.SetBool("facingFront", false);
                    animator.SetBool("facingBack", false);
                    animator.SetBool("facingLeft", true);*/
                    //SwitchAnimatorGO(2, true);
                }

                //if (d.x < 0 && !sr.flipX)
                //    sr.flipX = true;
                //else if (d.x > 0 && sr.flipX)
                //    sr.flipX = false;
            }
           // enabledAnim.SetBool("idle", false);
        }
        // Actual movement
        direction = new Vector2(d.x, d.y);
        direction = transform.TransformDirection(direction);
    }
}
