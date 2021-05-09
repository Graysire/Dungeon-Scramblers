using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ObjectDatabase : MonoBehaviour
{
    [SerializeField] protected List<GameObject> Objects;

    public List<GameObject> GetObjectList()
    {
        return Objects;
    }
}
