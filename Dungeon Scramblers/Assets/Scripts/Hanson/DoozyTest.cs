using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DoozyTest : MonoBehaviour
{
    [SerializeField]
    private Text Count;

    int count = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void IncrementCount()
    {
        count++;
        Count.text = "Count: " + count.ToString();
    }

    public void DecrementCount()
    {
        count--;
        Count.text = "Count: " + count.ToString();
    }
}
