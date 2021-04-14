using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{

    private float timeRemaining;        //Remaining time left
    private bool timerActive = false;   //Determines if timer is counting down
    public Text timeText;               //Text display to show time left to players

    //link to reference : https://gamedevbeginner.com/how-to-make-countdown-timer-in-unity-minutes-seconds/

    //Update handler stuff
    protected virtual void OnEnable()
    {
        UpdateHandler.UpdateOccurred += RunTimerCountdown;
    }
    protected virtual void OnDisable()
    {
        UpdateHandler.UpdateOccurred -= RunTimerCountdown;
    }


    public void Awake()
    {
        //Makes this game object and text object disabled on awake
        gameObject.SetActive(false);
        timeText.gameObject.SetActive(false);
    }

    //Initializes the timer values and enables timer countdown
        // Called by GameManager for each state of match
    public void InitializeAndStartTimer(float time)
    {
        Debug.Log("Initializing Timer...");

        timeRemaining = time;
        DisplayTime(timeRemaining);

        //Start timer by activating it and enable timer
        timeText.gameObject.SetActive(true);
        gameObject.SetActive(true);
        timerActive = true;
    }

    //Disables the timer from updating -- parameter determines if time ran out which enables game over state.
    //Called by timer when out of time.
    //Called Game Manager when round completed, scramblers all die, or when voting phase/overlord plan phase ended.
    public void DisableTimer(bool outOfTime)
    {
        Debug.Log("Disabling Timer...");

        //sets time to 0 and disable timer
        timerActive = false;
        timeRemaining = 0;
        timeText.gameObject.SetActive(false);
        gameObject.SetActive(false);

        //if out of time then send ping to GameManager
        if (outOfTime)
        {
            Debug.Log("Sending out of time ping to Game Manager...");

            //Send ping to GameManager that timer ran out so it can handle game over state
            GameManager.ManagerInstance.TimerOver();
        }
    }


    //Will run by update handler
    private void RunTimerCountdown()
    {
        Debug.Log("RunTimerCountdown Operating...");
        if (timerActive)
        {
            //Timer countdown
            if (timeRemaining > 0)
            {
                //update time remaining
                timeRemaining -= Time.deltaTime;
            }
            //Timer done counting down
            else
            {
                Debug.Log("Time ran out!");
                DisableTimer(true);
            }

            //Display the timer information onto player screens
            DisplayTime(timeRemaining);
        }
    }

    //Updates the text information with current time left
    private void DisplayTime(float timeToDisplay)
    {
        //Get the minutes and seconds left to display onto text object
        //float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay);

        //Set text object
        timeText.text = string.Format("{0:00}", seconds);
    }

}
