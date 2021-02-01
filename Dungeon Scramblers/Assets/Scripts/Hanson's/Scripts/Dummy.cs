using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dummy : MonoBehaviour, IDamageable<float>
{
    public void Damage(float damageTaken)
    {
        Debug.Log(gameObject + " Hit For " + damageTaken + " Damage.");
        GameManager.ManagerInstance.Scramblers[0].AddExperience(10);
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
