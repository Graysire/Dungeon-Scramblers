using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverlordEventHandler : MonoBehaviour
{
    public delegate void onOverlord();
    public static event onOverlord OverlordChange;



    private void Start()
    {
        if (OverlordChange != null)
            OverlordChange();
    }
}
