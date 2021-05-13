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
                //GameManager.ManagerInstance.IncrementEscapedScramblers(); //increment escaped scrambler count
                if (PhotonNetwork.CurrentRoom != null)
                {
                    //PhotonView OPview = gameObject.GetPhotonView();
                    //int PhotonID = gameObject.GetPhotonView().ViewID;
                    GameManager.ManagerInstance.IncrementEscapedScramblers();
                   // OPview.RPC("IncrementEscapedScramblers", RpcTarget.AllBuffered); 
                }
                else
                {
                    GameManager.ManagerInstance.IncrementEscapedScramblers();
                }
                sc.SetEscaped(true); //set scrambler as escaped
            }
        }
    }
}
