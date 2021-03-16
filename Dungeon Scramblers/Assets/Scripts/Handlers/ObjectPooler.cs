using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ObjectPooler : MonoBehaviourPunCallbacks
{
    // Layer assignment on instantiation 
    public static ObjectPooler sharedInstance;
    [SerializeField] protected GameObject objectToPool; // add another instance of this script on the GO to pool another object
    [SerializeField] protected int amountToPool;
    protected List<GameObject> objectsPooled;
    
    private void Awake()
    {
        sharedInstance = this;

    }
    private void OnEnable()
    {
        UpdateHandler.StartOccurred += PoolObjectAtStart;
    }

    private void OnDisable()
    {
        UpdateHandler.StartOccurred -= PoolObjectAtStart;
    }

    [PunRPC]
    protected void PoolObjectAtStart() {
        objectsPooled = new List<GameObject>();
        for (int i = 0; i < amountToPool; i++)
        {
            GameObject go;
            //PhotonID
            if (PhotonNetwork.CurrentRoom != null)
            {
                go = PhotonNetwork.Instantiate(objectToPool.name, transform.position, new Quaternion());
                go.transform.SetParent(this.gameObject.transform);
                photonView.RPC("PoolObjectAtStart", RpcTarget.OthersBuffered);
            }
            else
            {
                go = Instantiate(objectToPool, transform.position, new Quaternion());
            }
            //go.SetActive(false);
            objectsPooled.Add(go);
        }
    }
   [PunRPC]
    public GameObject GetPooledObject() {
        if (PhotonNetwork.CurrentRoom != null)
        {
            photonView.RPC("GetPooledObject", RpcTarget.All);
        }
        for (int i = 0; i < objectsPooled.Count; i++) {
            if (!objectsPooled[i].activeSelf) {
                //objectsPooled[i].SetActive(true);
                objectsPooled[i].GetComponent<ProjectileStats>().ShowProjectile();
                return objectsPooled[i];
            }
        }
        GameObject go;
        if (PhotonNetwork.CurrentRoom != null)
        {
            go = PhotonNetwork.Instantiate(objectToPool.name, transform.position, new Quaternion());
            go.transform.SetParent(gameObject.transform);
               
           
        }
        else
        {
            go = Instantiate(objectToPool, transform.position, new Quaternion());
        }
        go.SetActive(false);
        objectsPooled.Add(go);
        return objectsPooled[objectsPooled.Count - 1];
    }

    //[PunRPC]
    public GameObject GetPooledObject(Vector3 AttackTransform, Vector3 AttackEnd, GameObject Player, float AbilityAngle) {

        //Call funciton for other Players to set object active
        //if(PhotonNetwork.CurrentRoom != null)
        //{
        //    photonView.RPC("GetPooledObject", RpcTarget.All);

        //}

        //initialize this object since it is new
        if (objectsPooled == null)
        {
            PoolObjectAtStart();
        }

        //Modify ability angle to account for the rightward angle of reference
        float angle = AbilityAngle - 90;

        for (int i = 0; i < objectsPooled.Count; i++)
        {
            if (!objectsPooled[i].activeSelf)
            {
                objectsPooled[i].SetActive(true);
                objectsPooled[i].transform.position = AttackTransform;
                objectsPooled[i].transform.rotation = Quaternion.Euler(0, 0, angle);
                return objectsPooled[i];
            }
        }
        GameObject go;
        if (PhotonNetwork.CurrentRoom != null)
        {
            go = PhotonNetwork.Instantiate(objectToPool.name, AttackTransform,
                    Quaternion.Euler(0, 0, angle));
            go.transform.SetParent(this.gameObject.transform);
        }
        else
        {
            go = Instantiate(objectToPool, AttackTransform,
                    Quaternion.Euler(0, 0, angle));
        }
        objectsPooled.Add(go);
        return objectsPooled[objectsPooled.Count - 1];
    }

}
