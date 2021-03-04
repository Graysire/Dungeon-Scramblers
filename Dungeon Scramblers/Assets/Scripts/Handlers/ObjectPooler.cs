using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ObjectPooler : MonoBehaviour
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

    protected void PoolObjectAtStart() {
        objectsPooled = new List<GameObject>();
        for (int i = 0; i < amountToPool; i++)
        {
            GameObject go;
            //PhotonID
            if (PhotonNetwork.CurrentRoom != null)
            {
                go = PhotonNetwork.Instantiate(objectToPool.name, transform.position, new Quaternion());
            }
            else
            {
                go = PhotonNetwork.Instantiate(objectToPool.name, transform.position, new Quaternion());
            }
            go.SetActive(false);
            objectsPooled.Add(go);
        }
    }

    public GameObject GetPooledObject() {
        for (int i = 0; i < objectsPooled.Count; i++) {
            if (!objectsPooled[i].activeSelf) {
                objectsPooled[i].SetActive(true);
                return objectsPooled[i];
            }
        }
        GameObject go = PhotonNetwork.Instantiate(objectToPool.name, transform.position, new Quaternion());
        go.SetActive(false);
        objectsPooled.Add(go);
        return objectsPooled[objectsPooled.Count - 1];
    }

   // [PunRPC]
    public GameObject GetPooledObject(Vector3 AttackTransform, Vector3 AttackEnd, GameObject Player, float AbilityAngle) {
        //GetComponent<PhotonView>().RPC("GetPooledObject", RpcTarget.AllBuffered);

        //initialize this object since it is new
        if (objectsPooled == null)
        {
            PoolObjectAtStart();
        }

        for (int i = 0; i < objectsPooled.Count; i++)
        {
            if (!objectsPooled[i].activeSelf)
            {
                objectsPooled[i].SetActive(true);
                objectsPooled[i].transform.position = AttackTransform;
                objectsPooled[i].transform.rotation = Quaternion.Euler(0, 0, AttackEnd.x >= Player.transform.position.x ? -AbilityAngle : AbilityAngle);
                return objectsPooled[i];
            }
        }
        GameObject go = PhotonNetwork.Instantiate(objectToPool.name, AttackTransform,
                Quaternion.Euler(0, 0, AttackEnd.x >= Player.transform.position.x ? -AbilityAngle : AbilityAngle));
        objectsPooled.Add(go);
        return objectsPooled[objectsPooled.Count - 1];
    }

}
