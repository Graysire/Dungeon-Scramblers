using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

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
                sc.SetEscaped(true);
                
                //GameManager.ManagerInstance.IncrementEscapedScramblers(); //increment escaped scrambler count
                if (PhotonNetwork.CurrentRoom != null)
                {

                    Debug.Log("Exit Door Touched (networked)");
                    GameManager.ManagerInstance.IncrementEscapedScrambler();

                }
                else
                {
                    Debug.Log("Exit Door Touched (non-networked)");
                    GameManager.ManagerInstance.IncrementEscapedScrambler();
                }
            }
        }
    }
}
