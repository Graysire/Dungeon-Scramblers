using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    protected enum Stats { 
        health = 0,
        speed = 1,
    }

    //protected 

    private void OnEnable()
    {
        UpdateHandler.UpdateOccurred += testerMethod;
    }    
    private void OnDisable()
    {
        UpdateHandler.UpdateOccurred -= testerMethod;
    }

    private void testerMethod() {
        Debug.Log("Health = " + Stats.health);
        Debug.Log("Speed = " + Stats.speed.ToString());
    }
}
