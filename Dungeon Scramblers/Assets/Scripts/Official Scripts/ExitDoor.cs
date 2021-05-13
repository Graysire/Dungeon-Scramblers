using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ExitDoor : MonoBehaviour
{

    public virtual void OnTriggerEnter2D(Collider2D collision)
    {
        PhotonView OPview = gameObject.GetPhotonView();
        //If scrambler goes to exit room then increment escaped players
        Scrambler sc = collision.gameObject.GetComponent<Scrambler>();
        if (sc != null)
        {
            if (sc.GetEscaped() == false && sc.IsAlive())
            {
                //GameManager.ManagerInstance.IncrementEscapedScramblers(); //increment escaped scrambler count
                if (PhotonNetwork.CurrentRoom != null)
                {
                    

                    GameManager.ManagerInstance.IncrementEscapedScrambler();
                }
                else
                {
                    GameManager.ManagerInstance.IncrementEscapedScrambler();
                }
                OPview.RPC("SetEscaped", RpcTarget.AllBuffered, 1);  //set scrambler as escaped
            }
        }
    }
}
