using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dummy : AbstractPlayer
{
    public override void Damage(float damageTaken)
    {
        Debug.Log(gameObject + " Hit For " + damageTaken + " Damage.");
        GameManager.ManagerInstance.DistributeExperience(10.0f);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
