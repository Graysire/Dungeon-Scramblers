using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayBar : MonoBehaviour
{

    public Slider BarDisplay;
    
    public void SetValue(float ratio)
    {
        BarDisplay.value = ratio;
    }

}
