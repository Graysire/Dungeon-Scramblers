using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateHandler : MonoBehaviour
{
    public delegate void onUpdate();
    public static event onUpdate UpdateOccurred; // Use this variable to add events/methods onto
                                                 /* Usage (must have both enable and disable):
                                                  * -----------------------------------------------
                                                  * private void OnEnable(){
                                                  *     UpdateHandler.UpdateOccurred += methodName;
                                                  * }
                                                  * private void OnDisable(){
                                                  *     UpdateHandler.UpdateOccurred -= methodName;
                                                  * }
                                                  */

    private void Update()
    {
        if (UpdateOccurred != null)             // If there are methods attached to the UpdateOccurred event...
            UpdateOccurred();                   // Call all of those methods at the same time in one Update() function
    }
}
