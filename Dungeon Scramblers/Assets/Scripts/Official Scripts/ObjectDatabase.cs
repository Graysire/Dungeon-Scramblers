using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ObjectDatabase : MonoBehaviour
{
    [SerializeField] protected List<GameObject> Objects;

    public void Awake()
    {
        //Like the code to set up attack objects in Player.cs Awake()
        for (int i = 0; i < Objects.Count; i++)
        {
            if (PhotonNetwork.CurrentRoom != null)
            {
                Objects[i] = PhotonNetwork.Instantiate(Objects[i].name, gameObject.transform.position, new Quaternion());
                Objects[i].layer = gameObject.layer;
            }
            else
            {
                Objects[i] = Instantiate(Objects[i], gameObject.transform);
                Objects[i].layer = gameObject.layer;
            }
        }
    }

    public List<GameObject> GetObjectList()
    {
        return Objects;
    }
}
