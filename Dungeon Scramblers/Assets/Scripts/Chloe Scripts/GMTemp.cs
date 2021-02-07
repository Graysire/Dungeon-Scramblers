using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GMTemp : MonoBehaviour
{
    public GameObject PlayerTest;
    public GameObject EnemyTest;
    // Start is called before the first frame update
    void Start()
    {
        if(PhotonNetwork.IsConnectedAndReady)
        {
            if(PlayerTest != null)
            {
                int RandCoord = Random.Range(-4, 4);
                PhotonNetwork.Instantiate(PlayerTest.name, new Vector2(RandCoord, RandCoord), Quaternion.identity);
                Debug.Log("Player Spawning");
            }
            if(EnemyTest != null)
            {
                int RandCoord = Random.Range(-4, 4);
                PhotonNetwork.Instantiate(EnemyTest.name, new Vector2(RandCoord, RandCoord), Quaternion.identity);
             
            }
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
