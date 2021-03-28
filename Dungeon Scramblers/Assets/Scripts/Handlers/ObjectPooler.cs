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


    protected void PoolObjectAtStart() {
        objectsPooled = new List<GameObject>();
        for (int i = 0; i < amountToPool; i++)
        {
            GameObject go;
            //PhotonID
            if (PhotonNetwork.CurrentRoom != null)
            {
                float angle = 0;
                object[] SpawnGoParams = new object[] {transform.position, angle };
                go = SpawnGO(SpawnGoParams);
                PhotonNetwork.AllocateViewID(go);
            }
            else
            {
                go = Instantiate(objectToPool, transform.position, new Quaternion());
            }
            //go.SetActive(false);
            objectsPooled.Add(go);
        }
    }

    public GameObject GetPooledObject() {
        for (int i = 0; i < objectsPooled.Count; i++) {
            if (!objectsPooled[i].activeSelf) {
                //objectsPooled[i].SetActive(true);
                int PhotonID = objectsPooled[i].GetPhotonView().ViewID;
                objectsPooled[i].GetComponent<ProjectileStats>().ShowProjectile(PhotonID);
                return objectsPooled[i];
            }
        }
        GameObject go;
        if (PhotonNetwork.CurrentRoom != null)
        {
            float angle = 0;
            object[] SpawnGoParams = new object[] { objectToPool, transform.position, angle };
            go = SpawnGO(SpawnGoParams);
            PhotonNetwork.AllocateViewID(go);
        }
        else
        {
            go = Instantiate(objectToPool, transform.position, new Quaternion());
        }
        //go.SetActive(false);
        go.GetComponent<ProjectileStats>().ResetProjectiles();
        objectsPooled.Add(go);
        return objectsPooled[objectsPooled.Count - 1];
    }


    public GameObject GetPooledObject(Vector3 AttackTransform, Vector3 AttackEnd, GameObject Player, float AbilityAngle)
    {

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
                int PhotonID = objectsPooled[i].GetPhotonView().ViewID;
                objectsPooled[i].GetComponent<ProjectileStats>().ShowProjectile(PhotonID);
                //objectsPooled[i].SetActive(true);
                objectsPooled[i].transform.position = AttackTransform;
                objectsPooled[i].transform.rotation = Quaternion.Euler(0, 0, angle);
                return objectsPooled[i];
            }
        }
        GameObject go;
        if (PhotonNetwork.CurrentRoom != null)
        {
            object[] SpawnGoParams = new object[] { AttackTransform, angle };
            go = SpawnGO(SpawnGoParams);
            PhotonNetwork.AllocateViewID(go);
        }
        else
        {
            go = Instantiate(objectToPool, AttackTransform,
                    Quaternion.Euler(0, 0, angle));
        }
        objectsPooled.Add(go);
        return objectsPooled[objectsPooled.Count - 1];
    }

    //Spawns GOs  Across Networks
    GameObject SpawnGO(object[] SpawnParams)
    {
        //Get position from data[]
        Vector3 position = (Vector3)SpawnParams[0];
       // Debug.Log("Position:" + position);
        //Get angle from data[]
        float angle = (float)SpawnParams[1];
        //Debug.Log("angle:" + angle);
        //Spawn object at this location
        GameObject goSpawned = PhotonNetwork.InstantiateRoomObject(objectToPool.name, position,
          Quaternion.Euler(0, 0, angle));
        //Set Parent
        goSpawned.transform.SetParent(gameObject.transform);
        //return for GO reference
        return goSpawned;
    }

}
