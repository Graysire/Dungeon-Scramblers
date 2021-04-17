using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitDoor : MonoBehaviour
{

    public virtual void OnTriggerEnter2D(Collider2D collision)
    {
        //If scrambler goes to exit room then increment escaped players
        Scrambler sc = collision.gameObject.GetComponent<Scrambler>();
        if (sc != null)
        {
            if (sc.GetEscaped() == false && sc.IsAlive())
            {
                GameManager.ManagerInstance.IncrementEscapedScramblers(); //increment escaped scrambler count
                sc.SetEscaped(true); //set scrambler as escaped
            }
        }
    }
}
