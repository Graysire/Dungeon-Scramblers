using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private Camera cam;


    private Vector3 Origin;
    private Vector3 difference;


    // Update is called once per frame
    void Update()
    {
        PanCamera();
    }

    private void PanCamera()
    {
        //Save pos in worldspace on first click
        if (Input.GetMouseButtonDown(0))
        {
            Origin = cam.ScreenToWorldPoint(Input.mousePosition);
            Debug.Log("Origin: " + Origin);
        }

        //Get distance from previous point to current if button held down
        if (Input.GetMouseButton(0))
        {
            difference = Origin- cam.ScreenToWorldPoint(Input.mousePosition);
            Debug.Log("Difference: " + difference);
            cam.transform.position += difference;
        }
        //move to that destination
        

    }
}
