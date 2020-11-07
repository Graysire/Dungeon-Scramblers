using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Doozy.Engine.UI;
using System;

public class DoozyTest : MonoBehaviour
{
    [SerializeField]
    private UIButton[] Buttons;
    [SerializeField]
    private UIButton Timer;
    int[] count = {0,0,0};
    public double TimeRemaining = 10;
    // Start is called before the first frame update
    void Start()
    {
        // Set default time for the timer, this can be changed within the editor
        TimeRemaining = 10f;
    }

    // subcribe to updateOccured delegate if the script is enabled
    private void OnEnable()
    {
        UpdateHandler.UpdateOccurred += TimerCountDown;
    }

    // unsubcribe to updateOccurred delegate if the script is disabled
    private void OnDisable()
    {
        UpdateHandler.UpdateOccurred -= TimerCountDown;
    }

    // Update is called once per frame with update handler
    void TimerCountDown()
    {
        //TimeRemaining -= Time.deltaTime;
        //TimeRemaining = Math.Round(TimeRemaining, 2);
        //Timer.SetLabelText("Time: " + TimeRemaining);
    }

    // Temporary usage for update
    // Calculate timer coundown and update the display
    private void Update()
    {
        if(TimeRemaining >= 0)
        {
            TimeRemaining -= Time.deltaTime;
            TimeRemaining = Math.Round(TimeRemaining, 2);
            Timer.SetLabelText("Time: " + TimeRemaining);
        }
        else
        {
            Timer.SetLabelText("Time: 0");
            // Call handle result here, possibly pop-up tab with the item that the player get
        }

        
    }

    // Call onclick
    public void IncrementCount(int index)
    {
        count[index]++;
        Buttons[index].SetLabelText(count[index].ToString());
    }

    // Call on click into another item, the event are not currently working
    public void DecrementCount(int index)
    {
        count[index]--;
        Buttons[index].SetLabelText(count[index].ToString());
    }

    private void HandleResult()
    {

    }

}
