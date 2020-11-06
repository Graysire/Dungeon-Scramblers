using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Doozy.Engine.UI;

public class DoozyTest : MonoBehaviour
{
    [SerializeField]
    private UIButton[] Buttons;
    int[] count = {0,0,0};
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void IncrementCount(int index)
    {
        count[index]++;
        Buttons[index].SetLabelText(count[index].ToString());
    }

    public void DecrementCount(int index)
    {
        count[index]--;
        Buttons[index].SetLabelText(count[index].ToString());
    }
}
