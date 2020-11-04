using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] protected Rigidbody2D rb;
    protected enum Stats { 
        health = 0,
        movespeed = 1,
        attackdmg = 2,
        attackspeed = 3,
        abilitydmg = 4,
        defense = 5
    }

    [SerializeField] protected float[] stats = new float[] { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f };
    protected bool isDead = false;

    private void OnEnable()
    {
        UpdateHandler.UpdateOccurred += testerMethod;
        UpdateHandler.FixedUpdateOccurred += Move;
    }    
    private void OnDisable()
    {
        UpdateHandler.UpdateOccurred -= testerMethod;
        UpdateHandler.FixedUpdateOccurred -= Move;
    }

    private void testerMethod() {
        Debug.Log("Health = " + (int)Stats.health);
        Debug.Log("Speed = " + Stats.movespeed.ToString());
    }

    protected virtual void Move(){
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
        Vector2 movement = new Vector2(moveX, moveY);
        rb.AddForce(movement * stats[(int)Stats.movespeed]);
    }

    protected virtual void Attack() { 
        // BUTTONS TO ATTACK
    }

    protected virtual void UseAbility() { 
        // BUTTONS TO USE ABILITY
    }

    protected virtual void Die() {
        if (stats[(int)Stats.health] <= 0) {
            Debug.Log("Do something that indicates the player is dead...");
            isDead = true;
            // Death animation
        }
    }
    protected virtual void Vote() { 
        // was this supposed to be a thing?
    }
}
