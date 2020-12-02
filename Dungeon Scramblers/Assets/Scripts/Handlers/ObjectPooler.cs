using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
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
            GameObject go = (GameObject)Instantiate(objectToPool, this.transform);
            go.SetActive(false);
            objectsPooled.Add(go);
        }
    }

    public GameObject GetPooledObject() {
        for (int i = 0; i < objectsPooled.Count; i++) {
            if (!objectsPooled[i].activeSelf) {
                return objectsPooled[i];
            }
        }
        GameObject go = (GameObject)Instantiate(objectToPool, this.transform);
        go.SetActive(false);
        objectsPooled.Add(go);
        return objectsPooled[objectsPooled.Count - 1];
    }
}
