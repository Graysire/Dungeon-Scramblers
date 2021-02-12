using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldStats : ProjectileStats
{
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        //The layer of the projectile will be determined and it will only detect other side layer
        if (collision.gameObject.layer == 12 || collision.gameObject.layer == 13)
        {
            
            ProjectileStats projectile = collision.GetComponent<ProjectileStats>();
            projectile.ResetProjectiles();
            return;
        }
        base.OnTriggerEnter2D(collision);
        
    }
}
